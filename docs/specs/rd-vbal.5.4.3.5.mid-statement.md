# 5.4.3.5 Mid/MidB/Mid$/MidB$ Statement

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.4.3.5** Mid/MidB/Mid$/MidB$ Statement](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/2a8f3567-c8e0-4176-a802-cf2edeba425f).

## Syntax

|AST node|Instruction kind(s)|Notes|
|---|---|---|
|[MidStatementNode](../api/RDCore.SDK.Model.AST.Statements.MidStatementNode.html)|`Simple`|`Mid`, `Mid$`, `MidB` and `MidB$`. See [**RD-VBAL §3.4.2** Simple Statements](rd-vbal.3.4.2.simple-statements.md).|

`MidStatementNode` records the statement's spelling in two independent flags: `IsByteMode` distinguishes
`MidB`/`MidB$` from `Mid`/`Mid$`, and `IsStringInput` records the `$` suffix (`Mid$`/`MidB$`).

|Spelling|`IsByteMode`|`IsStringInput`|
|---|---|---|
|`Mid`|`false`|`false`|
|`Mid$`|`false`|`true`|
|`MidB`|`true`|`false`|
|`MidB$`|`true`|`true`|

The replacement-span mechanics of **MS-VBAL §5.4.3.5** split only on byte mode (`MidB`/`MidB$` versus
`Mid`/`Mid$`), never on the `$` suffix. `IsStringInput` is preserved because it mirrors the `VBVariant`/`VBString`
split of the `Mid`/`Mid$` function overloads, which matters for static semantics.

## Static Semantics

> [!NOTE]
> Reserved. This section has no content yet.

## Runtime Semantics

> [!NOTE]
> Reserved. This section has no content yet.

---
> ⏮️ [**RD-VBAL §5.4.3.4** Erase Statement](rd-vbal.5.4.3.4.erase-statement.md) | ⏭️ [**RD-VBAL §5.4.3.6** LSet Statement](rd-vbal.5.4.3.6.lset-statement.md)
