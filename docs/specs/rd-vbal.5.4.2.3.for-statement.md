# 5.4.2.3 For Statement

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.4.2.3** For Statement](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/389b1dc4-e608-4ed0-ae64-d88f62f12ea3).

## Syntax

|AST node|Instruction kind(s)|Notes|
|---|---|---|
|[ForStatementNode](../api/RDCore.SDK.Model.AST.Statements.ForStatementNode.html)|`ForOpener`|The `For` opener. Its `End` is the offset right past the `Next` closer.|
|— (synthesized)|`ForNext`|The `Next` closer; `Node` is `null`. Its `Target` is the body's first instruction; its `Matching` is the offset of its `ForOpener`.|

`Next` has no AST node of its own: the whole `For...Next` construct is one `ForStatementNode` with a `Body`
([**RD-VBAL §3.4.1** Block Statements](rd-vbal.3.4.1.block-statements.md)). The instruction fields are described in
[**RD-VBAL §3.5.2** Instruction](rd-vbal.3.5.2.instruction.md).

## Static Semantics

Reassigning a `For` loop counter inside the loop body is legal VBA.

Performance-related diagnostics should be issued when a
[VBCollectionType](../api/RDCore.SDK.Model.Types.Complex.VBCollectionType.html) is accessed by index within the body
of a `For...Next` loop ([**RD-VBAL §2.4.2** Non-intrinsic Types](rd-vbal.2.4.2.non-intrinsic-types.md)).

## Runtime Semantics

A `For` loop needs per-activation state. Its opener stashes a
[ForLoopState](../api/RDCore.SDK.Runtime.Shared.ForLoopState.html) on the activation, and its closer reads it back.

### ForOpener

1. Evaluate `start-value`, `end-value` and `step-increment` once each, in that order. A missing `step-clause`
   defaults to the integer value `1`; that default is never itself evaluated as a source expression.
2. Let-assign the counter to `start-value`, through the same Let-assignment machinery a `Let` statement uses
   ([**RD-VBAL §5.4.3.8** Let Statement](rd-vbal.5.4.3.8.let-statement.md)).
3. Stash `end` and `step`, together with the counter symbol and the counter's own expression node, as a
   `ForLoopState`, for every later step to reuse.
4. Check steps 1 and 2 of the **MS-VBAL §5.4.2.3** algorithm immediately: "if step is zero or positive and the
   counter already exceeds end", and "if step is negative and the counter already falls short".
5. When the counter is already out of range, skip straight to
   [Instruction](../api/RDCore.SDK.Semantics.Instructions.Instruction.html)`.End`, right past the loop. Otherwise, fall
   through into the body.

### ForNext

1. Read the stashed `ForLoopState` back via `Instruction.Matching`.
2. Read the counter's current value. The body may have reassigned the counter directly.
3. Add `step` to it through the addition operator
   ([**MS-VBAL §5.6.9.3** Arithmetic Operators](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/e070115f-8d40-40cf-ac6d-ab18b9c6c906);
   [**RD-VBAL §5.6.9.3** Arithmetic Operators](rd-vbal.5.6.9.3.arithmetic-operators.md)). The addition is an
   overflow-checked operation, not a bare CLR add.
4. Let-assign the sum back to the counter.
5. Re-test the counter the same way `ForOpener` did: branch back to the body (`Instruction.Target`) while the counter
   is still in range, and fall through past the loop otherwise.

### Range Test

The range checks of `ForOpener` and `ForNext` use the relational operators `>` and `<`
([**MS-VBAL §5.6.9.5** Relational Operators](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/f8acd631-55c1-4199-bc1e-022aaab6d9c8);
[**RD-VBAL §5.6.9.5** Relational Operators](rd-vbal.5.6.9.5.relational-operators.md)), not a raw numeric
comparison. The counter's declared type (`Currency`, `Decimal`, `Date` as `Double`, …) has spec-mandated comparison
semantics that a plain CLR `>`/`<` would get wrong.

