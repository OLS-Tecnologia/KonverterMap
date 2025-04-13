# CreateMap

Registers a mapping between two types.

```csharp
Konverter.Instance.CreateMap<User, UserDto>();
```

If both types have properties with the same name and type, they are mapped automatically.
