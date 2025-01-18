using System.Numerics;
using System.Text;
using Antlr4.Runtime;
using combgen.Datafield;

namespace combgen_test.ExpressionVisitor;

public partial class ExpressionVisitorTests
{
    [TestClass]
    public class IntegerArithmeticTest
    {
        [TestMethod]
        [DataRow("43", 43)]
        [DataRow("0", 0)]
        [DataRow("-1", -1)]
        [DataRow("57983", 57983)]
        [DataRow("2 + 49", 51)]
        [DataRow("73 + 0", 73)]
        [DataRow("73 + -1", 72)]
        [DataRow("39 + 382", 421)]
        [DataRow("2 + 3 * 4", 14)]  // Not 20
        [DataRow("(2 + 3) * 4", 20)]
        [DataRow("8 / 4 / 2", 1)]   // Not 4
        [DataRow("-5--5", 0)]
        [DataRow("-5 - -5", 0)]
        [DataRow("-5 - 5", -10)]
        [DataRow("2891 * 84", 242844)]
        public void IntegerArithmetic(string expression, int expectedResult)
        {
            AntlrInputStream inputStream = new AntlrInputStream(expression);
            combgenLexer lexer = new combgenLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            combgenParser parser = new combgenParser(commonTokenStream);
            combgenParser.ExpressionContext expressionContext = parser.expression();

            combgen.Visitors.ExpressionVisitor expressionVisitor = new combgen.Visitors.ExpressionVisitor(null, null);
            var result = expressionVisitor.Visit(expressionContext);
            
            Assert.AreEqual(expectedResult, result.GetObject() as int?);
        }
        
        [TestMethod]
        // Division by zero
        [DataRow("473 / 0")]
        [DataRow("0 / 0")]
        [DataRow("-1 / 0")]
        [DataRow("57983 / 0")]
        [DataRow("2891 * 84 / 0")]
        
        // Arithmetic overflow and underflow
        [DataRow("2047483647 + 1947483647")]
        [DataRow("98174892 + 4182794893789")]
        [DataRow("98174892 - 4182794893789")]
        [DataRow("-132949672 - 4893 * 163794328")]
        [DataRow("98174892 * 4182794893789")]
        [DataRow("32949672 * 4787289")]
        [DataRow("4892 * 8219 * 1893 * 9843 * 10934 * 9182 * 3741 * 16347")]
        
        // Syntax error
        [DataRow("34 + + + 323 +")]
        [DataRow("34 * {32}")]
        [DataRow("34 + + ")]
        public void IntegerArithmeticException(string expression)
        {
            try
            {
                AntlrInputStream inputStream = new AntlrInputStream(expression);
                combgenLexer lexer = new combgenLexer(inputStream);
                CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
                combgenParser parser = new combgenParser(commonTokenStream);
                parser.ErrorHandler = new BailErrorStrategy(); // Stops on the first syntax error
                combgenParser.ExpressionContext expressionContext = parser.expression();
                combgen.Visitors.ExpressionVisitor expressionVisitor = new combgen.Visitors.ExpressionVisitor(null, null);
                expressionVisitor.Visit(expressionContext);
            }
            catch (Exception e)
            {
                return;
            }
            
            Assert.Fail();
        }
    }
}