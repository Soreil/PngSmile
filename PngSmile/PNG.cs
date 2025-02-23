using System.Buffers.Binary;

namespace PngSmile;

public static class InterlacedIndexGenerator
{
    //We kinda have to count how many pixels we've processed so far as well as the size of the image
    //if we combine these two bits of information we can find the index reliably.
    //Alternatively we could write a generator which yields the indexes in order.
    public static IEnumerable<List<(int y, int x)>> GetInterLacedIndex(int width, int height)
    {
        /*   1 6 4 6 2 6 4 6
             7 7 7 7 7 7 7 7
             5 6 5 6 5 6 5 6
             7 7 7 7 7 7 7 7
             3 6 4 6 3 6 4 6
             7 7 7 7 7 7 7 7
             5 6 5 6 5 6 5 6
             7 7 7 7 7 7 7 7*/

        //1
        for (int j = 0; j < height; j += 8)
        {
            List<(int y, int x)> res1 = [];
            for (int i = 0; i < width; i += 8)
            {
                res1.Add((j, i));
            }
            yield return res1;
        }
        //2
        for (int j = 0; j < height; j += 8)
        {
            List<(int y, int x)> res2 = [];
            for (int i = 4; i < width; i += 8)
            {
                res2.Add((j, i));
            }
            yield return res2;
        }

        //3
        for (int j = 4; j < height; j += 8)
        {
            List<(int y, int x)> res3 = [];
            for (int i = 0; i < width; i += 4)
            {
                res3.Add((j, i));
            }
            yield return res3;
        }
        //4
        for (int j = 0; j < height; j += 4)
        {
            List<(int y, int x)> res4 = [];
            for (int i = 2; i < width; i += 4)
            {
                res4.Add((j, i));
            }
            yield return res4;
        }
        //5
        for (int j = 2; j < height; j += 4)
        {
            List<(int y, int x)> res5 = [];
            for (int i = 0; i < width; i += 2)
            {
                res5.Add((j, i));
            }
            yield return res5;
        }
        //6
        for (int j = 0; j < height; j += 2)
        {
            List<(int y, int x)> res6 = [];
            for (int i = 1; i < width; i += 2)
            {
                res6.Add((j, i));
            }
            yield return res6;
        }
        //7
        for (int j = 1; j < height; j += 2)
        {
            List<(int y, int x)> res7 = [];
            for (int i = 0; i < width; i += 1)
            {
                res7.Add((j, i));
            }
            yield return res7;
        }
    }

}


public class PNG
{
    //Magic string which all PNG files are supposed to contain as the first 8
    public static readonly byte[] FileSignature = [137, 80, 78, 71, 13, 10, 26, 10];
    public const int MinimalChunkSize = 12;

    //DecodeChunk reads a chunk from the provided span. No effort is made to handle spans which do not have a chunk at index 0
    public static int DecodeChunk(ReadOnlySpan<byte> bytes, out Chunk chunk)
    {
        if (bytes.Length < MinimalChunkSize) throw new ArgumentOutOfRangeException(nameof(bytes), bytes.Length, "Can't be a valid chunk, too small");

        var size = BinaryPrimitives.ReadUInt32BigEndian(bytes);
        if (size > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(bytes), size, "PNG files are not allowed to have the top bit set in size fields");

        var type = BinaryPrimitives.ReadUInt32BigEndian(bytes[4..]);
        if (bytes.Length < size + MinimalChunkSize) throw new InvalidDataException("Incorrect size");

        chunk = new Chunk(
            size,
            type,
            bytes.Slice(8, (int)size).ToArray(),
            BinaryPrimitives.ReadUInt32BigEndian(bytes[(8 + (int)size)..]));

        return (int)size + MinimalChunkSize;
    }

    public static bool IsPNG(ReadOnlySpan<byte> data) => data.StartsWith(FileSignature);

