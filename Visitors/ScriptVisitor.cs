using System.Linq.Expressions;
using System.Reflection;
using combgen.Datafield;
using combgen.Datatype;
using combgen.Parameters;
using combgen.Util;

namespace combgen.Visitors;

public class ScriptVisitor : combgenBaseVisitor<object?>
{
    Parameter _parameter = new Parameter();

    Dictionary<string, Datafield.Datafield> _datafields = new Dictionary<string, Datafield.Datafield>();
    
    public record ParamField
    {
        public required string ParamName;
        public required ushort ParamValue;
    }

    public record DatafieldAssignment
    {
        public required string FieldName;
        public required Datafield.Datafield dField;
    }

    static string unescapeString(string str)
    {
        return str.Substring(1, str.Length - 2);
    }
    public override object? VisitScript(combgenParser.ScriptContext context)
    {
        foreach (var paramAssign in context.parameterAssignment())
        {
            ParamField pf = Visit(paramAssign) as ParamField;
            _parameter.SetParameter(pf.ParamName, Convert.ToUInt16(pf.ParamValue));
        }

        foreach (var datafieldAssign in context.datafieldAssignment())
        {
            DatafieldAssignment df = Visit(datafieldAssign) as DatafieldAssignment;
            _datafields[df.FieldName] = df.dField;
        }
        
        List<int> radix = new List<int>();

        foreach (var memb in _datafields)
        {
            radix.Add(memb.Value.Count());
        }

        MixedRadixConverter mixedRadixConverter = new MixedRadixConverter(radix);

        for (int i = 0; i < mixedRadixConverter.Total(); i++)
        {
            List<int> result = mixedRadixConverter.ConvertToMixedRadix(i);
            Dictionary<string, int> values = new Dictionary<string, int>();

            for (int j = 0; j < _datafields.Count; j++)
            {
                values[_datafields.ElementAt(j).Key] = result[j];
            }

            ExpressionVisitor expressionVisitor = new ExpressionVisitor(_datafields, null, values);

            foreach (var expression in context.combinationalExpression().expression())
            {
                Console.Write(expressionVisitor.Visit(expression).ToString());
            }
            Console.WriteLine();

        }
        
        return base.VisitScript(context);
    }
    
    public override ParamField VisitParameterAssignment(combgenParser.ParameterAssignmentContext context)
    {
        ParamField pf = new ParamField
        {
            ParamName = context.IDENTIFIER().GetText(),
            ParamValue = Convert.ToUInt16(context.NUMBER().GetText())
        };

        return pf;
    }

    public override object VisitDatafieldAssignment(combgenParser.DatafieldAssignmentContext context)
    {
        var variableName = context.VARIABLE().GetText();

        var datafieldExpressionResult = context.datafieldExpression().Accept(this) as Datafield.Datafield;
        
        DatafieldAssignment df = new DatafieldAssignment()
        {
            FieldName = variableName,
            dField = datafieldExpressionResult
        };
        
        return df;
    }


    public override object? VisitStringDatafieldExpression(combgenParser.StringDatafieldExpressionContext context)
    {
        StringDatafield.CombinationalType ct = StringDatafield.CombinationalType.Singular;
        UInt16 count = 1;
        StringDatafield.StringFieldOrigin sfo = StringDatafield.StringFieldOrigin.LiteralList;
        
        if (context.NUMBER() is not null)
        {
            if (context.NCR() is not null)
                ct = StringDatafield.CombinationalType.NCR;
            if (context.NPR() is not null)
                ct = StringDatafield.CombinationalType.NPR;
            
            count = Convert.ToUInt16(context.NUMBER().GetText());
        }

        List<List<string>> data = Visit(context.stringDatafield()) as List<List<string>>;

        StringDatafield df = new StringDatafield(ct, data, count, sfo);
        
        return df;
    }

    public override object? VisitStringDatafield(combgenParser.StringDatafieldContext context)
    {
        List<List<string>> data = new List<List<string>>();
        
        if (context.literalStringDatafield() is not null)
        {
            foreach (var str in context.literalStringDatafield().DQ_STRING())
            {
                List<string> mb = new List<string>();
                mb.Add(unescapeString(str.GetText()));
                data.Add(mb);
            }

            foreach (var strA in context.literalStringDatafield().stringArray())
            {
                List<string>mb = new List<string>();
                foreach (var str in strA.DQ_STRING())
                {
                    mb.Add(unescapeString(str.GetText()));
                }
                data.Add(mb);
            }
        }
        
        // TODO throw exception of not all lists have the same length
        
        return data;
    }

    public override object? VisitIntDatafieldExpression(combgenParser.IntDatafieldExpressionContext context)
    {
        int from = Convert.ToInt32(context.intDatafield().integer()[0].GetText());
        int to = Convert.ToInt32(context.intDatafield().integer()[1].GetText());
        int interval;
        int count;

        if (context.intDatafield().NUMBER() is not null)
            interval = Convert.ToInt32(context.intDatafield().NUMBER().GetText());
        else
            interval = 1;

        if (context.NUMBER() is not null)
            count = Convert.ToInt32(context.NUMBER().GetText());
        else
            count = 1;
        
        IntDatafield intDatafield = new IntDatafield(from, to, interval, count);

        return intDatafield;
    }
}

