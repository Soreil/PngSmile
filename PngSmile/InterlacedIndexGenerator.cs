namespace PngSmile;

public static class InterlacedIndexGenerator
{
    //We kinda have to count how many pixels we've processed so far as well as the size of the image
    //if we combine these two bits of information we can find the index reliably.
    //Alternatively we could write a generator which yields the indexes in order.
    public static IEnumerable<List<(int y, int x)>> GetInterLacedIndex(int width, int height)
    {
        /*   1 6 4 6 2 6 4 6
             7 7 7 7 7 7 7 7
             5 6 5 6 5 6 5 6
             7 7 7 7 7 7 7 7
             3 6 4 6 3 6 4 6
             7 7 7 7 7 7 7 7
             5 6 5 6 5 6 5 6
             7 7 7 7 7 7 7 7*/

        //1
        for (int j = 0; j < height; j += 8)
        {
            List<(int y, int x)> res1 = [];
            for (int i = 0; i < width; i += 8)
            {
                res1.Add((j, i));
            }
            yield return res1;
        }
        //2
        for (int j = 0; j < height; j += 8)
        {
            List<(int y, int x)> res2 = [];
            for (int i = 4; i < width; i += 8)
            {
                res2.Add((j, i));
            }
            yield return res2;
        }

        //3
        for (int j = 4; j < height; j += 8)
        {
            List<(int y, int x)> res3 = [];
            for (int i = 0; i < width; i += 4)
            {
                res3.Add((j, i));
            }
            yield return res3;
        }
        //4
        for (int j = 0; j < height; j += 4)
        {
            List<(int y, int x)> res4 = [];
            for (int i = 2; i < width; i += 4)
            {
                res4.Add((j, i));
            }
            yield return res4;
        }
        //5
        for (int j = 2; j < height; j += 4)
        {
            List<(int y, int x)> res5 = [];
            for (int i = 0; i < width; i += 2)
            {
                res5.Add((j, i));
            }
            yield return res5;
        }
        //6
        for (int j = 0; j < height; j += 2)
        {
            List<(int y, int x)> res6 = [];
            for (int i = 1; i < width; i += 2)
            {
                res6.Add((j, i));
            }
            yield return res6;
        }
        //7
        for (int j = 1; j < height; j += 2)
        {
            List<(int y, int x)> res7 = [];
            for (int i = 0; i < width; i += 1)
            {
                res7.Add((j, i));
            }
            yield return res7;
        }
    }

}
