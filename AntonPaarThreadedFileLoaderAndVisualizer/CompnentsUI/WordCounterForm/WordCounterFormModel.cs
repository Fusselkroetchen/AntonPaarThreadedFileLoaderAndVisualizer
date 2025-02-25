using AntonPaarThreadedFileLoaderAndVisualizer.GenericComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AntonPaarThreadedFileLoaderAndVisualizer.CompnentsUI.WordCounterForm
{
    //INTERFACE
    interface WordCounterFormModelFactory
    {
        static abstract IWordCounterFormModel create();
    }

    interface IWordCounterFormModel
    {
        public delegate void DelegateOnViewStateChanged(WordCounterFormModelState state);
        public event DelegateOnViewStateChanged? onViewStateChanged;

        public void loadFile(string filePath);
        public void cancelLoading();
    }

    //CLASS
    class WordCounterFormModel : WordCounterFormModelFactory, IWordCounterFormModel
    {
        //DPIS
        private IFileLoaderThreadManager fileLoaderThreadManager;

        //FACTORY
        public static IWordCounterFormModel create()
        {
            return new WordCounterFormModel(
                FileLoaderThreadManager.create()
            );
        }

        //INIT
        private WordCounterFormModel(
            IFileLoaderThreadManager fileLoaderThreadManager
        )
        {
            this.fileLoaderThreadManager = fileLoaderThreadManager;

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
            if (onViewStateChanged != null) onViewStateChanged(uiState);
        }

        //FUNC - Command Pattern
        public void loadFile(string filePath)
        {
            uiState.loadAndParseButtonCaption = "Lade- und Parsevorgang abbrechen";
            uiState.isLoading = true;
            applyState();

            fileLoaderThreadManager.loadFileContentThreaded(
                filePath, 
                (result) => {
                    uiState.isLoading = false;
                    uiState.loadAndParseButtonCaption = "Datei laden und parsen";
                    applyState();
                }, 
                (progress) => {
                    if (uiState.isLoading == false) return;
                    uiState.progress = progress;
                    applyState();
                }
            );
        }

        public void cancelLoading()
        {
            fileLoaderThreadManager.cancelLoadFileContentThreaded();
            uiState.isLoading = false;
            uiState.progress = 0;
            uiState.loadAndParseButtonCaption = "Datei laden und parsen";
            applyState();
        }
    }
}
