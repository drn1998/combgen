using System.Reflection;
using combgen.Datatype;

namespace combgen.Visitors;

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
            //return new BooleanDataType(context.GetText().Contains("<") ? (int)leftInt.GetObject() < (int)rightInt.GetObject() : (int)leftInt.GetObject() > (int)rightInt.GetObject());
            switch (context.compOp().GetText())
            {
                case "<":
                    return new BooleanDataType((int)leftInt.GetObject() < (int)rightInt.GetObject());
                case ">":
                    return new BooleanDataType((int)leftInt.GetObject() > (int)rightInt.GetObject());
                case ">=":
                    return new BooleanDataType((int)leftInt.GetObject() >= (int)rightInt.GetObject());
                case "<=":
                    return new BooleanDataType((int)leftInt.GetObject() <= (int)rightInt.GetObject());
            }
        }

        throw new InvalidOperationException("CompareExpression requires integer operands.");
    }

    public override DataType VisitEqualityExpression(combgenParser.EqualityExpressionContext context)
    {
        var left = context.expression(0).Accept(this);
        var right = context.expression(1).Accept(this);

        if (left == null || right == null)
            throw new InvalidOperationException("Expressions cannot be null.");

        var leftObject = left.GetObject();
        var rightObject = right.GetObject();

        switch (context.eqOp().GetText())
        {
            case "==":
                return new BooleanDataType(Equals(leftObject, rightObject));
            case "!=":
                return new BooleanDataType(!Equals(leftObject, rightObject));
            default:
                throw new NotImplementedException($"Invalid equality operator: {context.eqOp().GetText()}.");
        }
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