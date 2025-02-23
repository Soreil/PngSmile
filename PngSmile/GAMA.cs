using System.Buffers.Binary;

namespace PngSmile;

public class GAMA
{
    public GAMA(Chunk chunk)
    {
        if (chunk.ChunkTypeString() != "gAMA") throw new InvalidDataException("Only gAMA allowed");
        ArgumentOutOfRangeException.ThrowIfNotEqual(chunk.Length, (uint)4, nameof(chunk.Length));
        this.GammaUint = BinaryPrimitives.ReadUInt32BigEndian(chunk.ChunkData);
        this.Gamma = GammaUint / 100000.0;
    }

    public uint GammaUint { get; }

    public double Gamma { get; }

    public double ComputeIntensity(double sample)
    {
        return Math.Pow(sample, Gamma);
    }
}
