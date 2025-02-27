using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AntonPaarThreadedFileLoaderAndVisualizer.CompnentsUI.WordCounterForm
{
    struct WordCounterFormModelState
    {
        public int progress;
        public bool isLoading;
        public string loadAndParseButtonCaption;
        public ListViewItem[]? listViewList;
        public uint listViewHash;
    }
}
