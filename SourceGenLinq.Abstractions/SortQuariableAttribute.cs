namespace SourceGenLinq.Abstractions;

[AttributeUsage(AttributeTargets.Class)]
public class SortQuariableAttribute<T> : Attribute where T : class;
