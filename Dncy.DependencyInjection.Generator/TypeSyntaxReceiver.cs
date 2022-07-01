using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dncy.DependencyInjection.Generator
{
    internal class TypeSyntaxReceiver : ISyntaxReceiver
    {

        public HashSet<TypeDeclarationSyntax> TypeDeclarationsWithAttributes { get; } = new();

        /// <inheritdoc />
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax declaration && declaration.AttributeLists.Any())
            {
                TypeDeclarationsWithAttributes.Add(declaration);
            }
        }
    }
}
