# Advanced Usage

## 🔁 Recursive Mapping

Use the second overload of `.ForMember` to call `.Map<T1, T2>()` within the expression:

```csharp
.ForMember(dest => dest.Profile, (src, map) =>
    src.Profile != null ? map.Map<Profile, ProfileDto>(src.Profile) : null
)
```

---

## 📋 Mapping Collections

```csharp
.ForMember(dest => dest.Items, (src, map) =>
    src.Items?.Select(i => map.Map<Item, ItemDto>(i)).ToList()
)
```

Supports lists, arrays and other `IEnumerable<>`.

---

## 🧠 Conditional Mapping

```csharp
.When(dest => dest.Status, src => src.IsActive && !string.IsNullOrEmpty(src.Status))
```

---

## ⚠️ Null Handling

`KonverterMap` is null-safe by default. But inside custom expressions, you should still check nulls manually to avoid exceptions.

```csharp
src.Address != null ? map.Map<Address, AddressDto>(src.Address) : null
```

---

## 🧩 Deep Nested Mapping

Supports multi-level mapping with recursive objects, like:

```csharp
Konverter.Instance.CreateMap<User, UserDto>()
    .ForMember(dest => dest.Manager, (src, map) => map.Map<User, UserDto>(src.Manager));
```

---

## 🚀 Combined Example with BeforeMap and AfterMap

```csharp
Konverter.Instance.CreateMap<User, UserDto>()
    .BeforeMap((src, dest) =>
    {
        if (string.IsNullOrWhiteSpace(src.FirstName))
            src.FirstName = "Anonymous";
    })
    .ForMember(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
    .AfterMap((src, dest) =>
    {
        dest.Tag = $"Welcome {dest.FullName}!";
    });
```

This setup normalizes the source, maps with custom logic, and finalizes with a post-processing step.
