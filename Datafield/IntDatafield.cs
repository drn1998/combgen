using System.Numerics;
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

    public override DataType Read(BigInteger combVal, int? aIndex, int? bIndex)
    {
        if (bIndex is not null) throw new ArgumentException("bIndex not applicable to integer datafield");

        if (aIndex is null)
        {
            if (_count == 1)
                return new IntDataType(_from + _interval * (int)combVal);
            
            throw new Exception("aIndex is necessary for multi-value integer data field.");
        }
        else
        {
            if (aIndex.Value >= _count) throw new Exception("aIndex is out of range of integer datafield.");

            MixedRadixConverter mixedRadixConverter = new MixedRadixConverter(Enumerable.Repeat((BigInteger)(_to - _from) / _interval + 1, _count).ToList());

            List<BigInteger> res = mixedRadixConverter.ConvertToMixedRadix(combVal);
            
            return new IntDataType((int)(_from + _interval * res[aIndex.Value]));
        }
    }

    public override int Count()
    {
        return Combinatorics.ipow((_to - _from) / _interval + 1, (short)_count);
    }

    public override string GetTable(BigInteger baseIndex, TableVerbosity tv, string title = "Value")
    {
        string output = "<table>";
        
        if (tv == TableVerbosity.Default)
        {
            if (_count == 1)
            {
                output += $"<tr><th>{title}</th><th>Code</th></tr>";
        
                for (int i = 0; i < Count(); i++)
                {
                    output += "<tr><td>" + (_from + _interval * i) + "</td>";
                    output += "<td style=\"text-align: right;\">" + (baseIndex * i).ToString("N0", customFormat) + "</td></tr>";
                }
            }
            else
            {
                output += $"<tr><th>{title}</th><th>Code</th></tr>";
        
                for (int i = 0; i < Count(); i++)
                {
                    MixedRadixConverter mixedRadixConverter = new MixedRadixConverter(Enumerable.Repeat((BigInteger)(_to - _from) / _interval + 1, _count).ToList());

                    List<BigInteger> res = mixedRadixConverter.ConvertToMixedRadix(i);
                    
                    output += "<tr><td>";

                    for (int j = 0; j < _count; j++)
                    {
                        output += _from + _interval * res[j];
                        if (j != _count - 1) output += ", ";
                    }

                    output += "</td>";
                    
                    output += "<td style=\"text-align: right;\">" + (baseIndex * i).ToString("N0", customFormat) + "</td></tr>";
                }
            }
        }
        
        output += "</table>";

        return output;
    }
}