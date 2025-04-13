# Example: User with Full Name

```csharp
Konverter.Instance.CreateMap<User, UserDto>()
    .ForMember(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
```

Useful when source has separate name fields and you want to concatenate.
