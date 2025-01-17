namespace combgen_test;

[TestClass]
public partial class CombinatoricsTest
{
    [TestClass]
    public class nPr_Test
    {
        [TestMethod]
        [DataRow(11, 3, 990)]
        [DataRow(11, 4, 7920)]
        [DataRow(27, 3, 17550)]
        public void nPr_ResultCorrect(int n, int k, int expected)
        {
            var result = combgen.Util.Combinatorics.nPr(n, k);
            
            Assert.AreEqual(result, expected);
        }
    }
    
    [TestClass]
    public class nCr_Test
    {
        [TestMethod]
        [DataRow(11, 3, 165)]
        [DataRow(11, 4, 330)]
        [DataRow(27, 3, 2925)]
        public void nCr_ResultCorrect(int n, int k, int expected)
        {
            var result = combgen.Util.Combinatorics.nCr(n, k);
            
            Assert.AreEqual(result, expected);
        }
    }
}