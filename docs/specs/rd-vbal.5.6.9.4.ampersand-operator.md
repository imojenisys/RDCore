# 5.6.9.4 & Operator

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.6.9.4** & Operator](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/9f072ffc-e943-4fcc-a4d0-f3c7db96abd9).

## Runtime Semantics

The `&` (concatenation) operator is a simple data operator: it validates its operands through
`OperatorRuntimeSemantics.LetCoerceNonNullOperand`
([**RD-VBAL §5.6.9.2** Simple Data Operators](rd-vbal.5.6.9.2.simple-data-operators.md)).

`BinaryConcatOperatorRuntimeSemantics.IsByteArray` unwraps a `Variant`
([VBVariantValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBVariantValue.html)) before matching a wrapped `Byte`
array.

> 👉 `v1 & v2` works on `Variant`s holding arrays the same as on declared arrays.

See [**RD-VBAL §5.5.1.2** Runtime semantics](rd-vbal.5.5.1.2.runtime-semantics.md) (§5.5.1.2.12 Let-coercion to
Variant) for why a `Variant` operand must be unwrapped before it is matched against a concrete value type.

---
> ⏮️ [**RD-VBAL §5.6.9.3** Arithmetic Operators](rd-vbal.5.6.9.3.arithmetic-operators.md) | ⏭️ [**RD-VBAL §5.6.9.5** Relational Operators](rd-vbal.5.6.9.5.relational-operators.md)
