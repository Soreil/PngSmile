using System.Buffers.Binary;

namespace PngSmile;

public class IHDR
{
    public uint ByteWidth => PixelFormat switch
    {
        PixelFormat.GrayScale1bit => Width / 8,
        PixelFormat.GrayScale2bit => Width / 4,
        PixelFormat.GrayScale4bit => Width / 2,
        PixelFormat.GrayScale8bit => Width,
        PixelFormat.GrayScale16bit => Width * 2,
        PixelFormat.RGB8 => Width * 3,
        PixelFormat.RGBA8 => Width * 4,
        PixelFormat.RGB16 => Width * 6,
        PixelFormat.RGBA16 => Width * 8,
    };

    public int BytesPerPixel => PixelFormat switch
    {
        PixelFormat.GrayScale16bit => 2,
        PixelFormat.RGB8 => 3,
        PixelFormat.RGB16 => 6,
        PixelFormat.RGBA8 => 4,
        PixelFormat.RGBA16 => 8,
        PixelFormat.GrayScaleA16 => 4,
        PixelFormat.GrayScaleA8 => 2,
        _ => 1
    };

    public readonly uint Width;
    public readonly uint Height;

    private readonly byte BitDepth;
    private readonly byte ColorType;
    private readonly byte CompressionMethod;
    private readonly byte FilterMethod;
    private readonly byte InterlaceMethod;

    public readonly PixelFormat PixelFormat;
    public readonly bool useInterlace;

    public IHDR(Chunk chunk)
    {
        if (chunk.ChunkTypeString() != "IHDR") throw new InvalidDataException("Only IHDR allowed");

        /* Width:              4 bytes
           Height:             4 bytes
           Bit depth:          1 byte
           Color type:         1 byte
           Compression method: 1 byte
           Filter method:      1 byte
           Interlace method:   1 byte
        */

        ArgumentOutOfRangeException.ThrowIfNotEqual((int)chunk.Length, 13, nameof(chunk.Length));

        var buf = chunk.ChunkData.AsSpan();

        Width = BinaryPrimitives.ReadUInt32BigEndian(buf);
        Height = BinaryPrimitives.ReadUInt32BigEndian(buf[4..]);

        ArgumentOutOfRangeException.ThrowIfZero(Width, nameof(Width));
        ArgumentOutOfRangeException.ThrowIfZero(Height, nameof(Height));

        BitDepth = buf[8];
        if (byte.PopCount(BitDepth) != 1) throw new Exception("Invalid BitDepth");

        ColorType = buf[9];

        CompressionMethod = buf[10];
        if (CompressionMethod != 0) throw new Exception("Unknown compression method");
        FilterMethod = buf[11];
        if (FilterMethod != 0) throw new Exception("Unknown filter method");
        InterlaceMethod = buf[12];
        if (InterlaceMethod > 1) throw new Exception("Invalid interlace method");
        useInterlace = InterlaceMethod == 1;

        PixelFormat = (ColorType, BitDepth) switch
        {
            (0, 1) => PixelFormat.GrayScale1bit,
            (0, 2) => PixelFormat.GrayScale2bit,
            (0, 4) => PixelFormat.GrayScale4bit,
            (0, 8) => PixelFormat.GrayScale8bit,
            (0, 16) => PixelFormat.GrayScale16bit,

            (2, 8) => PixelFormat.RGB8,
            (2, 16) => PixelFormat.RGB16,

            (3, 1) => PixelFormat.Palette1,
            (3, 2) => PixelFormat.Palette2,
            (3, 4) => PixelFormat.Palette4,
            (3, 8) => PixelFormat.Palette8,

            (4, 8) => PixelFormat.GrayScaleA8,
            (4, 16) => PixelFormat.GrayScaleA16,

            (6, 8) => PixelFormat.RGBA8,
            (6, 16) => PixelFormat.RGBA16,

            _ => throw new InvalidDataException("Unknown Pixel Format")
        };
    }
}
