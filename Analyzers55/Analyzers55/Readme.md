# C# Linter for CS236651 Naming Conventions

This project is a simple C# linter that enforces the naming conventions specified in the CS236651 course at the Technion. It focuses on identifier names and provides code fixes to ensure compliance with the defined policies.

*Authored by Dar Nissan - [dar.nissan@campus.technion.ac.il](mailto:dar.nissan@campus.technion.ac.il)*

---

## Naming Conventions Policy

- **Allowed Characters**: Identifiers may only contain lowercase letters, uppercase letters, numbers, or underscores. No other characters are permitted.
- **Class and Method Names**: Should be written in **UpperCamelCase**. This means multiple words are concatenated without spaces, and each word starts with an uppercase letter followed by lowercase letters. Underscores are not allowed.
- **Local Variable Names**: Should follow **lowerCamelCase**. Similar to UpperCamelCase, but the first word starts with a lowercase letter. Subsequent words start with an uppercase letter. Underscores are not allowed.
- **Global Constant Names**: Should be in **SNAKE_CASE**. Each word is in all uppercase letters, and words are separated by underscores. Numbers or lowercase letters are not permitted.

---

## Linter's Approach

### Capturing Target Identifiers

The linter leverages C# and Roslyn's use of [`SymbolKind`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.symbolkind) to identify and analyze symbols in the code. This allows the analyzer to focus on specific kinds of identifiers that need to adhere to the naming conventions.

However, it's important to note that `SymbolKind.Local` is **not** supported in Roslyn analyzers. To address this limitation and enforce the policy on local variables, the linter utilizes [`LocalDeclarationStatementSyntax`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/local-variable-declaration) to iterate over local variable declarations and apply the naming rules accordingly.

Regarding **global constants**, C# does not have a preprocessor `#define` like some other languages, so the term is a bit ambiguous. In this linter:

- A **global constant** is considered any field that is declared with both the `const` keyword and `public` access modifier.
- Additionally, fields declared with both `static` and `readonly` keywords are treated as global constants for the purposes of naming enforcement.

### Handling Invalid Characters

While regular expressions are effective for checking naming patterns, they are insufficient for handling invalid characters within identifiers. The built-in C# method [`Char.IsLetter`](https://learn.microsoft.com/en-us/dotnet/api/system.char.isletter) recognizes any valid Unicode letter, which might include letters from various languages and scripts.

To prevent identifiers that mix characters from different languages (which, while syntactically valid, could lead to confusion and maintainability issues), the linter implements a custom method to check for English letters only. This ensures that identifiers consist solely of English letters, digits, and underscores, adhering to the defined policy.

### Regular Expressions Used

The linter utilizes the following regular expressions to validate naming conventions:

- **UpperCamelCase**:

  ```csharp
  UpperCamelCaseRegex = new Regex(@"^([A-Z][a-z]*[0-9]*)+$");
  ```

- **lowerCamelCase**:

  ```csharp
  lowerCamelCaseRegex = new Regex(@"^[a-z]+[0-9]*([A-Z][a-z]*[0-9]*)*$");
  ```

- **SNAKE_CASE**:

  ```csharp
  SNAKE_CASE_REGEX = new Regex(@"^[A-Z]+(_[A-Z]+)*$");
  ```

These expressions ensure that identifiers match the specified patterns for each naming convention.

---

## Code-Fix Generator

### Auto-Generating Valid Identifier Names

The code-fix generator automatically creates valid identifier names based on the invalid ones detected by the linter. It processes each character of the original name:

- **Invalid Characters**: Removes any characters that are not English letters, digits, or underscores.
- **Case Adjustment**: Adjusts the case of letters to match the required naming convention (e.g., converting to uppercase or lowercase where appropriate).

By automating this transformation, the code-fix generator helps developers quickly comply with the naming policies without manual renaming.

### Fallback Names for Edge Cases

In some situations, an invalid identifier might not contain enough information to generate a meaningful name (e.g., when the name consists solely of invalid characters). For these edge cases, the code-fix provider uses predefined fallback names to ensure that the code remains functional and compliant.

**Example:**

Given the code:

```csharp
public const int _ = 7;
```

After applying the code fix, it would become:

```csharp
public const int FIX_ME_CONST = 7;
```

In this example, since the original name `_` doesn't provide meaningful context, the fallback name `FIX_ME_CONST` is used to indicate that the constant needs to be properly named.

---

By adhering to these conventions and utilizing the linter and code-fix generator, developers can maintain consistent and readable code that aligns with the course's standards.


# Extra Challenge

Unfortunately, due to poor time management on my side I wasn't able to complete the challenge.  I hope to revisit it someday ! 