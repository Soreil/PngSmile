namespace Deflate;

public readonly record struct BlockHeader
    (bool IsLastBlock, BlockEncoding Encoding);
