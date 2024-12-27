using combgen.Datatype;

namespace combgen.Datafield;

public abstract class Datafield
{
    public abstract DataType Read(int combVal, int? aIndex, int? bIndex);
    public abstract int Count();

    public abstract string GetTable(int baseIndex, bool verbose = false);
}