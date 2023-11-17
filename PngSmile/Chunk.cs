namespace PngSmile;

public record struct Chunk(uint Length, uint ChunkType, byte[] ChunkData, uint CRC)
{
    public readonly string ChunkTypeString() => new([
        (char)((ChunkType >> 24) & 0xff),
        (char)((ChunkType >> 16) & 0xff),
        (char)((ChunkType >> 8) & 0xff),
        (char)((ChunkType >> 0) & 0xff)]
        );
}
