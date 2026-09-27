# 5.6.14 Dictionary Access Expressions

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.6.14** Dictionary Access Expressions](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/f20c9ebc-3365-4614-9788-1cd50a504574).

## Syntax

|Expression|AST node|
|---|---|
|Dictionary access|[DictionaryAccessExpressionNode](../api/RDCore.SDK.Model.AST.Expressions.DictionaryAccessExpressionNode.html)|

See [**RD-VBAL §3.0.2** Node Types](rd-vbal.3.0.2.node-types.md).

The token that follows the `!` operator in a dictionary access expression represents a dictionary key. This is why
the LSP `Key` symbol kind fits it. See [**RD-VBAL §2.5.1** Runtime Entities](rd-vbal.2.5.1.runtime-entities.md).

A `!member` expression with no owner, inside a `With` block, is a with-expression. See
[**RD-VBAL §5.6.15** With Expressions](rd-vbal.5.6.15.with-expressions.md).

---
> ⏮️ [**RD-VBAL §5.6.13** Index Expressions](rd-vbal.5.6.13.index-expressions.md) | ⏭️ [**RD-VBAL §5.6.15** With Expressions](rd-vbal.5.6.15.with-expressions.md)
