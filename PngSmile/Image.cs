namespace PngSmile;

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
