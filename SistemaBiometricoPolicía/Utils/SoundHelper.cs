using System;
using System.Media;
using System.IO;

namespace SistemaBiometricoPolicia.Utils
{
    public static class SoundHelper
    {
        private static string rutaSonidos = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sonidos");

        // ? MÉTODOS ORIGINALES (mantener)
        public static void PlaySuccess()
        {
            Play("success.wav");
        }

        public static void PlayError()
        {
            Play("error.wav");
        }

        public static void PlayAlert()
        {
            Play("alert.wav");
        }

        // ? MÉTODOS NUEVOS: Alias para compatibilidad
        public static void ReproducirExito()
        {
            PlaySuccess();
        }

        public static void ReproducirError()
        {
            PlayError();
        }

        public static void ReproducirAlerta()
        {
            PlayAlert();
        }

        private static void Play(string archivo)
        {
            try
            {
                string rutaCompleta = Path.Combine(rutaSonidos, archivo);
                if (File.Exists(rutaCompleta))
                {
                    using (SoundPlayer player = new SoundPlayer(rutaCompleta))
                    {
                        player.Play();
                    }
                }
                else
                {
                    // Si no existe el archivo, usar sonidos del sistema
                    if (archivo.Contains("success"))
                        SystemSounds.Asterisk.Play();
                    else if (archivo.Contains("error"))
                        SystemSounds.Hand.Play();
                    else if (archivo.Contains("alert"))
                        SystemSounds.Exclamation.Play();
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al reproducir sonido: " + archivo, ex);
            }
        }
    }
}