Only the sign of `step` is read directly off its numeric magnitude. The **MS-VBAL §5.4.2.3** algorithm frames the
step's sign as a plain classification, not as a VBA-visible comparison expression.

|Step|Out of range when|
|---|---|
|Zero or positive|The counter exceeds `end` (`counter > end`).|
|Negative|The counter falls short of `end` (`counter < end`).|

### Errors

A label is scoped to the whole procedure, so a `GoTo` can land directly on the `Next` closer
([**RD-VBAL §5.4.1.1** Statement Labels](rd-vbal.5.4.1.1.statement-labels.md)). When `ForNext` finds no stashed
state for its `Matching` offset, its `ForOpener` never ran in this activation.

|Condition|Run-time error|
|---|---|
|`ForNext` finds no stashed `ForLoopState` for its `Matching` offset (a `GoTo` landed directly on the closer).|92 — For loop not initialized|
|The addition of `step` to the counter overflows.|6 — Overflow|

Run-time error 92 is [VBRuntimeErrorId](../api/RDCore.SDK.Model.Errors.VBRuntimeErrorId.html)`.ForLoopNotInitialized`
([**RD-VBAL §2.6.3** Runtime Errors](rd-vbal.2.6.3.runtime-errors.md)).

## Implementation

- `ProcedureExecutor` dispatches `ForOpener` and `ForNext`.
- The `Next` closer increments and tests the counter, so lowering synthesizes an instruction (`Node = null`) to hold
  that work ([**RD-VBAL §3.5.3** Lowering Block Statements](rd-vbal.3.5.3.lowering-block-statements.md)).
- `ForOpener` stashes `end`, `step`, the counter symbol and the counter's own expression node as a
  `RDCore.SDK.Runtime.Shared.ForLoopState`. The state is read back via
  [ICallStackFrame](../api/RDCore.SDK.Runtime.Abstract.Execution.ICallStackFrame.html)`.TryGetForLoopState`, and
  written via `CallStackFrame.SetForLoopState`.
- A `For` loop's state is richer than a single value: counter symbol, counter expression, end and step. It therefore
  has its own SDK type, `ForLoopState`, rather than the single block-state value that `With` and `Select Case` use
  ([**RD-VBAL §3.5.4** Execution](rd-vbal.3.5.4.execution.md)).
- `ForNext` uses the same `Instruction.Matching` field that a `Case` header uses for its back-reference to its
  `Select` ([**RD-VBAL §5.4.2.10** Select Case Statement](rd-vbal.5.4.2.10.select-case-statement.md)).
- The range test uses `RDCore.Runtime.Semantics.Operators.Relational.BinaryGtRelationalOperatorRuntimeSemantics` and
  `BinaryLtRelationalOperatorRuntimeSemantics`. The increment uses
  `RDCore.Runtime.Semantics.Operators.Arithmetic.BinaryAdditionOperatorRuntimeSemantics`.
- Every location-bearing node passed to the loop's operator calls is a real node the loop already has: the counter's
  own expression or, for the very first assignment, the loop's start expression. It is never a synthetic stand-in.
  The same rule applies to `Case` clause matching, which relies on the operator node parameter being widened to
  [ExpressionNode](../api/RDCore.SDK.Model.AST.Abstract.ExpressionNode.html)
  ([**RD-VBAL §5.4.2.10** Select Case Statement](rd-vbal.5.4.2.10.select-case-statement.md)).
- The message of `ForLoopNotInitialized` is the resx entry `VBForLoopNotInitialized_Verbose`, provided in both
  languages.

---
> ⏮️ [**RD-VBAL §5.4.2.2** While Statement](rd-vbal.5.4.2.2.while-statement.md) | ⏭️ [**RD-VBAL §5.4.2.4** For Each Statement](rd-vbal.5.4.2.4.for-each-statement.md)
