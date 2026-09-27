# 5.4.3.2 Local Constant Declarations

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.4.3.2** Local Constant Declarations](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/90382d70-261f-468f-92de-0068235c012b).

## Syntax

|Declaration|AST node|Symbol on the procedure's `Locals`|
|---|---|---|
|`Const` (in a procedure body)|[ConstantDeclarationNode](../api/RDCore.SDK.Model.AST.Declarations.ConstantDeclarationNode.html)|[VBLocalConstantSymbol](../api/RDCore.SDK.Model.Symbols.VBLocalConstantSymbol.html)|

`VBProcedureMemberSymbol.Locals` and `VBReturningMemberSymbol.Locals` list every `Dim`, `Static` and `Const`
declared in the procedure body; see
[**RD-VBAL §5.4.3.1** Local Variable Declarations](rd-vbal.5.4.3.1.local-variable-declarations.md).

## Runtime Semantics

In MS-VBAL, a local `Const`'s value is substituted at compile time; it has no run-time address.

> [!NOTE]
> **Not implemented.** A local `Const` is not modeled at run time: a local `Const`'s initializer expression is not
> passed to any point where the runtime could evaluate it. Reading a local `Const` at run time resolves to
> `InternalError`, rather than silently reading a wrong value.

## Implementation

`RDCore.Runtime.Execution.RuntimeProcedureInvoker.HoistLocals` hoists a procedure's `Dim` and `Static` locals
only; a local `Const` gets no storage.

---
> ⏮️ [**RD-VBAL §5.4.3.1** Local Variable Declarations](rd-vbal.5.4.3.1.local-variable-declarations.md) | ⏭️ [**RD-VBAL §5.4.3.3** ReDim Statement](rd-vbal.5.4.3.3.redim-statement.md)
