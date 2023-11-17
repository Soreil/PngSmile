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
}
