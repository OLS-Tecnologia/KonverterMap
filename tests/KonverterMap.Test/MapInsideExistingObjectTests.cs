using KonverterMap.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonverterMap.Test
{
    [TestClass]
    public class MapInsideExistingObjectTests
    {
        public class Pessoa
        {
            public int Id { get; set; }
            public string? Nome { get; set; }
            public string? Codigo { get; set; }
            public List<Endereco>? Enderecos { get; set; }
        }

        public class Endereco
        {
            public int Id { get; set; }
            public string? Rua { get; set; }
        }

        [TestInitialize]
        public void Setup()
        {
            Konverter.Instance.Reset();
            Konverter.Instance.CreateMap<Pessoa, Pessoa>();
            Konverter.Instance.CreateMap<Endereco, Endereco>();
        }

        [TestMethod]
        public void MapInto_ShouldUpdateFieldsInExistingObject()
        {
            var destino = new Pessoa
            {
                Id = 1,
                Nome = "Antigo",
                Codigo = "ABC123",
                Enderecos = new List<Endereco>
                {
                    new Endereco { Id = 1, Rua = "Rua Antiga" }
                }
            };

            var origem = new Pessoa
            {
                Id = 1,
                Nome = "Atualizado",
                Codigo = "DEF456",
                Enderecos = new List<Endereco>
                {
                    new Endereco { Id = 2, Rua = "Rua Nova" }
                }
            };

            Konverter.Instance.MapInto(origem, destino);

            Assert.AreEqual("Atualizado", destino.Nome);
            Assert.AreEqual("DEF456", destino.Codigo);
            Assert.IsNotNull(destino.Enderecos);
            Assert.AreEqual(1, destino.Enderecos!.Count);
            Assert.AreEqual("Rua Nova", destino.Enderecos[0].Rua);
        }

        [TestMethod]
        public void MapInto_ShouldThrow_WhenSourceIsNull()
        {
            Pessoa? origem = null;
            var destino = new Pessoa { Nome = "Teste" };

            Assert.ThrowsException<System.ArgumentNullException>(() =>
            {
                Konverter.Instance.MapInto(origem!, destino);
            });
        }

        [TestMethod]
        public void MapInto_ShouldThrow_WhenDestinationIsNull()
        {
            var origem = new Pessoa { Nome = "Origem" };
            Pessoa? destino = null;

            Assert.ThrowsException<System.ArgumentNullException>(() =>
            {
                Konverter.Instance.MapInto(origem, destino!);
            });
        }

        [TestMethod]
        public void MapInto_ShouldRespect_IgnoreConfiguration()
        {
            Konverter.Instance.CreateMap<Pessoa, Pessoa>()
                .Ignore(p => p.Codigo);

            var destino = new Pessoa
            {
                Id = 1,
                Nome = "Antes",
                Codigo = "NAO_DEVE_SER_SOBRESCRITO"
            };

            var origem = new Pessoa
            {
                Id = 1,
                Nome = "Depois",
                Codigo = "DEVE_SER_IGNORADO"
            };

            Konverter.Instance.MapInto(origem, destino);

            Assert.AreEqual("Depois", destino.Nome);
            Assert.AreEqual("NAO_DEVE_SER_SOBRESCRITO", destino.Codigo); // não mudou
        }

        [TestMethod]
        public void MapInto_ShouldApply_BeforeMapAction()
        {
            Konverter.Instance.CreateMap<Pessoa, Pessoa>()
                .Ignore(p => p.Nome)
                .BeforeMap((src, dest) => dest.Nome = "ForçadoAntes");

            var destino = new Pessoa
            {
                Id = 1,
                Nome = "Inicial"
            };

            var origem = new Pessoa
            {
                Id = 1,
                Nome = "Origem"
            };

            Konverter.Instance.MapInto(origem, destino);

            Assert.AreEqual("ForçadoAntes", destino.Nome);
        }


        [TestMethod]
        public void MapInto_ShouldApply_AfterMapAction()
        {
            Konverter.Instance.CreateMap<Pessoa, Pessoa>()
                .AfterMap((src, dest) => dest.Codigo = "AlteradoDepois");

            var destino = new Pessoa
            {
                Id = 1,
                Nome = "Antigo",
                Codigo = "Original"
            };

            var origem = new Pessoa
            {
                Id = 1,
                Nome = "Novo",
                Codigo = "NovoCodigo"
            };

            Konverter.Instance.MapInto(origem, destino);

            Assert.AreEqual("Novo", destino.Nome);                 // veio da origem
            Assert.AreEqual("AlteradoDepois", destino.Codigo);     // sobrescrito no AfterMap
        }

    }
}
