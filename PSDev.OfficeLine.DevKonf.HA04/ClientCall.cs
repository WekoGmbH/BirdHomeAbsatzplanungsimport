using System;
using System.IO;

namespace WEKO.BirdHome.Absatzplanungimport
{
    public class ClientCall : Sagede.OfficeLine.Engine.AppLibraryExecuteBase
    {
        protected override string Execute()
        {
            try
            {
                switch (base.FunctionName)
                {
                    case "OpenDialog":

                        var filename = string.Empty;
                        var file = GetFile(ref filename);
                        var blobPath = string.Empty;
                        var blobPathFile = string.Empty;

                        if (file != null)
                        {
                            // Blob-Storage
                            var blobProvider = new BlobProvider(base.Mandant.Credential.Name, base.Mandant.Credential.Password);
                            // Datei zum BlobStorage schicken und Tracking zurückgeben
                            blobPath = BlobHelper.GetBlobPath(base.Mandant);
                            filename = Path.GetFileName(filename);
                            /*filename = Path.GetFullPath(filename);*/
                            blobProvider.SaveBlob(filename, blobPath, file);
                            blobPathFile = string.Join("/", blobPath, filename);

                            base.Data.Fill("Importdatei", filename);
                            base.Data.Fill("BlobPathFile", blobPathFile);
                            base.Data.Fill("BlobPath", blobPath);
                        }
                        break;

                    default:
                        break;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Öffnet einen Open-File-Dialog und gibt das Ergebnis als
        /// Byte[] zurück. Wird keine Datei ausgewählt, wird null zurückgegeben.
        /// </summary>
        /// <returns></returns>
        public static byte[] GetFile(ref string filename)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Multiselect = false,
                CheckFileExists = true,
                ValidateNames = true,
                AddExtension = true,
                DefaultExt = "xlsx",
                Filter = "Excel-Dateien|*.xlsx",
                InitialDirectory = System.Environment.SpecialFolder.DesktopDirectory.ToString(),
                FileName = filename
            };

            dlg.ShowDialog();

            filename = dlg.FileName;

            if (String.IsNullOrEmpty(filename)) return null;
            if (!File.Exists(filename)) return null;

            var content = File.ReadAllBytes(filename);

            return content;
        }

        private void handleExecutionClient(byte[] content)
        {
        }
    }
}