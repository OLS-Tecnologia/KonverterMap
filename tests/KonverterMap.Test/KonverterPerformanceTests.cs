using System.Diagnostics;

namespace KonverterMap.Test
{
    [TestClass]
    public class KonverterPerformanceTests
    {
        private const int Volume = 100_000;

        [TestInitialize]
        public void Setup()
        {
            Konverter.Instance.CreateMap<Usuario, UsuarioDto>()
                .ForMember(p => p.NomeCompleto, u => $"{u.Nome} {u.Sobrenome}")
                .Ignore(p => p.Senha)
                .When(p => p.Email, u => !string.IsNullOrEmpty(u.Email));
        }

        [TestMethod]
        public void Map_HighVolume_ShouldPerformEfficiently()
        {
            var usuarios = new List<Usuario>();
            for (int i = 0; i < Volume; i++)
            {
                usuarios.Add(new Usuario
                {
                    Nome = "Nome" + i,
                    Sobrenome = "Sobrenome" + i,
                    Email = i % 2 == 0 ? $"email{i}@dominio.com" : null,
                    Senha = "1234",
                    Nascimento = DateTime.Now.AddYears(-20).AddDays(i)
                });
            }

            var stopwatch = Stopwatch.StartNew();

            var dtos = Konverter.Instance.Map<List<Usuario>, List<UsuarioDto>>(usuarios);

            stopwatch.Stop();
            Console.WriteLine($"Tempo para mapear {Volume} itens: {stopwatch.ElapsedMilliseconds} ms");

            Assert.AreEqual(Volume, dtos.Count);
            Assert.AreEqual("Nome0 Sobrenome0", dtos[0].NomeCompleto);
            Assert.IsNull(dtos[0].Senha);
        }
    }
}
