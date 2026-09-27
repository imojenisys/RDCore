# 5.4.2.10 Select Case Statement

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.4.2.10 Select Case Statement**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/94a2f0fe-bdbe-4f5d-b3f4-bbf339b0ac65).

## Syntax

|AST node|Instruction kind(s)|Notes|
|---|---|---|
|[SelectCaseStatementNode](../api/RDCore.SDK.Model.AST.Statements.SelectCaseStatementNode.html)|`Select`|The `Select Case` opener. Its `End` is right past the block.|
|[CaseExpressionStatementNode](../api/RDCore.SDK.Model.AST.Statements.CaseExpressionStatementNode.html)|`ConditionalBranch`|A `Case` header. Its `Matching` names the offset of its `Select` opener. Its `Else` is the offset to go to on no match: the next header in the chain, the first instruction of the `Case Else` body, or right past the whole construct.|
|[CaseElseClauseStatementNode](../api/RDCore.SDK.Model.AST.Statements.CaseElseClauseStatementNode.html)|—|`Case Else` needs no condition.|
|— (synthesized)|`Jump`|The trailing jump at the end of each branch's body. `Node` is `null`; `Target` is right past the whole construct.|

Each `Case` line's comma-separated conditions form their own node hierarchy under the abstract
[CaseRangeClauseNode](../api/RDCore.SDK.Model.AST.Statements.CaseRangeClauseNode.html). The three condition forms
of **MS-VBAL §5.4.2.10** are modelled independently, as three node types:

|Condition form|Example|AST node|
|---|---|---|
|Single value|`Case 5`|[CaseValueRangeClauseNode](../api/RDCore.SDK.Model.AST.Statements.CaseValueRangeClauseNode.html)|
|Comparison|`Case Is > 5`|[CaseComparisonRangeClauseNode](../api/RDCore.SDK.Model.AST.Statements.CaseComparisonRangeClauseNode.html)|
|Inclusive range|`Case 1 To 10`|[CaseToRangeClauseNode](../api/RDCore.SDK.Model.AST.Statements.CaseToRangeClauseNode.html)|

See [**RD-VBAL §3.4.1** Block Statements](rd-vbal.3.4.1.block-statements.md) and
[**RD-VBAL §3.5.2** Instruction](rd-vbal.3.5.2.instruction.md).

## Runtime Semantics

### Select

1. Evaluate the header expression (the selector) once.
2. Stash the selector on the activation, keyed by the `Select` instruction's own offset.
3. Fall through into the body, to the first `Case` header.

**MS-VBAL §5.4.2.10** says "the select-expression is immediately evaluated": the selector is evaluated once, ahead
of every case-clause. RD-VBA passes the evaluated selector through to every clause comparison, which matches this.

### Case Header

The executor dispatches `ConditionalBranch` separately for a `Case` header: a `Case` header has
[Instruction](../api/RDCore.SDK.Semantics.Instructions.Instruction.html)`.Matching` set instead of a Boolean
condition.

1. Read the enclosing `Select`'s stashed selector, via the header's `Matching` offset.
2. If the selector is a `Variant`, unwrap it.
3. If the selector is `Null`, go straight to the header's `Else` offset without evaluating any range clause. Every
   `Case` header does the same, so control reaches `Case Else`.
4. Otherwise, match the header's range clauses against the selector, as a real comparison or logical expression.
5. On a match, fall through into the branch's body. Otherwise, go to the header's `Else` offset: the next header in
   the chain, the first instruction of the `Case Else` body, or right past the whole construct.
6. After the branch's body runs, the synthesized trailing `Jump` goes right past the whole construct.

**MS-VBAL §5.4.2.10**: "If select-expression is the data value Null, only the case-else-clause is executed". A
`Null` can reach `Select Case` only through a `Variant` selector: a local declared `Long` can never hold `Null`.

### Range Clause Matching

A `Case` header's range clauses are matched as a real comparison or logical expression, the way
**MS-VBAL §5.4.2.10** phrases its own runtime semantics:

|Range clause|Example|Matched as|
|---|---|---|
|Value|`Case 5`|`selector = 5`|
|Comparison|`Case Is > 5`|`selector > 5`. The clause's own, already-normalized comparison-operator token picks the relational operator strategy directly.|
|`To`|`Case 1 To 10`|`(selector >= 1) And (selector <= 10)`|

Each comparison is evaluated through the same operator strategies every other expression uses
(`BinaryRelationalOperatorRuntimeSemantics`, `BinaryAndLogicalOperatorRuntimeSemantics`;
[**RD-VBAL §5.6.9.5** Relational Operators](rd-vbal.5.6.9.5.relational-operators.md),
[**RD-VBAL §5.6.9.8** Logical Operators](rd-vbal.5.6.9.8.logical-operators.md)). It is never a hand-rolled equality
or range check.

The already-evaluated selector is passed straight through to each clause comparison as an operand value. It is not
re-evaluated per clause.

## Implementation

- `ProcedureExecutor` dispatches `Select`, and the `ConditionalBranch` of a `Case` header.
- The selector is stashed via
  [ICallStackFrame](../api/RDCore.SDK.Runtime.Abstract.Execution.ICallStackFrame.html)`.TryGetBlockState` and
  `CallStackFrame.SetBlockState`, keyed by the opener's offset. A single block-state value is enough for the
  selector. There is no separate closer instruction to pop the stashed state on exit
  ([**RD-VBAL §3.5.4** Execution](rd-vbal.3.5.4.execution.md)).
- A `Case` header's `Matching` is the same field a `ForNext` instruction uses for its back-reference to its opener
  ([**RD-VBAL §5.4.2.3** For Statement](rd-vbal.5.4.2.3.for-statement.md)).
- `Select Case` needs no synthesized closer: falling out of the last branch, or out of `Case Else`, already lands
  where the construct's own `End`/`Else` chaining says it should
  ([**RD-VBAL §3.5.3** Lowering Block Statements](rd-vbal.3.5.3.lowering-block-statements.md)).
- `ProcedureExecutor.ExecuteCaseHeader`'s `Null`-selector short-circuit unwraps a `Variant` selector before testing
  it for `Null`.
- `RDCore.Runtime.Execution.CaseMatchEvaluator` matches the range clauses against the selector. It calls the specific
  operator strategy directly rather than going through `OperatorRuntimeSemanticsProvider`'s token dispatch, because
  it already knows which operator strategy it wants.
- `CaseMatchEvaluator` passes the clause's own real operand expression as the location-bearing node of each operator
  call. It never passes a fabricated stand-in node for the already-evaluated selector.
- This relies on the node parameter of `OperatorRuntimeSemantics<TContext,TFlags>` being typed
  [ExpressionNode](../api/RDCore.SDK.Model.AST.Abstract.ExpressionNode.html)
  ([**RD-VBAL §5.6.9.2** Simple Data Operators](rd-vbal.5.6.9.2.simple-data-operators.md)).

---
> ⏮️ [**RD-VBAL §5.4.2.9** Single-line If Statement](rd-vbal.5.4.2.9.single-line-if-statement.md) | ⏭️ [**RD-VBAL §5.4.2.11** Stop Statement](rd-vbal.5.4.2.11.stop-statement.md)
