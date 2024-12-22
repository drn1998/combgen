using combgen.Datatype;

namespace combgen.Datafield;

public class IntDatafield(int from, int to, int interval, int count) : Datafield
{
    private int _from = from;
    private int _to = to;
    private int _interval = interval;

    private int _count = count;

    public override DataType Read(int combVal, short? aIndex, short? bIndex)
    {
        if (bIndex is not null) throw new ArgumentException("bIndex not applicable to integer datafield");
        
        return new IntDataType(from + (interval * combVal));
    }

    public override int Count()
    {
        return _count * ((_to - _from) / _interval + 1);
    }
}