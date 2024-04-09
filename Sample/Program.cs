using SourceGenLinq.Abstractions;

using TesterSample;

Console.WriteLine("Hello, World!");

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

IQueryable<Tester> orderedTesters = testers.Sort(new()
{
    { TesterSort.TesterProperty.IdProperty, SortMode.Asc },
    { TesterSort.TesterProperty.CommentProperty, SortMode.Desc },
});

Console.WriteLine();

namespace TesterSample
{
    [SortQuariable<Tester>]
    public static partial class TesterSort2;

    public class Tester
    {
        public Guid Id { get; set; }

        public required string Name { get; set; }
        public required string Comment { get; set; }
        public required double Test { get; set; }

        public QazWsx? QazWsx { get; set; }
    }

    [SortQuariable<QazWsx>]
    public static partial class QazWsxSorter;

    public class QazWsx
    {
        public Guid Id { get; set; }
        public Guid Говно { get; set; }

        public required string Name { get; set; }
    }
}
