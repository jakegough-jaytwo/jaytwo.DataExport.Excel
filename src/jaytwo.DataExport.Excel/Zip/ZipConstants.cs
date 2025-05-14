namespace jaytwo.DataExport.Excel.Zip;

internal class ZipConstants
{
    public class GeneralPurposeBitFlags
    {
        public const ushort DataDescriptorFollows = 0x08; // bit 3
    }

    public class CompressionMethods
    {
        public const ushort NoCompression = 0;
        public const ushort Deflate = 8;
    }

    public class Versions
    {
        public const ushort Version20 = 20;
        public const ushort Version45 = 45;
    }
}
