# 5.4.2.5 Exit For Statement

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.4.2.5** Exit For Statement](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/aa978e90-6240-454c-a7af-0a3e80779dc7).

## Syntax

|AST node|Instruction kind(s)|Notes|
|---|---|---|
|[KeywordStatementNode](../api/RDCore.SDK.Model.AST.Statements.KeywordStatementNode.html) (`Token`: `"Exit For"`)|`ExitLoop`|`Target`: the offset right past the closer of the innermost enclosing `For` or `For Each` loop.|

See [**RD-VBAL §3.4.2** Simple Statements](rd-vbal.3.4.2.simple-statements.md) and
[**RD-VBAL §3.5.2** Instruction](rd-vbal.3.5.2.instruction.md).

## Static Semantics

`Exit For` needs an enclosing `For` or `For Each` loop
([**RD-VBAL §5.4.2.3** For Statement](rd-vbal.5.4.2.3.for-statement.md),
[**RD-VBAL §5.4.2.4** For Each Statement](rd-vbal.5.4.2.4.for-each-statement.md)). It resolves against the
innermost enclosing loop of that kind.

A `While…Wend` loop does not satisfy `Exit For`
([**RD-VBAL §5.4.2.2** While Statement](rd-vbal.5.4.2.2.while-statement.md)).

> [!NOTE]
> **Not implemented.** No diagnostic is reported for an `Exit For` that has no enclosing `For` or `For Each` loop.
> Lowering leaves the `ExitLoop` target unresolved (`null`).
> [VBCompileErrorId](../api/RDCore.SDK.Model.Errors.VBCompileErrorId.html) defines `ExitForNotWithinForNext` (9313)
> for this condition, but nothing reports it.

## Runtime Semantics

1. `ExitLoop` branches to [Instruction](../api/RDCore.SDK.Semantics.Instructions.Instruction.html)`.Target`,
   exactly like `Jump` ([**RD-VBAL §5.4.2.12** GoTo Statement](rd-vbal.5.4.2.12.goto-statement.md)).
2. No runtime search is needed: lowering has already resolved `Target` to the offset right past the innermost
   enclosing loop of the matching kind.

## Implementation

- [InstructionListLowering](../api/RDCore.SDK.Semantics.Instructions.InstructionListLowering.html) resolves the
  `ExitLoop` target ([**RD-VBAL §3.5.3** Lowering Block Statements](rd-vbal.3.5.3.lowering-block-statements.md)).
- `ProcedureExecutor` uses the same `case` arm for `ExitLoop` and `Jump`.

---
> ⏮️ [**RD-VBAL §5.4.2.4** For Each Statement](rd-vbal.5.4.2.4.for-each-statement.md) | ⏭️ [**RD-VBAL §5.4.2.6** Do Statement](rd-vbal.5.4.2.6.do-statement.md)
