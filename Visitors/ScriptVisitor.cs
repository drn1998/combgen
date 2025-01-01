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
    private Options _options;

    Dictionary<string, Datafield.Datafield> _datafields = new Dictionary<string, Datafield.Datafield>();

    public ScriptVisitor(Options opt)
    {
        _options = opt;
    }
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
    
    static bool ListsEqualLength(List<List<string>> listOfLists)
    {
        if (listOfLists.Count == 0)
            return true;

        int firstSize = listOfLists[0].Count;

        return listOfLists.All(innerList => innerList.Count == firstSize);
    }
    public override object? VisitScript(combgenParser.ScriptContext context)
    {
        foreach (var paramAssign in context.parameterAssignment())
        {
            ParamField pf = Visit(paramAssign) as ParamField;
            _parameter.SetParameter(pf.ParamName, pf.ParamValue);
        }
        
        if (context.datafieldAssignment().Length == 0)
            throw new Exception("No datafield assignments in source. At least one datafield is required.");

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

        if (_options is { Count: true, Enumerate: false })
        {
            Console.WriteLine(mixedRadixConverter.Total());
        } else if (_options.Enumerate)
        {
            for (int i = 0; i < mixedRadixConverter.Total(); i++)
            {
                List<int> result = mixedRadixConverter.ConvertToMixedRadix(i);
                Dictionary<string, int> values = new Dictionary<string, int>();

                for (int j = 0; j < _datafields.Count; j++)
                {
                    values[_datafields.ElementAt(j).Key] = result[j];
                }

                ExpressionVisitor expressionVisitor = new ExpressionVisitor(_datafields, null, values);

                string tmp = string.Empty;

                if (context.combinationalExpression() is null)
                    throw new Exception("No combinational expression found or empty.");
            
                foreach (var expression in context.combinationalExpression().expression())
                {
                    tmp += expressionVisitor.Visit(expression).ToString();
                }
                
                if(_options.Count) Console.Write(i.ToString().PadLeft(mixedRadixConverter.Total().ToString().Length) + ": ");
            
                Console.WriteLine(tmp);
            }
        } else if (_options.Table)
        {
            List<int> bases = mixedRadixConverter.Bases();
            
            for (int i = 0; i < _datafields.Count; i++)
            {
                Console.WriteLine(_datafields.ElementAt(i).Value.GetTable(bases[i], true, _datafields.ElementAt(i).Key.Substring(1)));
            }
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

        StringDatafieldObject sdo = Visit(context.stringDatafield()) as StringDatafieldObject;

        if (context.ORDERED() is null)
            sdo.Data = sdo.Data
                .OrderBy(innerList => innerList.FirstOrDefault())
                .ToList();
        else
            if (sdo.Origin == StringDatafield.StringFieldOrigin.OptionalString)
                throw new Exception("Canonical string order cannot be applied to optional string");

        if (ct is StringDatafield.CombinationalType.NPR or StringDatafield.CombinationalType.NCR && sdo.Origin == StringDatafield.StringFieldOrigin.OptionalString)
            throw new Exception("nPr or nCr mode cannot be applied to optional string");

        if (count == 0) throw new Exception("Cannot choose zero elements from string table");
        if (count > sdo.Data.Count) throw new Exception("String table has too few elements for applied combination mode");
        
        StringDatafield df = new StringDatafield(ct, sdo.Data, count, sdo.Origin);
        
        return df;
    }

    public record StringDatafieldObject
    {
        public List<List<string>> Data;
        public StringDatafield.StringFieldOrigin Origin;
    }

    public override object? VisitStringDatafield(combgenParser.StringDatafieldContext context)
    {
        StringDatafieldObject sdo = new StringDatafieldObject();
        List<List<string>> data = new List<List<string>>();
        
        if (context.literalStringDatafield() is not null)
        {
            sdo.Origin = StringDatafield.StringFieldOrigin.LiteralList;

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
            sdo.Origin = StringDatafield.StringFieldOrigin.File;
            
            string filename = unescapeString(context.fileStringDatafield().SQ_STRING().GetText());
            
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
            sdo.Origin = StringDatafield.StringFieldOrigin.OptionalString;
            
            List<string> empty = new List<string>() {""};
            List<string> optstr = new List<string>() {unescapeString(context.optionalStringDatafield().DQ_STRING().GetText())};
            
            data.Add(optstr);
            data.Add(empty);
        }
        
        if(!ListsEqualLength(data)) throw new Exception("Not all rows have the same number of columns");
        
        sdo.Data = data;
        
        return sdo;
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