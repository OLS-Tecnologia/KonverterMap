# Configuration

## 🛠 CreateMap

Registers a mapping between two types.

```csharp
Konverter.Instance.CreateMap<Source, Destination>();
```

If both types have properties with the same name and type, the mapping is automatic.

---

## 🧠 ForMember

Define a custom mapping for a specific property.

### 💡 Example 1 – basic logic

```csharp
.ForMember(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
```

### 💡 Example 2 – recursive mapping

```csharp
.ForMember(dest => dest.Address, (src, map) => map.Map<Address, AddressDto>(src.Address))
```

### 💡 Example 3 – mapping collections

```csharp
.ForMember(dest => dest.Orders, (src, map) =>
    src.Orders?.Select(o => map.Map<Order, OrderDto>(o)).ToList()
)
```

---

## 🙈 Ignore

Skips a property from being mapped.

```csharp
.Ignore(dest => dest.InternalCode)
```

---

## 🧪 When

Maps a property only if a condition is true.

```csharp
.When(dest => dest.Status, src => src.IsActive)
```

---

## 🔁 ReverseMap

Registers the reverse mapping automatically.

```csharp
Konverter.Instance.CreateMap<User, UserDto>().ReverseMap();
```

This is equivalent to:

```csharp
Konverter.Instance.CreateMap<User, UserDto>();
Konverter.Instance.CreateMap<UserDto, User>();
```
