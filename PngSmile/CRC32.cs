namespace PngSmile;

public class CRC32
{
    //x^32+x^26+x^23+x^22+x^16+x^12+x^11+x^10+x^8+x^7+x^5+x^4+x^2+x+1
    //0xedb88320L 


    /* Table of CRCs of all 8-bit messages. */
    private readonly uint[] crcTable = new uint[256];

    public CRC32()
    {
        
        MakeCrcTable(crcTable.AsSpan());
    }

    /* Make the table for a fast CRC. */
    private static void MakeCrcTable(Span<uint> table)
    {
        for (int n = 0; n < table.Length; n++)
        {
            var c = (uint)n;
            for (int k = 0; k < 8; k++)
            {
                if ((c & 1) != 0)
                    c = 0xedb88320U ^ (c >> 1);
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
    private uint UpdateCrc(uint crc, ReadOnlySpan<byte> buf)
    {
        uint c = crc;

        foreach(var item in buf)
        {
            c = crcTable[(c ^ item) & 0xff] ^ (c >> 8);
        }
        return c;
    }

    /* Return the CRC of the bytes buf[0..len-1]. */
    public uint Crc(ReadOnlySpan<byte> buf) => UpdateCrc(0xffffffffU, buf) ^ 0xffffffffU;

}
