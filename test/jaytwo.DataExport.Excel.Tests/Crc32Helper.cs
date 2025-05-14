using System;
using System.IO;
using Force.Crc32;

namespace jaytwo.DataExport.Excel.Tests;

public static class Crc32Helper
{
    public static uint ComputeCrc(byte[] data)
    {
        var crc32 = new Crc32Algorithm();
        crc32.TransformBlock(data, 0, data.Length, null, 0);
        crc32.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return BitConverter.ToUInt32(crc32.Hash!, 0);
    }

    public static uint ComputeCrc(Stream input)
    {
        // Reset position if possible
        if (input.CanSeek)
        {
            input.Position = 0;
        }

        var crc32 = new Crc32Algorithm(); // implements HashAlgorithm

        byte[] buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            crc32.TransformBlock(buffer, 0, bytesRead, null, 0);
        }

        crc32.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        return BitConverter.ToUInt32(crc32.Hash!, 0);
    }
}
