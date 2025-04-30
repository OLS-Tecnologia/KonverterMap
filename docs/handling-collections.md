📘 Handling Collections and Preventing Cycles
markdown
Copiar
Editar
# Handling Collections and Preventing Cycles

KonverterMap automatically handles mapping for collection properties in a safe and efficient way. Starting from version X.X.X, important improvements were added to prevent infinite loops and preserve performance during recursive mapping.

## ✅ Collection Detection

The internal method `IsCollection(Type type)` recognizes the following types as collections:

- `List<T>`
- `IEnumerable<T>`
- `ICollection<T>`
- `HashSet<T>`
- Arrays (`T[]`)
- `ObservableCollection<T>`, and similar

> ❗ `string` and `ArrayList` are **excluded** to avoid false positives.

## ✅ Element Type Extraction

The method `ExtractElementType(Type collectionType)` safely extracts the element type from generic collections or arrays.

If the provided type is not a recognized collection, an `InvalidOperationException` is thrown.

## 🔁 Cycle Prevention

KonverterMap internally uses a dictionary (`alreadyInitializedObjects`) to:

- Track which objects have already been mapped
- Temporarily mark objects being processed as `null!` to detect recursion
- Prevent infinite loops in circular references such as:

```csharp
Pessoa → PessoaEndereco → Cidade → Uf → Cidades → PessoaEndereco
🔄 Direct Copy for Identical Types
If the source and destination property types are exactly the same, KonverterMap will assign the object directly instead of recursively mapping it again.

This improves performance and reduces overhead.

🧠 Example
csharp
Copiar
Editar
public class PessoaEndereco
{
    public Cidade Cidade { get; set; }
}

public class Cidade
{
    public Uf Uf { get; set; }
}

public class Uf
{
    public ICollection<Cidade> Cidades { get; set; } // circular reference!
}
When mapping PessoaEndereco, KonverterMap will safely avoid remapping the same circular path Cidade → Uf → Cidades, preventing StackOverflowException.