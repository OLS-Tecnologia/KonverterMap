# BeforeMap

Executes custom logic **before** mapping begins.

```csharp
.BeforeMap((src, dest) =>
{
    src.Name = src.Name?.Trim();
})
```
