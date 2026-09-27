# 2.6.1 Syntax Errors

A **syntax error** is raised while the parser traverses the *concrete syntax tree* (CST). It is a token the grammar cannot place.

|||
|---|---|
|Code family|`VBC`, range `VBC00001`–`VBC00999`|
|Title|_Syntax error_|
|Raised by|the parser (concrete syntax tree)|
|Source metadata|[VBSyntaxErrorInfo](../api/RDCore.SDK.Model.Errors.VBSyntaxErrorInfo.html); its `ErrorId` is a [VBCompileErrorId](../api/RDCore.SDK.Model.Errors.VBCompileErrorId.html)|
|Severity|`Error`|
|Detail|the faulted token and its expected role, on `Diagnostic.data`|

## Syntax and Semantic Compilation Errors

MS-VBAL does not distinguish a compile-time error raised in CST semantics from one raised in AST semantics. RDCore splits CST-semantics (syntax) errors and AST-semantics ([semantic compilation](rd-vbal.2.6.2.semantic-compilation-errors.md)) errors by numeric range only.

Every `VBCompileErrorId` value below 9300 belongs to the parser; `[9300..]` is reserved for semantic compilation errors. Errors issued from the parser are normally syntax errors with a `VBCompileErrorId` value between 42 and 999; see [**RD-VBAL §1.1.4** Core Diagnostics](rd-vbal.1.1.4.core-diagnostics.md).

Conditions that need symbol information (an undefined name, a duplicate declaration) are not syntax errors. They are reported later, as semantic compilation errors.

## Fallback and Dedicated Codes

`VBC00001` is the general syntax-error fallback code: when the input is invalid and no more specific code applies, the parser reports `VBC00001`.

The parser narrows its output: recurring syntax-error shapes are promoted to a dedicated code in the `VBC00042`–`VBC00999` range.

A numeric literal whose value does not fit its type is the `NumericLiteralOverflow` syntax error (`VBC00042`), located at the literal. This includes a literal whose value does not fit its suffix-forced type, a floating-point literal that overflows to infinity, and an unsuffixed radix literal beyond 32 bits; see [**RD-VBAL §3.2.0** Literal Expressions](rd-vbal.3.2.0.literals.md).

## Conditional Compilation

> [!NOTE]
> **Not implemented.** A `#If` that splits a single statement across conditional-compilation branches (`#If` / `#Else` / `#End If`) is unparseable by the RD-VBA grammar. Such a statement reports located `VBC` diagnostics (`VBC00001`), which a client can anchor to a source range.

## Published Codes

|Code|Title|Condition|
|---|---|---|
|[`VBC00001`](../diagnostics/vbc00001.md)|Syntax error|a token the grammar cannot place|
|[`VBC00042`](../diagnostics/vbc00042.md)|Numeric literal overflow|a numeric literal outside the range of its type|

---
> ⏮️ [**RD-VBAL §2.6** Diagnostics](rd-vbal.2.6.diagnostics.md) | ⏭️ [**RD-VBAL §2.6.2** Semantic Compilation Errors](rd-vbal.2.6.2.semantic-compilation-errors.md)
