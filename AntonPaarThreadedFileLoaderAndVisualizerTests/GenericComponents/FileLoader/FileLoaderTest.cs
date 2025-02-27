namespace AntonPaarThreadedFileLoaderAndVisualizerTests;

using AntonPaarThreadedFileLoaderAndVisualizer.GenericComponents;
using System;
using System.IO;

[TestClass]
public class FileLoaderTest
{
    [TestMethod]
    public void TestFileNotExists()
    {

        IFileLoader fileLoader = FileLoader.create();
        FileLoaderResult result = fileLoader.loadFileContent("C:\\NOTEXISTINGFILE.TXT", null);

        Assert.IsTrue(
            result.fileLoadProcessResultStatus == FileLoadProcessResultStatus.fileNotFound,
            "Die Datei existiert nicht"
        );
    }

    [TestMethod]
    public void TestLoadFile()
    {
         string fileName = "FileLoaderTestFile.txt";
         string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

        // Überprüfe, ob die Test-Datei existiert
        Assert.IsTrue(File.Exists(filePath), $"Die Datei {fileName} existiert nicht.");

        IFileLoader fileLoader = FileLoader.create();
        double globalProgress = 0;
        FileLoaderResult result = fileLoader.loadFileContent(filePath, (progress) =>
        {
            globalProgress = progress;
        });

        Assert.IsTrue(globalProgress == 100);
        Assert.IsTrue(result.fileLoadProcessResultStatus == FileLoadProcessResultStatus.success);
        Assert.IsTrue(result.fileContent == "test \r\n");
    }
}
