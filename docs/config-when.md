# When

Map a property only if a condition is met.

```csharp
.When(dest => dest.IsActive, src => src.Status == "ACTIVE")
```
