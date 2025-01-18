using System.Numerics;
using System.Text;
using Antlr4.Runtime;
using combgen;
using combgen.Visitors;

namespace combgen_test.Example;

public partial class ExampleTest
{
    [TestMethod]
    [DataRow("../../../../combgen/Example/example_01.combgen", "35")]
    [DataRow("../../../../combgen/Example/example_02.combgen", "18")]
    [DataRow("../../../../combgen/Example/example_03.combgen", "120")]
    [DataRow("../../../../combgen/Example/example_04.combgen", "6")]
    [DataRow("../../../../combgen/Example/example_05.combgen", "15")]
    [DataRow("../../../../combgen/Example/example_06.combgen", "24")]
    [DataRow("../../../../combgen/Example/example_07.combgen", "24")]
    [DataRow("../../../../combgen/Example/example_08.combgen", "12")]
    [DataRow("../../../../combgen/Example/example_09.combgen", "140")]
    [DataRow("../../../../combgen/Example/example_10.combgen", "144")]
    [DataRow("../../../../combgen/Example/example_11.combgen", "6")]
    public void TestExample(string file, string countLiteral)
    {
        Options opt = new Options
        {
            FileName = file,
            Count = true
        };

        BigInteger count = BigInteger.Parse(countLiteral);

        try
        {
            AntlrInputStream inputStream = new AntlrFileStream(opt.FileName, Encoding.UTF8);
            combgenLexer lexer = new combgenLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            combgenParser parser = new combgenParser(commonTokenStream);
            parser.ErrorHandler = new BailErrorStrategy(); // Stops on the first syntax error
            combgenParser.ScriptContext scriptContext = parser.script();

            ScriptVisitor scriptVisitor = new ScriptVisitor(opt);
            
            Assert.AreEqual(count, scriptVisitor.Visit(scriptContext));
        }
        catch (Exception e)
        {
            Assert.Fail();
        }
    }
}