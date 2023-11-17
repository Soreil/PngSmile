namespace PngSmile;

/*Color Allowed    Interpretation
Type    Bit Depths
   
   0       1,2,4,8,16  Each pixel is a grayscale sample.
   
   2       8,16        Each pixel is an R, G, B triple.
   
   3       1,2,4,8     Each pixel is a palette index;
                       a PLTE chunk must appear.
   
   4       8,16        Each pixel is a grayscale sample,
                       followed by an alpha sample.
   
   6       8,16        Each pixel is an R, G, B triple,
                      followed by an alpha sample.
*/
public enum PixelFormat
{
    GrayScale1bit,
    GrayScale2bit,
    GrayScale4bit,
    GrayScale8bit,
    GrayScale16bit,

    RGB8,
    RGB16,

    Palette1,
    Palette2,
    Palette4,
    Palette8,

    GrayScaleA8,
    GrayScaleA16,

    RGBA8,
    RGBA16,
}
