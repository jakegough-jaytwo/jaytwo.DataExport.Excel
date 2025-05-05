using System;
using System.IO;
using System.IO.Compression;

namespace jaytwo.StreamingExcelExport;

public static class ZipHelper
{
    public static void CreateZipFromFolder(string sourceFolderPath, string destinationZipFilePath)
    {
        if (!Directory.Exists(sourceFolderPath))
        {
            throw new DirectoryNotFoundException($"Source folder not found: {sourceFolderPath}");
        }

        // If the destination zip file already exists, delete it
        if (File.Exists(destinationZipFilePath))
        {
            File.Delete(destinationZipFilePath);
        }

        ZipFile.CreateFromDirectory(sourceFolderPath, destinationZipFilePath, CompressionLevel.Optimal, includeBaseDirectory: false);
    }
}
