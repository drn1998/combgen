using System.Numerics;
using combgen.Datatype;

namespace combgen.Datafield;

public class FloatDatafield(float from, float to, float interval) : Datafield
{
    public override DataType Read(BigInteger combVal, int? aIndex, int? bIndex)
    {
        if (aIndex is not null || bIndex is not null) throw new ArgumentException("Index not applicable to numeric datafield");

        return new FloatDataType(from + interval * (int)combVal);
    }

    public override int Count()
    {
        return Convert.ToInt32((to - from) / interval + 1.0f);
    }

    public override string GetTable(BigInteger baseIndex, TableVerbosity tv = TableVerbosity.Default, string title = "Value")
    {
        string output = "<table>";
        
        if (tv == TableVerbosity.Default)
        {
            output += $"<tr><th>{title}</th><th>Code</th></tr>";
        
            for (int i = 0; i < Count(); i++)
            {
                output += "<tr><td>" + (from + interval * i) + "</td>";
                output += "<td style=\"text-align: right;\">" + (baseIndex * i).ToString("N0", CustomFormat) + "</td></tr>";
            }
        }
        
        output += "</table>";

        return output;
    }
}