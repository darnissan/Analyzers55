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
    
   private static readonly Regex WordSplitterRegex = new Regex(
    @"[A-Z]?[a-z]+|\d+|[A-Z]+(?![a-z])",
    RegexOptions.Compiled);

private string GenerateCorrectName(ISymbol symbol)
{
    var originalName = symbol.Name;

    // Remove invalid characters (characters that are not letters, numbers, or underscores)
    var validChars = originalName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray();
    var cleanedName = new string(validChars);

    // Split the name into words using regular expression
    var words = SplitIntoWordsIncludingNumbers(cleanedName);
    var wordsWithNoNumbers = SplitIntoWordsDroppingNumbers(cleanedName);

    // Apply naming conventions based on symbol kind
    string newName;
    switch (symbol)
    {
        case IMethodSymbol _:
        case INamedTypeSymbol _:
            // UpperCamelCase
            newName = SuitableClassMethodName(originalName);
            break;
        case ILocalSymbol _:
            // lowerCamelCase
            newName = SuitableLocalVarName(originalName);
            break;
        case IFieldSymbol fieldSymbol when fieldSymbol.IsConst:
            // SNAKE_CASE
            newName = SuitableGlobalConstVarName(originalName);
            break;
        default:
            newName = cleanedName; // Fallback to cleaned name if symbol kind is unhandled
            break;
    }



    return newName;
}

private string SuitableClassMethodName(string originalName)
{
    if (string.IsNullOrEmpty(originalName))
    {
        return "FixMe"; // Or you can throw an exception if preferred
    }
    
    // First, drop all invalid characters
    var validChars = originalName.Where(c => char.IsLetterOrDigit(c)).ToArray();
    var cleanedName = new string(validChars);
    
    // Capitalize the first letter and every letter after a digit
    char[] chars = new char[cleanedName.Length];

    // Capitalize the first character if it's a letter
    chars[0] = char.IsLetter(cleanedName[0]) ? char.ToUpper(cleanedName[0]) : cleanedName[0];

    // Process the rest of the characters
    for (int i = 1; i < cleanedName.Length; i++)
    {
        char currentChar = cleanedName[i];
        char previousChar = cleanedName[i - 1];

        if (char.IsLetter(currentChar) && char.IsDigit(previousChar))
        {
            // Capitalize if the previous character is a digit
            chars[i] = char.ToUpper(currentChar);
        }
        else
        {
            // Keep the character as is
            chars[i] = currentChar;
        }
    }

    // Create the transformed string
    string transformedName = new string(chars);

    // Return the transformed name
    return transformedName;
}


private string SuitableLocalVarName(string originalName)
{
    if (string.IsNullOrEmpty(originalName))
    {
        return "fixMe"; // Or you can throw an exception if preferred
    }
    
    // First, drop all invalid characters
    var validChars = originalName.Where(c => char.IsLetterOrDigit(c)).ToArray();
    var cleanedName = new string(validChars);
    
    // Capitalize the first letter and every letter after a digit
    char[] chars = new char[cleanedName.Length];

    // Capitalize the first character if it's a letter
    chars[0] = char.IsLetter(cleanedName[0]) ? char.ToLower(cleanedName[0]) : cleanedName[0];

    // Process the rest of the characters
    for (int i = 1; i < cleanedName.Length; i++)
    {
        char currentChar = cleanedName[i];
        char previousChar = cleanedName[i - 1];

        if (char.IsLetter(currentChar) && char.IsDigit(previousChar))
        {
            // Capitalize if the previous character is a digit
            chars[i] = char.ToUpper(currentChar);
        }
        else
        {
            // Keep the character as is
            chars[i] = currentChar;
        }
    }

    // Create the transformed string
    string transformedName = new string(chars);

    // Return the transformed name
    return transformedName;
}


private string SuitableGlobalConstVarName(string originalName)
{
    if (string.IsNullOrEmpty(originalName))
    {
        return "FIX_ME"; // Or you can throw an exception if preferred
    }
    
    // First, drop all invalid characters
    var validChars = originalName.Where(c => char.IsLetter(c) || c== '_').ToArray();
    
    var cleanedName = new string(validChars);
     cleanedName = Regex.Replace(cleanedName, "_+", "_");
     cleanedName = cleanedName.ToUpper();
     return cleanedName;
}



private IEnumerable<string> SplitIntoWordsIncludingNumbers(string name)
{
    var words = new List<string>();
    var currentWord = new StringBuilder();

    foreach (var c in name)
    {
        if (c == '_')
        {
            if (currentWord.Length > 0)
            {
                words.Add(currentWord.ToString());
                currentWord.Clear();
            }
        }
        else if (char.IsUpper(c))
        {
            if (currentWord.Length > 0)
            {
                words.Add(currentWord.ToString());
                currentWord.Clear();
            }
            currentWord.Append(c);
        }
        else
        {
            // Include lowercase letters and digits
            currentWord.Append(c);
        }
    }

    if (currentWord.Length > 0)
    {
        words.Add(currentWord.ToString());
    }

    return words;
}

private IEnumerable<string> SplitIntoWordsDroppingNumbers(string name)
{
    var words = new List<string>();
    var currentWord = new StringBuilder();

    foreach (var c in name)
    {
        if (c == '_')
        {
            if (currentWord.Length > 0)
            {
                words.Add(currentWord.ToString());
                currentWord.Clear();
            }
        }
        else if (char.IsUpper(c))
        {
            if (currentWord.Length > 0)
            {
                words.Add(currentWord.ToString());
                currentWord.Clear();
            }
            currentWord.Append(c);
        }
        else if (char.IsLower(c))
        {
            // Include lowercase letters
            currentWord.Append(c);
        }
        // Else ignore digits and other characters
    }

    if (currentWord.Length > 0)
    {
        words.Add(currentWord.ToString());
    }

    return words;
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

// Provided regular expressions for validation
private static readonly Regex UpperCamelCaseRegex = new Regex(@"^([A-Z][a-z]*\d*)+$", RegexOptions.Compiled);
private static readonly Regex lowerCamelCaseRegex = new Regex(@"^[a-z]+[A-Za-z0-9]*$", RegexOptions.Compiled);
private static readonly Regex SNAKE_CASE_REGEX = new Regex(@"^[A-Z]+(_[A-Z]+)*$", RegexOptions.Compiled);
}