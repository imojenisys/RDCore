# 5.6.9.2 Simple Data Operators

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.6.9.2** Simple Data Operators](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/dccbdeae-5b2e-4c2e-857e-1ad9b861e196).

## Runtime Semantics

The _evaluation pipeline_ of all operators follows a fixed sequence of three steps:

1. **Effective type.** The _effective type_ of the operation is determined, based on the _declared type_ of its
   _operands_.
2. **Validation.** All non-null operands (every operand that is not a
   [VBNullValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBNullValue.html)) are let-coerced to the determined
   effective type of the operation ([**RD-VBAL §5.5.1.2** Runtime semantics](rd-vbal.5.5.1.2.runtime-semantics.md)).
3. **Evaluation.** A templated method evaluates a result from the validated operands.

The sequence may be aborted at any point to return an _error result_. An error result encapsulates
[VBRuntimeErrorInfo](../api/RDCore.SDK.Model.Errors.VBRuntimeErrorInfo.html) error metadata.

### Operand validation

The arithmetic, relational and concatenation (`&`) operators validate their operands through
`OperatorRuntimeSemantics.LetCoerceNonNullOperand`.

`LetCoerceNonNullOperand` has a `TypeInfo`-equality short-circuit: an operand whose `TypeInfo` equals the destination
type is not coerced. The short-circuit never skips a
[VBVariantValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBVariantValue.html) operand: a `Variant` operand is always
let-coerced, even when its `TypeInfo` equals the destination type.

A `VBVariantValue`'s `TypeInfo` mirrors its wrapped value's; see
[**RD-VBAL §5.5.1.2** Runtime semantics](rd-vbal.5.5.1.2.runtime-semantics.md) (§5.5.1.2.12 Let-coercion to Variant).

## Computation in the effective type

An operator's result is computed in the operation's effective type. The result of the evaluation step (step 3) is
computed in the effective type's own representation, and never through a `Double` intermediate.

|Effective type|Computed in|
|---|---|
|`Long`|The `Long` representation, a 32-bit `int`.|
|`Currency`, `Decimal`|`decimal`|
|`Single`|`float`|

See [**RD-VBAL §5.6.9.3** Arithmetic Operators](rd-vbal.5.6.9.3.arithmetic-operators.md) for checked arithmetic and
the `^` operator, [**RD-VBAL §5.6.9.5** Relational Operators](rd-vbal.5.6.9.5.relational-operators.md) for
comparisons, and [**RD-VBAL §5.6.9.8** Logical Operators](rd-vbal.5.6.9.8.logical-operators.md) for bitwise
computation.

## Implementation

- The node parameter of `OperatorRuntimeSemantics<TContext,TFlags>` is typed
  [ExpressionNode](../api/RDCore.SDK.Model.AST.Abstract.ExpressionNode.html).
- Nothing in the operator runtime-semantics hierarchy reads `.Left`, `.Right` or `.Token` off the node parameter.
  Only the node's identity (`.Identity`) and its source location are read, to attribute the operation and its errors
  to the node.
- Only the token-dispatch layer, `OperatorRuntimeSemanticsProvider`, reads `.Token` off the operator node.

Requiring a real binary or unary operator node
([VBBinaryOperatorExpressionNode](../api/RDCore.SDK.Model.AST.Expressions.VBBinaryOperatorExpressionNode.html),
[VBUnaryOperatorExpressionNode](../api/RDCore.SDK.Model.AST.Expressions.VBUnaryOperatorExpressionNode.html)) as the
node parameter of the operator strategies is not load-bearing. It would force synthetic-node fabrication, which RD-VBA
rules out, as it does for let-coercion ([**RD-VBAL §5.5.1.2** Runtime semantics](rd-vbal.5.5.1.2.runtime-semantics.md)).

See [**RD-VBAL §5.4.2.10** Select Case Statement](rd-vbal.5.4.2.10.select-case-statement.md) for a caller that
invokes the operator strategies directly, with its own expression node.

---
> ⏮️ [**RD-VBAL §5.6.9.1** Operator Precedence and Associativity](rd-vbal.5.6.9.1.operator-precedence-and-associativity.md) | ⏭️ [**RD-VBAL §5.6.9.3** Arithmetic Operators](rd-vbal.5.6.9.3.arithmetic-operators.md)
