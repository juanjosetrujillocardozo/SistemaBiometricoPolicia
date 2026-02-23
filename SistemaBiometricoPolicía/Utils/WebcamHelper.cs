using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia.Utils
{
    public static class WebcamHelper
    {
        private static VideoCaptureDevice videoSource;
        private static PictureBox pictureBoxDestino;
        private static Bitmap ultimoFrame;
        private static readonly object _lockFrame = new object();  // ? NUEVO: lock para thread safety

        public static bool HayWebcamDisponible()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            return videoDevices.Count > 0;
        }

        public static void Iniciar(PictureBox pictureBox)
        {
            try
            {
                // ? NUEVO: Si ya está corriendo, detener primero
                if (videoSource != null && videoSource.IsRunning)
                    Detener();

                pictureBoxDestino = pictureBox;

                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                    throw new Exception("No se encontró ninguna cámara web conectada.");

                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += VideoSource_NewFrame;
                videoSource.Start();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al iniciar la cámara: {ex.Message}");
            }
        }

        private static void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // ? NUEVO: Verificar que el destino no esté destruido antes de procesar
                if (pictureBoxDestino == null || pictureBoxDestino.IsDisposed) return;

                Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

                // ? NUEVO: Guardar ultimoFrame con lock para thread safety
                lock (_lockFrame)
                {
                    ultimoFrame?.Dispose();
                    ultimoFrame = (Bitmap)frame.Clone();
                }

                pictureBoxDestino.BeginInvoke(new Action(() =>
                {
                    // ? NUEVO: Doble check dentro del BeginInvoke (puede ejecutarse tarde)
                    if (pictureBoxDestino == null || pictureBoxDestino.IsDisposed)
                    {
                        frame.Dispose();
                        return;
                    }
                    ActualizarImagenPictureBox(frame);
                }));
            }
            catch { }
        }

        private static void ActualizarImagenPictureBox(Bitmap nuevoFrame)
        {
            try
            {
                if (pictureBoxDestino == null || pictureBoxDestino.IsDisposed)
                {
                    nuevoFrame?.Dispose();  // ? NUEVO: Evitar leak si el destino ya no existe
                    return;
                }
                var viejaImagen = pictureBoxDestino.Image;
                pictureBoxDestino.Image = nuevoFrame;
                viejaImagen?.Dispose();
            }
            catch { }
        }

        public static bool EstaActiva()
        {
            try { return videoSource != null && videoSource.IsRunning; }
            catch { return false; }
        }

        public static Bitmap CapturarFoto()
        {
            // ? NUEVO: Lock para leer ultimoFrame de forma segura
            lock (_lockFrame)
            {
                return ultimoFrame != null ? (Bitmap)ultimoFrame.Clone() : null;
            }
        }

        public static void Detener()
        {
            try
            {
                if (videoSource != null)
                {
                    videoSource.NewFrame -= VideoSource_NewFrame;

                    if (videoSource.IsRunning)
                    {
                        videoSource.SignalToStop();
                        videoSource.WaitForStop();  // ? CORREGIDO: WaitForStop en lugar de Stop()
                    }
                    videoSource = null;
                }

                // ? NUEVO: Limpiar ultimoFrame con lock
                lock (_lockFrame)
                {
                    ultimoFrame?.Dispose();
                    ultimoFrame = null;
                }

                if (pictureBoxDestino != null)
                {
                    if (!pictureBoxDestino.IsDisposed)
                    {
                        pictureBoxDestino.Image?.Dispose();
                        pictureBoxDestino.Image = null;
                    }
                    pictureBoxDestino = null;  // ? NUEVO: Soltar referencia siempre
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al detener cámara", ex);
            }
        }
    }
}