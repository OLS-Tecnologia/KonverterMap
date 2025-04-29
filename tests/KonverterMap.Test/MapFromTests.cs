using KonverterMap.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonverterMap.Test
{
    public class Cidade
    {
        public string Nome { get; set; }
        public ICollection<Filial>? Filiais { get; set; }
    }

    public class CidadeDTO
    {
        public string Nome { get; set; }
    }

    public class Empresa
    {
        public string RazaoSocial { get; set; }
    }

    public class EmpresaDTO
    {
        public string RazaoSocial { get; set; }
    }

    public class Filial
    {
        public int Id { get; set; }
        public Cidade Cidade { get; set; }
        public Empresa Empresa { get; set; }
    }

    public class FilialDTO
    {
        public int Id { get; set; }
        public CidadeDTO Cidade { get; set; }
        public EmpresaDTO Empresa { get; set; }
    }

    [TestClass]
    public class MapFromTests
    {
        [TestInitialize]
        public void Setup()
        {
            Konverter.Instance.CreateMap<Cidade, CidadeDTO>().ReverseMap();
            Konverter.Instance.CreateMap<Empresa, EmpresaDTO>().ReverseMap();

            Konverter.Instance.CreateMap<Filial, FilialDTO>()
                .ForMember(dest => dest.Cidade, Map.MapFrom<Filial, Cidade, CidadeDTO>(src => src.Cidade))
                .ForMember(dest => dest.Empresa,
                    Map.MapFrom<Filial, Empresa, EmpresaDTO>(src => src.Empresa))
                .ReverseMap();
        }

        [TestMethod]
        public void Should_Map_Using_MapFrom_Helper()
        {
            var filial = new Filial
            {
                Id = 1,
                Cidade = new Cidade { Nome = "São Paulo" },
                Empresa = new Empresa { RazaoSocial = "OLS Tecnologia" }
            };

            var dto = Konverter.Instance.Map<Filial, FilialDTO>(filial);

            Assert.AreEqual("São Paulo", dto.Cidade.Nome);
            Assert.AreEqual("OLS Tecnologia", dto.Empresa.RazaoSocial);

            var back = Konverter.Instance.Map<FilialDTO, Filial>(dto);
            Assert.AreEqual("São Paulo", back.Cidade.Nome);
            Assert.AreEqual("OLS Tecnologia", back.Empresa.RazaoSocial);
        }
    }
}
