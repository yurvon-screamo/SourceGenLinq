# SourceGenLinq

Simple implementation of flexible parameterized sorting for IQuariable using IncrementalGenerator.

## Usage

1. Install library:

```xml
  <ItemGroup>
    <PackageReference Include="SourceGenLinq.Abstractions" Version="0.0.5" />
    <PackageReference Include="SourceGenLinq.Generator" Version="0.0.5" OutputItemType="Analyzer" ReferenceOutputAssembly="false"/>
  </ItemGroup>
```

1. Define DTO class:

```csharp
public class Tester
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
    public required string Comment { get; set; }
    public required double Test { get; set; }

    public QazWsx? QazWsx { get; set; }
}

public class QazWsx;
```

2. Define sorter with generic attribute `SortQuariable`:

```csharp
[SortQuariable<Tester>]
public static class MyTesterSorter;
```

3. Use:

```csharp
IQueryable<Tester> testers = new Tester[]
{
  new()
  {
      Id = Guid.NewGuid(),
      Name = "Test",
      Comment = "Test",
      Test = 22,
  },
  new()
  {
      Id = Guid.NewGuid(),
      Name = "Test 2",
      Comment = "Test 3",
      Test = 100,
  },
}.AsQueryable();

IQueryable<Tester> orderedTesters = testers.Sort([
  new TesterSortInput()
  {
      Name = SortMode.Asc,
  },
  new TesterSortInput()
  {
      Comment = SortMode.Asc,
  },
]);

```
