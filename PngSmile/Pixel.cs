namespace PngSmile;

public record struct PixelRGB(byte R, byte G, byte B) : IPixel
{
    public bool IsGrayScale => false;
    public PixelRGBA16 ToRGBA16() => new PixelRGBA16((ushort)(R << 8), (ushort)(G
        << 8), (ushort)(B << 8), 0xFFFF);
}
public record struct PixelRGBA(byte R, byte G, byte B, byte A) : IPixel
{
    public bool IsGrayScale => false;
    public PixelRGBA16 ToRGBA16() => new PixelRGBA16((ushort)(R << 8), (ushort)(G
        << 8), (ushort)(B << 8), (ushort)(A << 8));
}

public record struct PixelRGB16(ushort R, ushort G, ushort B) : IPixel
{
    public bool IsGrayScale => false;
    public PixelRGBA16 ToRGBA16() => new PixelRGBA16(R, G, B, 0xFFFF);
}
public record struct PixelRGBA16(ushort R, ushort G, ushort B, ushort A) : IPixel
{
    public bool IsGrayScale => false;
    public PixelRGBA16 ToRGBA16() => this;
    public PixelRGB PixelRGB => new PixelRGB((byte)(R >> 8), (byte)(G >> 8), (byte)(B >> 8));
}

public record struct PixelGray1(byte val) : IPixel
{
    public bool IsGrayScale => true;
    public PixelRGBA16 ToRGBA16() => new PixelRGBA16((ushort)(val << 15), (ushort)(val << 15), (ushort)(val << 15), 0xFFFF);
}
public record struct PixelGray2(byte val) : IPixel
{
    public bool IsGrayScale => true;
    public PixelRGBA16 ToRGBA16() => new PixelRGBA16((ushort)(val << 14), (ushort)(val << 14), (ushort)(val << 14), 0xFFFF);
}
public record struct PixelGray4(byte val) : IPixel
{
    public bool IsGrayScale => true;
    public PixelRGBA16 ToRGBA16() => new PixelRGBA16((ushort)(val << 12), (ushort)(val << 12), (ushort)(val << 12), 0xFFFF);
}
public record struct PixelGray8(byte val) : IPixel
{
    public bool IsGrayScale => true;
    public PixelRGBA16 ToRGBA16() => new PixelRGBA16((ushort)(val << 8), (ushort)(val << 8), (ushort)(val << 8), 0xFFFF);
}
public record struct PixelGray16(ushort val) : IPixel
{
    public bool IsGrayScale => true;
    public PixelRGBA16 ToRGBA16() => new PixelRGBA16(val, val, val, 0xFFFF);
}
