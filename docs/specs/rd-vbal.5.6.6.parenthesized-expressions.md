# 5.6.6 Parenthesized Expressions

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.6.6** Parenthesized Expressions](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/8c1b7427-670d-45e2-bc4b-39ab84ba5b20).

🧩 The `"__c()_op"` _explicit let-coercion operator_ is reserved for the case where the source code encloses the
coerced expression in parentheses. The parentheses explicitly force that _let-coercion_ frame. See
[**RD-VBAL §3.3.1** Unary Operators](rd-vbal.3.3.1.unary-operators.md) and
[**RD-VBAL §5.5.1** Let-coercion](rd-vbal.5.5.1.let-coercion.md).

---
> ⏮️ [**RD-VBAL §5.6.5** Literal Expressions](rd-vbal.5.6.5.literal-expressions.md) | ⏭️ [**RD-VBAL §5.6.7** TypeOf...Is Expressions](rd-vbal.5.6.7.typeof-is-expressions.md)
