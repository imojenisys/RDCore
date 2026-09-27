# 5.5.1.2 Runtime semantics

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.5.1.2** Runtime semantics](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/3e5fb49f-eb20-4562-a6bd-4a26dc5fa733).

_Let-coercion_ is the implicit conversion applied to an operand, or to an assignment RHS, so that its value fits a
required _destination declared type_.

In the operator evaluation pipeline, the validation step let-coerces all non-null operands
(non-[VBNullValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBNullValue.html)) to the determined _effective type_ of
the operation ([**RD-VBAL §5.6.9.2** Simple Data Operators](rd-vbal.5.6.9.2.simple-data-operators.md)).


## Let-coercion provider

Let-coercion is driven by a let-coercion _provider_, `LetCoercionRuntimeSemanticsProvider`. The provider uses
provider/strategy dispatch: it dispatches to a per-destination-type let-coercion _strategy_, selected by the
destination type.

`LetCoercionRuntimeSemanticsProvider.EvaluateLetCoercionSemantics` evaluates a let-coercion in this order:

1. When the source value is a `Variant`, the provider unwraps it, once, centrally, before dispatching to a strategy.
   The unwrap is recursive, because a `Variant` may wrap another `Variant`.
2. The provider resolves the strategy by walking the base-type chain of the destination
   [VBType](../api/RDCore.SDK.Model.Types.Abstract.VBType.html). One strategy keyed on `VBNumericType` serves every
   concrete numeric type (§5.5.1.2.1).
3. The provider evaluates the strategy inside a frame of its coercion frame stack.

Every let-coercion strategy casts `frame.SourceValue` directly to its own concrete value type. The provider unwraps a
`Variant` source before dispatch so that every strategy's own direct cast sees the real wrapped value rather than the
[VBVariantValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBVariantValue.html).

Strategy dispatch is destination-keyed. It still picks the right strategy for a `Variant` source, because the
`Variant`'s `TypeInfo` mirrors its wrapped value's (§5.5.1.2.12).


## Coercion frame stack

The let-coercion provider maintains a coercion frame stack of
[LetCoercionStackFrame](../api/RDCore.SDK.Runtime.Shared.LetCoercionStackFrame.html) frames.

A _recursive let-coercion_ is a strategy that must coerce through an intermediate type, e.g.
`Date → Double → Integer`. The coercion frame stack detects a recursive let-coercion and reports it as
`OutOfStackSpace` ([VBRuntimeErrorId](../api/RDCore.SDK.Model.Errors.VBRuntimeErrorId.html)) rather than overflowing
the call stack.

A [VBTypeDescValue](../api/RDCore.SDK.Model.Values.Meta.VBTypeDescValue.html) is used in the implementation of
let-coercion: a let-coercion frame describes its destination type with one (`DestinationTypeDesc`), where semantics
demand knowledge of a _data type_ where a _value_ is normally required
([**RD-VBAL §2.4.3** Meta and Advanced Types](rd-vbal.2.4.3.meta-and-advanced-types.md)).


## Let-coercion result

Each let-coercion step yields a [LetCoercionResult](../api/RDCore.SDK.Runtime.Shared.LetCoercionResult.html):

|Result|Carries|
|---|---|
|`Success`|The coerced [VBTypedValue](../api/RDCore.SDK.Model.Values.Abstract.VBTypedValue.html).|
|`Error`|A run-time error, such as `TypeMismatch` or `Overflow` (`VBRuntimeErrorId`).|
|`NotApplicable`|No coerced value and no error.|


## Let-coercion strategies

The let-coercion strategy contract is
[ILetCoercionRuntimeSemantics](../api/RDCore.SDK.Runtime.Abstract.ILetCoercionRuntimeSemantics.html).
`ILetCoercionRuntimeSemantics.EvaluateLetCoercion` and `ILetCoercionRuntimeSemantics.Analyze` take the coerced value's
own [ExpressionNode](../api/RDCore.SDK.Model.AST.Abstract.ExpressionNode.html) rather than a
[VBOperatorExpression](../api/RDCore.SDK.Model.AST.Expressions.VBOperatorExpression.html).

