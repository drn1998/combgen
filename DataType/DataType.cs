using System.Globalization;

namespace combgen.Datatype;

public abstract class DataType
{
    public abstract object GetObject();
}

public class BooleanDataType : DataType
{
    private readonly bool _bValue;

    public BooleanDataType(bool bValue)
    {
        this._bValue = bValue;
    }

    public override string ToString()
    {
        switch (_bValue)
        {
            case false: return "false";
            case true: return "true";
        }
    }

    public override object GetObject()
    {
        return _bValue;
    }
}

public class StringDataType : DataType
{
    private readonly string _sValue;

    public StringDataType(string sValue)
    {
        this._sValue = sValue;
    }
    
    public override string ToString() => _sValue;

    public override object GetObject()
    {
        return _sValue;
    }
}

public class IntDataType : DataType
{
    private readonly int _iValue;

    public IntDataType(int iValue)
    {
        this._iValue = iValue;
    }
    
    public override string ToString() => _iValue.ToString();

    public override object GetObject()
    {
        return _iValue;
    }
}

public class FloatDataType : DataType
{
    private readonly float _fValue;

    public FloatDataType(float fValue)
    {
        this._fValue = fValue;
    }
    
    public override string ToString() => _fValue.ToString(CultureInfo.InvariantCulture);

    public override object GetObject()
    {
        return _fValue;
    }
}

public class StringListDataType : DataType
{
    private readonly List<string> _lValue;

    public StringListDataType()
    {
        _lValue = new List<string>();
    }
    
    public StringListDataType(List<string> lValue)
    {
        this._lValue = new List<string>(lValue);
    }
    
    public override string ToString() => "list(" + _lValue.Count + ") {" + string.Join(", ", _lValue) + "}";

    public override object GetObject()
    {
        return _lValue;
    }
}