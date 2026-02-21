using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using SistemaBiometricoPolicia.Data;
using SistemaBiometricoPolicia.Utils;

namespace SistemaBiometricoPolicia.Biometric
{
    public class EstudianteIdentificadoEventArgs : EventArgs
    {
        public int EstudianteId { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Seccion { get; set; }
        public string NumeroDocumento { get; set; }
    }

    public class BiometricService : DPFP.Capture.EventHandler
    {
        private Capture Capturer;
        public event EventHandler<EstudianteIdentificadoEventArgs> EstudianteIdentificado;
        public event EventHandler<string> MensajeEstado;

        public BiometricService()
        {
            try
            {
                StatusHub.PushEvento("?? Inicializando SDK de DigitalPersona...");
                Capturer = new Capture();

                if (Capturer != null)
                {
                    Capturer.EventHandler = this;
                    StatusHub.Set("BIO", EstadoComponente.Ok, "SDK Inicializado");
                    StatusHub.PushEvento("? SDK de DigitalPersona inicializado correctamente.");
                    MensajeEstado?.Invoke(this, "Lector biométrico inicializado correctamente.");
                }
                else
                {
                    StatusHub.Set("BIO", EstadoComponente.Error, "SDK no pudo crear objeto Capture");
                    StatusHub.PushEvento("? ERROR: No se pudo crear objeto Capture del SDK.");
                    MensajeEstado?.Invoke(this, "ERROR: No se pudo inicializar el SDK.");
                }
            }
            catch (Exception ex)
            {
                StatusHub.Set("BIO", EstadoComponente.Error, "Error SDK: " + ex.Message);
                StatusHub.PushEvento("? ERROR CRÍTICO al inicializar SDK: " + ex.Message);
                LogHelper.RegistrarError("BiometricService Constructor", ex);
                MensajeEstado?.Invoke(this, "No se pudo inicializar el lector: " + ex.Message);
            }
        }

        public void IniciarCaptura()
        {
            if (Capturer != null)
            {
                try
                {
                    StatusHub.PushEvento("? Intentando iniciar captura del lector...");
                    Capturer.StartCapture();
                    StatusHub.Set("BIO", EstadoComponente.Ok, "Lector Activo - Esperando huella");
                    StatusHub.PushEvento("? Lector biométrico ACTIVO. Luz roja debería encenderse.");
                    MensajeEstado?.Invoke(this, "Esperando huella...");
                }
                catch (Exception ex)
                {
                    StatusHub.Set("BIO", EstadoComponente.Error, "No se pudo iniciar captura");
                    StatusHub.PushEvento("? ERROR: No se pudo iniciar captura. ¿Driver WBF instalado?");
                    StatusHub.PushEvento("   Detalle: " + ex.Message);
                    LogHelper.RegistrarError("IniciarCaptura", ex);
                    MensajeEstado?.Invoke(this, "Error al iniciar captura: " + ex.Message);
                }
            }
            else
            {
                StatusHub.Set("BIO", EstadoComponente.Error, "Capturer es NULL");
                StatusHub.PushEvento("? ERROR: Objeto Capturer no existe. SDK no inicializado.");
                MensajeEstado?.Invoke(this, "ERROR: Lector no inicializado.");
            }
        }

        public void DetenerCaptura()
        {
            if (Capturer != null)
            {
                try
                {
                    Capturer.StopCapture();
                    StatusHub.PushEvento("? Lector biométrico detenido.");
                }
                catch (Exception ex)
                {
                    StatusHub.PushEvento("? Error al detener captura: " + ex.Message);
                }
            }
        }

        public void OnComplete(object capture, string readerSerialNumber, Sample sample)
        {
            StatusHub.PushEvento("? Huella capturada físicamente del lector.");
            MensajeEstado?.Invoke(this, "Procesando huella...");
            ProcesarHuella(sample);
        }

        public void OnFingerGone(object capture, string readerSerialNumber)
        {
            StatusHub.PushEvento("?? Dedo retirado del lector.");
        }

        public void OnFingerTouch(object capture, string readerSerialNumber)
        {
            StatusHub.PushEvento("? Dedo detectado en el lector, capturando...");
            MensajeEstado?.Invoke(this, "Dedo detectado, mantenga la posición...");
        }

        public void OnReaderConnect(object capture, string readerSerialNumber)
        {
            StatusHub.Set("BIO", EstadoComponente.Ok, "Lector Conectado");
            StatusHub.PushEvento("? Lector biométrico CONECTADO físicamente.");
            StatusHub.PushEvento("   Serial: " + readerSerialNumber);
            MensajeEstado?.Invoke(this, "Lector conectado.");
        }

        public void OnReaderDisconnect(object capture, string readerSerialNumber)
        {
            StatusHub.Set("BIO", EstadoComponente.Advertencia, "Lector Desconectado");
            StatusHub.PushEvento("? Lector biométrico DESCONECTADO físicamente.");
            StatusHub.PushEvento("   Serial: " + readerSerialNumber);
            MensajeEstado?.Invoke(this, "? Lector desconectado.");
        }

        public void OnSampleQuality(object capture, string readerSerialNumber, CaptureFeedback feedback)
        {
            if (feedback == CaptureFeedback.Good)
            {
                StatusHub.PushEvento("? Calidad de huella: BUENA");
            }
            else
            {
                StatusHub.PushEvento("? Calidad de huella insuficiente: " + feedback.ToString());
                MensajeEstado?.Invoke(this, "Calidad de huella baja, intente nuevamente.");
            }
        }

