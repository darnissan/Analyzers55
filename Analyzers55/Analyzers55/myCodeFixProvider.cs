using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Sample.Analyzers;

namespace Analyzers55;

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(myCodeFixProvider)), Shared]
public class myCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = 
        ImmutableArray.Create(MyAnalyzer.DiagnosticId);
    public override FixAllProvider? GetFixAllProvider() => null;
    
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the syntax node representing the identifier that needs fixing
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var identifierNode = root?.FindNode(diagnosticSpan);
        if (identifierNode == null )
            return;

        // Register a code action to fix the naming violation
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Fix naming convention",
                createChangedSolution: c => FixNamingAsync(context.Document, identifierNode, c),
                equivalenceKey: "FixNamingConvention"),
            diagnostic);
        
 
    }
    private async Task<Solution> FixNamingAsync(Document document, SyntaxNode identifierNode, CancellationToken cancellationToken)
    {
  
        
        // Obtain the symbol associated with the identifier
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var symbol = semanticModel?.GetDeclaredSymbol(identifierNode, cancellationToken);

        if (symbol == null)
        {
            return document.Project.Solution;
        }

        // Generate the correct name based on your naming policy
        var newName = GenerateCorrectName(symbol);
        
       
        // Use Roslyn's Renamer to apply the new name across the solution
        var solution = document.Project.Solution;
        var optionSet = solution.Workspace.Options;
        var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

        return newSolution;
    }
    
    
    private string GenerateCorrectName(ISymbol symbol)
    {
        var originalName = symbol.Name;

        // Remove invalid characters (characters that are not letters, numbers, or underscores)
        var validChars = originalName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray();
        var cleanedName = new string(validChars);

        // Split the name into words based on casing and underscores
        var words = SplitIntoWords(cleanedName);

        // Apply naming conventions based on symbol kind
        switch (symbol)
        {
            case IMethodSymbol _:
            case INamedTypeSymbol _:
                // UpperCamelCase
                return string.Concat(words.Select(UppercaseFirstLetter));
            case ILocalSymbol _:
                // lowerCamelCase
                return LowercaseFirstLetter(string.Concat(words.Select(UppercaseFirstLetter)));
            case IFieldSymbol fieldSymbol when fieldSymbol.IsConst:
                // SNAKE_CASE
                return string.Join("_", words).ToUpperInvariant();
            default:
                return cleanedName; // Fallback to cleaned name if symbol kind is unhandled
        }
    }
    private IEnumerable<string> SplitIntoWords(string name)
    {
        var words = new List<string>();
        var sb = new StringBuilder();

        for (int i = 0; i < name.Length; i++)
        {
            var currentChar = name[i];
            var nextChar = i + 1 < name.Length ? name[i + 1] : '\0';

            sb.Append(currentChar);

            bool isEndOfWord = false;

            if (char.IsUpper(currentChar))
            {
                if (char.IsLower(nextChar))
                {
                    // Transition from uppercase to lowercase (e.g., 'T' in 'Test')
                    isEndOfWord = false;
                }
                else if (char.IsUpper(nextChar))
                {
                    // Consecutive uppercase letters (part of an acronym)
                    isEndOfWord = false;
                }
                else
                {
                    isEndOfWord = true;
                }
            }
            else if (char.IsLower(currentChar))
            {
                if (char.IsUpper(nextChar))
                {
                    // Transition from lowercase to uppercase
                    isEndOfWord = true;
                }
                else
                {
                    isEndOfWord = false;
                }
            }
            else if (currentChar == '_')
            {
                isEndOfWord = true;
            }
            else if (char.IsDigit(currentChar))
            {
                if (!char.IsDigit(nextChar))
                {
                    isEndOfWord = true;
                }
            }

            if (isEndOfWord)
            {
                if (sb.Length > 0)
                {
                    words.Add(sb.ToString().Trim('_'));
                    sb.Clear();
                }
            }
        }

        if (sb.Length > 0)
        {
            words.Add(sb.ToString().Trim('_'));
        }

        return words.Where(w => !string.IsNullOrEmpty(w));
    }

    private string UppercaseFirstLetter(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;
        return char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant();
    }

    private string LowercaseFirstLetter(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;
        return char.ToLowerInvariant(word[0]) + word.Substring(1);
    }
}