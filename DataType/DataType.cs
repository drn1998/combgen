namespace combgen.Datatype;

public abstract class DataType
{
    public abstract object GetObject();
}

public class BooleanDataType : DataType
{
    private bool bValue;

    public BooleanDataType(bool bValue)
    {
        this.bValue = bValue;
    }

    public override string ToString()
    {
        switch (bValue)
        {
            case false: return "false";
            case true: return "true";
        }
    }

    public override object GetObject()
    {
        return bValue;
    }
}

public class StringDataType : DataType
{
    private string sValue;

    public StringDataType(string sValue)
    {
        this.sValue = sValue;
    }
    
    public override string ToString() => sValue;

    public override object GetObject()
    {
        return sValue;
    }
}

public class IntDataType : DataType
{
    private int iValue;

    public IntDataType(int iValue)
    {
        this.iValue = iValue;
    }
    
    public override string ToString() => iValue.ToString();

    public override object GetObject()
    {
        return iValue;
    }
}

public class FloatDataType : DataType
{
    private float fValue;

    public FloatDataType(float fValue)
    {
        this.fValue = fValue;
    }
    
    public override string ToString() => fValue.ToString();

    public override object GetObject()
    {
        return fValue;
    }
}

public class StringListDataType : DataType
{
    private List<string> lValue;

    public StringListDataType()
    {
        lValue = new List<string>();
    }
    
    public StringListDataType(List<string> lValue)
    {
        this.lValue = new List<string>(lValue);
    }
    
    public override string ToString() => "list(" + lValue.Count + ") {" + string.Join(", ", lValue) + "}";

    public override object GetObject()
    {
        return lValue;
    }
}