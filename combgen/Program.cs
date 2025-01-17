using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Antlr4.Runtime;
using combgen.Parameters;
using combgen.Visitors;
using McMaster.Extensions.CommandLineUtils;

namespace combgen;

public class Options
{
    public bool Count {get; set; }
    public bool Enumerate {get; set; }
    public bool Table {get; set;}
    public string? FileName { get; set;}
    
    public int? Random { get; set; }
}

public class Program
{
    [Argument(0)]
    [Required]
    public string Filename { get; }
    [Option]
    public bool Enumerate { get; set; }
    [Option]
    public bool Table { get; set; }
    [Option]
    public bool Count { get; set; }
    [Option]
    public int RandomCombinations { get; set; }

    public void OnExecute()
    {
        Options opt = new Options
        {
            FileName = Filename,
            Enumerate = Enumerate,
            Table = Table,
            Count = Count,
            Random = RandomCombinations
        };
        #if DEBUG
            AntlrInputStream inputStream = new AntlrFileStream(opt.FileName, Encoding.UTF8);
            combgenLexer lexer = new combgenLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            combgenParser parser = new combgenParser(commonTokenStream);
            combgenParser.ScriptContext scriptContext = parser.script();
        
            ScriptVisitor scriptVisitor = new ScriptVisitor(opt);
            scriptVisitor.Visit(scriptContext);
        #else
        try
        {
            AntlrInputStream inputStream = new AntlrFileStream(opt.FileName, Encoding.UTF8);
            combgenLexer lexer = new combgenLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            combgenParser parser = new combgenParser(commonTokenStream);
            combgenParser.ScriptContext scriptContext = parser.script();
        
            ScriptVisitor scriptVisitor = new ScriptVisitor(opt);
            scriptVisitor.Visit(scriptContext);
        }
        catch (Exception e)
        {
            TextWriter tw = Console.Error;
            tw.WriteLine(e.Message);
        }
        #endif
    }
    public static int Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);
}