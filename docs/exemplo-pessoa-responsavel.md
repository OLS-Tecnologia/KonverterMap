# Example: Recursive Pessoa → Responsável

```csharp
Konverter.Instance.CreateMap<Pessoa, Pessoa>()
    .ForMember(dest => dest.Responsavel, (src, map) =>
        src.Responsavel != null ? map.Map<Pessoa, Pessoa>(src.Responsavel) : null)
    .Ignore(dest => dest.InverseIdResponsavelNavigation);
```

Demonstrates recursion and ignoring back-reference.