Every let-coercion strategy uses the node parameter opaquely: identity and location only, for error reporting. Because
of that, the narrower `VBOperatorExpression` type is not load-bearing for any strategy.

RD-VBA rules out synthetic-node fabrication for let-coercion: a construct that has no operator node of its own in
source passes its own expression (§5.5.1.2.2). The operator runtime semantics' node parameter follows the same rule
([**RD-VBAL §5.6.9.2** Simple Data Operators](rd-vbal.5.6.9.2.simple-data-operators.md)).


## 5.5.1.2.1 Let-coercion between numeric types

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.5.1.2.1** Let-coercion between numeric types](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/56fb78c3-a0ea-4bf0-8ee4-eb5f89bda4f7).

One let-coercion strategy keyed on [VBNumericType](../api/RDCore.SDK.Model.Types.Abstract.VBNumericType.html),
`VBNumericLetCoercionTypeRuntimeSemantics`, serves every concrete numeric type.

Let-coercion between numeric types validates that the source value is within the destination's representable range.
When the source value is outside that range, it raises `Overflow`. Otherwise:

|Conversion|Result|
|---|---|
|Widening|The value is copied, converted to the destination's representation.|
|Narrowing to a wider-or-equal integral type|The value is copied, converted to the destination's representation.|
|Floating-point or fixed-point value → integral type (narrowing)|The value is rounded to the nearest integer (§5.5.1.2.1.1) before conversion.|
|Integral → floating-point|🎯 A plain widening copy (see the divergence note below).|

A `Date` source let-coerced to a numeric destination reports `ConversionSemanticFlags.DateSerial`
([ConversionSemanticFlags](../api/RDCore.SDK.Semantics.Flags.ConversionSemanticFlags.html)).
`VBNumericLetCoercionTypeRuntimeSemantics.DateSerialFlagsOf` computes the `DateSerial` flag for a `Date` source.

> [!NOTE]
> 🎯 **RD-VBAL diverges from MS-VBAL** in the integral → floating-point block of **MS-VBAL §5.5.1.2.1**.
>
> MS-VBAL specifies that block as a verbatim copy of the preceding (narrowing) block, including the finite-value and
> banker's-rounding checks. No integer value can meet the finite-value and banker's-rounding conditions, and
> integral → floating-point is an unambiguously widening conversion.
>
> RD-VBAL treats integral → floating-point let-coercion as a plain widening copy.

### MS-VBAL divergence principle

**RDCore** implements the MS-VBAL type-coercion rules verbatim, except for the resolved specification errors noted in
this section ([**RD-VBAL §5.0** Semantics](rd-vbal.5.0.semantics.md)).

- Divergences from MS-VBAL caused by obvious copy/paste and transcription errors in the MS specification are resolved
  in favour of the evident intent.
- Anything in MS-VBAL that implicitly depends on the Windows Registry, ActiveX, or MSForms is out of scope for the
  RD-VBA run-time. MS-VBAL requirements with such an implicit dependency are resolved in favour of the evident intent.

### 5.5.1.2.1.1 Banker's rounding

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.5.1.2.1.1** Banker's rounding](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/98152b5a-4d86-4acb-b875-66cb1f49433e).

The rounding used when narrowing a floating-point or fixed-point value to an integral type is round-half-to-even
("banker's rounding").


## 5.5.1.2.2 Let-coercion to and from Boolean

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.5.1.2.2** Let-coercion to and from Boolean](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/3a9f5227-5fd5-4240-949a-51ffc32e71a9).

A `Date` source let-coerced to a `Boolean` destination reports `ConversionSemanticFlags.DateSerial`.
`VBBooleanLetCoercionRuntimeSemantics` reuses `VBNumericLetCoercionTypeRuntimeSemantics.DateSerialFlagsOf` for it
(§5.5.1.2.1).

A condition passes its own expression through to the `Boolean` let-coercion strategy, with no synthetic node standing
in for an operator that is not present in source
([**RD-VBAL §3.5.4** Execution](rd-vbal.3.5.4.execution.md)).


## 5.5.1.2.3 Let-coercion to and from Date

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.5.1.2.3** Let-coercion to and from Date](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/8a4a9201-4e7f-4856-b9fc-5927d2879723).

