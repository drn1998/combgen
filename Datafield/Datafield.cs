using System.Globalization;
using System.Numerics;
using combgen.Datatype;

namespace combgen.Datafield;

public abstract class Datafield
{
    public enum TableVerbosity
    {
        Compact,
        Default,
        Verbose
    };
    
    public NumberFormatInfo customFormat = new NumberFormatInfo
    {
        NumberGroupSeparator = "\u202F", // Thin space
        NumberGroupSizes = new[] { 3 },  // Grouping every 3 digits
    };
    public abstract DataType Read(BigInteger combVal, int? aIndex, int? bIndex);
    public abstract int Count();
    public abstract string GetTable(BigInteger baseIndex, TableVerbosity tv = TableVerbosity.Default, string title = "Value");
}