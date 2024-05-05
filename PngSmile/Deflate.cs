
using System.Buffers.Binary;
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;

namespace Deflate;

public enum BlockEncoding
{
    literal,
    staticHuffman,
    dynamicHuffman,
    reserved
}

public readonly record struct BlockHeader
    (bool IsLastBlock, BlockEncoding Encoding);

public class Deflate
{
    private const int MaxWindowSize = 32768;

}

public record ZLibDataStream(
    byte CompressionMethod,
    byte AdditionalFlags,
    byte[] CompressedDataBlocks,
    uint CheckValue
    )
{
    public bool IsValid => CMFValid && CINFOValid && CheckSumValid && !FDICT;

    public int CMF => (CompressionMethod & 0x0f);
    public bool CMFValid => CMF == 8;
    public int CINFO => ((CompressionMethod >> 4) & 0x0f);
    public bool CINFOValid => CINFO < 8;
    //CINFO is the base-2 logarithm of the LZ77 window
    //size, minus eight(CINFO= 7 indicates a 32K window size).
    public int WindowSize => 1 << (CINFO + 8);

    public int FCHECK => AdditionalFlags & 0x1f;
    public bool FDICT => (AdditionalFlags & 0x20) != 0;
    public int FLEVEL => AdditionalFlags >> 6;

    public ushort CheckSum => (ushort)((CompressionMethod << 8) + AdditionalFlags);

    public bool CheckSumValid => CheckSum % 31 == 0;

    public byte[] Decode()
    {
        if (!IsValid) throw new Exception();

        using var ds = new DeflateStream(new MemoryStream(CompressedDataBlocks), CompressionMode.Decompress);
        List<byte> output = [];

        var buf = new byte[1024];
        var pos = 0;
        for (; ; )
        {
            var read = ds.Read(buf, 0, 1024);
            if (read == 0) break;

            pos += read;
            output.AddRange(buf);
        }
        return [.. output];


        int index = 0;
        var headerBlock = CompressedDataBlocks[index];
        var hHeader = new BlockHeader(
            (headerBlock & 0x01) == 1,
            (BlockEncoding)((headerBlock >> 1) & 0x03)
            );


        if (hHeader.Encoding == BlockEncoding.literal)
        {
            index += 1;
            var LEN = BinaryPrimitives.ReadUInt16BigEndian(CompressedDataBlocks.AsSpan()[index..]);
            index += 2;
            var LLEN = BinaryPrimitives.ReadUInt16BigEndian(CompressedDataBlocks.AsSpan()[index..]);
            index += 2;
            if ((LEN | LLEN) != 0xffff) throw new Exception();
            var outputBytes = CompressedDataBlocks.AsSpan()[index..(index + LEN)];
            return outputBytes.ToArray();
        }
        else
        {
            if (hHeader.Encoding == BlockEncoding.dynamicHuffman)
            {
                //read representation of code trees(see subsection below)
            }

            return CompressedDataBlocks[index..];

            //using var ds = new DeflateStream(new MemoryStream(CompressedDataBlocks[index..]), CompressionMode.Decompress);
            //List<byte> outputBytes = [];

            //var buf = new byte[1024];
            //var pos = 0;
            //for (; ; )
            //{
            //    var read = ds.Read(buf, 0, 1024);
            //    if (read == 0) break;

            //    pos += read;
            //    outputBytes.AddRange(buf);
            //}
            //return outputBytes.ToArray();

            /*     loop (until end of block code recognized)
                     decode literal/length value from input stream
                     if value < 256
                        copy value (literal byte) to output stream
                     otherwise
                        if value = end of block (256)
                           break from loop
                        otherwise (value = 257..285)
                           decode distance from input stream

                           move backwards distance bytes in the output
                           stream, and copy length bytes from this
                           position to the output stream.
                  end loop */

        }



    }
}
