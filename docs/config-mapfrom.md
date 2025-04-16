
# Using `MapFrom` for Recursive and Custom Mapping

The `MapFrom` helper allows you to reuse existing mappings inside another mapping configuration.
It's especially useful for nested objects and collections, and provides functionality similar to AutoMapper's `context.Mapper.Map(...)`.

## 🧠 What does it do?

It gives you access to the Konverter instance within a `ForMember` call, so you can call:

```csharp
konverter.Map<TSource, TDestination>(source)
```

inside a mapping delegate.

---

## ✅ How to use

### Option 1: Using `using static`

To enable direct use of `MapFrom` without the `Map.` prefix:

```csharp
using static KonverterMap.Map;

Konverter.Instance.CreateMap<Filial, FilialDTO>()
    .ForMember(dest => dest.Cidade,
        MapFrom<Filial, Cidade, CidadeDTO>(src => src.Cidade))
    .ForMember(dest => dest.Empresa,
        MapFrom<Filial, Empresa, EmpresaDTO>(src => src.Empresa));
```

---

### Option 2: Without `using static`

Use the full prefix when not importing the helper statically:

```csharp
Konverter.Instance.CreateMap<Filial, FilialDTO>()
    .ForMember(dest => dest.Cidade,
        Map.MapFrom<Filial, Cidade, CidadeDTO>(src => src.Cidade))
    .ForMember(dest => dest.Empresa,
        Map.MapFrom<Filial, Empresa, EmpresaDTO>(src => src.Empresa));
```

---

## 🔄 ReverseMap compatibility

If you call `.ReverseMap()` and you’ve already configured:

```csharp
CreateMap<Cidade, CidadeDTO>().ReverseMap();
CreateMap<Empresa, EmpresaDTO>().ReverseMap();
```

The `MapFrom` references will work automatically in both directions.

---

## 🧪 Full working example

```csharp
Konverter.Instance.CreateMap<Cidade, CidadeDTO>().ReverseMap();
Konverter.Instance.CreateMap<Empresa, EmpresaDTO>().ReverseMap();

Konverter.Instance.CreateMap<Filial, FilialDTO>()
    .ForMember(dest => dest.Cidade,
        Map.MapFrom<Filial, Cidade, CidadeDTO>(src => src.Cidade))
    .ForMember(dest => dest.Empresa,
        Map.MapFrom<Filial, Empresa, EmpresaDTO>(src => src.Empresa))
    .ReverseMap();
```

This enables recursive, clean, and safe mapping for nested objects.

