using combgen.Datatype;
using combgen.Util;

namespace combgen.Datafield;

public class StringDatafield(StringDatafield.CombinationalType combinationalType, List<List<string>> data, UInt16 count, StringDatafield.StringFieldOrigin origin)
    : Datafield
{
    private List<List<string>> _data = data;
    private CombinationalType _combinationalType = combinationalType;
    private UInt16 _count = count;
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

    public override DataType Read(int combVal, short? aIndex, short? bIndex)
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
                return 1;
            default: throw new InvalidOperationException();
        }
    }
}