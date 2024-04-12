using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SourceGenLinq.Generator;

[Generator]
public class OrderByIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<Target> valuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (p, _) => IsSyntaxTargetForGeneration(p),
                transform: (p, _) => GetSemanticTarget(p))
            .Where(t => t.DataType is not null);

        context.RegisterSourceOutput(valuesProvider, (c, p) =>
        {
            if (p.DataType is null || p.NamespacePrefix is null)
            {
                return;
            }

            string identity = p.DataType;

            string orderSource = string.Empty;
            string orderTypeSource = string.Empty;

            foreach (string propertyName in p.PropertyNames)
            {
                orderTypeSource += $"\n     public global::SourceGenLinq.Abstractions.SortMode? {propertyName} {{ get; set; }}";

                orderSource += $@"
            if (input.{propertyName} is not null)
            {{
                if (input.{propertyName} is global::SourceGenLinq.Abstractions.SortMode.Asc) {{
                    sources = sources.OrderBy(t => t.{propertyName});
                }}
                else 
                {{
                    sources = sources.OrderByDescending(t => t.{propertyName});
                }}

                continue;
            }}
";
            }

            string source = p.NamespacePrefix == string.Empty
                ? string.Empty
                : $"namespace {p.NamespacePrefix};\n\n";

            source += @$"
public class {identity}SortInput {{
{orderTypeSource}
}}
";

            source += $@"
public static partial class {identity}Sorter
{{
    public static global::System.Linq.IQueryable<{identity}> Sort(this global::System.Linq.IQueryable<{identity}> sources, global::System.Collections.Generic.IReadOnlyList<{identity}SortInput> inputs)
    {{
        foreach ({identity}SortInput input in inputs)
        {{
            {orderSource}
        }}

        return sources;
    }}
}}
";

            string file;

            if (p.NamespacePrefix == string.Empty)
            {
                file = p.DataType;
            }
            else
            {
                file = p.NamespacePrefix + '.' + p.DataType;
            }

            c.AddSource($"{file}.Generated.cs", source);
        });
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclaration &&
            classDeclaration.AttributeLists.Any();
    }

    private static Target GetSemanticTarget(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax classDeclaration)
        {
            foreach (AttributeListSyntax listSyntax in classDeclaration.AttributeLists)
            {
                foreach (AttributeSyntax attribute in listSyntax.Attributes)
                {
                    TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(attribute);

                    if (typeInfo.Type?.Name == "SortQuariableAttribute")
                    {
                        IEnumerable<GenericNameSyntax> childNodes = attribute
                            .ChildNodes()
                            .OfType<GenericNameSyntax>();

                        foreach (GenericNameSyntax node in childNodes)
                        {
                            TypeArgumentListSyntax argumentListSyntax = node.TypeArgumentList;

                            foreach (TypeSyntax typeSyntax in argumentListSyntax.Arguments)
                            {
                                ITypeSymbol? typeSymbol = context
                                    .SemanticModel
                                    .GetTypeInfo(typeSyntax)
                                    .Type;

                                if (typeSymbol is null)
                                {
                                    continue;
                                }

                                INamespaceSymbol namespaceSymbol = typeSymbol.ContainingNamespace;

                                IEnumerable<IPropertySymbol> propertySymbols = typeSymbol
                                    .GetMembers()
                                    .OfType<IPropertySymbol>();

                                List<string> propertyNames = new();

                                foreach (IPropertySymbol property in propertySymbols)
                                {
                                    if (property.IsStatic || property.IsWriteOnly)
                                    {
                                        continue;
                                    }

                                    ITypeSymbol propertyType = property.Type;

                                    if (propertyType.IsValueType || propertyType.Name == nameof(String))
                                    {
                                        propertyNames.Add(property.Name);
                                    }
                                }

                                string namespacePrefix =
                                    namespaceSymbol.IsGlobalNamespace
                                    ? string.Empty
                                    : namespaceSymbol.ToDisplayString();

                                return new(
                                    typeSymbol.Name,
                                    namespacePrefix,
                                    propertyNames.ToImmutableArray());
                            }
                        }
                    }
                }
            }
        }

        return new(null, null, default);
    }

    private readonly struct Target(string? dataType, string? namespacePrefix, ImmutableArray<string> propertyNames)
    {
        public string? DataType { get; } = dataType;
        public string? NamespacePrefix { get; } = namespacePrefix;
        public ImmutableArray<string> PropertyNames { get; } = propertyNames;
    }
}
