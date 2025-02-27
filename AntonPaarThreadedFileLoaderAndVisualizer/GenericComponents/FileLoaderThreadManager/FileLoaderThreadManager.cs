using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Threading;

/**
 * Bei dem FileLoader und dem FileLoaderThreadManager wurde besonders auf Clean Code geachtet.
 * Ggf. mit mehr Recherche hätte auch noch die Geschwindigkeit beim Laden optimiert werden können,
 * so dass jeder Buffer-Abschnitt im Stream mit einem Thread ausgelesen wird.
 * Mir war aber die saubere Trennung der Themen nach SOLID in dem Fall wichtiger.
 */
namespace AntonPaarThreadedFileLoaderAndVisualizer.GenericComponents
{
    using LoadProgressStatus = int;

    //INTERFACES
    public interface IFileLoaderThreadManagerFactory
    {
        static abstract IFileLoaderThreadManager create();
    }
    public interface IFileLoaderThreadManager
    {
        public void loadFileContentThreaded(
            string filePath,
            Action<FileLoaderResult> onLoadFileContentFinnished,
            Action<LoadProgressStatus>? onProgressChanged
        );

        public void cancelLoadFileContentThreaded();
    }
    
    //CLASS
    class FileLoaderThreadManager : IFileLoaderThreadManagerFactory, IFileLoaderThreadManager
    {
        //DPIS
        private IFileLoader fileLoader;

        //FACTORY
        public static IFileLoaderThreadManager create()
        {
            return new FileLoaderThreadManager(
                FileLoader.create()
            );
        }

        //INIT
        private FileLoaderThreadManager(
            IFileLoader fileLoader
        )
        {
            this.fileLoader = fileLoader;

            //merke Main-Thread
            dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }

        //FUNC
        private Dispatcher dispatcher;
        private Task? task;
        private CancellationTokenSource? cts;
        private CancellationToken? token;
        private ConcurrentQueue<CancellationTokenSource> dispatcherCancellationTokenSourceList = new ConcurrentQueue<CancellationTokenSource>();
        public void loadFileContentThreaded(
            string filePath,
            Action<FileLoaderResult> onLoadFileContentFinnished,
            Action<LoadProgressStatus>? onProgressChanged
        ) {
            cancelLoadFileContentThreaded();

            cts = new CancellationTokenSource();
            var newToken = cts.Token;
            token = newToken;

            task = Task.Run(() => {
                double progressFPS = 0;
                double time1 = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                double time2 = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                FileLoaderResult result = fileLoader.loadFileContent(filePath, (progress) =>
                {
                    //FPS
                    time2 = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                    double deltaTime = time2 - time1;

                    progressFPS = 1 / deltaTime * 1000;

                    //Zuviele Zugriffe auf den UI Thread wird unterbunden, so das die UI nicht ausgebremst wird.
                    //Maximal X Zugriffe in der Sekunde. Nagut ist nicht wirklich notwendig hier aber so als Beispiel.
                    if (progressFPS > 5) { return; } 

                    //FPS begrenzen
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                    this.dispatcher.Invoke(
                        () => { 
                            if (onProgressChanged != null) onProgressChanged(progress);
                        }, 
                        DispatcherPriority.ApplicationIdle, 
                        cancellationTokenSource.Token
                    );

                    dispatcherCancellationTokenSourceList.Enqueue(
                        cancellationTokenSource
                    );

                });

                //public void Invoke(Action callback, DispatcherPriority priority, CancellationToken cancellationToken);

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                this.dispatcher.Invoke(
                    () => onLoadFileContentFinnished(result), 
                    DispatcherPriority.ApplicationIdle,
                    cancellationTokenSource.Token
                );


                dispatcherCancellationTokenSourceList.Enqueue(
                    cancellationTokenSource
                );
            }, newToken);
        }

        public void cancelLoadFileContentThreaded()
        {
            fileLoader.cancelFileLoad();
            cts?.Cancel();

            //lock (dispatcherCancellationTokenSourceList) { //Wird nicht benötigt da die Liste Threadsafe ist.
                foreach (var cancellationTokenSource in dispatcherCancellationTokenSourceList)
                {
                    cancellationTokenSource.Cancel();
                    
                }
                dispatcherCancellationTokenSourceList.Clear();
            //}
        }

        ~FileLoaderThreadManager() {
            cancelLoadFileContentThreaded();
        }
    }
}
