public static class Base62Converter
{
    private static readonly string Characters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Encode(long id)
    {
        if (id == 0) return Characters[0].ToString();

        var sb = new System.Text.StringBuilder();
        while (id > 0)
        {
            // Pega o resto da divisão por 62 para encontrar o caractere
            long remainder = id % 62;
            sb.Insert(0, Characters[(int)remainder]); // Insere no início para inverter a ordem
            id /= 62; // Divide o ID por 62
        }
        return sb.ToString();
    }

    // Você também precisaria da função Decode para testes ou outras funcionalidades
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