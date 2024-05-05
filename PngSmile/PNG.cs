using CommunityToolkit.HighPerformance;

using System.Buffers.Binary;

namespace PngSmile;

public class PNG
{
    //Magic string which all PNG files are supposed to contain as the first 8
    public static readonly byte[] FileSignature = [137, 80, 78, 71, 13, 10, 26, 10];
    public const int MinimalChunkSize = 12;

    //DecodeChunk reads a chunk from the provided span. No effort is made to handle spans which do not have a chunk at index 0
    public static int DecodeChunk(ReadOnlySpan<byte> bytes, out Chunk chunk)
    {
        if (bytes.Length < MinimalChunkSize) throw new ArgumentOutOfRangeException("Can't be a valid chunk, too small");

        var size = BinaryPrimitives.ReadUInt32BigEndian(bytes);
        if (size > int.MaxValue) throw new ArgumentOutOfRangeException("PNG files are not allowed to have the top bit set in size fields");

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

    public static Span<byte> Filter(ReadOnlySpan<byte> span, ReadOnlySpan<byte> prior, byte filterByte)
    {
        if (filterByte == 0) return new Span<byte>([.. span]);
        else if (filterByte == 1)
        {
            var res = new Span<byte>([.. span]);
            for (int i = 3; i < res.Length; i++)
            {
                res[i] += res[i - 3];
            }
            return res;
        }
        else if (filterByte == 2)
        {
            var res = new Span<byte>([.. span]);
            for (int i = 3; i < res.Length; i++)
            {
                res[i] += prior[i];
            }
            return res;
        }
        else if (filterByte == 3)
        {
            //   Average(x) + floor((Raw(x-bpp)+Prior(x))/2)
            var res = new Span<byte>([.. span]);
            for (int i = 3; i < res.Length; i++)
            {
                res[i] += (byte)((res[i - 3] + prior[i]) / 2);
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
                    i < 3 ? (byte)0 : res[i - 3],
                    prior[i],
                    i < 3 ? (byte)0 : prior[i - 3])
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

        IHDR? ihdr = null;
        GAMA? gama = null;
        CHRM? chrm = null;
        List<IDAT> idatChunks = [];

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
            sw.Write($"P3\n{ihdr.Width} {ihdr.Height}\n255\n");

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

                for (int j = 0; j < ihdr.Width; j++)
                {
                    var pixel = makePixel(filteredSpan, j, ihdr);

                    var res = pixel switch
                    {
                        PixelRGB p => $"{p.R} {p.G} {p.B}",
                        PixelRGB16 p => $"{p.R >> 8} {p.G >> 8} {p.B >> 8}",
                        PixelRGBA p => $"{p.R} {p.G} {p.B}",
                        PixelRGBA16 p => $"{p.R >> 8} {p.G >> 8} {p.B >> 8}",
                        PixelGray1 p => $"{p.val << 7} {p.val << 7} {p.val << 7}",
                        PixelGray2 p => $"{p.val << 6} {p.val << 6} {p.val << 6}",
                        PixelGray4 p => $"{p.val << 3} {p.val << 3} {p.val << 3}",
                        PixelGray8 p => $"{p.val} {p.val} {p.val}",
                        PixelGray16 p => $"{p.val >> 8} {p.val >> 8} {p.val >> 8}",
                        _ => throw new NotImplementedException()
                    };

                    sw.Write($"{res}\n");
                }

                previousSpan = filteredSpan;
            }

            sw.Flush();
            return fs;
        }
        throw new Exception();
    }

    private object makePixel(ReadOnlySpan<byte> bytes, int j, IHDR ihdr)
    {
        return ihdr.PixelFormat switch
        {
            PixelFormat.RGB8 => new PixelRGB(bytes[j * 3], bytes[j * 3 + 1], bytes[j * 3 + 2]),
            PixelFormat.RGB16 => new PixelRGB16(BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 6)..(j * 6 + 1)]),
            BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 6 + 2)..(j * 6 + 3)]),
            BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 6 + 4)..(j * 6 + 5)])),

            PixelFormat.RGBA8 => new PixelRGBA(bytes[j * 4], bytes[j * 4 + 1], bytes[j * 4 + 2], bytes[j * 4 + 3]),
            PixelFormat.RGBA16 => new PixelRGBA16(BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 8)..(j * 8 + 1)]),
            BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 8 + 2)..(j * 8 + 3)]),
            BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 8 + 4)..(j * 8 + 5)]),
            BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 8 + 6)..(j * 8 + 7)])),

            PixelFormat.GrayScale1bit => new PixelGray1((byte)(bytes[j / 8] >> (j % 8) & 0x1)),
            PixelFormat.GrayScale2bit => new PixelGray2((byte)(bytes[j / 4] >> (j % 4) & 0x3)),
            PixelFormat.GrayScale4bit => new PixelGray4((byte)(bytes[j / 2] >> (j % 2) & 0x7)),
            PixelFormat.GrayScale8bit => new PixelGray8(bytes[j]),
            PixelFormat.GrayScale16bit => new PixelGray16(BinaryPrimitives.ReadUInt16BigEndian(bytes[(j * 2)..(j * 2 + 1)])),

            _ => throw new NotImplementedException()
        };
    }


}