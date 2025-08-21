namespace UrlShortener;

public static class Base62Converter
{
    private static readonly string Characters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Encode(long id)
    {
        if (id == 0) return Characters[0].ToString();

        var sb = new System.Text.StringBuilder();
        while (id > 0)
        {
            
            long remainder = id % 62;
            sb.Insert(0, Characters[(int)remainder]); 
            id /= 62; 
        }
        return sb.ToString();
    }


    public static long Decode(string shortCode)
    {
        long id = 0;
        for (int i = 0; i < shortCode.Length; i++)
        {
            int charPosition = Characters.IndexOf(shortCode[i]);
            id = id * 62 + charPosition;
        }
        return id;
    }
}