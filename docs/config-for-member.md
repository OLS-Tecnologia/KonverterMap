# ForMember

Allows custom mapping logic for a specific property.

```csharp
.ForMember(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
```

Also supports recursive or nested mapping:

```csharp
.ForMember(dest => dest.Address, (src, map) => map.Map<Address, AddressDto>(src.Address))
```
