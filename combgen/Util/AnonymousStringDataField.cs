namespace combgen.Util;

public class AnonymousStringDataField
{
    public static List<List<string>> Generate(int number)
    {
        if (number <= 1 || number > 26)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Number must be between 2 and 26, inclusive.");
        }

        List<List<string>> result = new List<List<string>>();

        for (int i = 0; i < number; i++)
        {
            string letter = char.ConvertFromUtf32(65 + i);
            result.Add(new List<string> { letter });
        }

        return result;
    }
}