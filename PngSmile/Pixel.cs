namespace PngSmile;

public record struct PixelRGB(byte R, byte G, byte B);
public record struct PixelRGBA(byte R, byte G, byte B, byte A);

public record struct PixelRGB16(ushort R, ushort G, ushort B);
public record struct PixelRGBA16(ushort R, ushort G, ushort B, ushort A);

public record struct PixelGray1(byte val);
public record struct PixelGray2(byte val);
public record struct PixelGray4(byte val);
public record struct PixelGray8(byte val);
public record struct PixelGray16(ushort val);