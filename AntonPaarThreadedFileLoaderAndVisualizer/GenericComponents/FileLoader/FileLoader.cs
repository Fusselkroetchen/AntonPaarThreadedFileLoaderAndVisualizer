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

        /// <summary>
        /// Lädt den Datei-Content mit prozess callback in Thead Chunks.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="onProgressChanged"></param>
        /// <returns></returns>
        public Task<FileLoaderResult> loadFileContentChunkedAsync(
            string filePath,
            Action<int>? onProgressChanged,
            int chunkSize = 8192,  // Größere Puffergröße für mehr Performance
            int parallelTasks = 4  // Anzahl paralleler Lesevorgänge
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

        public async Task<FileLoaderResult> loadFileContentChunkedAsync(
            string filePath,
            Action<int>? onProgressChanged,
            int chunkSize = 8192,
            int parallelTasks = 4
        )
        {
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

            FileInfo fileInfo = new FileInfo(filePath);
            long totalBytes = fileInfo.Length;
            long bytesReadTotal = 0;

            // Speicher für die gelesenen Chunks (Reihenfolge bleibt erhalten)
            string[] chunks = new string[(totalBytes + chunkSize - 1) / chunkSize]; // Array für garantierte Reihenfolge

            object lockObj = new object();
            long currentOffset = 0;

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < parallelTasks; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    byte[] buffer = new byte[chunkSize];

                    // Jeder Thread bekommt seinen eigenen FileStream!
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, chunkSize, true))
                    {
                        while (true)
                        {
                            long localOffset;
                            int chunkIndex;

                            // Kritischen Abschnitt synchronisieren
                            lock (lockObj)
                            {
                                if (currentOffset >= totalBytes) break;
                                localOffset = currentOffset;
                                chunkIndex = (int)(currentOffset / chunkSize);
                                currentOffset += chunkSize;
                            }

                            fs.Seek(localOffset, SeekOrigin.Begin);
                            int readBytes = await fs.ReadAsync(buffer, 0, chunkSize);

                            if (readBytes == 0) break;

                            string chunkText = Encoding.Default.GetString(buffer, 0, readBytes);

                            lock (lockObj)
                            {
                                chunks[chunkIndex] = chunkText; // Richtiger Platz im Array
                                bytesReadTotal += readBytes;
                                int progress = (int)((double)bytesReadTotal / totalBytes * 100);
                                onProgressChanged?.Invoke(progress);
                            }

                            if (isCanceled) return;
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            if (isCanceled)
            {
                return new FileLoaderResult
                {
                    fileLoadProcessResultStatus = FileLoadProcessResultStatus.canceled
                };
            }

            // Alle Chunks in der richtigen Reihenfolge zusammenführen
            string fileContent = string.Concat(chunks);

            return new FileLoaderResult
            {
                fileLoadProcessResultStatus = FileLoadProcessResultStatus.success,
                fileContent = fileContent
            };
        }

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
