using System.Diagnostics;
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
        OptionalString
    }

    public override DataType Read(int combVal, int? aIndex, int? bIndex)
    {
        if (_combinationalType == CombinationalType.Singular)
        {
            if (bIndex is not null)
                throw new Exception("Invalid array access");
            
            if (aIndex is null)
                return new StringDataType(_data[combVal][0]);
            else
            {
                if(aIndex.Value < 0 || aIndex.Value >= _data.First().Count)
                    throw new Exception("Invalid array access: Out of range");
                
                return new StringDataType(_data[combVal][aIndex.Value]);
            }
                
        }
        
        List<List<string>> results = new List<List<string>>();
        List<List<string>> dcopy = new List<List<string>>(_data);
        List<int> pos = new List<int>(_count);

        if (_combinationalType == CombinationalType.NPR)
        {
            // Calculate lehmer code to insert right entries into results variable
            int totalCount = _data.Count;
            List<int> divisors = new List<int>(_count);
            int k = _count - 1;

            for (int i = totalCount - 1; k >= 0; i--)
            {
                divisors.Add(Combinatorics.nPr(i, k));
                k--;
            }

            int number = combVal;
            
            for (int i = 0; i < divisors.Count; i++)
            {
                pos.Add(number / divisors[i]);
                number %= divisors[i];
            }
        }

        if (_combinationalType == CombinationalType.NCR)
        {
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
        }
        
        foreach (var p in pos)
        {
            results.Add(dcopy[p]);
            if(_combinationalType == CombinationalType.NPR)
                dcopy.RemoveAt(p);
        }

        if (aIndex is null && bIndex is null)
        {
            return new StringListDataType(results.Select(r => r[0]).ToList());
        }
        
        if (aIndex is null && bIndex is not null)   // Arrow operator
        {
            return new StringListDataType(results.Select(r => r[bIndex.Value]).ToList());
        }

        if (aIndex is not null && bIndex is null)
        {
            return new StringDataType(results[aIndex.Value][0]);
        }

        if (aIndex is not null && bIndex is not null)
        {
            return new StringDataType(results[aIndex.Value][bIndex.Value]);
        }
        
        throw new NotImplementedException("nCr/nPr read access unimplemented");
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
}