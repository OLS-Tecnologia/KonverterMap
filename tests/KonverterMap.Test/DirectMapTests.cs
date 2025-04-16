using KonverterMap.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonverterMap.Test
{
    [TestClass]
    public class DirectMapTests
    {
        [TestInitialize]
        public void Setup()
        {
            Konverter.Instance.CreateMap<EntidadeA, EntidadeB>()
                .ForMember(dest => dest.Nome, src => src.Name)
                .ForMember(dest => dest.IncData, (src, map) => src.AddDate != null ? src.AddDate : DateTime.Now);
        }

        [TestMethod]
        public void TestDirectMap_WithoutPreConfig()
        {
            EntidadeA ent = new EntidadeA()
            {
                Id = 1,
                Name = "A",
                AddDate = null
            };

            var entB = Konverter.Instance.Map<EntidadeA, EntidadeB>(ent);
        }
    }

    public class EntidadeA
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? AddDate { get; set; }
    }

    public class EntidadeB
    {
        public string Nome { get; set; }
        public DateTime IncData { get; set;}
    }
}
