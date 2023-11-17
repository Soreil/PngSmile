
using Force.Crc32;

namespace PngSmile;

public record struct Pixel(byte R, byte G, byte B);

public class CRC32
{
    //x^32+x^26+x^23+x^22+x^16+x^12+x^11+x^10+x^8+x^7+x^5+x^4+x^2+x+1
    //0xedb88320L 


    /* Table of CRCs of all 8-bit messages. */
    private readonly ulong[] crc_table = new ulong[256];

    public CRC32()
    {
        
        Make_crc_table(crc_table.AsSpan());
    }

    /* Make the table for a fast CRC. */
    private static void Make_crc_table(Span<ulong> table)
    {

        for (int n = 0; n < table.Length; n++)
        {
            var c = (ulong)n;
            for (int k = 0; k < 8; k++)
            {
                if ((c & 1) != 0)
                    c = 0xedb88320UL ^ (c >> 1);
                else
                    c >>= 1;
            }
            table[n] = c;
        }
    }

    /* Update a running CRC with the bytes buf[0..len-1]--the CRC
       should be initialized to all 1's, and the transmitted value
       is the 1's complement of the final running CRC (see the
       crc() routine below)). */
    private ulong Update_crc(ulong crc, Span<byte> buf)
    {
        ulong c = crc;

        foreach(var item in buf)
        {
            c = crc_table[(c ^ item) & 0xff] ^ (c >> 8);
        }
        return c;
    }

    /* Return the CRC of the bytes buf[0..len-1]. */
    private ulong Crc(Span<byte> buf) => Update_crc(0xffffffffUL, buf) ^ 0xffffffffUL;

}


public class PNG
{
    public byte[] FileSignature = [137, 80, 78, 71, 13, 10, 26, 10];

}



public record struct Chunk(uint Length, uint ChunkType, byte[] ChunkData, uint CRC);

public class Image<T> where T : struct
{
    private T[,] Pixels { get; }

    public void Serialize(BinaryWriter bw)
    {


        for (int y = 0; y < Pixels.GetLength(0); y++)
        {
            for (int x = 0; x < Pixels.GetLength(1); x++)
            {
                var pixel = Pixels[y, x];
            }
        }
    }
}
