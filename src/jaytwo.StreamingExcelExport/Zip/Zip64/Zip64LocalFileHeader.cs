using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using jaytwo.StreamingExcelExport.Zip.Zip32;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

internal class Zip64LocalFileHeader : Zip32LocalFileHeader, IZipPart
{
    public const ushort Zip64ExtraFieldHeaderId = 0x0001;
    public const uint SeeZip64ExtraFields = 0xFFFFFFFF;
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

    public static new Zip64LocalFileHeader CreateDefault(ushort compressionMethod, string fileName)
    {
        var result = new Zip64LocalFileHeader
        {
            Signature = KnownSignature,
            VersionNeededToExtract = ZipConstants.Versions.Version45,
            GeneralPurposeBitFlag = ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows,
            CompressionMethod = compressionMethod,
            LastModTime = 0,
            LastModDate = 0,
            Crc32 = 0,
            CompressedSize = SeeZip64ExtraFields,
            UncompressedSize = SeeZip64ExtraFields,
            FileName = fileName,
            FileNameLength = (ushort)Encoding.UTF8.GetByteCount(fileName ?? string.Empty),
            Zip64CompressedSize = 0,
            Zip64UncompressedSize = 0,
        };

        result.ExtraField = result.BuildZip64ExtraField();
        result.ExtraFieldLength = (ushort)result.ExtraField.Length;

        return result;
    }

    public static new Zip64LocalFileHeader Parse(byte[] bytes)
    {
        var result = new Zip64LocalFileHeader();
        Load(result, bytes);
        return result;
    }

    public override string ToString()
        => $"LFH64[\"{FileName}\", Size={Zip64CompressedSize}, CRC={Crc32}]";

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

    private byte[] BuildZip64ExtraField()
    {
        var extra = new Zip64Extra
        {
            HeaderId = Zip64ExtraFieldHeaderId,
            DataLength = 16,
            UncompressedSize = Zip64UncompressedSize ?? 0,
            CompressedSize = Zip64CompressedSize ?? 0,
        };

        return extra.ToByteArray();
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