        // ? MÉTODO PÚBLICO: ExtraerCaracteristicas
        public static FeatureSet ExtraerCaracteristicas(Sample sample, DataPurpose purpose)
        {
            try
            {
                var extractor = new FeatureExtraction();
                CaptureFeedback feedback = CaptureFeedback.None;
                var features = new FeatureSet();
                extractor.CreateFeatureSet(sample, purpose, ref feedback, ref features);

                if (feedback == CaptureFeedback.Good)
                {
                    StatusHub.PushEvento("? Características extraídas correctamente.");
                    return features;
                }

                StatusHub.PushEvento("? Extracción de características falló: " + feedback.ToString());
                return null;
            }
            catch (Exception ex)
            {
                StatusHub.PushEvento("? Error al extraer características: " + ex.Message);
                LogHelper.RegistrarError("ExtraerCaracteristicas", ex);
                return null;
            }
        }

        // ? MÉTODO PÚBLICO: ConvertirSampleABitmap
        public static Bitmap ConvertirSampleABitmap(Sample sample)
        {
            try
            {
                SampleConversion conversor = new SampleConversion();
                Bitmap bitmap = null;
                conversor.ConvertToPicture(sample, ref bitmap);
                return bitmap;
            }
            catch (Exception ex)
            {
                StatusHub.PushEvento("? Error al convertir huella a imagen: " + ex.Message);
                LogHelper.RegistrarError("ConvertirSampleABitmap", ex);
                return null;
            }
        }

        private void ProcesarHuella(Sample sample)
        {
            try
            {
                StatusHub.PushEvento("?? Procesando huella capturada...");
                var features = ExtraerCaracteristicas(sample, DataPurpose.Verification);

                if (features == null)
                {
                    MensajeEstado?.Invoke(this, "Huella de mala calidad, repita.");
                    return;
                }

                byte[] featuresBytes = ConvertirFeaturesToBytes(features);
                if (featuresBytes == null || featuresBytes.Length == 0)
                {
                    StatusHub.PushEvento("? Error al serializar características de la huella.");
                    MensajeEstado?.Invoke(this, "Error al procesar huella.");
                    return;
                }

                StatusHub.PushEvento("?? Comparando con base de datos...");

                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    string sql = @"SELECT h.Id, h.EstudianteId, h.TemplateHuella,
                                          e.Nombres, e.Apellidos, e.Seccion, e.NumeroDocumento
                                   FROM Huellas h
                                   INNER JOIN Estudiantes e ON e.Id = h.EstudianteId";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        int huellasComparadas = 0;
                        while (reader.Read())
                        {
                            huellasComparadas++;
                            byte[] templateBytes = (byte[])reader["TemplateHuella"];

                            if (CompararHuellas(featuresBytes, templateBytes))
                            {
                                StatusHub.PushEvento("? ESTUDIANTE IDENTIFICADO: " + reader["Nombres"] + " " + reader["Apellidos"]);
                                MensajeEstado?.Invoke(this, "? Estudiante identificado.");

                                EstudianteIdentificado?.Invoke(this, new EstudianteIdentificadoEventArgs
                                {
                                    EstudianteId = Convert.ToInt32(reader["EstudianteId"]),
                                    Nombre = reader["Nombres"].ToString(),
                                    Apellidos = reader["Apellidos"].ToString(),
                                    Seccion = reader["Seccion"].ToString(),
                                    NumeroDocumento = reader["NumeroDocumento"].ToString()
                                });
                                return;
                            }
                        }

                        StatusHub.PushEvento($"? Huella NO registrada (comparadas: {huellasComparadas} huellas).");
                    }
                }

                MensajeEstado?.Invoke(this, "? Huella no registrada.");
            }
            catch (Exception ex)
            {
                StatusHub.PushEvento("? ERROR al procesar huella: " + ex.Message);
                LogHelper.RegistrarError("ProcesarHuella", ex);
                MensajeEstado?.Invoke(this, "Error procesando huella: " + ex.Message);
            }
        }

        private byte[] ConvertirFeaturesToBytes(FeatureSet features)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    features.Serialize(ms);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                StatusHub.PushEvento("? Error al serializar FeatureSet: " + ex.Message);
                LogHelper.RegistrarError("ConvertirFeaturesToBytes", ex);
                return null;
            }
        }

        private bool CompararHuellas(byte[] features1, byte[] features2)
        {
            try
            {
                if (Math.Abs(features1.Length - features2.Length) > 100)
                    return false;

                int minLength = Math.Min(features1.Length, features2.Length);
                int coincidencias = 0;

                for (int i = 0; i < minLength; i++)
                {
                    if (Math.Abs(features1[i] - features2[i]) <= 5)
                        coincidencias++;
                }

                double similitud = (double)coincidencias / minLength * 100;
                return similitud >= 85.0;
            }
            catch (Exception ex)
            {
                StatusHub.PushEvento("? Error al comparar huellas: " + ex.Message);
                LogHelper.RegistrarError("CompararHuellas", ex);
                return false;
            }
        }

        public byte[] CapturarYSerializarHuella(Sample sample)
        {
            try
            {
                var features = ExtraerCaracteristicas(sample, DataPurpose.Enrollment);
                if (features != null)
                {
                    return ConvertirFeaturesToBytes(features);
                }
                return null;
            }
            catch (Exception ex)
            {
                StatusHub.PushEvento("? Error al capturar y serializar huella: " + ex.Message);
                LogHelper.RegistrarError("CapturarYSerializarHuella", ex);
                return null;
            }
        }
    }
}