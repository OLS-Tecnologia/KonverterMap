# CreateMap

`CreateMap<TSource, TDestination>()` defines the mapping between two types. Once registered, KonverterMap can automatically copy matching properties from source to destination.

### ✅ Basic Example

```csharp
Konverter.Instance.CreateMap<User, UserDto>();
```

If both classes have properties with the same name and compatible types, mapping is done automatically.

---

### 🧪 Example: Automatic Property Matching

```csharp
public class User { public string Name { get; set; } }
public class UserDto { public string Name { get; set; } }

Konverter.Instance.CreateMap<User, UserDto>();
```

---

### 🧩 Example: Nested Types + ForMember

```csharp
Konverter.Instance.CreateMap<User, UserDto>()
    .ForMember(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
```

You can chain `ForMember`, `Ignore`, and other configuration methods after `CreateMap`.
