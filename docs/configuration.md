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

### 💡 Basic Example

```csharp
.ForMember(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
```

### 💡 Recursive Mapping

```csharp
.ForMember(dest => dest.Address, (src, map) =>
    map.Map<Address, AddressDto>(src.Address)
)
```

### 💡 List Mapping

```csharp
.ForMember(dest => dest.Orders, (src, map) =>
    src.Orders?.Select(o => map.Map<Order, OrderDto>(o)).ToList()
)
```

---

## 🙈 Ignore

Skips a property from being mapped.

```csharp
.Ignore(dest => dest.Password)
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

Equivalent to:

```csharp
Konverter.Instance.CreateMap<User, UserDto>();
Konverter.Instance.CreateMap<UserDto, User>();
```

---

## 🧩 BeforeMap

Executes a function before the mapping starts.

```csharp
.BeforeMap((src, dest) =>
{
    if (string.IsNullOrWhiteSpace(src.FirstName))
        src.FirstName = "Unknown";
});
```

Useful for:
- Normalizing data
- Assigning default values
- Preparing data structures

---

## 🎯 AfterMap

Executes a function after the mapping is completed.

```csharp
.AfterMap((src, dest) =>
{
    dest.FullName = $"{src.FirstName} {src.LastName}";
});
```

Useful for:
- Concatenating mapped values
- Populating derived fields
- Final adjustments
