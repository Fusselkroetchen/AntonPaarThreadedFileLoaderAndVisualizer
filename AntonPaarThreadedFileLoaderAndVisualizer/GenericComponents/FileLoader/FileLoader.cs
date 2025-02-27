using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AntonPaarThreadedFileLoaderAndVisualizer.GenericComponents
{
    //DATA
    using LoadProgressStatus = int;

    //INTERFACES
    public interface IFileLoaderFactory
    {
        static abstract IFileLoader create();
    }

    public interface IFileLoader
    {
        /// <summary>
        /// Lädt den Datei-Content mit prozess callback.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="onProgressChanged"></param>
        /// <returns></returns>
        public FileLoaderResult loadFileContent(
            string filePath,
            Action<LoadProgressStatus>? onProgressChanged
        );

        public void cancelFileLoad();
    }

    //CLASS

    public class FileLoader : IFileLoader, IFileLoaderFactory
    {
        //Factory
        public static IFileLoader create()
        {
            return new FileLoader();
        }

        //FUNC
        private bool isCanceled = false; 
        /// <summary>
        /// Lädt den Datei-Content mit prozess callback.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="onProgressChanged"></param>
        /// <returns></returns>
        public FileLoaderResult loadFileContent(
            string filePath, 
            Action<LoadProgressStatus>? onProgressChanged
        ) {
            isCanceled = false;

            if (!File.Exists(filePath))
            {
                return new FileLoaderResult
                {
                    fileLoadProcessResultStatus = FileLoadProcessResultStatus.fileNotFound
                };
            }

            if (!isFileReadableByPermission(filePath))
            {
                return new FileLoaderResult
                {
                    fileLoadProcessResultStatus = FileLoadProcessResultStatus.permissionDenied
                };
            }

            // Hole die Dateigröße, um den Fortschritt berechnen zu können
            FileInfo fileInfo = new FileInfo(filePath);
            long totalBytes = fileInfo.Length;
            long bytesRead = 0;

            StringBuilder fileContentBuilder = new StringBuilder();

            // Erstelle einen StreamReader mit ANSI-Encoding
            using (StreamReader reader = new StreamReader(filePath, Encoding.Default))
            {
                char[] buffer = new char[1024]; // Puffergröße
                int charsRead;

                while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileContentBuilder.Append(buffer, 0, charsRead);
                    
                    bytesRead += charsRead; // Bei ANSI-Kodierung entspricht charsRead der Anzahl der gelesenen Bytes 
                    //bytesRead += charsRead * sizeof(char); // Bei UTF16-Kodierung entspricht charsRead * sizeof(chars) der Anzahl der gelesenen Bytes 
                    double progress = (double)bytesRead / totalBytes * 100;
                    
                    // Update den Fortschritt
                    onProgressChanged?.Invoke((int)progress);

                    if (isCanceled) {

                        return new FileLoaderResult
                        {
                            fileLoadProcessResultStatus = FileLoadProcessResultStatus.canceled
                        };
                    }
                    
                }

                return new FileLoaderResult
                {
                    fileLoadProcessResultStatus = FileLoadProcessResultStatus.success,
                    fileContent = fileContentBuilder.ToString()
                };
            }
        }
        /// <summary>
        /// Checkt datei permission
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool isFileReadableByPermission(string filePath)
        {
            // Überprüfen der Leseberechtigung
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                FileSecurity fileSecurity = fileInfo.GetAccessControl();

                // Der aktuelle Benutzer
                WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
                AuthorizationRuleCollection rules = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));

                bool hasReadPermission = false;

                foreach (FileSystemAccessRule rule in rules)
                {
                    // Überprüfen, ob der Benutzer Leserechte hat
                    if (rule.AccessControlType == AccessControlType.Allow &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.Read))
                    {
                        hasReadPermission = true;
                        break;
                    }
                }

                if (hasReadPermission)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

/**
 * Wenn der Loader in der While-Schleife eine große Datei liest, ist dieser Thread blockiert und kann
 * nicht mehr reagieren. Auch Abbruch-Befehle von außen funktionieren dann nicht. 
 * Mit Thread.Sleep() könnte dies verhindert werden, aber das würde die While-Schleife nur 
 * unnötig langsam machen. Daher habe ich mich für eine Cancel-Funktion entschieden.
 * Natürlich könnte ich auch ein FPS Counter für die While Schleife einbauen um von Zeit zu Zeit
 * auf System-Befehle zu hören aber in dem Fall denke ich Keep it Simple.
*/

        public void cancelFileLoad()
        {
            isCanceled = true;
        }
    }
}
