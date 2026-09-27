# 5.6.9.3 Arithmetic Operators

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.6.9.3 Arithmetic Operators**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/e070115f-8d40-40cf-ac6d-ab18b9c6c906).

## Runtime Semantics

Arithmetic operators are simple data operators
([**RD-VBAL §5.6.9.2** Simple Data Operators](rd-vbal.5.6.9.2.simple-data-operators.md)):

- They validate their operands through `OperatorRuntimeSemantics.LetCoerceNonNullOperand`.
- Their result is computed in the effective type's own representation, never through a `Double` intermediate:
  `Long` in a 32-bit `int`, `Currency` and `Decimal` in `decimal`, `Single` in `float`.

Arithmetic runs in a _checked_ context. An integral or fixed-point result that does not fit the effective type raises
`Overflow` rather than wrapping or silently narrowing.

|Condition|Run-time error|
|---|---|
|An integral or fixed-point result does not fit the effective type.|6 — Overflow|

Run-time error 6 is [VBRuntimeErrorId](../api/RDCore.SDK.Model.Errors.VBRuntimeErrorId.html)`.Overflow`. For division
by zero, see §5.6.9.3.6.

## 5.6.9.3.1 Unary - Operator

This section corresponds to [**MS-VBAL §5.6.9.3.1 Unary - Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/181ac001-7156-4fed-a518-fb90b828ec59).

> [!NOTE]
> Reserved. This section has no content yet.

## 5.6.9.3.2 + Operator

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.6.9.3.2 + Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/91621d4b-3da3-4fe9-9581-adda857efe05).

🧩 The explicit addition of runtime semantics for a unary `+` operator is a language core extension
([**RD-VBAL §1.1.2** Language Core Extensions](rd-vbal.1.1.2.language-core-extensions.md)).

👉 A `For` loop's `ForNext` instruction adds `step` to the counter through the addition operator,
`RDCore.Runtime.Semantics.Operators.Arithmetic.BinaryAdditionOperatorRuntimeSemantics`. The step addition is therefore
an overflow-checked operation, not a bare CLR add
([**RD-VBAL §5.4.2.3** For Statement](rd-vbal.5.4.2.3.for-statement.md)).

## 5.6.9.3.3 Binary - Operator

This section corresponds to [**MS-VBAL §5.6.9.3.3 Binary - Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/c966fa50-f49f-44ab-a8f8-f2691fffe048).

> [!NOTE]
> Reserved. This section has no content yet.

## 5.6.9.3.4 * Operator

This section corresponds to [**MS-VBAL §5.6.9.3.4 * Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/917bed61-f19f-4e7d-9f93-2e44d4ec7432).

> [!NOTE]
> Reserved. This section has no content yet.

## 5.6.9.3.5 / Operator

This section corresponds to [**MS-VBAL §5.6.9.3.5 / Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/8628f21c-3f02-42b1-908c-201bd7ffe1b5).

> [!NOTE]
> Reserved. This section has no content yet.

## 5.6.9.3.6 \ Operator and Mod Operator

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.6.9.3.6 \ Operator and Mod Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/09fdf266-a9d5-4ecb-b7d8-ffd1361cb349).

### Static Semantics

> [!NOTE]
> **Not implemented.** The `Mod` operator token has no mapped static semantics rule. The static semantics of a `Mod`
> expression defer to [VBUnknownType](../api/RDCore.SDK.Model.Types.VBUnknownType.html)
> ([**RD-VBAL §5.0** Semantics](rd-vbal.5.0.semantics.md)).

### Runtime Semantics

|Condition|Run-time error|
|---|---|
|An integral division or `Mod` by zero.|11 — Division by zero|

Run-time error 11 is `VBRuntimeErrorId.DivisionByZero`.

## 5.6.9.3.7 ^ Operator

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.6.9.3.7 ^ Operator**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/c3f526af-9d58-47f9-a6be-22ab81e8747a).

The `^` operator is the sole exception to computation in the effective type
([**RD-VBAL §5.6.9.2** Simple Data Operators](rd-vbal.5.6.9.2.simple-data-operators.md)): its effective type is always
`Double`. The `^` operator is evaluated as IEEE-754 exponentiation.

---
> ⏮️ [**RD-VBAL §5.6.9.2** Simple Data Operators](rd-vbal.5.6.9.2.simple-data-operators.md) | ⏭️ [**RD-VBAL §5.6.9.4** & Operator](rd-vbal.5.6.9.4.ampersand-operator.md)
