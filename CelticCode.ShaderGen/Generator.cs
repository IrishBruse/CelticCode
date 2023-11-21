namespace CelticCode.ShaderGen;

using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class Generator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // retreive the populated receiver
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
        {
            return;
        }

        Compilation compilation = context.Compilation;

        // loop over the candidate fields, and keep the ones that are actually annotated
        List<ITypeSymbol> symbols = [];
        foreach (ClassDeclarationSyntax decl in receiver.ClassDeclarations)
        {
            SemanticModel model = compilation.GetSemanticModel(decl.SyntaxTree);
            if (model.GetDeclaredSymbol(decl, context.CancellationToken) is ITypeSymbol symbol)
            {
                symbols.Add(symbol);
            }
        }

        StringBuilder sb = new();
        foreach (ITypeSymbol symbol in symbols)
        {
            sb.AppendLine("// " + symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }
    }
}

/// <summary>
/// Created on demand before each generation pass
/// </summary>
internal class SyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> ClassDeclarations { get; } = [];

    /// <summary>
    /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
    /// </summary>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        // any field with at least one attribute is a candidate for property generation
        if (syntaxNode is ClassDeclarationSyntax decl)
        {
            ClassDeclarations.Add(decl);
        }
    }
}
