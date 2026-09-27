# 5.6.13 Index Expressions

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.6.13** Index Expressions](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/551030b2-72a4-4c95-9cb0-fb8f8c8774b4).

## Syntax

|Expression|AST node|
|---|---|
|Index|[IndexExpressionNode](../api/RDCore.SDK.Model.AST.Expressions.IndexExpressionNode.html)|

See [**RD-VBAL §3.0.2** Node Types](rd-vbal.3.0.2.node-types.md).

## Runtime Semantics

`RuntimeExpressionEvaluator.EvaluateIndex` evaluates an index expression.

### Procedure Callee

An `IndexExpressionNode` whose `Callee` is a bare name resolving to a `Sub`/`Function`/`Property Get` is checked for
that before the usual recursive `Evaluate(Callee)`. Such an index expression invokes the procedure with the index
expression's own arguments. See
[**RD-VBAL §5.3.1.11** Procedure Invocation Argument Processing](rd-vbal.5.3.1.11.procedure-invocation-argument-processing.md).

The check precedes `Evaluate(Callee)` because, otherwise, a bare-name `Callee` would already have been auto-invoked
with zero arguments by `SimpleName`'s own dispatch, before the index expression could supply its arguments. See
[**RD-VBAL §5.6.10** Simple Name Expressions](rd-vbal.5.6.10.simple-name-expressions.md).

An index expression with a procedure `Callee` is the only shape that recurses (e.g. `Foo(n - 1)`, even from within
`Foo`'s own body). A bare `Foo`, from within `Foo`'s own body, reads the function result variable instead
([**RD-VBAL §5.6.10** Simple Name Expressions](rd-vbal.5.6.10.simple-name-expressions.md)).

### Variant Holding an Array

`RuntimeExpressionEvaluator.EvaluateIndex` unwraps a `Variant` holding an array, so `v(0)` indexes a `Variant` holding
an array the same as a declared array. It unwraps a
[VBVariantValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBVariantValue.html) before matching a wrapped
[VBArrayValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBArrayValue.html). See
[**RD-VBAL §5.5.1.2** Runtime semantics](rd-vbal.5.5.1.2.runtime-semantics.md) (Let-coercion to Variant).


## 5.6.13.1 Argument Lists

This section corresponds to [**MS-VBAL §5.6.13.1** Argument Lists](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/5b35d806-1305-4427-a120-d25a71c45c02).

> [!NOTE]
> Reserved. This section has no content yet.

See [**RD-VBAL §5.3.1.11** Procedure Invocation Argument Processing](rd-vbal.5.3.1.11.procedure-invocation-argument-processing.md#argument-mapping).


## 5.6.13.2 Argument List Queues

This section corresponds to [**MS-VBAL §5.6.13.2** Argument List Queues](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/9a66cbdb-d2af-40e4-b6f2-b63bcde2a1f3).

> [!NOTE]
> Reserved. This section has no content yet.

---
> ⏮️ [**RD-VBAL §5.6.12** Member Access Expressions](rd-vbal.5.6.12.member-access-expressions.md) | ⏭️ [**RD-VBAL §5.6.14** Dictionary Access Expressions](rd-vbal.5.6.14.dictionary-access-expressions.md)
