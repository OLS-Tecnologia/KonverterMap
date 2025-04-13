# Example: Mapping Order List

```csharp
Konverter.Instance.CreateMap<Customer, CustomerDto>()
    .ForMember(dest => dest.Orders, (src, map) =>
        src.Orders?.Select(o => map.Map<Order, OrderDto>(o)).ToList());
```

Automatically maps a list of child objects using the existing registered mapping.
