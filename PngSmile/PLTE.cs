using System.ComponentModel;
using System.Drawing;

namespace PngSmile;

public class PLTE
{
    public List<PaletteEntry> PaletteEntries;
    public PLTE(Chunk chunk)
    {
        if (chunk.ChunkTypeString() != "PLTE") throw new InvalidDataException("Only PLTE allowed");

        ArgumentOutOfRangeException.ThrowIfGreaterThan((int)chunk.Length, 256 * 3, nameof(chunk.Length));
        ArgumentOutOfRangeException.ThrowIfGreaterThan((int)chunk.Length % 3, 0, nameof(chunk.Length));
        ArgumentOutOfRangeException.ThrowIfLessThan((int)chunk.Length, 1 * 3, nameof(chunk.Length));

        var palettes = chunk.ChunkData.Chunk(3).Select(p => new PaletteEntry(p[0], p[1], p[2]));

        PaletteEntries = palettes.ToList();
    }
}
