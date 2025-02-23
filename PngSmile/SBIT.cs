namespace PngSmile;

internal class SBIT
{
    public List<byte> SignificantBits { get; }
    public SBIT(Chunk chunk)
    {
        SignificantBits = chunk.ChunkData.ToList();
    }
}