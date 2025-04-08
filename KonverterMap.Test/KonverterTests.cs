namespace KonverterMap.Test
{
    public class Usuario
    {
        public string Nome { get; set; }
        public string Sobrenome { get; set; }
        public string Senha { get; set; }
        public string Email { get; set; }
        public DateTime Nascimento { get; set; }
    }

    public class UsuarioDto
    {
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
    }

    [TestClass]
    public class KonverterTests
    {
        [TestInitialize]
        public void Setup()
        {
            Konverter.Instance.CreateMap<Usuario, UsuarioDto>()
                .ForMember(p => p.NomeCompleto, u => $"{u.Nome} {u.Sobrenome}")
                .Ignore(p => p.Senha)
                .When(p => p.Email, u => !string.IsNullOrEmpty(u.Email))
                .ReverseMap();
        }

        [TestMethod]
        public void Map_SimpleProperties_ShouldMapSuccessfully()
        {
            var usuario = new Usuario { Nome = "Ana", Sobrenome = "Silva", Email = "ana@email.com", Senha = "12345678" };
            var dto = Konverter.Instance.Map<Usuario, UsuarioDto>(usuario);

            Assert.AreEqual("Ana Silva", dto.NomeCompleto);
            Assert.AreEqual("ana@email.com", dto.Email);
        }

        [TestMethod]
        public void Map_WithCustomMapping_ShouldMapUsingDelegate()
        {
            var usuario = new Usuario { Nome = "João", Sobrenome = "Souza" };
            var dto = Konverter.Instance.Map<Usuario, UsuarioDto>(usuario);

            Assert.AreEqual("João Souza", dto.NomeCompleto);
        }

        [TestMethod]
        public void Map_WithIgnore_ShouldNotMapIgnoredProperty()
        {
            var usuario = new Usuario { Nome = "Carlos", Senha = "123456" };
            var dto = Konverter.Instance.Map<Usuario, UsuarioDto>(usuario);

            Assert.IsNull(dto.Senha);
        }

        [TestMethod]
        public void Map_WithCondition_ShouldMapOnlyWhenConditionTrue()
        {
            var usuarioComEmail = new Usuario { Email = "teste@email.com" };
            var usuarioSemEmail = new Usuario { Email = null };

            var dto1 = Konverter.Instance.Map<Usuario, UsuarioDto>(usuarioComEmail);
            var dto2 = Konverter.Instance.Map<Usuario, UsuarioDto>(usuarioSemEmail);

            Assert.AreEqual("teste@email.com", dto1.Email);
            Assert.IsNull(dto2.Email); 
        }

        [TestMethod]
        public void Map_WithReverseMap_ShouldWorkBidirectionally()
        {
            var dto = new UsuarioDto { Email = "email@teste.com" };
            var usuario = Konverter.Instance.Map<UsuarioDto, Usuario>(dto);

            Assert.AreEqual("email@teste.com", usuario.Email);
        }
    }
}
