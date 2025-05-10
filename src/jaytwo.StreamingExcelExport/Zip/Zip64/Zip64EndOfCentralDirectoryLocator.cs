using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

internal class Zip64EndOfCentralDirectoryLocator : IZipPart
{
    public const uint KnownSignature = 0x07064b50;

    public uint? Signature { get; set; }

    public uint? CentralDirectoryStartDisk { get; set; }

    public uint? TotalDisks { get; set; }

    public ulong? Zip64EndOfCentralDirectoryOffset { get; set; } // Where the Zip64 EOCD starts

    public static Zip64EndOfCentralDirectoryLocator CreateDefault(ulong zip64EndOfCentralDirectoryOffset)
        => new Zip64EndOfCentralDirectoryLocator
        {
            Signature = KnownSignature,
            CentralDirectoryStartDisk = 0,
            TotalDisks = 1,
            Zip64EndOfCentralDirectoryOffset = zip64EndOfCentralDirectoryOffset,
        };

    public static Zip64EndOfCentralDirectoryLocator Parse(byte[] bytes)
    {
        // not validating byte length here, it's still useful to parse as much as we can for debugging

        var result = new Zip64EndOfCentralDirectoryLocator();
        result.Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset);
        result.CentralDirectoryStartDisk = BitConverter.ToUInt32(bytes, Offsets.CentralDirectoryStartDisk);
        result.Zip64EndOfCentralDirectoryOffset = BitConverter.ToUInt64(bytes, Offsets.Zip64EndOfCentralDirectoryOffset);
        result.TotalDisks = BitConverter.ToUInt32(bytes, Offsets.TotalDisks);

        return result;
    }

    public void WriteTo(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(Signature ?? throw new InvalidOperationException($"{nameof(Signature)} is required."));
        writer.Write(CentralDirectoryStartDisk ?? throw new InvalidOperationException($"{nameof(CentralDirectoryStartDisk)} is required."));
        writer.Write(Zip64EndOfCentralDirectoryOffset ?? throw new InvalidOperationException($"{nameof(Zip64EndOfCentralDirectoryOffset)} is required."));
        writer.Write(TotalDisks ?? throw new InvalidOperationException($"{nameof(TotalDisks)} is required."));
    }

    public override string ToString() =>
        $"EOCDL64[Offset={Zip64EndOfCentralDirectoryOffset}]";

    public static class Offsets
    {
        public const int SignatureOffset = 0;
        public const int CentralDirectoryStartDisk = SignatureOffset + 4;
        public const int Zip64EndOfCentralDirectoryOffset = CentralDirectoryStartDisk + 4;
        public const int TotalDisks = Zip64EndOfCentralDirectoryOffset + 8;
    }
}
