# SourceGenLinq

Simple implementation of flexible parameterized sorting for IQuariable using IncrementalGenerator.

## Usage

> Tested on net8

0. Install library:

```xml
  <ItemGroup>
	<PackageReference Include="SourceGenLinq.Abstractions" Version="0.0.1" />
	<PackageReference Include="SourceGenLinq.Generator" Version="0.0.1" OutputItemType="Analyzer" ReferenceOutputAssembly="false"/>
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
public static partial class MyTesterSorter;
```

3. Use:

```csharp
IQueryable<Tester> testers = default!;

testers = testers.Sort(new()
{
    { MyTesterSorter.TesterProperty.IdProperty, SortMode.Asc },
    { MyTesterSorter.TesterProperty.CommentProperty, SortMode.Desc },
});
```
