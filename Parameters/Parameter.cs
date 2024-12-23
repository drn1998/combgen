namespace combgen.Parameters;

public class Parameter
{
    private Dictionary<string, int> _parameters = new Dictionary<string, int>();
    
    private List<string> _allowedParameters = [
        "auto-mode",
        "temp-abs-zero"
    ];

    static string parameter_to_config(string parameter)
    {
        return parameter.ToUpperInvariant().Replace("-", "_");
    }

    static string config_to_parameter(string config)
    {
        return config.ToLowerInvariant().Replace("_", "-");
    }

    public void SetParameter(string parameter, int value)
    {
        if(_allowedParameters.Contains(config_to_parameter(parameter)))
            _parameters[config_to_parameter(parameter)] = value;
        else
            throw new Exception($"Parameter {parameter} does not exist.");
    }

    public int GetValueByParameter(string parameter)
    {
        if(_allowedParameters.Contains(config_to_parameter(parameter)))
            return _parameters[config_to_parameter(parameter)];
        else
            throw new Exception($"Parameter {parameter} does not exist.");
    }

    public Parameter()
    {
        // Default values
        SetParameter("auto-mode", 0);
        SetParameter("temp-abs-zero", 1);
    }
}