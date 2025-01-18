namespace combgen_test;

[TestClass]
public partial class CombinatoricsTest
{
    // Test sets were checked for correctness by pen-and-paper arithmetic, bc *and* a GENIE 92SC calculator
    
    [TestClass]
    public class nPr_Test
    {
        [TestMethod]
        [DataRow(1, 1, 1)]
        [DataRow(3, 3, 6)]
        [DataRow(4, 3, 24)]
        [DataRow(4, 4, 24)]
        [DataRow(7, 6, 5040)]
        [DataRow(7, 7, 5040)]
        [DataRow(8, 3, 336)]
        [DataRow(11, 3, 990)]
        [DataRow(11, 4, 7920)]
        [DataRow(17, 4, 57120)]
        [DataRow(27, 3, 17550)]
        [DataRow(38, 4, 1771560)]
        public void nPr_ResultCorrect(int n, int k, int expected)
        {
            var result = combgen.Util.Combinatorics.nPr(n, k);
            
            Assert.AreEqual(result, expected);
            
            // Result is not arbitrary
            Assert.AreNotEqual(result + 38, expected);
            Assert.AreNotEqual(result - 23, expected);
            Assert.AreNotEqual(result * 3, expected);
        }
    }
    
    [TestClass]
    public class nCr_Test
    {
        [TestMethod]
        [DataRow(1, 1, 1)]
        [DataRow(5, 5, 1)]
        [DataRow(6, 6, 1)]
        [DataRow(7, 7, 1)]
        [DataRow(8, 8, 1)]
        [DataRow(9, 9, 1)]
        [DataRow(8, 3, 56)]
        [DataRow(9, 5, 126)]
        [DataRow(11, 3, 165)]
        [DataRow(11, 4, 330)]
        [DataRow(27, 3, 2925)]
        public void nCr_ResultCorrect(int n, int k, int expected)
        {
            var result = combgen.Util.Combinatorics.nCr(n, k);
            
            Assert.AreEqual(result, expected);
            
            // Result is not arbitrary
            Assert.AreNotEqual(result + 38, expected);
            Assert.AreNotEqual(result - 23, expected);
            Assert.AreNotEqual(result * 3, expected);
        }
    }
    
    [TestClass]
    public class ipow_Test
    {
        [TestMethod]
        [DataRow(1, 1, 1)]
        [DataRow(5, 2, 25)]
        [DataRow(5, 3, 125)]
        [DataRow(3, 3, 27)]
        [DataRow(12, 1, 12)]
        [DataRow(12, 2, 144)]
        [DataRow(17, 4, 83521)]
        public void nCr_ResultCorrect(int n, int pow, int expected)
        {
            var result = combgen.Util.Combinatorics.ipow(n, (short)pow);
            
            Assert.AreEqual(result, expected);
            
            // Result is not arbitrary
            Assert.AreNotEqual(result + 38, expected);
            Assert.AreNotEqual(result - 23, expected);
            Assert.AreNotEqual(result * 3, expected);
        }
    }
}