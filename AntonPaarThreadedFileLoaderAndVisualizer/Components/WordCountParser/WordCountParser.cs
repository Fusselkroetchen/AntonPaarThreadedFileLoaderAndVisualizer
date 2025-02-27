using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntonPaarThreadedFileLoaderAndVisualizer.Components
{
    using LoadProgressStatus = int;
    //INTERFACE
    interface IWordCountParserFactory
    {
        static abstract IWordCountParser create();
    }
    interface IWordCountParser
    {
        public (ListViewItem[], uint) parseForWordPairs(
            string text,
            Action<LoadProgressStatus>? onProgressChanged,
            bool sortByValue = true, 
            bool descending = true
        );
        public (ListViewItem[], uint) parseForWordPairsChunked(
            string text,
            Action<int> onProgressChanged,
            bool sortByValue = true,
            bool descending = true,
            int numThreads = 4
        );
        public (ListViewItem[], uint) sortFromLastParsedData(
            bool sortByValue = true, 
            bool descending = true
        );
    }

    //CLASS
    class WordCountParser : IWordCountParserFactory, IWordCountParser
    {
        //DPI

        //FACTORY
        public static IWordCountParser create()
        {
            return new WordCountParser(

            );
        }

        //INIT
        private WordCountParser()
        {

        }

        //FUNC
        private Dictionary<string, int> lastResultsParsedDictonary;

        /// <summary>
        /// Wenn der Benutzer sortieren möchte muss nicht die ganze Liste neu geparsed werden.
        /// Somit kann vom letzten geparsten Stand neu sortiert werden.
        /// </summary>
        /// <param name="sortByValue"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public (ListViewItem[], uint) sortFromLastParsedData(
            bool sortByValue = true, bool descending = true
        )
        {
            return sortAndConvertData(lastResultsParsedDictonary, sortByValue, descending);
        }

        public (ListViewItem[], uint) parseForWordPairs(  
            string text,
            Action<LoadProgressStatus>? onProgressChanged,
            bool sortByValue = true, bool descending = true
        )
        {
            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int totalWords = words.Length;
            int processed = 0;

            // Wörter zählen in einem einzigen Durchlauf
            var wordCounts = new Dictionary<string, int>();

            foreach (var word in words)
            {
                if (wordCounts.ContainsKey(word))
                    wordCounts[word]++;
                else
                    wordCounts[word] = 1;

                processed++;
                if (totalWords > 0) // Fortschritt nur berechnen, wenn Wörter vorhanden sind
                    onProgressChanged?.Invoke((processed * 100) / totalWords);
            }

            lastResultsParsedDictonary = wordCounts;

            return sortAndConvertData(wordCounts, sortByValue, descending);
        }

        private (ListViewItem[], uint) sortAndConvertData(
            Dictionary<string, int> wordCounts, 
            bool sortByValue = true, 
            bool descending = true
        ) {
            IOrderedEnumerable<KeyValuePair<string, int>> sortedResult;

            if (sortByValue)
            {
                sortedResult = descending
                    ? wordCounts.OrderByDescending(kv => kv.Value, Comparer<int>.Default)
                    : wordCounts.OrderBy(kv => kv.Value, Comparer<int>.Default);
            }
            else
            {
                sortedResult = descending
                    ? wordCounts.OrderByDescending(kv => kv.Key, StringComparer.Ordinal)
                    : wordCounts.OrderBy(kv => kv.Key, StringComparer.Ordinal);
            }

            ListViewItem[] result = sortedResult
                .Select(item => new ListViewItem(new[] { item.Key, item.Value.ToString() }))
                .ToArray();

            return (result, ComputeFNV1aHash(result));
        }

        /// <summary>
        /// Nutzt Hardware ressourcen Vollständig aus, ohne die UI zu blockieren. Somit geht das Parsen viel
        /// schneller.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onProgressChanged"></param>
        /// <param name="sortByValue"></param>
        /// <param name="descending"></param>
        /// <param name="numThreads"></param>
        /// <returns></returns>
        public (ListViewItem[], uint) parseForWordPairsChunked(
            string text,
            Action<int> onProgressChanged,
            bool sortByValue = true,
            bool descending = true,
            int numThreads = 4
        )
        {
            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int totalWords = words.Length;

            if (totalWords == 0) return (Array.Empty<ListViewItem>(),0);

            // Dynamische Anpassung: Falls es mehr Threads als Wörter gibt, reduzieren
            numThreads = Math.Min(numThreads, totalWords);

            var partitions = SplitTextIntoChunks(words, numThreads);
            var wordCounts = new ConcurrentDictionary<string, int>();
            long processed = 0;
            int remainingThreads = numThreads;

            // Parallel Verarbeitung
            Parallel.ForEach(partitions, partition =>
            {
                var localCounts = new Dictionary<string, int>();

                foreach (var word in partition)
                {
                    if (localCounts.ContainsKey(word))
                        localCounts[word]++;
                    else
                        localCounts[word] = 1;

                    Interlocked.Increment(ref processed);
                }

                // Ergebnisse sicher zusammenführen
                foreach (var kv in localCounts)
                {
                    wordCounts.AddOrUpdate(kv.Key, kv.Value, (_, old) => old + kv.Value);
                }

                // Der letzte Thread meldet den Fortschritt als 100%
                if (Interlocked.Decrement(ref remainingThreads) == 0)
                {
                    onProgressChanged?.Invoke(100);
                }
            });

            lastResultsParsedDictonary = new Dictionary<string, int>(wordCounts);

            return sortAndConvertData(lastResultsParsedDictonary, sortByValue, descending);
        }

        /// <summary>
        /// Teilt die Wörter in numChunks Teile auf, ohne dass Wörter abgeschnitten werden.
        /// </summary>
        private static List<string[]> SplitTextIntoChunks(string[] words, int numChunks)
        {
            int totalWords = words.Length;
            int approxChunkSize = totalWords / numChunks;
            var partitions = new List<string[]>();
            int startIndex = 0;

            for (int i = 0; i < numChunks; i++)
            {
                int endIndex = (i == numChunks - 1) ? totalWords : startIndex + approxChunkSize;
                partitions.Add(words[startIndex..endIndex]);
                startIndex = endIndex;
            }

            return partitions;
        }

        /// <summary>
        /// FNV-1a Hash (32 Bit) für maximale Geschwindigkeit.
        /// </summary>
        private uint ComputeFNV1aHash(ListViewItem[] items)
        {
            const uint FNV_OFFSET_BASIS = 2166136261;
            const uint FNV_PRIME = 16777619;

            uint hash = FNV_OFFSET_BASIS;

            foreach (var item in items)
            {
                foreach (char c in item.Text)
                {
                    hash ^= c;
                    hash *= FNV_PRIME;
                }
                foreach (char c in item.SubItems[1].Text)
                {
                    hash ^= c;
                    hash *= FNV_PRIME;
                }
            }

            return hash;
        }
    }
}
