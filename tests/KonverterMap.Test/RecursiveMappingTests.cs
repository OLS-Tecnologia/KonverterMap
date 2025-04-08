
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KonverterMap;
using System.Collections.Generic;
using System.Linq;

namespace KonverterMap.Test
{
    [TestClass]
    public class RecursiveMappingTests
    {
        public class Classificacao
        {
            public string Tipo { get; set; }
        }

        public class PessoaEndereco
        {
            public string Rua { get; set; }
        }

        public class Pessoa
        {
            public string Nome { get; set; }
            public Classificacao Classificacao { get; set; }
            public List<PessoaEndereco> PessoaEnderecos { get; set; }
        }

        [TestInitialize]
        public void Init()
        {
            Konverter.Instance.CreateMap<Classificacao, Classificacao>();
            Konverter.Instance.CreateMap<PessoaEndereco, PessoaEndereco>();
            Konverter.Instance.CreateMap<Pessoa, Pessoa>()
                .ForMember(p => p.Classificacao, (src, map) =>
                    src.Classificacao != null ? map.Map<Classificacao, Classificacao>(src.Classificacao) : null
                )
                .ForMember(p => p.PessoaEnderecos, (src, map) =>
                    src.PessoaEnderecos?.Select(pe => map.Map<PessoaEndereco, PessoaEndereco>(pe)).ToList()
                );
        }

        [TestMethod]
        public void Should_Map_Class_With_ForMember_And_Konverter_Map()
        {
            var origem = new Pessoa { Nome = "Ana", Classificacao = new Classificacao { Tipo = "A" } };
            var destino = Konverter.Instance.Map<Pessoa, Pessoa>(origem);

            Assert.IsNotNull(destino);
            Assert.AreEqual("Ana", destino.Nome);
            Assert.IsNotNull(destino.Classificacao);
            Assert.AreEqual("A", destino.Classificacao.Tipo);
        }

        [TestMethod]
        public void Should_Map_Nested_Object()
        {
            var origem = new Pessoa { Nome = "Carlos", Classificacao = new Classificacao { Tipo = "B" } };
            var destino = Konverter.Instance.Map<Pessoa, Pessoa>(origem);

            Assert.IsNotNull(destino.Classificacao);
            Assert.AreEqual("B", destino.Classificacao.Tipo);
        }

        [TestMethod]
        public void Should_Map_List_Of_Objects()
        {
            var origem = new Pessoa
            {
                Nome = "João",
                Classificacao = new Classificacao() { Tipo = "classificação" },
                PessoaEnderecos = new List<PessoaEndereco>
                {
                    new PessoaEndereco { Rua = "Rua A" },
                    new PessoaEndereco { Rua = "Rua B" }
                }
            };

            var destino = Konverter.Instance.Map<Pessoa, Pessoa>(origem);

            Assert.IsNotNull(destino.PessoaEnderecos);
            Assert.AreEqual(2, destino.PessoaEnderecos.Count);
            Assert.AreEqual("Rua A", destino.PessoaEnderecos[0].Rua);
        }

        [TestMethod]
        public void Should_Map_When_Nested_Object_Is_Null()
        {
            var origem = new Pessoa { Nome = "Lucas", Classificacao = null };
            var destino = Konverter.Instance.Map<Pessoa, Pessoa>(origem);

            Assert.IsNull(destino.Classificacao);
        }

        [TestMethod]
        public void Should_Map_Empty_List()
        {
            var origem = new Pessoa { Nome = "Julia", PessoaEnderecos = new List<PessoaEndereco>() };
            var destino = Konverter.Instance.Map<Pessoa, Pessoa>(origem);

            Assert.IsNotNull(destino.PessoaEnderecos);
            Assert.AreEqual(0, destino.PessoaEnderecos.Count);
        }
    }
}
