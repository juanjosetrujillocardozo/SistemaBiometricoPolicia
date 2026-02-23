using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia.Utils
{
    public static class WebcamHelper
    {
        private static VideoCaptureDevice videoSource;
        private static PictureBox pictureBoxDestino;
        private static Bitmap ultimoFrame;

        public static bool HayWebcamDisponible()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            return videoDevices.Count > 0;
        }

        // ? MÉTODO ESTÁTICO: Iniciar
        public static void Iniciar(PictureBox pictureBox)
        {
            try
            {
                pictureBoxDestino = pictureBox;

                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                {
                    throw new Exception("No se encontró ninguna cámara web conectada.");
                }

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
                Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

                if (pictureBoxDestino != null)
                {
                    if (pictureBoxDestino.InvokeRequired)
                    {
                        pictureBoxDestino.BeginInvoke(new Action(() =>
                        {
                            ActualizarImagenPictureBox(frame);
                        }));
                    }
                    else
                    {
                        ActualizarImagenPictureBox(frame);
                    }
                }

                ultimoFrame?.Dispose();
                ultimoFrame = (Bitmap)frame.Clone();
            }
            catch { }
        }

        private static void ActualizarImagenPictureBox(Bitmap nuevoFrame)
        {
            try
            {
                var viejaImagen = pictureBoxDestino.Image;
                pictureBoxDestino.Image = nuevoFrame;
                viejaImagen?.Dispose();
            }
            catch { }
        }

        public static bool EstaActiva()
        {
            try
            {
                return videoSource != null && videoSource.IsRunning;
            }
            catch
            {
                return false;
            }
        }

        // ? MÉTODO ESTÁTICO: CapturarFoto
        public static Bitmap CapturarFoto()
        {
            if (ultimoFrame == null)
                return null;

            return (Bitmap)ultimoFrame.Clone();
        }

        // ? MÉTODO ESTÁTICO: Detener
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
                        videoSource.Stop();
                    }
                    videoSource = null;
                }

                if (pictureBoxDestino != null)
                {
                    pictureBoxDestino.Image?.Dispose();
                    pictureBoxDestino.Image = null;
                }

                ultimoFrame?.Dispose();
                ultimoFrame = null;
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al detener cámara", ex);
            }
        }
    }
}