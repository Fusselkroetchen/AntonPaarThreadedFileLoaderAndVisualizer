using AntonPaarThreadedFileLoaderAndVisualizer.CompnentsUI.WordCounterForm;
using System.Reflection;
using System.IO;
using AntonPaarThreadedFileLoaderAndVisualizer.GenericComponents;

namespace AntonPaarThreadedFileLoaderAndVisualizer
{
    public partial class WordCounterForm : Form
    {
        private IWordCounterFormModel wordCounterFormModel = WordCounterFormModel.create();
        private bool isLoading = false;

        public WordCounterForm()
        {
            InitializeComponent();

            //init MVVM
            wordCounterFormModel.onViewStateChanged += WordCounterFormModel_onViewStateChanged;
        }

        private void WordCounterFormModel_onViewStateChanged(WordCounterFormModelState state)
        {
            progressBar1.Value = state.progress;
            button1.Text = state.loadAndParseButtonCaption;
            isLoading = state.isLoading;
        }

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
                Title = "Wähle eine Textdatei aus",
                InitialDirectory = exePath
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                wordCounterFormModel.loadFile(filePath);
            }
        }
    }
}
