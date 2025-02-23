using System.Buffers.Binary;

namespace PngSmile;

public class CHRM
{
    public CHRM(Chunk chunk)
    {
        if (chunk.ChunkTypeString() != "cHRM") throw new InvalidDataException("Only cHRM allowed");
        ArgumentOutOfRangeException.ThrowIfNotEqual(chunk.Length, (uint)32, nameof(chunk.Length));

        uint[] uints = new uint[8];

        var chunkDataView = chunk.ChunkData.AsSpan();
        for (int i = 0; i < uints.Length; i++)
        {
            uints[i] = BinaryPrimitives.ReadUInt32BigEndian(chunkDataView[(i * 4)..]);
        }

        WhitePointXUint = uints[0];
        WhitePointX = uints[0] / 100000.0;

        WhitePointYUint = uints[1];
        WhitePointY = uints[1] / 100000.0;

        RedXUint = uints[2];
        RedX = uints[2] / 100000.0;

        RedYUint = uints[3];
        RedY = uints[3] / 100000.0;

        GreenXUint = uints[4];
        GreenX = uints[4] / 100000.0;

        GreenYUint = uints[5];
        GreenY = uints[5] / 100000.0;

        BlueXUint = uints[6];
        BlueX = uints[6] / 100000.0;

        BlueYUint = uints[7];
        BlueY = uints[7] / 100000.0;
    }

    //White Point x: 4 bytes
    uint WhitePointXUint { get; }
    double WhitePointX { get; }
    //White Point y: 4 bytes
    uint WhitePointYUint { get; }
    double WhitePointY { get; }
    //Red x:         4 bytes
    uint RedXUint { get; }
    double RedX { get; }
    //Red y:         4 bytes
    uint RedYUint { get; }
    double RedY { get; }
    //Green x:       4 bytes
    uint GreenXUint { get; }
    double GreenX { get; }
    //Green y:       4 bytes
    uint GreenYUint { get; }
    double GreenY { get; }
    //Blue x:        4 bytes
    uint BlueXUint { get; }
    double BlueX { get; }
    //Blue y:        4 bytes
    uint BlueYUint { get; }
    double BlueY { get; }

}
