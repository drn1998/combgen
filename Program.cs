using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Antlr4.Runtime;
using combgen.Parameters;
using combgen.Visitors;
using CommandLine;

namespace combgen;

public class Options
{
    [Option('c', "count", Required = false, HelpText = "Print the number of combinations.", SetName = "output")]
    public bool Count {get; set; }
    
    [Option('e', "enumerate", Required = false, HelpText = "Enumerate the combinations", SetName = "output")]
    public bool Enumerate {get; set; }
        
    [Option('T', "table", Required = false, HelpText = "Print the table as HTML.", SetName = "table")]
    public bool Table {get; set;}
        
    [Value(0, MetaName = "input file",
        HelpText = "Input source file to be processed.",
        Required = true)]
    public string? FileName { get; set;}
}
class Program
{
    static void Main(string[] args)
    {
        Options opt = CommandLine.Parser.Default.ParseArguments<Options>(args).Value;
        
        if(opt is null) Environment.Exit(1);
        
        AntlrInputStream inputStream = new AntlrFileStream(opt.FileName, Encoding.UTF8);
        combgenLexer lexer = new combgenLexer(inputStream);
        CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
        combgenParser parser = new combgenParser(commonTokenStream);
        combgenParser.ScriptContext scriptContext = parser.script();
        
        ScriptVisitor scriptVisitor = new ScriptVisitor(opt);  // Give script visitor options in constructor
        try
        {
            scriptVisitor.Visit(scriptContext);
        }
        catch (Exception e)
        {
            TextWriter tw = Console.Error;
            tw.WriteLine(e.Message);
        }
        
    }
}