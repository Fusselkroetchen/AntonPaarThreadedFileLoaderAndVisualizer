using AntonPaarThreadedFileLoaderAndVisualizer.GenericComponents;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

/**
 * Aufgabentrennung zwischen Thread Managing und Wordzählung.
 * Die Thread-Manager entkoppeln noch einmal die Aufgaben von der UI vollständig.
 * Das heißt nicht, dass beim Parsen zur Geschwindigkeitsoptimierung keine Threads 
 * verwendet werden können. Hier geht es nur um die Entkoppelung.
 */
namespace AntonPaarThreadedFileLoaderAndVisualizer.Components.WordCountParserThreadManager
{
    using LoadProgressStatus = int;

    //INTERFACE
    public interface IWordCountParserThreadManagerFactory
    {
        static abstract IWordCountParserThreadManager create();
    }
    public interface IWordCountParserThreadManager
    {
        /// <summary>
        /// Erste iteration der Funktion. Wurde weiter entwickelt zu parseForWordPairsChunked.
        /// Aus Demonstrationszwecken immer noch vorhanden.
        /// 
        /// Zählt vorhandene Wörter.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onLoadFileContentFinnished"></param>
        /// <param name="onProgressChanged"></param>
        /// <param name="sortByValue"></param>
        /// <param name="descending"></param>
        public void parseForWordPairs(
            string text,
            Action<WordCountParserResult?> onLoadFileContentFinnished,
            Action<LoadProgressStatus>? onProgressChanged,
            bool sortByValue = true,
            bool descending = true
        );

        /// <summary>
        /// Nutzt Hardware ressourcen Vollständig aus, ohne die UI zu blockieren. Somit geht das Parsen viel
        /// schneller.
        /// 
        /// Zählt vorhandene Wörter.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onLoadFileContentFinnished"></param>
        /// <param name="onProgressChanged"></param>
        /// <param name="sortByValue"></param>
        /// <param name="descending"></param>
        /// <param name="numThreads"></param>
        public void parseForWordPairsChunked(
            string text,
            Action<WordCountParserResult?> onLoadFileContentFinnished,
            Action<LoadProgressStatus>? onProgressChanged,
            bool sortByValue = true,
            bool descending = true,
            int numThreads = 4
        );

        public void cancelParseForWordPairs();

        /// <summary>
        /// Wenn der Benutzer sortieren möchte muss nicht die ganze Liste neu geparsed werden.
        /// Somit kann vom letzten geparsten Stand neu sortiert werden.
        /// </summary>
        /// <param name="onFinnished"></param>
        /// <param name="sortByValue"></param>
        /// <param name="descending"></param>
        public void sortFromLastParsedData(
            Action<WordCountParserResult?> onFinnished, 
            bool sortByValue = true,
            bool descending = true
        );
    }

    //CLASS
    class WordCountParserThreadManager : IWordCountParserThreadManagerFactory, IWordCountParserThreadManager
    {
        //DPI
        IWordCountParser wordCountParser;

        //FACTORY
        public static IWordCountParserThreadManager create()
        {
            return new WordCountParserThreadManager(
                WordCountParser.create()
            );
        }

        //INIT
        private WordCountParserThreadManager(IWordCountParser wordCountParser)
        {
            this.wordCountParser = wordCountParser;

            //merke Main-Thread
            dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }

        //FUNC
        private Dispatcher dispatcher;
        private Task? task;
        private CancellationTokenSource? cts;
        private CancellationToken? token;
        private ConcurrentQueue<CancellationTokenSource> dispatcherCancellationTokenSourceList = new ConcurrentQueue<CancellationTokenSource>();

        /// <summary>
        /// Erste iteration der Funktion. Wurde weiter entwickelt zu parseForWordPairsChunked.
        /// Aus Demonstrationszwecken immer noch vorhanden.
        /// 
        /// Zählt vorhandene Wörter.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onLoadFileContentFinnished"></param>
        /// <param name="onProgressChanged"></param>
        /// <param name="sortByValue"></param>
        /// <param name="descending"></param>
        public void parseForWordPairs(
            string text,
            Action<WordCountParserResult?> onLoadFileContentFinnished,
            Action<LoadProgressStatus>? onProgressChanged,
            bool sortByValue = true,
            bool descending = true
        ) {
            cancelParseForWordPairs();

            cts = new CancellationTokenSource();
            var newToken = cts.Token;
            token = newToken;

            task = Task.Run(() => {
                double progressFPS = 0;
                double time1 = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                double time2 = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                WordCountParserResult? result = wordCountParser.parseForWordPairs(text, (progress) =>
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

                }, sortByValue, descending);

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

        /// <summary>
        /// Nutzt Hardware ressourcen Vollständig aus, ohne die UI zu blockieren. Somit geht das Parsen viel
        /// schneller.
        /// 
        /// Zählt vorhandene Wörter.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onLoadFileContentFinnished"></param>
        /// <param name="onProgressChanged"></param>
        /// <param name="sortByValue"></param>
        /// <param name="descending"></param>
        /// <param name="numThreads"></param>
        public void parseForWordPairsChunked(
            string text,
            Action<WordCountParserResult?> onLoadFileContentFinnished,
            Action<LoadProgressStatus>? onProgressChanged,
            bool sortByValue = true,
            bool descending = true,
            int numThreads = 4
        )
        {
            cancelParseForWordPairs();

            cts = new CancellationTokenSource();
            var newToken = cts.Token;
            token = newToken;

            task = Task.Run(() =>
            {
                double progressFPS = 0;
                double time1 = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                double time2 = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                WordCountParserResult? result = wordCountParser.parseForWordPairsChunked(text, (progress) =>
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
                        () =>
                        {
                            if (onProgressChanged != null) onProgressChanged(progress);
                        },
                        DispatcherPriority.ApplicationIdle,
                        cancellationTokenSource.Token
                    );

                    dispatcherCancellationTokenSourceList.Enqueue(
                        cancellationTokenSource
                    );

                }, sortByValue, descending, numThreads);

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
            });
        }

        public void cancelParseForWordPairs()
        {
            //fileLoader.cancelFileLoad();
            cts?.Cancel();

            //lock (dispatcherCancellationTokenSourceList) { //Wird nicht benötigt da die Liste Threadsafe ist.
            foreach (var cancellationTokenSource in dispatcherCancellationTokenSourceList)
            {
                cancellationTokenSource.Cancel();

            }
            dispatcherCancellationTokenSourceList.Clear();
            //}
        }

        /// <summary>
        /// Wenn der Benutzer sortieren möchte muss nicht die ganze Liste neu geparsed werden.
        /// Somit kann vom letzten geparsten Stand neu sortiert werden.
        /// </summary>
        /// <param name="onFinnished"></param>
        /// <param name="sortByValue"></param>
        /// <param name="descending"></param>
        public void sortFromLastParsedData(
            Action<WordCountParserResult?> onFinnished,
            bool sortByValue = true,
            bool descending = true
        )
        {
            task = Task.Run(() =>
            {
                WordCountParserResult? result = wordCountParser.sortFromLastParsedData(sortByValue, descending);
                this.dispatcher.Invoke(
                    () => onFinnished(result),
                    DispatcherPriority.Normal
                );
            });
        }

        ~WordCountParserThreadManager()
        {
            cancelParseForWordPairs();
        }
    }
}
