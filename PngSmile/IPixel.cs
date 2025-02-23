namespace PngSmile;

public interface IPixel
{
    public bool IsGrayScale { get; }
    public PixelRGBA16 ToRGBA16();
}