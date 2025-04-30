using KonverterMap.Utils;

namespace KonverterMap.Test
{
    [TestClass]
    public class ReflectionUtilsTests
    {
        [TestMethod]
        public void IsCollection_ShouldReturnTrue_ForList()
        {
            Assert.IsTrue(ReflectionUtils.IsCollection(typeof(List<string>)));
        }

        [TestMethod]
        public void IsCollection_ShouldReturnTrue_ForArray()
        {
            Assert.IsTrue(ReflectionUtils.IsCollection(typeof(string[])));
        }

        [TestMethod]
        public void IsCollection_ShouldReturnFalse_ForString()
        {
            Assert.IsFalse(ReflectionUtils.IsCollection(typeof(string)));
        }

        [TestMethod]
        public void IsCollection_ShouldReturnTrue_ForIEnumerable()
        {
            Assert.IsTrue(ReflectionUtils.IsCollection(typeof(IEnumerable<int>)));
        }
    }
}
