using AntonPaarThreadedFileLoaderAndVisualizer.Components;
using AntonPaarThreadedFileLoaderAndVisualizer.Components.WordCountParserThreadManager;
using AntonPaarThreadedFileLoaderAndVisualizer.GenericComponents;
using AntonPaarThreadedFileLoaderAndVisualizer.Ressources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AntonPaarThreadedFileLoaderAndVisualizer.CompnentsUI.WordCounterForm
{
    //INTERFACE
    interface IWordCounterFormModelFactory
    {
        static abstract IWordCounterFormModel create();
    }

    interface IWordCounterFormModel
    {
        public delegate void DelegateOnViewStateChanged(WordCounterFormModelState state);
        public event DelegateOnViewStateChanged? onViewStateChanged;

        public void loadFile(string filePath);
        public void cancelLoading();
        public void toggleWordSort();
        public void toggleCountSort();
    }

    //CLASS
    /// <summary>
    /// Die ist das View-Model für die WPS-Form nach MVVM/Command-Pattern
    /// </summary>
    class WordCounterFormModel : IWordCounterFormModelFactory, IWordCounterFormModel
    {
        //DPIS
        private readonly IFileLoaderThreadManager fileLoaderThreadManager;
        private readonly IWordCountParserThreadManager wordCountParserThreadManager;

        //FACTORY
        public static IWordCounterFormModel create()
        {
            return new WordCounterFormModel(
                FileLoaderThreadManager.create(),
                WordCountParserThreadManager.create()
            );
        }

        //INIT
        private WordCounterFormModel(
            IFileLoaderThreadManager fileLoaderThreadManager,
            IWordCountParserThreadManager wordCountParserThreadManager
        )
        {
            this.fileLoaderThreadManager = fileLoaderThreadManager;
            this.wordCountParserThreadManager = wordCountParserThreadManager;

            uiState = new WordCounterFormModelState
            {
                progress = 0,
                loadAndParseButtonCaption = "",
                isLoading = false
            };

            applyState();
        }

        //FUNC - MVVM Pattern
        public event IWordCounterFormModel.DelegateOnViewStateChanged? onViewStateChanged;
        private WordCounterFormModelState uiState;

        private void applyState()
        {
            onViewStateChanged?.Invoke(uiState);
        }

        //FUNC - Command Pattern
        public void loadFile(string filePath)
        {
            uiState.listViewList = null;
            uiState.loadAndParseButtonCaption = Translations.cancelLoadParse;
            uiState.isLoading = true;
            applyState();

            fileLoaderThreadManager.loadFileContentThreaded(
                filePath, 
                (result) => {
                    processFileLoaderResults(result);
                }, 
                (progress) => {
                    if (uiState.isLoading == false) return;
                    uiState.progress = progress;
                    applyState();
                }
            );
        }

        private void processFileLoaderResults(FileLoaderResult result)
        {
            switch (result.fileLoadProcessResultStatus)
            {
                case FileLoadProcessResultStatus.success:
                    parseFile(result.fileContent!);
                    break;

                case FileLoadProcessResultStatus.fileNotFound:
                case FileLoadProcessResultStatus.permissionDenied:
                case FileLoadProcessResultStatus.canceled:
                    resetLoadingState();
                    break;
            }
        }

        private void parseFile(string text)
        {
            wordCountParserThreadManager.parseForWordPairsChunked(
                text,
                (result) =>
                {
                    if (result != null) { 
                        uiState.listViewList = result.Value.data;
                        uiState.listViewHash = result.Value.hash;
                    }
                    resetLoadingState();
                },
                (progress) =>
                {
                    if (uiState.isLoading == false) return;
                    uiState.progress = progress;
                    applyState();
                }, 
                true, 
                true
            );
        }

        private void resetLoadingState()
        {
            wordCountParserThreadManager.cancelParseForWordPairs();
            lastCountDescendSort = false;
            lastWordDescendSort = false;
            uiState.isLoading = false;
            uiState.progress = 0;
            uiState.loadAndParseButtonCaption = Translations.loadFileAndParse;
            applyState();
        }

        public void cancelLoading()
        {
            uiState.loadAndParseButtonCaption = Translations.loadFileAndParse;
            fileLoaderThreadManager.cancelLoadFileContentThreaded();
            resetLoadingState();
        }

        private bool lastWordDescendSort = false;

        public void toggleWordSort()
        {
            wordCountParserThreadManager.sortFromLastParsedData((result) =>
            {
                if (result != null) { 
                    uiState.listViewList = result.Value.data;
                    uiState.listViewHash = result.Value.hash;
                }
                applyState();

                lastWordDescendSort = !lastWordDescendSort;
            }, false, lastWordDescendSort);
        }

        private bool lastCountDescendSort = false;
        public void toggleCountSort()
        {
            wordCountParserThreadManager.sortFromLastParsedData((result) =>
            {
                if (result != null)
                {
                    uiState.listViewList = result.Value.data;
                    uiState.listViewHash = result.Value.hash;
                }
                applyState();

                lastCountDescendSort = !lastCountDescendSort;
            }, true, lastCountDescendSort);
        }
    }
}
