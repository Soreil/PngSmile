using Deflate;

using System.Buffers.Binary;

namespace PngSmile;

public class IDAT
{
    public ZLibDataStream Stream { get; }

    public IDAT(Chunk chunk, IHDR header)
    {
        Stream = new ZLibDataStream(
            chunk.ChunkData[0],
            chunk.ChunkData[1],
            chunk.ChunkData[2..^4],
            BinaryPrimitives.ReadUInt32BigEndian(chunk.ChunkData.AsSpan()[^4..])
            );
    }
}
