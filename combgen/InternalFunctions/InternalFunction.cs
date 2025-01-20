using combgen.Datatype;

namespace combgen.InternalFunctions;

[AttributeUsage(AttributeTargets.Method)]
public class FunctionName : Attribute
{
    public string Name { get; }

    public FunctionName(string name)
    {
        Name = name;
    }
}
public partial class InternalFunctions
{
    [FunctionName("capitalize")]
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
    [FunctionName("toUpper")]
    public static DataType toUpper(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");
        if (args[0] is not StringDataType) throw new Exception("Invalid data type: Must be a string");
        
        string input = (string)args[0].GetObject();
        
        return new StringDataType(input.ToUpper());
    }
    [FunctionName("toLower")]
    public static DataType toLower(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");
        if (args[0] is not StringDataType) throw new Exception("Invalid data type: Must be a string");
        
        string input = (string)args[0].GetObject();
        
        return new StringDataType(input.ToLower());
    }
    
    [FunctionName("cel_to_fahr")]
    public static DataType CelsiusToFahrenheit(List<DataType> args)
    {
        if (args.Count != 1) 
            throw new ArgumentException("Invalid number of arguments: Must be 1");

        var celsius = args[0].GetObject();

        return celsius switch
        {
            int intValue => new IntDataType(intValue * 9 / 5 + 32),
            float floatValue => new FloatDataType(floatValue * 9.0f / 5.0f + 32.0f),
            _ => throw new ArgumentException("Invalid data type: Must be an integer or floating-point")
        };
    }
    [FunctionName("fahr_to_cel")]
    public static DataType FahrenheitToCelsius(List<DataType> args)
    {
        if (args.Count != 1) 
            throw new ArgumentException("Invalid number of arguments: Must be 1");

        var celsius = args[0].GetObject();

        return celsius switch
        {
            int intValue => new IntDataType((intValue - 32) * 5 / 9),
            float floatValue => new FloatDataType((floatValue - 32.0f) * 5.0f/9.0f),
            _ => throw new ArgumentException("Invalid data type: Must be an integer or floating-point")
        };
    }
    [FunctionName("cel_to_kelvin")]
    public static DataType CelsiusToKelvin(List<DataType> args)
    {
        if (args.Count != 1) 
            throw new ArgumentException("Invalid number of arguments: Must be 1");

        var celsius = args[0].GetObject();

        return celsius switch
        {
            int intValue => new IntDataType(intValue + 273),
            float floatValue => new FloatDataType(floatValue + 273.15f),
            _ => throw new ArgumentException("Invalid data type: Must be an integer or floating-point")
        };
    }
    [FunctionName("fahr_to_kelvin")]
    public static DataType FahrenheitToKelvin(List<DataType> args)
    {
        if (args.Count != 1) 
            throw new ArgumentException("Invalid number of arguments: Must be 1");

        var celsius = args[0].GetObject();

        return celsius switch
        {
            int intValue => new IntDataType((intValue - 32) * 5 / 9 + 273),
            float floatValue => new FloatDataType((floatValue - 32.0f) * 5.0f / 9.0f + 273.15f),
            _ => throw new ArgumentException("Invalid data type: Must be an integer or floating-point")
        };
    }
    [FunctionName("kelvin_to_cel")]
    public static DataType KelvinToCelsius(List<DataType> args)
    {
        if (args.Count != 1) 
            throw new ArgumentException("Invalid number of arguments: Must be 1");

        var celsius = args[0].GetObject();

        return celsius switch
        {
            int intValue => new IntDataType(intValue - 273),
            float floatValue => new FloatDataType(floatValue - 273.15f),
            _ => throw new ArgumentException("Invalid data type: Must be an integer or floating-point")
        };
    }
    [FunctionName("kelvin_to_fahr")]
    public static DataType KelvinToFahrenheit(List<DataType> args)
    {
        if (args.Count != 1) 
            throw new ArgumentException("Invalid number of arguments: Must be 1");

        var celsius = args[0].GetObject();

        return celsius switch
        {
            int intValue => new IntDataType((intValue - 273) * 9 / 5 + 32),
            float floatValue => new FloatDataType((floatValue - 273.15f) * 9.0f / 5.0f + 32.0f),
            _ => throw new ArgumentException("Invalid data type: Must be an integer or floating-point")
        };
    }
    
