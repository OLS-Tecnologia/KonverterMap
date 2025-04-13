# AfterMap

`AfterMap` runs after all mapping is completed. It is useful for setting derived fields or final adjustments.

---

### ✅ Example 1: Set combined field

```csharp
.AfterMap((src, dest) =>
{
    dest.FullName = $"{src.FirstName} {src.LastName}";
});
```

---

### ✅ Example 2: Audit note

```csharp
.AfterMap((src, dest) =>
{
    dest.Notes = $"Mapped on {DateTime.Now}";
});
```

---

### ✅ Example 3: Derived calculations

```csharp
.AfterMap((src, dest) =>
{
    dest.IsAdult = src.Age >= 18;
});
```
