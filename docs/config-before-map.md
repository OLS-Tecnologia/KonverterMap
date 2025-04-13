# BeforeMap

`BeforeMap` allows you to run logic before the actual mapping happens.

---

### ✅ Example 1: Normalize strings

```csharp
.BeforeMap((src, dest) =>
{
    src.FirstName = src.FirstName?.Trim();
    src.LastName = src.LastName?.Trim();
});
```

---

### ✅ Example 2: Add default values

```csharp
.BeforeMap((src, dest) =>
{
    if (string.IsNullOrEmpty(src.Country))
        src.Country = "Brazil";
});
```

---

### ✅ Example 3: Initialize lists

```csharp
.BeforeMap((src, dest) =>
{
    src.Items ??= new List<Item>();
});
```
