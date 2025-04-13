# AfterMap

Executes custom logic **after** mapping has completed.

```csharp
.AfterMap((src, dest) =>
{
    dest.FullName = $"{src.FirstName} {src.LastName}";
})
```
