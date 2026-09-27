# 5.4.3.3 ReDim Statement

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.4.3.3** ReDim Statement](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/22b5d372-0a54-4617-9462-4934b5edc88c).

## Syntax

|AST node|Instruction kind(s)|Notes|
|---|---|---|
|[RedimDeclarationNode](../api/RDCore.SDK.Model.AST.Declarations.RedimDeclarationNode.html)|— (a declaration node)|`ReDim` and `ReDim Preserve`. `ReDim` is modelled as a declaration node, not a statement node, because it declares or resizes storage.|

See [**RD-VBAL §3.4.2** Simple Statements](rd-vbal.3.4.2.simple-statements.md).

## Static Semantics

`ReDim` may be used to declare a `VBResizableArrayValue`, or to redimension an already-declared
`VBResizableArrayValue`. `ReDim` is illegal to use with any `VBFixedSizeArrayValue`. See
[**RD-VBAL §2.4.1** Intrinsic Types](rd-vbal.2.4.1.intrinsic-types.md).

The target name of a `ReDim` statement decides what the statement is:

|Target name|The `ReDim` statement is|
|---|---|
|Resolves to a local, a parameter, or a module field|A re-dimension of an existing array.|
|Unqualified, and resolves to nothing|An implicit declaration.|

For an implicit `ReDim` declaration:

- The declaration pass introduces a procedure-local
  [VBResizableArrayType](../api/RDCore.SDK.Model.Types.VBResizableArrayType.html) symbol.
- The procedure-local symbol is marked as `ReDim`-introduced.
- The implicit declaration is legal under `Option Explicit`; see
  [**RD-VBAL §5.2.1** Option Directives](rd-vbal.5.2.1.option-directives.md).
- A later analysis pass raises a semantic flag at the site of the implicit declaration; see
  [**RD-VBAL §1.1.3** Core Semantic Flags](rd-vbal.1.1.3.core-semantic-flags.md).

`ReDim` bounds are ordinary run-time expressions, not constant expressions. The declaration pass keeps `ReDim`
bounds verbatim, as it does declared array bounds
([**RD-VBAL §5.4.3.1** Local Variable Declarations](rd-vbal.5.4.3.1.local-variable-declarations.md)).

## Runtime Semantics

> [!NOTE]
> Reserved. This section has no content yet.

## Implementation

|Type or member|Role|
|---|---|
|`RedimDeclarationNode`|One `ReDim` target (**RDCore.SDK**).|
|[VBLocalVariableSymbol](../api/RDCore.SDK.Model.Symbols.VBLocalVariableSymbol.html)`.DeclaredBy`|[LocalDeclarationKind](../api/RDCore.SDK.Model.Symbols.LocalDeclarationKind.html)`.ReDim` marks a local introduced by an implicit `ReDim` declaration.|

---
> ⏮️ [**RD-VBAL §5.4.3.2** Local Constant Declarations](rd-vbal.5.4.3.2.local-constant-declarations.md) | ⏭️ [**RD-VBAL §5.4.3.4** Erase Statement](rd-vbal.5.4.3.4.erase-statement.md)
