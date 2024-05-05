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

    public Stream Decode(ReadOnlySpan<byte> stream)
    {
        if (!IsPNG(stream)) throw new Exception();
        var input = stream[8..];

        List<Chunk> chunks = [];
        while (input.Length != 0)
        {
            var read = PNG.DecodeChunk(input, out var chunk);

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
            using var sw = new StreamWriter(fs);
            sw.Write($"P3\n{ihdr.Width} {ihdr.Height}\n255\n");

            Span<byte> previousSpan = [];
            for (int i = 0; i < ihdr.Height; i++)
            {
                var filterByte = dat[(int)ihdr.Width * 3 * i + i];

                var startIndex = (int)ihdr.Width * 3 * i + i + 1;
                var span = dat.Slice(startIndex, (int)ihdr.Width * 3);

                var filteredSpan = PNG.Filter(span, previousSpan, filterByte);

                for (int j = 0; j < ihdr.Width; j++)
                {
                    sw.Write($"{(int)(filteredSpan[j * 3])} {(int)(filteredSpan[j * 3 + 1])} {(int)(filteredSpan[j * 3 + 2])}\n");
                }

                previousSpan = filteredSpan;
            }

            sw.Flush();
            return fs;
        }
        return null;
    }

}