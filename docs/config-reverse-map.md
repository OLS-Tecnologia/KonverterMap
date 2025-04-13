# ReverseMap

`.ReverseMap()` automatically registers the reverse mapping, inverting source and destination types.

---

### ✅ Example

```csharp
Konverter.Instance.CreateMap<User, UserDto>()
                 .ReverseMap();
```

---

### 🔁 Useful for:

- DTO <-> Entity
- FormModel <-> Model
- ViewModel <-> Domain
