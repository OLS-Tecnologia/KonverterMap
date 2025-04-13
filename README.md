# KonverterMap

![NuGet](https://img.shields.io/nuget/v/KonverterMap.svg)
![Frameworks](https://img.shields.io/badge/.NET-Standard%202.0%20%7C%20.NET%208-blue)
![License](https://img.shields.io/github/license/olstecnologia/KonverterMap.svg)
![Build](https://github.com/olstecnologia/KonverterMap/actions/workflows/ci.yml/badge.svg)

**KonverterMap** é uma alternativa leve, poderosa e extensível ao AutoMapper.

✔️ Simples  
✔️ Performático  
✔️ Open source  
✔️ Compatível com .NET Standard 2.0 e .NET 8  


O **KonverterMap** é uma biblioteca open source de mapeamento de objetos com foco em simplicidade, performance e extensibilidade. Foi criada para atender às necessidades comuns de mapeamento de DTOs, ViewModels e entidades, com uma API fluente e fácil de usar.

---

## ✨ Principais Recursos

- `CreateMap<TSource, TDestination>()`
- `ForMember(...)` com expressões lambda
- `Ignore(...)` para propriedades específicas
- `When(...)` para mapeamento condicional
- `ReverseMap()` para gerar o mapeamento inverso
- Suporte a coleções (`List<T>`, `IEnumerable<T>`, etc)
- Sem reflexão pesada ou uso de IL — performance previsível e legível

---

## 📦 Instalação

Via [NuGet](https://www.nuget.org/packages/KonverterMap):

```bash
Install-Package KonverterMap
```

Ou via CLI:

```bash
dotnet add package KonverterMap
```

---

## 🚀 Exemplo Rápido

```csharp
Konverter.Instance
    .CreateMap<Usuario, UsuarioDto>()
    .ForMember(dest => dest.NomeCompleto, src => $"{src.Nome} {src.Sobrenome}")
    .Ignore(dest => dest.Senha);

var usuario = new Usuario { Nome = "João", Sobrenome = "Silva", Senha = "123" };
var dto = Konverter.Instance.Map<Usuario, UsuarioDto>(usuario);
```

---

## 🎯 Compatibilidade
|Framework |	Suporte
|.NET Standard 2.0 |	✅
|.NET Framework 4.6.1+ |	✅
|.NET 6, 7, 8 |	✅
|Xamarin / Mono |	✅


## 🔄 Comparativo com AutoMapper

| Recurso                | AutoMapper                    | KonverterMap ✅        |
|------------------------|-------------------------------|------------------------|
| API Fluente            | ✅                           | ✅                     |
| ForMember com Lambda   | ✅                           | ✅                     |
| ReverseMap             | ✅                           | ✅                     |
| Mapeamento Condicional | ✅                           | ✅                     |
| Ignore                 | ✅                           | ✅                     |
| Performance Alta       | ✅                           | ✅                     |
| Licença Livre          | ❌ (restrições)              | ✅ (MIT)               |
| AfterMap / BeforeMap	 | ✅	                       |✅            |

---

## 📁 Estrutura

```
KonverterMap.sln
├── src/              # Biblioteca principal
├── tests/            # Testes automatizados
└── README.md         # Este arquivo
```

---

## 🧪 Testes e Performance

- Testes automatizados com MSTest
- Teste de performance com 100 mil objetos
- Suporte a cobertura de código com Coverlet + ReportGenerator

---

## 📄 Licença

Este projeto está licenciado sob a Licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

## 🙌 Contribuição

Contribuições são muito bem-vindas! Sinta-se à vontade para abrir issues, forks e pull requests.

---

Criado com 💙 por [OLS Tecnologia](https://www.olstecnologia.com.br) e [Fábio de Oliveira Santos](https://github.com/olstecnologia).
