using KonverterMap.Utils;
using static KonverterMap.Test.RecursiveMappingTests;

namespace KonverterMap.Test
{
    [TestClass]
    public class PreventsCircularReferenceTests
    {
        [TestMethod]
        public void Should_Map_Entities_With_Circular_Reference_Without_StackOverflow()
        {
            var cidade = new Cidades();
            var uf = new Uf { Cidades = new List<Cidades> { cidade } };
            cidade.Uf = uf;

            Konverter.Instance.CreateMap<Uf, Uf>()
                .Ignore(u => u.Cidades);

            var result = Konverter.Instance.Map<Cidades, Cidades>(cidade);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Uf);
            Assert.IsNull(result.Uf!.Cidades);
        }

        [TestMethod]
        public void Should_Reuse_Already_Mapped_Instances()
        {
            var cidade = new Cidades { Nome = "Teste" };
            var endereco = new PessoaEndereco { Cidade = cidade };
            var pessoa = new Pessoa { PessoaEnderecos = new List<PessoaEndereco> { endereco, endereco } };

            var result = Konverter.Instance.Map<Pessoa, Pessoa>(pessoa);

            var cidade1 = result.PessoaEnderecos[0].Cidade;
            var cidade2 = result.PessoaEnderecos[1].Cidade;

            Assert.AreSame(cidade1, cidade2);
        }

        [TestMethod]
        public void Should_Not_Throw_When_Type_Has_SelfReference()
        {
            var node = new Node { Name = "Root" };
            node.Child = node;

            var result = Konverter.Instance.Map<Node, Node>(node);

            Assert.IsNotNull(result);
            Assert.AreSame(result, result.Child); 
        }

        [TestMethod]
        public void Should_Copy_List_When_Elements_Have_Same_Type()
        {
            var cidades = new List<Cidades> { new Cidades { Nome = "A" }, new Cidades { Nome = "B" } };

            var result = Konverter.Instance.Map<List<Cidades>, List<Cidades>>(cidades);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("A", result[0].Nome);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Should_Throw_When_ExtractElementType_From_Invalid_Type()
        {
            var _ = ReflectionUtils.ExtractElementType(typeof(string)); // not a collection
        }

        [TestMethod]
        public void Should_Map_List_Of_Source_To_List_Of_Destination()
        {
            var produtos = new List<Produto>
            {
                new Produto { Nome = "P1" },
                new Produto { Nome = "P2" }
            };

            Konverter.Instance.CreateMap<Produto, ProdutoDTO>();

            var result = Konverter.Instance.Map<List<Produto>, List<ProdutoDTO>>(produtos);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("P1", result[0].Nome);
        }

        [TestMethod]
        public void Should_Assign_Same_Instance_When_Types_Are_Equal()
        {
            var produto = new Produto { Nome = "Original" };

            var result = Konverter.Instance.Map<Produto, Produto>(produto);

            Assert.AreEqual(produto.Nome, result.Nome);
            Assert.AreNotSame(produto, result); // cópia direta
        }


    }

    public class Cidades
    {
        public string Nome { get; set; }
        public Uf Uf { get; set; }
    }

    public class Uf
    {
        public List<Cidades> Cidades { get; set; }
    }

    public class PessoaEndereco
    {
        public Cidades Cidade { get; set; }
    }

    public class Pessoa
    {
        public List<PessoaEndereco> PessoaEnderecos { get; set; }
    }

    public class Produto { public string Nome { get; set; } }
    public class ProdutoDTO { public string Nome { get; set; } }

    public class Node
    {
        public string Name { get; set; }
        public Node Child { get; set; }
    }

}
