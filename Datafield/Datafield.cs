using combgen.Datatype;

namespace combgen.Datafield;

public abstract class Datafield
{
    public abstract DataType Read(int combVal, short? aIndex, short? bIndex);
    public abstract int Count();
}