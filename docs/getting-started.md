# Getting Started

## 📦 Installation

Install via NuGet:

```bash
dotnet add package KonverterMap
```

Or manually edit your `.csproj`:

```xml
<PackageReference Include="KonverterMap" Version="1.0.1" />
```

---

## 🚀 First Map

Suppose you have two classes:

```csharp
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class UserDto
{
    public string FullName { get; set; }
}
```

### 🔧 Setup the map

```csharp
Konverter.Instance.CreateMap<User, UserDto>()
    .ForMember(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
```

### ▶️ Execute mapping

```csharp
var user = new User { FirstName = "John", LastName = "Doe" };
var dto = Konverter.Instance.Map<User, UserDto>(user);
Console.WriteLine(dto.FullName); // John Doe
```
