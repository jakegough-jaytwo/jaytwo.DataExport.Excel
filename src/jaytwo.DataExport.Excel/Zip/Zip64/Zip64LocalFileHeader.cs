using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using jaytwo.DataExport.Excel.Zip.Zip32;

namespace jaytwo.DataExport.Excel.Zip.Zip64;

internal class Zip64LocalFileHeader : Zip32LocalFileHeader, IZipPart
{
    public const ushort Zip64ExtraFieldHeaderId = 0x0001;
    private const int Zip64ExtraFieldTotalLength = 20; // 2 + 2 + 8 + 8

    public bool HasValidZip64ExtraField => ParseZip64ExtraField() != null;

    public ushort? ParsedZip64HeaderId => ParseZip64ExtraField()?.HeaderId;

    public ushort? ParsedZip64DataLength => ParseZip64ExtraField()?.DataLength;

    public ulong? Zip64UncompressedSize
    {
        get => ParseZip64ExtraField()?.UncompressedSize;
        set => UpdateZip64ExtraField(uncompressedSize: value);
    }

    public ulong? Zip64CompressedSize
    {
        get => ParseZip64ExtraField()?.CompressedSize;
        set => UpdateZip64ExtraField(compressedSize: value);
    }

    public override string ToString()
        => $"LFH64[\"{FileName}\", Size={Zip64CompressedSize}, CRC={Crc32}]";

    internal static bool TryParse(byte[] bytes, out Zip64LocalFileHeader result, int offset = 0)
    {
        result = new Zip64LocalFileHeader();
        return TryLoad(result, bytes, offset);
    }

    internal static new Zip64LocalFileHeader Parse(byte[] bytes, int offset = 0)
    {
        var result = new Zip64LocalFileHeader();
        Load(result, bytes, offset);
        return result;
    }

    private Zip64Extra? ParseZip64ExtraField()
        => Zip64Extra.TryParse(ExtraField, out var result) ? result : null;

    private void UpdateZip64ExtraField(ulong? uncompressedSize = null, ulong? compressedSize = null)
    {
        var extra = ParseZip64ExtraField() ?? new Zip64Extra(Zip64ExtraFieldHeaderId, 16);
        extra.UncompressedSize = uncompressedSize ?? extra.UncompressedSize;
        extra.CompressedSize = compressedSize ?? extra.CompressedSize;

        ExtraField = extra.ToByteArray();
        ExtraFieldLength = (ushort)ExtraField.Length;
    }

    private record struct Zip64Extra
    {
        public Zip64Extra(
            ushort headerId,
            ushort dataLength,
            ulong uncompressedSize = 0,
            ulong compressedSize = 0)
        {
            HeaderId = headerId;
            DataLength = dataLength;
            UncompressedSize = uncompressedSize;
            CompressedSize = compressedSize;
        }

        public ushort HeaderId { get; set; }

        public ushort DataLength { get; set; }

        public ulong UncompressedSize { get; set; }

        public ulong CompressedSize { get; set; }

        public static bool TryParse(byte[]? bytes, out Zip64Extra zip64Extra)
        {
            try
            {
                zip64Extra = Parse(bytes);
                return true;
            }
            catch
            {
                zip64Extra = default;
                return false;
            }
        }

        public static Zip64Extra Parse(byte[]? bytes)
        {
            if (bytes == null || bytes.Length < Zip64ExtraFieldTotalLength)
            {
                throw new InvalidOperationException("Invalid Zip64 extra field");
            }

            return new Zip64Extra
            {
                HeaderId = BitConverter.ToUInt16(bytes, 0),
                DataLength = BitConverter.ToUInt16(bytes, 2),
                UncompressedSize = BitConverter.ToUInt64(bytes, 4),
                CompressedSize = BitConverter.ToUInt64(bytes, 12),
            };
        }

        public byte[] ToByteArray()
        {
            var buffer = new byte[20];
            Buffer.BlockCopy(BitConverter.GetBytes(HeaderId), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(DataLength), 0, buffer, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(UncompressedSize), 0, buffer, 4, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(CompressedSize), 0, buffer, 12, 8);
            return buffer;
        }
    }
}
