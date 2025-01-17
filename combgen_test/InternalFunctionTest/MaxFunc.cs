using combgen.Datatype;

namespace combgen_test;

[TestClass]
public partial class InternalFunctionTest
{
    [TestClass]
    public class MaxFunctionTests
    {
        [TestMethod]
        [DataRow(new object[] { 42, 21, 99 }, 99)]
        [DataRow(new object[] { 15, 10, 25 }, 25)]
        [DataRow(new object[] { 33, 15, 33 }, 33)]
        [DataRow(new object[] { 100, 200, 50 }, 200)]
        [DataRow(new object[] { -10, -20, 0 }, 0)]
        [DataRow(new object[] { 0 }, 0)]
        [DataRow(new object[] { 49 }, 49)]
        [DataRow(new object[] { -49, -49 }, -49)]
        [DataRow(new object[] { 0, 0 }, 0)]
        [DataRow(new object[] { 49, 0 }, 49)]
        [DataRow(new object[] { -49, 0 }, 0)]
        [DataRow(new object[] { 3 }, 3)]
        public void MaxFunc_ValidInputs_ReturnsExpectedMaximum(object[] inputValues, int expectedMaximum)
        {
            List<DataType> dataTypes = new List<DataType>();
            foreach (var value in inputValues)
            {
                dataTypes.Add(new IntDataType((int)value));
            }

            var result = combgen.InternalFunctions.InternalFunctions.Max(dataTypes).GetObject();

            Assert.AreEqual(expectedMaximum, result);
        }
    }

}