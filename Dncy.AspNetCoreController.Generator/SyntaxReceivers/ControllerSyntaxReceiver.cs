using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dncy.AspNetCoreController.Generator.SyntaxReceivers;

public class ControllerSyntaxReceiver: ISyntaxReceiver
{
    public HashSet<TypeDeclarationSyntax> ControllerTypeDeclarationsWithAttributes { get; } = new();

    /// <inheritdoc />
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is TypeDeclarationSyntax declaration && declaration.AttributeLists.Any())
        {
            ControllerTypeDeclarationsWithAttributes.Add(declaration);
        }
    }
}