`VBDateLetCoercionRuntimeSemantics` runs only when `Date` is the destination type.

> 👉 A `Date` source let-coerced to a numeric or `Boolean` destination is handled by the numeric or `Boolean`
> strategy, which reports its `DateSerial` flag (§5.5.1.2.1, §5.5.1.2.2).


## 5.5.1.2.4 Let-coercion to and from String

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.5.1.2.4** Let-coercion to and from String](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/00113388-401b-41c2-8107-dc3fc0485554).

Floating-point value types define a `SignificantIntegerDigits`. It is used to represent floating-point values correctly
as [VBStringValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBStringValue.html) in conversions and coercions to
`String` ([**RD-VBAL §2.5.2.1.1** Numeric Values](rd-vbal.2.5.2.1.1.numeric-values.md)):

|Value type|Significant digits (`SignificantIntegerDigits`)|
|---|---|
|[VBSingleValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBSingleValue.html)|7|
|[VBDoubleValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBDoubleValue.html)|15|


## 5.5.1.2.5 Let-coercion to String \* length (fixed-length strings)

This section corresponds to [**MS-VBAL §5.5.1.2.5** Let-coercion to String * length (fixed-length strings)](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/442a2cdd-118a-4c0b-99d1-9b494633fabb).

> [!NOTE]
> Reserved. This section has no content yet.


## 5.5.1.2.6 Let-coercion to and from resizable Byte()

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.5.1.2.6** Let-coercion to and from resizable Byte()](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/acf31907-ebaf-4830-9892-bfabe6fe1416).

[VBResizableByteArrayType](../api/RDCore.SDK.Model.Types.VBResizableByteArrayType.html) has specific let-coercion
semantics attached, allowing implicit conversion to and from
[VBStringType](../api/RDCore.SDK.Model.Types.VBStringType.html)
([**RD-VBAL §2.4.1** Intrinsic Types](rd-vbal.2.4.1.intrinsic-types.md)).


## 5.5.1.2.7 Let-coercion to and from non-Byte arrays

This section corresponds to [**MS-VBAL §5.5.1.2.7** Let-coercion to and from non-Byte arrays](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/38315eed-8ea8-4e2a-b91c-25a96910407a).

> [!NOTE]
> Reserved. This section has no content yet.


## 5.5.1.2.8 Let-coercion to and from a UDT

This section corresponds to [**MS-VBAL §5.5.1.2.8** Let-coercion to and from a UDT](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/d5727cbc-068f-45f0-8119-7e7f13db9d54).

> [!NOTE]
> Reserved. This section has no content yet.


## 5.5.1.2.9 Let-coercion to and from Error

This section corresponds to [**MS-VBAL §5.5.1.2.9** Let-coercion to and from Error](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/a7ac9943-2198-46cf-9fb6-a3dd5e029e44).

> [!NOTE]
> Reserved. This section has no content yet.


## 5.5.1.2.10 Let-coercion from Null

This section corresponds to [**MS-VBAL §5.5.1.2.10** Let-coercion from Null](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/deab39e3-8dbf-4bbe-b3f5-5aa12c542fe6).

> [!NOTE]
> Reserved. This section has no content yet.


## 5.5.1.2.11 Let-coercion from Empty

This section corresponds to [**MS-VBAL §5.5.1.2.11** Let-coercion from Empty](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/e2637723-3010-449d-bead-9f17533dc105).

> [!NOTE]
> Reserved. This section has no content yet.


## 5.5.1.2.12 Let-coercion to Variant

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.5.1.2.12** Let-coercion to Variant](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/2ae5553a-6515-4967-9b91-e06b527b137f): `Variant` let-coercion and storage.

Any value except a class or `Nothing` let-coerces to `Variant` as a copy. A value let-coerced to `Variant` is wrapped
in a `VBVariantValue` ([**RD-VBAL §2.5.2.1.5** Variant Values](rd-vbal.2.5.2.1.5.variant-values.md)).

### Type information

A `VBVariantValue`'s own `TypeInfo` deliberately mirrors its wrapped value's `TypeInfo`. Ordinary destination-type
dispatch, in let-coercion and in operator and effective-type determination, therefore picks the same strategy it
would for the unwrapped value.

