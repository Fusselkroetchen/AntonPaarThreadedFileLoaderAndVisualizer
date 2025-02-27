using AntonPaarThreadedFileLoaderAndVisualizer.CompnentsUI.WordCounterForm;
using System.Reflection;
using System.IO;
using AntonPaarThreadedFileLoaderAndVisualizer.GenericComponents;
using AntonPaarThreadedFileLoaderAndVisualizer.Ressources;

namespace AntonPaarThreadedFileLoaderAndVisualizer
{
    public partial class WordCounterForm : Form
    {
        private IWordCounterFormModel wordCounterFormModel = WordCounterFormModel.create();
        private bool isLoading = false;
        private uint listViewHash;

        public WordCounterForm()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            //init MVVM
            wordCounterFormModel.onViewStateChanged += WordCounterFormModel_onViewStateChanged;
        }

        ~WordCounterForm ()
        {
            wordCounterFormModel.onViewStateChanged -= WordCounterFormModel_onViewStateChanged;
        }

        /// <summary>
        /// MVVM UI State change
        /// </summary>
        /// <param name="state"></param>
        private void WordCounterFormModel_onViewStateChanged(WordCounterFormModelState state)
        {
            progressBar1.Value = state.progress;
            button1.Text = state.loadAndParseButtonCaption;
            isLoading = state.isLoading;

            if (state.listViewList != null && listViewHash != state.listViewHash)
            {
                listView1.Items.Clear();
                listView1.Items.AddRange(state.listViewList);
                listViewHash = state.listViewHash;
            }
        }

        /// <summary>
        /// File open dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (isLoading)
            {
                wordCounterFormModel.cancelLoading();
                return;
            }

            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = Translations.SelectFile,
                InitialDirectory = exePath
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                wordCounterFormModel.loadFile(filePath);
            }
        }

        /// <summary>
        /// Sortierungsänderung durch Klick auf Header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {

            switch (e.Column)
            {
                case 0:
                    wordCounterFormModel.toggleWordSort();
                    break;
                case 1:
                    wordCounterFormModel.toggleCountSort();
                    break;
            }

        }
    }
}
