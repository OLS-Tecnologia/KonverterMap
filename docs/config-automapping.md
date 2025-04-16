
# Automatic Mapping Without Configuration

KonverterMap allows object-to-object mapping **without requiring a prior `CreateMap<,>()` configuration**.

This feature simplifies development when working with simple objects or during rapid prototyping.

---

## ✅ How it works

If the source and destination classes have:

- Properties with the **same name**
- And **compatible or convertible types**

Then KonverterMap can map them automatically.

---

## ✨ Example: Implicit Mapping

```csharp
var user = new User
{
    Name = "Fábio",
    Email = "fabio@example.com"
};

var dto = Konverter.Instance.Map<User, UserDto>(user);
// Even without CreateMap<User, UserDto>(), this works
```

---

## 🧠 When to use implicit mapping?

- For quick tests and prototypes
- When objects have matching names and types
- To reduce boilerplate in straightforward mappings

---

## ⚠️ What it won't do

Implicit mapping does **not support**:

- `.ForMember(...)` customizations
- `.Ignore(...)`
- `.When(...)`, `.BeforeMap(...)`, `.AfterMap(...)`
- `.ReverseMap()`

---

## ✅ Recommendation

Use implicit mapping for:

- Simple scenarios
- Fast prototyping

Use explicit mapping (`CreateMap<,>()`) for:

- Complex mappings
- Reusability and maintainability
- Full control of transformation logic
