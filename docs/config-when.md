# When

The `When` method allows conditional mapping: a destination property is mapped only if the condition returns true.

---

### ✅ Example 1: Map only if status is active

```csharp
.When(dest => dest.IsActive, src => src.Status == "ACTIVE")
```

---

### ✅ Example 2: Skip if value is null or empty

```csharp
.When(dest => dest.Description, src => !string.IsNullOrEmpty(src.Description))
```

---

### ✅ Example 3: Apply condition on numeric fields

```csharp
.When(dest => dest.Discount, src => src.Discount > 0)
```
