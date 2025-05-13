using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

internal class Zip64EndOfCentralDirectoryLocator : IZipPart
{
    public const uint KnownSignature = 0x07064b50;

    public uint? Signature { get; set; }

    public uint? CentralDirectoryStartDisk { get; set; }

    public uint? TotalDisks { get; set; }

    public ulong? Zip64EndOfCentralDirectoryOffset { get; set; } // Where the Zip64 EOCD starts

    public void WriteTo(Stream stream, bool validate = true)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(ThrowIfNull(x => x.Signature) ?? default);
        writer.Write(ThrowIfNull(x => x.CentralDirectoryStartDisk) ?? default);
        writer.Write(ThrowIfNull(x => x.Zip64EndOfCentralDirectoryOffset) ?? default);
        writer.Write(ThrowIfNull(x => x.TotalDisks) ?? default);

        TValue ThrowIfNull<TValue>(Expression<Func<Zip64EndOfCentralDirectoryLocator, TValue>> propertyExpression)
            => ValidationHelper.EnsureNotNull(this, propertyExpression, validate);
    }

    public override string ToString() =>
        $"EOCDL64[Offset={Zip64EndOfCentralDirectoryOffset}]";

    internal static bool TryParse(byte[] bytes, out Zip64EndOfCentralDirectoryLocator result, int offset = 0)
    {
        result = new Zip64EndOfCentralDirectoryLocator();
        return TryLoad(result, bytes, offset);
    }

    internal static Zip64EndOfCentralDirectoryLocator Parse(byte[] bytes, int offset = 0)
    {
        var result = new Zip64EndOfCentralDirectoryLocator();
        Load(result, bytes, offset);
        return result;
    }

    protected static bool TryLoad(Zip64EndOfCentralDirectoryLocator result, byte[] bytes, int offset = 0)
    {
        try
        {
            Load(result, bytes, offset);
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected static void Load(Zip64EndOfCentralDirectoryLocator result, byte[] bytes, int offset = 0)
    {
        // not validating byte length here, it's still useful to parse as much as we can for debugging

        result.Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset);
        result.CentralDirectoryStartDisk = BitConverter.ToUInt32(bytes, Offsets.CentralDirectoryStartDisk);
        result.Zip64EndOfCentralDirectoryOffset = BitConverter.ToUInt64(bytes, Offsets.Zip64EndOfCentralDirectoryOffset);
        result.TotalDisks = BitConverter.ToUInt32(bytes, Offsets.TotalDisks);
    }

    public static class Offsets
    {
        public const int SignatureOffset = 0;
        public const int CentralDirectoryStartDisk = SignatureOffset + 4;
        public const int Zip64EndOfCentralDirectoryOffset = CentralDirectoryStartDisk + 4;
        public const int TotalDisks = Zip64EndOfCentralDirectoryOffset + 8;
    }
}
