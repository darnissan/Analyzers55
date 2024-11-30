
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sample.Analyzers
{
    /// <summary>
    /// Analyzer for reporting syntax node diagnostics.
    /// It reports diagnostics for implicitly typed local variables, recommending explicit type specification.
    /// </summary>
    /// <remarks>
    /// For analyzers that requires analyzing symbols or syntax nodes across compilation, see <see cref="CompilationStartedAnalyzer"/> and <see cref="CompilationStartedAnalyzerWithCompilationWideAnalysis"/>.
    /// For analyzers that requires analyzing symbols or syntax nodes across a code block, see <see cref="CodeBlockStartedAnalyzer"/>.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALERRRRRRRTTTTTTTTTTTTTTTTTTTTTTTTTTTT";
        private const string Title = "Declare explicit type for local declarations.";
        private static readonly Regex UpperCamelCaseRegex = new Regex(@"^([A-Z][a-z]*[0-9]*)+$", RegexOptions.Compiled);
        private static readonly Regex lowerCamelCaseRegex = new Regex(@"^[a-z]+[0-9]*([A-Z][a-z]*[0-9]*)*$", RegexOptions.Compiled);
        private static readonly Regex SNAKE_CASE_REGEX = new Regex(@"^[A-Z]+([_][A-Z]+)*$", RegexOptions.Compiled);
        public const string MessageFormat =
            "Local '{0}' is implicitly typed. Consider specifying its type explicitly in the declaration.";

        private const string Description = "Declare explicit type for local declarations.";
        
        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                DiagnosticId,
                Title,
                MessageFormat,
                "Naming",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: Description);
        

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

         public ImmutableArray<SymbolKind> SymbolKinds => ImmutableArray.Create(SymbolKind.NamedType,
            SymbolKind.Property, SymbolKind.Method, SymbolKind.Local, SymbolKind.Field);

         public ImmutableArray<SyntaxKind> SyntaxKinds =>
             ImmutableArray.Create( SyntaxKind.LocalDeclarationStatement);
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbolKinds, SymbolKinds);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxKinds, SyntaxKinds);
        }

        private static void AnalyzeSyntaxKinds(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is LocalDeclarationStatementSyntax localDeclaration)
            {
                if (localDeclaration.Declaration.Variables.Count == 0) return;
                for (var index = 0; index < localDeclaration.Declaration.Variables.Count; index++)
                {
                    var variable = localDeclaration.Declaration.Variables[index];
                    var variableText = variable.Identifier.ValueText;
                    if (lowerCamelCaseRegex.IsMatch(variableText) == false)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Identifier.GetLocation(), variableText));
                    }
                }
            }
        }
        private static void AnalyzeIdentifierName(SyntaxNodeAnalysisContext context)
        {
            // Check if the node is an IdentifierNameSyntax
            if (!(context.Node is IdentifierNameSyntax identifierName))
                return;

            // Get the semantic model to analyze the symbol
            var semanticModel = context.SemanticModel;
            
            // Get the symbol for the identifier
            var symbol = semanticModel.GetSymbolInfo(identifierName).Symbol;
            
            // If no symbol found, return
            if (symbol == null)
                return;
            
            // Check if the symbol represents a type
            if (symbol is INamedTypeSymbol typeSymbol)
            {
                // Verify if it's a class type
                if (typeSymbol.TypeKind == TypeKind.Class)
                {
                    // Check naming convention
                    if (!UpperCamelCaseRegex.IsMatch(symbol.Name))
                    {
                        var diagnostic = Diagnostic.Create(
                            Rule, 
                            symbol.Locations[0], 
                            symbol.Name
                        );
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
        
        private static void AnalyzeSymbolKinds(SymbolAnalysisContext context)
        {
            var symbolKind = context.Symbol.Kind;
            if (symbolKind == SymbolKind.NamedType && context.Symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeKind == TypeKind.Class)
            {
                if (!UpperCamelCaseRegex.IsMatch(context.Symbol.Name))
                {
                    var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            if (symbolKind == SymbolKind.Method && context.Symbol is IMethodSymbol methodSymbol)
            {
                if (!UpperCamelCaseRegex.IsMatch(methodSymbol.Name))
                {
                    var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
            
            if ((symbolKind == SymbolKind.Field && context.Symbol is IFieldSymbol fieldSymbol) )
            {
                if (fieldSymbol.IsConst && fieldSymbol.DeclaredAccessibility == Accessibility.Public)
                {
                    if (!SNAKE_CASE_REGEX.IsMatch(fieldSymbol.Name))
                    {
                        var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
          
            }
        
            }
            
    }
        
        
        





    }