    public Span<byte> Filter(ReadOnlySpan<byte> span, ReadOnlySpan<byte> prior, byte filterByte)
    {
        if (filterByte == 0) return new Span<byte>([.. span]);
        else if (filterByte == 1)
        {
            var res = new Span<byte>([.. span]);
            for (int i = ihdr!.BytesPerPixel; i < res.Length; i++)
            {
                res[i] += res[i - ihdr.BytesPerPixel];
            }
            return res;
        }
        else if (filterByte == 2)
        {
            var res = new Span<byte>([.. span]);
            for (int i = ihdr!.BytesPerPixel; i < res.Length; i++)
            {
                res[i] += prior[i];
            }
            return res;
        }
        else if (filterByte == 3)
        {
            //   Average(x) + floor((Raw(x-bpp)+Prior(x))/2)
            var res = new Span<byte>([.. span]);
            for (int i = ihdr!.BytesPerPixel; i < res.Length; i++)
            {
                res[i] += (byte)((res[i - ihdr.BytesPerPixel] + prior[i]) / 2);
            }
            return res;
        }
        else if (filterByte == 4)
        {
            //   Paeth(x) = Raw(x) - PaethPredictor(Raw(x - bpp), Prior(x), Prior(x - bpp))
            var res = new Span<byte>([.. span]);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = (byte)(res[i] + PaethPredictor(
                    i < ihdr!.BytesPerPixel ? (byte)0 : res[i - ihdr.BytesPerPixel],
                    prior[i],
                    i < ihdr.BytesPerPixel ? (byte)0 : prior[i - ihdr.BytesPerPixel])
                    );
            }
            return res;

        }
        else throw new NotImplementedException();
    }

    private static byte PaethPredictor(byte a, byte b, byte c)
    {
        //; a = left, b = above, c = upper left
        //p := a + b - c; initial estimate
        //pa:= abs(p - a); distances to a, b, c
        //pb := abs(p - b)
        //pc:= abs(p - c)
        //; return nearest of a, b, c,
        //; breaking ties in order a, b, c.
        //if pa <= pb AND pa <= pc then return a
        //else if pb <= pc then return b
        //else return c

        var p = a + b - c;
        var pa = Math.Abs(p - a);
        var pb = Math.Abs(p - b);
        var pc = Math.Abs(p - c);

        if (pa < pb && pa < pc) return a;
        else if (pb <= pc) return b;
        else return c;
    }

    IHDR? ihdr = null;
    GAMA? gama = null;
    CHRM? chrm = null;
    List<IDAT> idatChunks = [];

    public Stream ReadImage(ReadOnlySpan<byte> buffer)
    {
        if (!IsPNG(buffer)) throw new Exception();
        var input = buffer[8..];

        List<Chunk> chunks = [];
        while (input.Length != 0)
        {
            var read = DecodeChunk(input, out var chunk);

            chunks.Add(chunk);
            input = input[read..];
        }

        var labels = chunks.Select(c => c.ChunkTypeString()).ToList();
        labels.ForEach(Console.WriteLine);


        foreach (var chunk in chunks)
        {
            switch (chunk.ChunkTypeString())
            {
                case "IHDR":
                    {
                        ihdr = new IHDR(chunk);
                    }
                    break;
                case "gAMA":
                    {
                        gama = new GAMA(chunk);
                    }
                    break;
                case "cHRM":
                    {
                        chrm = new CHRM(chunk);
                    }
                    break;
                case "IDAT":
                    {
                        var idat = new IDAT(chunk, ihdr!);
                        idatChunks.Add(idat);
                    }
                    break;
                case "IEND":
                    {
                    }

                    break;
                default:
                    throw new NotImplementedException($"Unhandled chunk type:{chunk.ChunkTypeString()}");
            }
        }

        foreach (var idat in idatChunks)
        {
            var dat = new Span<byte>(idat.Stream.Decode());

            var fs = new MemoryStream();
            var sw = new StreamWriter(fs);
            sw.Write($"P3\n{ihdr!.Width} {ihdr.Height}\n255\n");

            if (ihdr.useInterlace)
            {
                var indexes = InterlacedIndexGenerator.GetInterLacedIndex((int)ihdr.Width, (int)ihdr.Height);

                PixelRGB[,] pixels = new PixelRGB[ihdr.Height, ihdr.Width];

                Span<byte> previousSpan = [];

                var bytesNeeded = (int n) =>
                {
                    var bits = ihdr.PixelFormat switch
                    {
                        PixelFormat.GrayScale1bit => 1,
                        PixelFormat.GrayScale2bit => 2,
                        PixelFormat.GrayScale4bit => 4,
                        PixelFormat.GrayScale8bit => 8,
                        PixelFormat.GrayScale16bit => 16,
                        PixelFormat.RGB8 => 8 * 3,
                        PixelFormat.RGBA8 => 8 * 4,
                        PixelFormat.RGB16 => 8 * 6,
                        PixelFormat.RGBA16 => 8 * 8,
                        _ => throw new NotImplementedException()
                    };

                    var totalBits = n * bits;
                    return (int)Math.Ceiling(totalBits / 8.0);
                };

                int offset = 0;
                foreach (var list in indexes)
                {
                    //todo: add offset of previous line, this isn't constant
                    var filterByte = dat[offset];
                    var startIndex = offset + 1;
                    var needed = bytesNeeded(list.Count);
                    offset += needed + 1;
                    var span = dat.Slice(startIndex, needed);
                    var filteredSpan = Filter(span, previousSpan, filterByte);
                    previousSpan = filteredSpan;

                    int j = 0;
                    foreach (var (y, x) in list)
                    {
                        var pixel = makePixel(filteredSpan, j, ihdr);
                        var pix = pixel.ToRGBA16().PixelRGB;

                        pixels[y, x] = pix;
                        j++;
                    }
                }

                foreach (var pix in pixels)
                {
                    var res = $"{pix.R} {pix.G} {pix.B}";

                    sw.Write($"{res}\n");
                }
            }
            else
            {
                Span<byte> previousSpan = [];
                for (int row = 0; row < ihdr.Height; row++)
                {
                    //For example 96 bytes for a 32 pixel wide image with
                    //3 bytes per pixel (rgb8)
                    //We need 1 more byte per row for the filterbyte.
                    var filterByte = dat[(int)ihdr.ByteWidth * row + row];

                    var startIndex = (int)ihdr.ByteWidth * row + row + 1;

                    var span = dat.Slice(startIndex, (int)ihdr.ByteWidth);

                    var filteredSpan = Filter(span, previousSpan, filterByte);

                    //TODO:(sjon) Add support for interlaced reading! currently basi0g01.png fails here
                    //it's first byte of pixel data is 0b1111_0000 which means it has 4 bits set and the bottom
                    //4 are unused. This corresponds to the bits in position (0,7,15,23) in the first row.

                    //Line two has pixel data 0b1010_0000 which corresponds to having a black pixel in
                    //the W character of the image as well as a pixel in the gradient triangle
                    for (int j = 0; j < ihdr.Width; j++)
                    {
                        var pixel = makePixel(filteredSpan, j, ihdr);
                        var pix = pixel.ToRGBA16().PixelRGB;
                        //var res = pixel switch
                        //{
                        //    PixelRGB p => $"{p.R} {p.G} {p.B}",
                        //    PixelRGB16 p => $"{p.R >> 8} {p.G >> 8} {p.B >> 8}",
                        //    PixelRGBA p => $"{p.R} {p.G} {p.B}",
                        //    PixelRGBA16 p => $"{p.R >> 8} {p.G >> 8} {p.B >> 8}",
                        //    PixelGray1 p => $"{p.val << 8} {p.val << 8} {p.val << 8}",
                        //    PixelGray2 p => $"{p.val} {p.val} {p.val}",
                        //    PixelGray4 p => $"{p.val} {p.val} {p.val}",
                        //    PixelGray8 p => $"{p.val} {p.val} {p.val}",
                        //    PixelGray16 p => $"{p.val >> 8} {p.val >> 8} {p.val >> 8}",
                        //    _ => throw new NotImplementedException()
                        //};

                        var res = $"{pix.R} {pix.G} {pix.B}";

                        sw.Write($"{res}\n");
                    }

                    previousSpan = filteredSpan;
                }
            }

            sw.Flush();
            return fs;
        }
        throw new Exception();
    }

    private static IPixel makePixel(ReadOnlySpan<byte> bytes, int j, IHDR ihdr)
    {
        return ihdr.PixelFormat switch
        {
            PixelFormat.RGB8 => new PixelRGB(bytes[j * 3], bytes[j * 3 + 1], bytes[j * 3 + 2]),
            PixelFormat.RGB16 => new PixelRGB16(BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 6)..(j * 6 + 2)]),
            BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 6 + 2)..(j * 6 + 4)]),
            BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 6 + 4)..(j * 6 + 6)])),

            PixelFormat.RGBA8 => new PixelRGBA(bytes[j * 4], bytes[j * 4 + 1], bytes[j * 4 + 2], bytes[j * 4 + 3]),
            PixelFormat.RGBA16 => new PixelRGBA16(BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 8)..(j * 8 + 1)]),
            BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 8 + 2)..(j * 8 + 3)]),
            BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 8 + 4)..(j * 8 + 5)]),
            BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 8 + 6)..(j * 8 + 7)])),

            PixelFormat.GrayScale1bit => new PixelGray1((bytes[j / 8] & (1 << (7 - (j % 8)))) == 0 ? (byte)0 : (byte)1),
            PixelFormat.GrayScale2bit => new PixelGray2((byte)((bytes[j / 4] >> ((j % 4) * 2)) & 0x3)),
            PixelFormat.GrayScale4bit => new PixelGray4((byte)((bytes[j / 2] >> ((j % 2) * 4)) & 0xf)),
            PixelFormat.GrayScale8bit => new PixelGray8(bytes[j]),
            PixelFormat.GrayScale16bit => new PixelGray16(BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 2)..(j * 2 + 2)])),

            _ => throw new NotImplementedException()
        };
    }


}