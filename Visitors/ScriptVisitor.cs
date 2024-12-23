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
        public required int ParamValue;
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
            _parameter.SetParameter(pf.ParamName, pf.ParamValue);
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
            ParamValue = Convert.ToInt32(context.NUMBER().GetText())
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
        int count = 1;
        StringDatafield.StringFieldOrigin sfo = StringDatafield.StringFieldOrigin.LiteralList;
        
        if (context.NUMBER() is not null)
        {
            if (context.NCR() is not null)
                ct = StringDatafield.CombinationalType.NCR;
            if (context.NPR() is not null)
                ct = StringDatafield.CombinationalType.NPR;
            
            count = Convert.ToInt32(context.NUMBER().GetText());
        }

        // TODO Exception if optional string is combined with nCr/nPr
        List<List<string>> data = Visit(context.stringDatafield()) as List<List<string>>;

        if (context.ORDERED() is null)
        {
            data = data
                .OrderBy(innerList => innerList.FirstOrDefault())
                .ToList();
        }

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

        if (context.fileStringDatafield() is not null)
        {
            string filename = context.fileStringDatafield().SQ_STRING().GetText().Substring(1, context.fileStringDatafield().SQ_STRING().GetText().Length - 2);
            
            using (StreamReader reader = new StreamReader(filename))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] columns = line.Split('\t');

                    data.Add(new List<string>(columns));
                }
            }
        }

        if (context.optionalStringDatafield() is not null)
        {
            List<string> empty = new List<string>() {""};
            List<string> optstr = new List<string>() {unescapeString(context.optionalStringDatafield().DQ_STRING().GetText())};
            
            data.Add(optstr);
            data.Add(empty);
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