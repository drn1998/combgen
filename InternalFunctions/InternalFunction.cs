using combgen.Datatype;

namespace combgen.InternalFunctions;

public class InternalFunctions
{
    public static DataType Capitalize(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");
        if (args[0] is not StringDataType) throw new Exception("Invalid data type: Must be a string");
        
        string input = (string)args[0].GetObject();
        
        if (string.IsNullOrEmpty(input))
            return new StringDataType(input);

        char[] chars = input.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (char.IsLetter(chars[i]))
            {
                chars[i] = char.ToUpper(chars[i]);
                break;
            }
        }

        return new StringDataType(new string(chars));
    }

    public static DataType CelToFah(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");
        if (args[0] is not IntDataType) throw new Exception("Invalid data type: Must be an integer");
        
        // Convert celsius to fahrenheit
        int celsius = (int)args[0].GetObject();
        
        return new IntDataType(celsius * 9/5 + 32);
    }
}