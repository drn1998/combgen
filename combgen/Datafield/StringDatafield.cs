using System.Numerics;
using combgen.Datatype;
using combgen.Util;

namespace combgen.Datafield;

public class StringDatafield(StringDatafield.CombinationalType combinationalType, List<List<string>> data, int count, StringDatafield.StringFieldOrigin origin)
    : Datafield
{
    private List<List<string>> _data = data;
    private CombinationalType _combinationalType = combinationalType;
    private int _count = count;
    private StringFieldOrigin _origin = origin;
    
    public enum CombinationalType
    {
        Singular,
        NPR,
        NCR
    }

    public enum StringFieldOrigin
    {
        LiteralList,
        File,
        SqlQuery,          // Not yet in use
        OptionalString,
        AnonymousList
    }

    private List<List<string>> GenComb(int combVal)
    {
        List<List<string>> results = new List<List<string>>();
        List<List<string>> dcopy = new List<List<string>>(_data);
        List<int> pos = new List<int>(_count);

        switch (_combinationalType)
        {
            case CombinationalType.Singular:
                results.Add(_data[combVal]);
                return results;    // No selection from set necessary/permissible
            case CombinationalType.NPR:
                int totalCount = _data.Count;
                List<int> divisors = new List<int>(_count);
                int k = _count - 1;

                for (int i = totalCount - 1; k >= 0; i--)
                {
                    divisors.Add(Combinatorics.nPr(i, k));
                    k--;
                }

                int number = combVal;
            
                foreach (var t in divisors)
                {
                    pos.Add(number / t);
                    number %= t;
                }
                break;
            case CombinationalType.NCR:
                int a = _data.Count;
                int b = _count;
                int x = (Combinatorics.nCr(_data.Count, _count) - 1) - combVal;

                for (int i = 0; i < _count; i++)
                {
                    a--;
                    while (Combinatorics.nCr(a, b) > x)
                    {
                        a--;
                    }

                    pos.Add(_data.Count - 1 - a);
                    x -= Combinatorics.nCr(a, b);
                    b--;
                }
                break;
            default: throw new Exception("Supposedly unreachable code executed.");
        }
        
        foreach (var p in pos)
        {
            results.Add(dcopy[p]);
            if(_combinationalType == CombinationalType.NPR)
                dcopy.RemoveAt(p);
        }

        return results;
    }

    public override DataType Read(BigInteger combVal, int? aIndex, int? bIndex)
    {
        int _combVal = (int)combVal;
        if (_combinationalType == CombinationalType.Singular)
        {
            if (bIndex is not null)
                throw new Exception("Invalid array access");
            
            if (aIndex is null)
                return new StringDataType(_data[_combVal][0]);
            
            if(aIndex.Value < 0 || aIndex.Value >= _data.First().Count)
                throw new Exception("Invalid array access: Out of range");
                
            return new StringDataType(GenComb(_combVal)[0][aIndex.Value]);    
        }

        // Access in case of two-dimensional result table
        
        List<List<string>> results = GenComb(_combVal);

        if (aIndex is null && bIndex is null)
            return new StringListDataType(results.Select(r => r[0]).ToList());
        
        if (aIndex is null && bIndex is not null)
            return new StringListDataType(results.Select(r => r[bIndex.Value]).ToList());

        if (aIndex is not null && bIndex is null)
            return new StringDataType(results[aIndex.Value][0]);

        if (aIndex is not null && bIndex is not null)
            return new StringDataType(results[aIndex.Value][bIndex.Value]);
        
        throw new NotImplementedException("Unimplemented access mode");
    }

    public override int Count()
    {
        switch (_combinationalType)
        {
            case CombinationalType.Singular:
                return _data.Count;
            case CombinationalType.NPR:
                return Combinatorics.nPr(_data.Count, _count);
            case CombinationalType.NCR:
                return Combinatorics.nCr(_data.Count, _count);
            default: throw new InvalidOperationException();
        }
    }

    public override string GetTable(BigInteger baseIndex, TableVerbosity tv = TableVerbosity.Default, string title = "Value")
    {
        /* TODO: In non-verbose mode, nCr and nPr shall provide values to calculate the value instead of presenting
           every combination with its pre-calculated code (as this saves space with the tradeoff of having to do
           manual calculations). It may also be desirable to have a enum for verbosity level instead of just the
           boolean. */

        string output;
        
        if (_origin == StringFieldOrigin.OptionalString)
        {
            List<List<string>> curList = GenComb(1);

            output = $"<table><tr><th colspan=\"2\" style=\"text-align:center\">~&nbsp;{title}</th></tr><tr><td>{curList[0][0]}</td><td style=\"text-align: right;\">" + baseIndex.ToString("N0", CustomFormat) + "</td></tr></table>";

        }
        else
        {
            output = "<table>";
            
            output += $"<tr><th>{title}</th><th>Code</th></tr>";
        
            for (int i = 0; i < Count(); i++) {
                    List<List<string>> curList = GenComb(i);
                    output += "<tr><td>" + string.Join(", ", curList.Select(x => x[0]).ToList()) + "</td>";
                    output += "<td style=\"text-align: right;\">" + (baseIndex * i).ToString("N0", CustomFormat) + "</td></tr>";
            }
            
            output += "</table>";
        }

        return output;
    }
}