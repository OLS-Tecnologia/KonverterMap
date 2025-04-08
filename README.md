# KonverterMap

![NuGet](https://img.shields.io/nuget/v/KonverterMap.svg) 
![License](https://img.shields.io/github/license/olstecnologia/KonverterMap)

> Uma alternativa leve, fluente e gratuita ao AutoMapper para .NET 8+ 🔄

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
    .ForMember(p => p.NomeCompleto, u => $"{u.Nome} {u.Sobrenome}")
    .Ignore(p => p.Senha)
    .When(p => p.Email, u => !string.IsNullOrWhiteSpace(u.Email))
    .ReverseMap();

var dto = Konverter.Instance.Map<Usuario, UsuarioDto>(usuario);
```

---

## 🔄 Comparativo com AutoMapper

| Recurso                | AutoMapper                    | KonverterMap ✅        |
|------------------------|-------------------------------|------------------------|
| API Fluente            | ✅                           | ✅                     |
| ForMember com Lambda   | ✅                           | ✅                     |
| ReverseMap             | ✅                           | ✅                     |
| Mapeamento Condicional | ✅                           | ✅                     |
| Ignore                 | ✅                           | ✅                     |
| Performance Alta       | ✅                           | ✅                     |
| Licença Livre          | ❌ (pagamento para empresas) | ✅ (MIT)    |

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

## 🙌 Contribuindo

Contribuições são muito bem-vindas! Sinta-se à vontade para abrir issues, forks e pull requests.

---

Criado com 💙 por [OLS Tecnologia](https://www.olstecnologia.com.br) e [Fábio de Oliveira Santos](https://github.com/olstecnologia).