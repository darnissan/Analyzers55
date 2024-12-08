
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzers55
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NamingSyntacticAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CS236651";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.CS236651Title),
            Resources.ResourceManager, typeof(Resources));

        private static readonly Regex UpperCamelCaseRegex = new Regex(@"^([A-Z][a-z]*[0-9]*)+$", RegexOptions.Compiled);

        private static readonly Regex lowerCamelCaseRegex =
            new Regex(@"^[a-z]+[0-9]*([A-Z][a-z]*[0-9]*)*$", RegexOptions.Compiled);

        private static readonly Regex SNAKE_CASE_REGEX = new Regex(@"^[A-Z]+([_][A-Z]+)*$", RegexOptions.Compiled);

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(Resources.CS236651MessageFormat),
            Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(Resources.CS236651Description),
            Resources.ResourceManager, typeof(Resources));

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
            SymbolKind.Property, SymbolKind.Method, SymbolKind.Field);

        public ImmutableArray<SyntaxKind> SyntaxKinds =>
            ImmutableArray.Create(SyntaxKind.LocalDeclarationStatement);

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
                        context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Identifier.GetLocation(),
                            variableText));
                    }
                }
            }
        }

        private static void AnalyzeSymbolKinds(SymbolAnalysisContext context)
        {
            var symbolKind = context.Symbol.Kind;
            if (symbolKind == SymbolKind.NamedType && context.Symbol is INamedTypeSymbol namedTypeSymbol &&
                namedTypeSymbol.TypeKind == TypeKind.Class)
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

            if ((symbolKind == SymbolKind.Field && context.Symbol is IFieldSymbol fieldSymbol))
            {
                if ((fieldSymbol.IsConst && fieldSymbol.DeclaredAccessibility == Accessibility.Public )||
                    (fieldSymbol.IsStatic && fieldSymbol.IsReadOnly)) 
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
        
        
        






