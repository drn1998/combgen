using System.Text;
using Antlr4.Runtime;
using combgen.Parameters;
using combgen.Visitors;

namespace combgen;

class Program
{
    static void Main(string[] args)
    {
        AntlrInputStream inputStream = new AntlrFileStream("../../../Example/example_08.combgen", Encoding.UTF8);
        combgenLexer lexer = new combgenLexer(inputStream);
        CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
        combgenParser parser = new combgenParser(commonTokenStream);
        combgenParser.ScriptContext scriptContext = parser.script();
        
        ScriptVisitor scriptVisitor = new ScriptVisitor();
        scriptVisitor.Visit(scriptContext);
    }
}