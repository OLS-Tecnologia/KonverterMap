using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonverterMap.Test
{
    [TestClass]
    public class BeforeAfterMapTests
    {
        public class Origem
        {
            public string Nome { get; set; }
            public int Idade { get; set; }
        }

        public class Destino
        {
            public string Nome { get; set; }
            public int Idade { get; set; }
            public string Mensagem { get; set; }
        }

        [TestInitialize]
        public void Init()
        {
            Konverter.Instance.CreateMap<Origem, Destino>()
                .BeforeMap((src, dest) =>
                {
                    if (string.IsNullOrWhiteSpace(src.Nome))
                        src.Nome = "Desconhecido";
                })
                .AfterMap((src, dest) =>
                {
                    dest.Mensagem = $"Usuário: {dest.Nome}, Idade: {dest.Idade}";
                });
        }

        [TestMethod]
        public void BeforeMap_ShouldSetNomeIfNull()
        {
            var origem = new Origem { Nome = null, Idade = 25 };
            var destino = Konverter.Instance.Map<Origem, Destino>(origem);

            Assert.AreEqual("Desconhecido", destino.Nome);
        }

        [TestMethod]
        public void AfterMap_ShouldSetMensagem()
        {
            var origem = new Origem { Nome = "Fábio", Idade = 35 };
            var destino = Konverter.Instance.Map<Origem, Destino>(origem);

            Assert.AreEqual("Usuário: Fábio, Idade: 35", destino.Mensagem);
        }

        [TestMethod]
        public void BeforeAndAfterMap_ShouldWorkTogether()
        {
            var origem = new Origem { Nome = "", Idade = 42 };
            var destino = Konverter.Instance.Map<Origem, Destino>(origem);

            Assert.AreEqual("Desconhecido", destino.Nome);
            Assert.AreEqual("Usuário: Desconhecido, Idade: 42", destino.Mensagem);
        }
    }
}