    [FunctionName("if")]
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
    [FunctionName("max")]
    public static DataType Max(List<DataType> args)
    {
        if (args.Count == 0) throw new Exception("Invalid number of arguments: Must be 1 to n");
        
        if (args.All(arg => arg is not IntDataType)) throw new Exception("Invalid data type: Must be integer");
        
        return new IntDataType(args.Max(arg => (int)arg.GetObject()));
    }
    [FunctionName("min")]
    public static DataType Min(List<DataType> args)
    {
        if (args.Count == 0) throw new Exception("Invalid number of arguments: Must be 1 to n");
        
        if (args.All(arg => arg is not IntDataType)) throw new Exception("Invalid data type: Must be integer");
        
        return new IntDataType(args.Min(arg => (int)arg.GetObject()));
    }
    [FunctionName("countElem")]
    public static DataType CountElements(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");
        
        List<string> sl = (List<string>)args[0].GetObject();
        
        return new IntDataType(sl.Count);
    }
    [FunctionName("strLen")]
    public static DataType StrLen(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");

        string str = (string)args[0].GetObject();
        
        return new IntDataType(str.Length);
    }
    [FunctionName("sphVol")]
    public static DataType SphVol(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");
        
        float vol = (float) args[0].GetObject();
        
        if (float.IsPositive(vol))
            return new FloatDataType(4.0f/3.0f * float.Pi * float.Pow(vol, 3));
        
        throw new Exception("Invalid SphVol argument (must be positive)");
    }
    [FunctionName("sqrt")]
    public static DataType sqrt(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");
        
        float vol = (float) args[0].GetObject();
        
        if (float.IsPositive(vol))
            return new FloatDataType(float.Sqrt(vol));
        
        throw new Exception("Invalid sqrt argument (must be positive)");
    }
    [FunctionName("leftPad")]
    public static DataType LeftPad(List<DataType> args)
    {
        if (args.Count != 2) throw new Exception("Invalid number of arguments: Must be 2 (string, int)");
        if (args[0] is not StringDataType) throw new Exception("Invalid data type: First argument to LeftPad be a string");
        if (args[1] is not IntDataType) throw new Exception("Invalid data type: Second argument to LeftPad be an integer");

        string str = (string)args[0].GetObject();
        int i = (int)args[1].GetObject();

        if (i < str.Length) throw new Exception("Padding length is below string length.");

        return new StringDataType(str.PadLeft(i));
    }
    [FunctionName("rightPad")]
    public static DataType RightPad(List<DataType> args)
    {
        if (args.Count != 2) throw new Exception("Invalid number of arguments: Must be 2 (string, int)");
        if (args[0] is not StringDataType) throw new Exception("Invalid data type: First argument to RightPad be a string");
        if (args[1] is not IntDataType) throw new Exception("Invalid data type: Second argument to RightPad be an integer");

        string str = (string)args[0].GetObject();
        int i = (int)args[1].GetObject();

        if (i < str.Length) throw new Exception("Padding length is below string length.");
        
        return new StringDataType(str.PadRight(i));
    }
    [FunctionName("groupcat")]
    public static DataType groupcat(List<DataType> args)
    {
        if (args.Count < 2 || args.Count > 3)
            throw new Exception("Invalid number of arguments: Must be 2 or 3");

        if (args[0] is not StringListDataType)
            throw new Exception("Invalid data type: First argument must be a list of strings");

        if (args[1] is not StringDataType)
            throw new Exception("Invalid data type: Second argument must be a string");

        List<string> stringList = (List<string>)args[0].GetObject();
        string separator = (string)args[1].GetObject();

        if (args.Count == 2)
        {
            return new StringDataType(string.Join(separator, stringList));
        }
        else if (args.Count == 3)
        {
            if (args[2] is not StringDataType)
                throw new Exception("Invalid data type: Third argument must be a string");

            string finalSeparator = (string)args[2].GetObject();

            if (stringList.Count <= 1)
            {
                return new StringDataType(string.Join(separator, stringList));
            }
            else
            {
                string result = string.Join(separator, stringList.Take(stringList.Count - 1))
                                + finalSeparator
                                + stringList.Last();
                return new StringDataType(result);
            }
        }

        throw new Exception("Unexpected logic error: This point should not be reached.");
    }
    [FunctionName("exclude")]
    public static DataType exclude(List<DataType> args)
    {
        if (args.Count != 2)
            throw new Exception("Invalid number of arguments: Must be 2");

        if (args[0] is not StringListDataType)
            throw new Exception("Invalid data type: First argument must be a list of strings");

        if (args[1] is not IntDataType)
            throw new Exception("Invalid data type: Second argument must be an integer");

        List<string> stringList = (List<string>)args[0].GetObject();
        int indexToRemove = (int)args[1].GetObject();

        if (indexToRemove < 0 || indexToRemove >= stringList.Count)
            throw new Exception("Invalid index: Out of range");

        List<string> resultList = new List<string>(stringList);
        resultList.RemoveAt(indexToRemove);

        return new StringListDataType(resultList);
    }
    [FunctionName("irand")]
    public static DataType irand(List<DataType> args)
    {
        Random random = new Random();
        if (args.Count == 0) return new IntDataType(random.Next());
        if (args.Count == 1) return new IntDataType(random.Next((int)args[0].GetObject()));
        // Check if max is not below min
        if (args.Count == 2) return new IntDataType(random.Next((int)args[0].GetObject(), (int)args[1].GetObject()));
        
        throw new Exception("irand must be called with 0, 1 or 2 arguments.");
    }
    [FunctionName("STR")]
    public static DataType str(List<DataType> args)
    {
        if (args.Count != 1 || args.Count != 2) throw new Exception("Invalid number of arguments: Must be 1 or 2");
        if (args[0] is FloatDataType && args[1] is IntDataType)
        {
            float f = (float)args[0].GetObject();
            int dp = (int)args[1].GetObject();
            return new StringDataType(f.ToString("F" + dp.ToString()));
        }

        return new StringDataType(args[0].ToString());
    }
    [FunctionName("INT")]
    public static DataType INT(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");

        switch (args[0].GetObject())
        {
            case int i:
                return new IntDataType(i);
            case string s:
                return new IntDataType(Convert.ToInt32(s));
            case float f:
                return new IntDataType(Convert.ToInt32(f));
            case bool b:
                return new IntDataType(b ? 1 : 0);
            case List<string> l:
                throw new Exception("Cannot convert string list to int");
        }
        
        throw new Exception("Unknown data type");
    }
    [FunctionName("FLOAT")]
    public static DataType FLOAT(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");

        switch (args[0].GetObject())
        {
            case int i:
                return new FloatDataType(Convert.ToSingle(i));
            case string s:
                return new FloatDataType(Convert.ToSingle(s));
            case float f:
                return new FloatDataType(f);
            case bool b:
                return new FloatDataType(b ? 1.0f : 0.0f);  // This seems odd... Maybe it should throw an exception
            case List<string> l:
                throw new Exception("Cannot convert string list to int");
        }
        
        throw new Exception("Unknown data type");
    }
    [FunctionName("C")]
    public static DataType C(List<DataType> args)
    {
        if (args.Count != 1) throw new Exception("Invalid number of arguments: Must be 1");
        if (args[0] is not IntDataType) throw new Exception("Invalid data type: Must be an integer");
        
        int input = (int)args[0].GetObject();

        return new StringDataType(new string(char.ConvertFromUtf32(input)));
    }
}