using System.Numerics;
using System.Text.RegularExpressions;
using System.Web;
using combgen.Datafield;
using combgen.Parameters;
using combgen.Util;

namespace combgen.Visitors;

public class ScriptVisitor : combgenBaseVisitor<object?>
{
    Parameter _parameter = new Parameter();
    private Options _options;

    private readonly Dictionary<string, Datafield.Datafield> _datafields = new();

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
        public required Datafield.Datafield DField;
    }

    private static string UnescapeString(string str)
    {
        return Regex.Unescape(str.Substring(1, str.Length - 2));
    }
    
    private static int SectionLevel(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        Match match = Regex.Match(input, @"^#+");
        return match.Success ? Int32.Min(match.Value.Length, 6) : 0;
    }
    
    static string SectionString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return Regex.Replace(input, @"^#+", "");
    }
    
    static bool ListsEqualLength(List<List<string>> listOfLists)
    {
        if (listOfLists.Count == 0)
            return true;

        int firstSize = listOfLists[0].Count;

        return listOfLists.All(innerList => innerList.Count == firstSize);
    }

    private static List<BigInteger> GenerateUniqueRandoms(int k, BigInteger n)
    {
        if (k > n + 1)
            throw new ArgumentException("Number of random combinations above count/maximum.");

        List<BigInteger> result = new List<BigInteger>();
        HashSet<BigInteger> usedNumbers = new HashSet<BigInteger>();
        Random random = new Random();

        while (result.Count < k)
        {
            byte[] bytes = n.ToByteArray();
            random.NextBytes(bytes);
        
            BigInteger randomValue = new BigInteger(bytes);
        
            if (randomValue < 0)
                randomValue = -randomValue;
        
            randomValue = randomValue % (n + 1);

            if (!usedNumbers.Contains(randomValue))
            {
                usedNumbers.Add(randomValue);
                result.Add(randomValue);
            }
        }

        return result;
    }

    void PrintCombination(MixedRadixConverter mrc, combgenParser.ScriptContext sc, BigInteger c, BigInteger max)
    {
        List<BigInteger> result = mrc.ConvertToMixedRadix(c);
        Dictionary<string, BigInteger> values = new Dictionary<string, BigInteger>();

        for (int j = 0; j < _datafields.Count; j++)
        {
            values[_datafields.ElementAt(j).Key] = result[j];
        }

        ExpressionVisitor expressionVisitor = new ExpressionVisitor(_datafields, values);

        string tmp = string.Empty;

        if (sc.combinationalExpression() is null)
            throw new Exception("No combinational expression found or empty.");
            
        foreach (var expression in sc.combinationalExpression().expression())
        {
            tmp += expressionVisitor.Visit(expression).ToString();
        }
                
        if(_options.Count) Console.Write(c.ToString().PadLeft(max.ToString().Length) + ": ");
            
        Console.WriteLine(tmp);
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
            _datafields[df.FieldName] = df.DField;
        }
        
        List<BigInteger> radix = new List<BigInteger>();

        if (_options.Random == 0)
            _options.Random = null;

        foreach (var memb in _datafields)
        {
            radix.Add(memb.Value.Count());
        }

        MixedRadixConverter mixedRadixConverter = new MixedRadixConverter(radix);
        
        if (_options.Random is not null && _options.Enumerate == true)
            throw new Exception("Cannot use options random and enumerate together.");

        if (_options is { Count: true, Random: null, Enumerate: false })
        {
            Console.WriteLine(mixedRadixConverter.Total());
            return mixedRadixConverter.Total();
        }
        if (_options.Enumerate)
        {
            for (BigInteger i = 0; i < mixedRadixConverter.Total(); i++)
                PrintCombination(mixedRadixConverter, context, i, mixedRadixConverter.Total());
        } else if (_options.Table)
        {
            List<BigInteger> bases = mixedRadixConverter.Bases();
            
            Console.WriteLine("<!DOCTYPE html>\n<html>\n  <head>\n    <meta charset=\"utf-8\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n\t\n      <style>\n        p {\n          font-family: sans-serif;\n          font-size: 11px;\n          font-style: italic;\n        }\n        h1, h2, h3, h4, h5, h6 {\n          font-family: sans-serif;\n        }\n        h1 {\n          font-size: 2em;\n        }\n        h2 {\n          font-size: 1.6em;\n        }\n        h3 {\n          font-size: 1.4em;\n        }\n        h4 {\n          font-size: 1.1em;\n        }\n        h5 {\n          font-size: 1em;\n        }\n        h6 {\n          font-size: 0.8em;\n        }\n        td, th {\n            font-family: sans-serif;\n            font-size: 9px;\n        }\n        body {\n            column-count: 2;\n            column-fill: auto\n        }\n    </style></head>\n\t<body>");
            
            Console.WriteLine("<p>Range: 0 to " + (mixedRadixConverter.Total() - 1).ToString("N0", Datafield.Datafield.CustomFormat) + "</p>");
            
            for (int i = 0; i < _datafields.Count; i++)
            {
                foreach (var sectAbv in _datafields.ElementAt(i).Value.SectionsAbove)
                {
                    Console.WriteLine("<h" + SectionLevel(sectAbv) + ">" + SectionString(sectAbv).Trim() + "</h" + SectionLevel(sectAbv) + ">");
                }
                if(_datafields.ElementAt(i).Value.Annotation is not null)
                    Console.WriteLine("<p>" + _datafields.ElementAt(i).Value.Annotation + "</p>");
                Console.WriteLine(_datafields.ElementAt(i).Value.GetTable(bases[i], Datafield.Datafield.TableVerbosity.Default, _datafields.ElementAt(i).Key.Substring(1)));
            }
            
            Console.WriteLine("</body>\n</html>");
        } else if (_options.Random is not null)
        {
            List<BigInteger> randoms = GenerateUniqueRandoms(_options.Random.Value, mixedRadixConverter.Total());

            randoms.Sort();
            
            foreach (var val in randoms)
                PrintCombination(mixedRadixConverter, context, val, randoms.Last());
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
        var datafieldExpressionResult = context.datafieldExpression().Accept(this) as Datafield.Datafield;
        var variableName = context.VARIABLE().GetText();
        string? annotation = null;
        datafieldExpressionResult.SectionsAbove = new List<string>();

        foreach (var s in context.SECTION())
        {
            datafieldExpressionResult.SectionsAbove.Add(s.GetText());
        }
        
        if(context.ANNOTATION() is not null)
            annotation = " " + context.ANNOTATION().GetText().Substring(3, context.ANNOTATION().GetText().Length - 6);

        datafieldExpressionResult.Annotation = annotation != null ? HttpUtility.HtmlEncode(Regex.Replace(annotation.Trim(), @"\s+", " ")) : null;
        
        DatafieldAssignment df = new DatafieldAssignment()
        {
            FieldName = variableName,
            DField = datafieldExpressionResult
        };
        
        return df;
    }
    
    public override object? VisitStringDatafieldExpression(combgenParser.StringDatafieldExpressionContext context)
    {
        StringDatafield.CombinationalType ct = StringDatafield.CombinationalType.Singular;
        
        int count = 1;  // Default
        
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
        {
            switch (sdo.Origin)
            {
                case StringDatafield.StringFieldOrigin.OptionalString:
                    throw new Exception("Canonical string order cannot be applied to optional string");
                case StringDatafield.StringFieldOrigin.AnonymousList:
                    throw new Exception("Canonical string order cannot be applied to anonymous string list");
            }
        }

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
                mb.Add(UnescapeString(str.GetText()));
                data.Add(mb);
            }

            foreach (var strA in context.literalStringDatafield().stringArray())
            {
                List<string>mb = new List<string>();
                foreach (var str in strA.DQ_STRING())
                {
                    mb.Add(UnescapeString(str.GetText()));
                }
                data.Add(mb);
            }
        }

        if (context.fileStringDatafield() is not null)
        {
            sdo.Origin = StringDatafield.StringFieldOrigin.File;
            
            string filename = UnescapeString(context.fileStringDatafield().SQ_STRING().GetText());
            
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
            List<string> optstr = new List<string>() {UnescapeString(context.optionalStringDatafield().DQ_STRING().GetText())};
            
            data.Add(optstr);
            data.Add(empty);
        }

        if (context.anonymousStringDatafield() is not null)
        {
            sdo.Origin = StringDatafield.StringFieldOrigin.AnonymousList;
            
            data = AnonymousStringDataField.Generate(Convert.ToInt32(context.anonymousStringDatafield().NUMBER().GetText()));
        }
        
        if(!ListsEqualLength(data)) throw new Exception("Not all rows have the same number of columns");
        
        sdo.Data = data;
        
        return sdo;
    }

    public override object? VisitIntDatafieldExpression(combgenParser.IntDatafieldExpressionContext context)
    {
        int from = Convert.ToInt32(context.intDatafield().integer()[0].GetText());
        int to = Convert.ToInt32(context.intDatafield().integer()[1].GetText());
        int interval = 1;
        int count = 1;

        if (context.intDatafield().NUMBER() is not null)
            interval = Convert.ToInt32(context.intDatafield().NUMBER().GetText());

        if (context.NUMBER() is not null)
            count = Convert.ToInt32(context.NUMBER().GetText());
        
        IntDatafield intDatafield = new IntDatafield(from, to, interval, count);

        return intDatafield;
    }
}