# 5.6.9.5 Relational Operators

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.6.9.5 Relational Operators**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/f8acd631-55c1-4199-bc1e-022aaab6d9c8).

## Runtime Semantics

Relational operators are simple data operators: they validate their operands through
`OperatorRuntimeSemantics.LetCoerceNonNullOperand`
([**RD-VBAL §5.6.9.2** Simple Data Operators](rd-vbal.5.6.9.2.simple-data-operators.md)).

Relational operators compare in the effective type, and yield a
[VBBooleanValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBBooleanValue.html).

|Effective type|Compared in|
|---|---|
|Integral|The effective type's own representation: for example `Long` as a 32-bit `int`, and `LongLong` as a 64-bit `long`.|
|`Boolean`|A 64-bit integer (`long`).|
|Fixed-point (`Currency`, `Decimal`)|`decimal`|

|Condition|Run-time error|
|---|---|
|An operand is `NaN`.|6 — Overflow|

Run-time error 6 is [VBRuntimeErrorId](../api/RDCore.SDK.Model.Errors.VBRuntimeErrorId.html)`.Overflow`.

👉 The range checks of a `For` loop use the relational operators
`RDCore.Runtime.Semantics.Operators.Relational.BinaryGtRelationalOperatorRuntimeSemantics` and
`BinaryLtRelationalOperatorRuntimeSemantics`, not a raw numeric comparison
([**RD-VBAL §5.4.2.3** For Statement](rd-vbal.5.4.2.3.for-statement.md)).

### Variant String/Numeric comparison

The `Variant` String/Numeric comparison exception implements **MS-VBAL §5.6.9.5**. RD-VBA implements it as run-time
behaviour.

The exception applies when both relational operands are `Variant`, one originally holding a `String` value and the
other a numeric value:

- The numeric operand is always considered less than the `String` operand, regardless of their actual values.
- RD-VBA never attempts to coerce the `String` operand to a number. Depending on its content, that coercion would
  fail, or succeed incorrectly.

`BinaryRelationalOperatorRuntimeSemantics` short-circuits the exception before normal coercion runs:

1. It detects the `Variant` String/Numeric case before normal effective-type determination and coercion run.
2. It reduces the case to a synthetic `Integer` rank for each operand (table below).
3. The relational operator's ordinary `Integer` evaluation branch compares the synthetic ranks.

|Operand|Synthetic `Integer` rank|
|---|---|
|The numeric-holding `Variant`|`0`|
|The `String`-holding `Variant`|`1`|

The short-circuit runs before normal coercion because normal coercion would otherwise try, and fail, to coerce the
`String` to a number. The synthetic-rank reduction means no bespoke evaluation path is needed for the `Variant`
String/Numeric case.

## 5.6.9.5.1 = Operator

This section corresponds to [**MS-VBAL §5.6.9.5.1 = Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/34660fe0-2ce9-4526-b0c3-00613c9fa19e).

> [!NOTE]
> Reserved. This section has no content yet.

## 5.6.9.5.2 <> Operator

This section corresponds to [**MS-VBAL §5.6.9.5.2 <> Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/a5a4bd2c-a89d-4e88-a634-40a8bca8d458).

> [!NOTE]
> Reserved. This section has no content yet.

## 5.6.9.5.3 < Operator

This section corresponds to [**MS-VBAL §5.6.9.5.3 < Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/6b25b2c5-a96f-4e20-b77e-d328a13c66cd).

> [!NOTE]
> Reserved. This section has no content yet.

## 5.6.9.5.4 > Operator

This section corresponds to [**MS-VBAL §5.6.9.5.4 > Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/c648bb41-d1b0-4462-9da1-a37c251521a4).

> [!NOTE]
> Reserved. This section has no content yet.

## 5.6.9.5.5 <= Operator

This section corresponds to [**MS-VBAL §5.6.9.5.5 <= Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/66dfebc2-a5e1-43fd-965f-75334e04f5ec).

> [!NOTE]
> Reserved. This section has no content yet.

## 5.6.9.5.6 >= Operator

This section corresponds to [**MS-VBAL §5.6.9.5.6 >= Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/0720f474-42ca-429c-ac1d-09f2453128a6).

> [!NOTE]
> Reserved. This section has no content yet.

---
> ⏮️ [**RD-VBAL §5.6.9.4** & Operator](rd-vbal.5.6.9.4.ampersand-operator.md) | ⏭️ [**RD-VBAL §5.6.9.6** Like Operator](rd-vbal.5.6.9.6.like-operator.md)
