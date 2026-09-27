# 5.3.1.5 Parameter Lists

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.3.1.5 Parameter Lists**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/78bcb344-4966-4401-bb55-72729790ebee).

## Syntax

A declared parameter is a [VBParameterSymbol](../api/RDCore.SDK.Model.Symbols.VBProject.VBParameterSymbol.html);
a trailing `ParamArray` parameter is a
[ParamArrayParameterSymbol](../api/RDCore.SDK.Model.Symbols.VBProject.ParamArrayParameterSymbol.html).

The parameter list of a `Property Let` or `Property Set` follows this **MS-VBAL §5.3.1.5** rule:

```
property-parameters = "(" [parameter-list ","] value-param ")"
```

## Static Semantics

In the `property-parameters` rule, `value-param` is never bracketed, so it is always mandatory, index
parameters or not. A `Property Let` or `Property Set` with no parameters has no value parameter to receive
what is being assigned, and raises [VBC09321](../diagnostics/vbc09321.md). See also
[**RD-VBAL §5.3.1.7** Property Declarations](rd-vbal.5.3.1.7.property-declarations.md).

> 👉 UDT values **MUST** be passed by reference (`ByRef`). See
> [**RD-VBAL §2.5.2.1.3** User-Defined Type (UDT) Values](rd-vbal.2.5.2.1.3.udt-values.md).

### Optional parameters

`VBParameterSymbol.DefaultValue` holds an `Optional` parameter's default value.

|Declaration|`VBParameterSymbol.DefaultValue`|
|---|---|
|`Optional` with an `= ...` default-value clause|The default value, pre-computed.|
|`Optional` with no `= ...` default-value clause|`null`|

When an `Optional` parameter has no argument mapped to it, the invocation uses `VBParameterSymbol.DefaultValue`
directly. When `DefaultValue` is `null`, it falls back to the parameter's declared type's default value. See
[**RD-VBAL §5.3.1.11** Procedure Invocation Argument Processing](rd-vbal.5.3.1.11.procedure-invocation-argument-processing.md).

### ParamArray parameters

How the arguments passed to a `ParamArray` parameter are collected is described in
[**RD-VBAL §5.3.1.11** Procedure Invocation Argument Processing](rd-vbal.5.3.1.11.procedure-invocation-argument-processing.md).

---
> ⏮️ [**RD-VBAL §5.3.1.4** Function Type Declarations](rd-vbal.5.3.1.4.function-type-declarations.md) | ⏭️ [**RD-VBAL §5.3.1.6** Subroutine and Function Declarations](rd-vbal.5.3.1.6.subroutine-and-function-declarations.md)
