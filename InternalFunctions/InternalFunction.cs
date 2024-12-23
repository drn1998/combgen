using combgen.Datatype;

namespace combgen.InternalFunctions;

public partial class InternalFunctions
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

    // TODO Allow two arguments instead of three and return empty string at else-case
    public static DataType If(List<DataType> args)
    {
        bool expressionTrue = false;
        
        if (args.Count != 3 && args.Count != 2) throw new Exception("Invalid number of arguments: Must be 2 or 3");
        if (args[0] is not BooleanDataType or IntDataType) throw new Exception("First argument must be boolean or integer");
        
        if (args[0] is BooleanDataType)
            expressionTrue = (bool)args[0].GetObject();
        
        if (args[0] is IntDataType)
            if((int)args[0].GetObject() > 0)
                expressionTrue = true;
        
        if(args.Count == 3)
            return expressionTrue ? args[1] : args[2];
        if (args.Count == 2)
            return expressionTrue ? args[1] : new StringDataType("");
        
        throw new Exception("Invalid number of arguments (should be unreachable!)");
    }

    public static DataType Max(List<DataType> args)
    {
        if (args.Count == 0) throw new Exception("Invalid number of arguments: Must be 1 to n");
        
        if (args.All(arg => arg is not IntDataType)) throw new Exception("Invalid data type: Must be integer");
        
        return new IntDataType(args.Max(arg => (int)arg.GetObject()));
    }
    
    public static DataType Min(List<DataType> args)
    {
        if (args.Count == 0) throw new Exception("Invalid number of arguments: Must be 1 to n");
        
        if (args.All(arg => arg is not IntDataType)) throw new Exception("Invalid data type: Must be integer");
        
        return new IntDataType(args.Min(arg => (int)arg.GetObject()));
    }

    public static DataType CountElements(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");
        
        List<string> sl = (List<string>)args[0].GetObject();
        
        return new IntDataType(sl.Count);
    }
}