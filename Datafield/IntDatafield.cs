using System.Xml;
using combgen.Datatype;
using combgen.Util;

namespace combgen.Datafield;

public class IntDatafield(int from, int to, int interval, int count) : Datafield
{
    private int _from = from;
    private int _to = to;
    private int _interval = interval;
    private int _count = count;

    public override DataType Read(int combVal, int? aIndex, int? bIndex)
    {
        if (bIndex is not null) throw new ArgumentException("bIndex not applicable to integer datafield");

        if (aIndex is null)
        {
            if (_count == 1)
                return new IntDataType(_from + _interval * combVal);
            else
                throw new Exception("aIndex is necessary for multi-value integer data field.");
        }
        else
        {
            if (aIndex.Value >= _count) throw new Exception("aIndex is out of range of integer datafield.");

            MixedRadixConverter mixedRadixConverter = new MixedRadixConverter(Enumerable.Repeat((_to - _from) / _interval + 1, _count).ToList());

            List<int> res = mixedRadixConverter.ConvertToMixedRadix(combVal);
            
            return new IntDataType(_from + _interval * res[aIndex.Value]);
        }
    }

    public override int Count()
    {
        return Combinatorics.ipow((_to - _from) / _interval + 1, (short)_count);
    }

    public override string GetTable(int baseIndex, bool verbose = false, string title = "Value")
    {
        string output = "<table>";
        
        if (verbose)
        {
            if (_count == 1)
            {
                output += $"<tr><th>{title}</th><th>Code</th></tr>";
        
                for (int i = 0; i < Count(); i++)
                {
                    output += "<tr><td>" + (_from + _interval * i) + "</td>";
                    output += "<td>" + (baseIndex * i) + "</td></tr>";
                }
            }
        }
        
        output += "</table>";

        return output;
    }
}