using combgen.Datatype;

namespace combgen_test;

public partial class InternalFunctionTest
{
    [TestClass]
    public class MinFunctionTests
    {
        [TestMethod]
        [DataRow(new object[] { 42, 21, 99 }, 21)]
        [DataRow(new object[] { 15, 10, 25 }, 10)]
        [DataRow(new object[] { 15, 33, 15 }, 15)]
        [DataRow(new object[] { 100, 200, 50 }, 50)]
        [DataRow(new object[] { -10, -20, 0 }, -20)]
        [DataRow(new object[] { 0 }, 0)]
        [DataRow(new object[] { 49 }, 49)]
        [DataRow(new object[] { -49, -49 }, -49)]
        [DataRow(new object[] { 0, 0 }, 0)]
        [DataRow(new object[] { 49, 0 }, 0)]
        [DataRow(new object[] { -49, 0 }, -49)]
        [DataRow(new object[] { 3 }, 3)]
        public void MinFunc_ValidInputs_ReturnsExpectedMinimum(object[] inputValues, int expectedMinimum)
        {
            List<DataType> dataTypes = new List<DataType>();
            foreach (var value in inputValues)
            {
                dataTypes.Add(new IntDataType((int)value));
            }

            var result = combgen.InternalFunctions.InternalFunctions.Min(dataTypes).GetObject();

            Assert.AreEqual(expectedMinimum, result);
        }
    }

}