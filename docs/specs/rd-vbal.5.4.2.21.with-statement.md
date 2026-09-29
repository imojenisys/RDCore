# 5.4.2.21 With Statement

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.4.2.21** With Statement](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/52caae3d-3ded-436f-a36a-8d5a30c21600).

## Syntax

|AST node|Instruction kind|Notes|
|---|---|---|
|[WithStatementNode](../api/RDCore.SDK.Model.AST.Statements.WithStatementNode.html)|`With`|The opener's [Instruction](../api/RDCore.SDK.Semantics.Instructions.Instruction.html)`.End` is right past the block.|

`With...End With` is a block statement
([**RD-VBAL §3.4.1** Block Statements](rd-vbal.3.4.1.block-statements.md)). Its opener lowers to the
[InstructionKind](../api/RDCore.SDK.Semantics.Instructions.InstructionKind.html) `With`
([**RD-VBAL §3.5.2** Instruction](rd-vbal.3.5.2.instruction.md)).

## Static Semantics

Lowering records the enclosing `With` block on every instruction inside it
([**RD-VBAL §3.5.3** Lowering Block Statements](rd-vbal.3.5.3.lowering-block-statements.md)):

- Every instruction lexically inside a `With` block, however deeply nested (through an `If` or a loop), carries
  `Instruction.EnclosingWith` set to that `With`'s opener offset.
- When lowering leaves a `With` block, `EnclosingWith` is restored to its value before the block.
- `EnclosingWith` is computed once, at lowering time. It is not tracked as a runtime stack the interpreter
  pushes and pops.

Because `EnclosingWith` is static, a `GoTo` into or out of a `With` block leaves no stale state to unwind
([**RD-VBAL §5.4.2.12** GoTo Statement](rd-vbal.5.4.2.12.goto-statement.md)).

## Runtime Semantics

The executor runs the `With` instruction as follows:

1. The header expression (the target) is evaluated once.
2. The target is Set-coerced or Let-coerced:
   - a class-valued target is Set-assigned through
     [ISetCoercionRuntimeSemantics](../api/RDCore.SDK.Runtime.Abstract.ISetCoercionRuntimeSemantics.html)
     ([**RD-VBAL §5.5.2.2** Runtime semantics](rd-vbal.5.5.2.2.runtime-semantics.md));
   - a UDT-valued target is Let-assigned through `ILetCoercionRuntimeSemanticsProvider.EvaluateLetCoercionSemantics`
     directly.
3. The result is stored on the activation, keyed by the `With` instruction's own offset.
4. Execution falls through into the body.

The `With` block has no separate closer instruction to pop the stored target on exit.

Every `Simple` and `ConditionalBranch` instruction's `RuntimeEvaluationContext` is recomputed before each
dispatch, from `Instruction.EnclosingWith` ([**RD-VBAL §3.5.4** Execution](rd-vbal.3.5.4.execution.md)). A
`.Member` or `!member` with-expression then resolves against the innermost enclosing `With`'s stored target,
however control reached the instruction, including via `GoTo`
([**RD-VBAL §5.6.15** With Expressions](rd-vbal.5.6.15.with-expressions.md)).

## Implementation

- `RDCore.Runtime.Execution.ProcedureExecutor` dispatches `With` instructions.
- `RDCore.Runtime.Semantics.Statements.WithStatementRuntimeSemantics` performs the Set-coercion or Let-coercion
  of step 2.
- The target is stored through `CallStackFrame.SetBlockState` and read back through
  [ICallStackFrame](../api/RDCore.SDK.Runtime.Abstract.Execution.ICallStackFrame.html)`.TryGetBlockState`,
  keyed by the opener's offset. `Select Case` stores its selector the same way
  ([**RD-VBAL §5.4.2.10** Select Case Statement](rd-vbal.5.4.2.10.select-case-statement.md)); a single
  block-state value is enough for either.
- `RuntimeExpressionEvaluator` resolves the with-expression target through its `EnclosingWithTarget`
  resolution.

---
> ⏮️ [**RD-VBAL §5.4.2.20** RaiseEvent Statement](rd-vbal.5.4.2.20.raiseevent-statement.md) | ⏭️ [**RD-VBAL §5.4.2.22** End Statement](rd-vbal.5.4.2.22.end-statement.md)
