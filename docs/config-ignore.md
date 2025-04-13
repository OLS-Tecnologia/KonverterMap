# Ignore

Use `.Ignore(...)` to skip mapping a destination property.

---

### ✅ Example 1: Skipping an audit field

```csharp
.Ignore(dest => dest.CreatedDate)
```

---

### ✅ Example 2: Avoid mapping a navigation property

```csharp
.Ignore(dest => dest.ParentCategory)
```

---

### ✅ Example 3: Prevent cyclic reference

```csharp
.Ignore(dest => dest.InverseNavigation)
```
