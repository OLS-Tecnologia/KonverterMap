using KonverterMap.Utils;

namespace KonverterMap.Test
{
    [TestClass]
    public class ExceptionTest
    {
        [TestMethod]
        public void MapObject_ShouldThrowException_WhenPropertyTypesAreIncompatible()
        {
            var source = new ClasseOrigemInvalida { Id = "abc" }; 

            try
            {
                var result = Konverter.Instance.Map<ClasseOrigemInvalida, ClasseDestinoInvalido>(source);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (KonverterMappingException ex)
            {
                StringAssert.Contains(ex.Message, "Property: Id");
                StringAssert.Contains(ex.Message, "SourceType: System.String");
                StringAssert.Contains(ex.Message, "DestinationType: System.Int32");
            }
        }

        [TestMethod]
        public void MapObject_ShouldMapSuccessfully_WhenPropertyTypesAreCompatible()
        {
            var source = new ClasseOrigemValida { Id = 42 };

            var result = Konverter.Instance.Map<ClasseOrigemValida, ClasseDestinoValido>(source);

            Assert.IsNotNull(result);
            Assert.AreEqual(42, result.Id);
        }
    }

    public class ClasseOrigemInvalida
    {
        public string Id { get; set; } = string.Empty;
    }

    public class ClasseDestinoInvalido
    {
        public int Id { get; set; }
    }

    public class ClasseOrigemValida
    {
        public int Id { get; set; }
    }

    public class ClasseDestinoValido
    {
        public int Id { get; set; }
    }
}