public class ExpressionVisitor : combgenBaseVisitor<DataType>
{
    private Dictionary<string, Datafield.Datafield> _datafields; // For variable access
    private readonly IDictionary<string, Func<List<DataType>, DataType>> _functions; // For function calls
    private Dictionary<string, int> _combVal;

    public ExpressionVisitor(Dictionary<string, Datafield.Datafield> datafields, IDictionary<string, Func<List<DataType>, DataType>> functions, Dictionary<string, int> combVal)
    {
        _datafields = datafields;
        _functions = functions;
        _combVal = combVal;
    }

    public override DataType VisitIntExpression(combgenParser.IntExpressionContext context)
    {
        int value = int.Parse(context.integer().GetText());
        return new IntDataType(value);
    }

    public override DataType VisitBooleanExpression(combgenParser.BooleanExpressionContext context)
    {
        bool value = context.boolean().GetText() == "true";
        return new BooleanDataType(value);
    }

    public override DataType VisitStringExpression(combgenParser.StringExpressionContext context)
    {
        string value = context.dqString().GetText().Trim('"');
        return new StringDataType(value);
    }

    public override DataType VisitAddExpression(combgenParser.AddExpressionContext context)
    {
        var left = context.expression(0).Accept(this);
        var right = context.expression(1).Accept(this);

        if (left is IntDataType leftInt && right is IntDataType rightInt)
        {
            return new IntDataType(context.GetText().Contains("+") ? (int)leftInt.GetObject() + (int)rightInt.GetObject() : (int)leftInt.GetObject() - (int)rightInt.GetObject());
        }

        throw new InvalidOperationException("AddExpression requires integer operands.");
    }

    public override DataType VisitMulExpression(combgenParser.MulExpressionContext context)
    {
        var left = context.expression(0).Accept(this);
        var right = context.expression(1).Accept(this);

        if (left is IntDataType leftInt && right is IntDataType rightInt)
        {
            return new IntDataType(context.GetText().Contains("*") ? (int)leftInt.GetObject() * (int)rightInt.GetObject() : (int)leftInt.GetObject() / (int)rightInt.GetObject());
        }

        throw new InvalidOperationException("MulExpression requires integer operands.");
    }

    public override DataType VisitCompareExpression(combgenParser.CompareExpressionContext context)
    {
        var left = context.expression(0).Accept(this);
        var right = context.expression(1).Accept(this);

        if (left is IntDataType leftInt && right is IntDataType rightInt)
        {
            return new BooleanDataType(context.GetText().Contains("<") ? (int)leftInt.GetObject() < (int)rightInt.GetObject() : (int)leftInt.GetObject() > (int)rightInt.GetObject());
        }

        throw new InvalidOperationException("CompareExpression requires integer operands.");
    }

    public override DataType VisitEqualityExpression(combgenParser.EqualityExpressionContext context)
    {
        var left = context.expression(0).Accept(this);
        var right = context.expression(1).Accept(this);

        bool isEqual = context.GetText().Contains("=") ? left.Equals(right) : !left.Equals(right);
        return new BooleanDataType(isEqual);
    }

    public override DataType VisitNegatedExpression(combgenParser.NegatedExpressionContext context)
    {
        var value = context.expression().Accept(this);

        if (value is BooleanDataType booleanValue)
        {
            return new BooleanDataType(!(bool)booleanValue.GetObject());
        }

        throw new InvalidOperationException("NegatedExpression requires a boolean operand.");
    }

    public override DataType VisitParenthesizedExpression(combgenParser.ParenthesizedExpressionContext context)
    {
        return context.expression().Accept(this);
    }

    public override DataType VisitVariableExpression(combgenParser.VariableExpressionContext context)
    {
        short? aIndex, bIndex;

        if(context.variableAccess().expression().Length > 0)
            aIndex = Convert.ToInt16(context.variableAccess().expression()[0].Accept(this).GetObject());
        else
            aIndex = null;
        
        if(context.variableAccess().expression().Length > 1)
            bIndex = Convert.ToInt16(context.variableAccess().expression()[0].Accept(this).GetObject());
        else
            bIndex = null;

        return _datafields[context.variableAccess().VARIABLE().GetText()].Read(_combVal[context.variableAccess().VARIABLE().GetText()], aIndex, bIndex);
    }
    
    public override DataType VisitFunctionCallExpression(combgenParser.FunctionCallExpressionContext context)
    {
        // Get the function name
        string functionName = context.functionCall().IDENTIFIER().GetText();

        // Get the arguments
        var arguments = context.functionCall().expression()
            .Select(expr => expr.Accept(this)) // Evaluate each expression
            .ToList();

        // Find the method in InternalFunctions class
        var method = typeof(InternalFunctions.InternalFunctions).GetMethod(functionName,
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        if (method == null)
            throw new Exception($"Function '{functionName}' not found.");

        // Ensure the method accepts the correct number and types of parameters
        var parameters = method.GetParameters();
        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(List<DataType>))
            throw new Exception($"Function '{functionName}' has an invalid signature.");

        // Call the method and return the result
        try
        {
            return (DataType)method.Invoke(null, new object[] { arguments });
        }
        catch (TargetInvocationException ex)
        {
            throw new Exception($"Error invoking function '{functionName}': {ex.InnerException?.Message}", ex.InnerException);
        }
    }

}