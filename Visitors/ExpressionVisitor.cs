using System.Globalization;
using System.Reflection;
using combgen.Datatype;
using combgen.InternalFunctions;

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
    
    public override DataType VisitFloatExpression(combgenParser.FloatExpressionContext context)
    {
        float value = Convert.ToSingle(context.@float().GetText(), CultureInfo.InvariantCulture);
        return new FloatDataType(value);
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
            // TODO this whole "contains" needs to be replaced with something more concise, reliable and systematic
            return new IntDataType(context.GetText().Contains("+") ? (int)leftInt.GetObject() + (int)rightInt.GetObject() : (int)leftInt.GetObject() - (int)rightInt.GetObject());
        }
        
        if (left is FloatDataType leftFloat && right is FloatDataType rightFloat)
        {
            return new FloatDataType(context.GetText().Contains("+") ? (float)leftFloat.GetObject() + (float)rightFloat.GetObject() : (float)leftFloat.GetObject() - (float)rightFloat.GetObject());
        }

        if (left is StringDataType leftString && right is StringDataType rightString)
        {
            if(context.GetText().Contains("+")) return new StringDataType((string)leftString.GetObject() + (string)rightString.GetObject());
        }

        throw new InvalidOperationException("AddExpression requires scalar or string operands.");
    }

    public override DataType VisitMulExpression(combgenParser.MulExpressionContext context)
    {
        var left = context.expression(0).Accept(this);
        var right = context.expression(1).Accept(this);

        if (left is IntDataType leftInt && right is IntDataType rightInt)
        {
            return new IntDataType(context.GetText().Contains("*") ? (int)leftInt.GetObject() * (int)rightInt.GetObject() : (int)leftInt.GetObject() / (int)rightInt.GetObject());
        }
        
        if (left is FloatDataType leftFloat && right is FloatDataType rightFloat)
        {
            return new FloatDataType(context.GetText().Contains("*") ? (float)leftFloat.GetObject() * (float)rightFloat.GetObject() : (float)leftFloat.GetObject() / (float)rightFloat.GetObject());
        }

        throw new InvalidOperationException("MulExpression requires scalar operands.");
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
        
        if (left is FloatDataType leftFloat && right is FloatDataType rightFloat)
        {
            switch (context.compOp().GetText())
            {
                case "<":
                    return new BooleanDataType((float)leftFloat.GetObject() < (float)rightFloat.GetObject());
                case ">":
                    return new BooleanDataType((float)leftFloat.GetObject() > (float)rightFloat.GetObject());
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

    public override DataType VisitLogicalExpression(combgenParser.LogicalExpressionContext context)
    {
        var left = context.expression(0).Accept(this);
        var right = context.expression(1).Accept(this);

        if (left == null || right == null)
            throw new InvalidOperationException("Expressions cannot be null.");

        var leftObject = left.GetObject();
        var rightObject = right.GetObject();

        if (leftObject is not bool || rightObject is not bool)
            throw new Exception("LogicalExpression requires boolean operands.");

        switch (context.logOp().GetText())
        {
            case "and":
                return new BooleanDataType((bool)leftObject && (bool)rightObject);
            case "or":
                return new BooleanDataType((bool)leftObject || (bool)rightObject);
            default:
                throw new NotImplementedException($"Invalid equality operator: {context.logOp().GetText()}.");
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
        int? aIndex, bIndex;

        if(context.variableAccess().expression().Length > 0)
            aIndex = Convert.ToInt16(context.variableAccess().expression()[0].Accept(this).GetObject());
        else
            aIndex = null;
        
        if(context.variableAccess().expression().Length > 1)
            bIndex = Convert.ToInt16(context.variableAccess().expression()[1].Accept(this).GetObject());
        else
            bIndex = null;
        
        // TODO implement arrow operator (returns list of every [aIndex] string together)

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
        
        Type type = typeof(InternalFunctions.InternalFunctions);

        // Use LINQ to find the method with the desired attribute
        var targetMethod = type.GetMethods().FirstOrDefault(method => method.GetCustomAttribute<FunctionName>()?.Name == functionName);

        if (targetMethod != null)
        {
            var instance = Activator.CreateInstance(type);
            return (DataType)targetMethod.Invoke(instance, [arguments]);
        }

        throw new Exception($"Unable to call function {functionName}: Not found");
    }

}