- The runtime instance of a let-coerced `Variant` stays a `VBVariantValue`. The C# instance of a `Variant` value is
  `VBVariantValue`, not the wrapped value's concrete type.
- A `VBVariantValue` wraps the whole value, not just a scalar.
- A `Variant` may wrap another `Variant`.

### Storage

Storage round-trips a `VBVariantValue` as a
[VBRuntimeVariantValue](../api/RDCore.SDK.Model.Values.Runtime.VBRuntimeVariantValue.html) box.
`VBRuntimeVariantValue` boxes the wrapped `VBTypedValue` of a `Variant` itself: storage holds the wrapped value inside
the box.

The `VBRuntimeVariantValue` box follows the same pattern a
[VBArrayValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBArrayValue.html) uses via
[VBRuntimeArrayValue](../api/RDCore.SDK.Model.Values.Runtime.VBRuntimeArrayValue.html)
([**RD-VBAL §2.5.2.1.2** Array Values](rd-vbal.2.5.2.1.2.array-values.md)).

> 👉 A `Variant` read back from a variable, array element, or field carries the exact value that was stored, never a
> fresh, unrelated `Empty`.

### Unwrapping

Because `TypeInfo` mirrors the wrapped value:

- Any code that short-circuits on a `TypeInfo` match must unwrap a `VBVariantValue` first.
- Any code that pattern-matches a `VBTypedValue` operand against a concrete value type directly must unwrap a
  `VBVariantValue` first.
- Unwrapping a `VBVariantValue` must be recursive, because a `Variant` may wrap another `Variant`.

Code that does not unwrap a `VBVariantValue` sees the box instead of the value.

|Site|`Variant` handling|See|
|---|---|---|
|`LetCoercionRuntimeSemanticsProvider.EvaluateLetCoercionSemantics`|Unwraps a `VBVariantValue` source once, centrally, for every let-coercion.|Let-coercion provider, above|
|`OperatorRuntimeSemantics.LetCoerceNonNullOperand`|Does not take its "already the right type" short-circuit for a `Variant` operand: the operand is let-coerced through the provider, which unwraps it.|[**RD-VBAL §5.6.9.2** Simple Data Operators](rd-vbal.5.6.9.2.simple-data-operators.md)|
|`SetCoercionRuntimeSemantics`|Unwraps a `VBVariantValue` before its own object pattern-match.|[**RD-VBAL §5.5.2.2** Runtime semantics](rd-vbal.5.5.2.2.runtime-semantics.md)|
|`RuntimeExpressionEvaluator.EvaluateIndex`|Unwraps a `VBVariantValue` before matching a wrapped array.|[**RD-VBAL §5.6.13** Index Expressions](rd-vbal.5.6.13.index-expressions.md)|
|`ProcedureExecutor.ExecuteForEachOpener`|Unwraps a `VBVariantValue` before matching a wrapped array.|[**RD-VBAL §5.4.2.4** For Each Statement](rd-vbal.5.4.2.4.for-each-statement.md)|
|`BinaryConcatOperatorRuntimeSemantics.IsByteArray`|Unwraps a `VBVariantValue` before matching a wrapped array.|[**RD-VBAL §5.6.9.4** & Operator](rd-vbal.5.6.9.4.ampersand-operator.md)|


## 5.5.1.2.13 Let-coercion to and from a class or Object or Nothing

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.5.1.2.13** Let-coercion to and from a class or Object or Nothing](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/9b8fc7b4-8329-497d-b2dd-2d0fa2b7b48c).

The default member of a class type can be implicitly invoked through let-coercion, yielding the _data value_ of the
object ([**RD-VBAL §2.4.2** Non-intrinsic Types](rd-vbal.2.4.2.non-intrinsic-types.md);
[**RD-VBAL §5.6.2** Expression Evaluation](rd-vbal.5.6.2.expression-evaluation.md)).

---
> ⏮️ [**RD-VBAL §5.5.1.1** Static semantics](rd-vbal.5.5.1.1.static-semantics.md) | ⏭️ [**RD-VBAL §5.5.2** Set-coercion](rd-vbal.5.5.2.set-coercion.md)
