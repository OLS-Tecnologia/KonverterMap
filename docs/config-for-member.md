# ForMember

The `ForMember` method lets you control how a specific destination property is populated.

---

### ✅ Example 1: Concatenated fields

```csharp
.ForMember(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
```

---

### ✅ Example 2: Mapping nested objects

```csharp
.ForMember(dest => dest.Address, (src, map) =>
    map.Map<Address, AddressDto>(src.Address))
```

---

### ✅ Example 3: Mapping collections

```csharp
.ForMember(dest => dest.Products, (src, map) =>
    src.Products?.Select(p => map.Map<Product, ProductDto>(p)).ToList())
```
