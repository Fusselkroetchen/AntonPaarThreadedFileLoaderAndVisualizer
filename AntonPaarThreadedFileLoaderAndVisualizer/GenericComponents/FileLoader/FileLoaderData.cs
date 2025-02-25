using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntonPaarThreadedFileLoaderAndVisualizer.GenericComponents
{
    public enum FileLoadProcessResultStatus : int
    {
        [Description("Success")]
        success = 0,
        [Description("File not found")]
        fileNotFound = 1,
        [Description("Permission denied")]
        permissionDenied = 3,
        [Description("canceled")]
        canceled = 4
    };

    public struct FileLoaderResult
    {
        public FileLoadProcessResultStatus fileLoadProcessResultStatus;
        public string? fileContent;
    }
}
