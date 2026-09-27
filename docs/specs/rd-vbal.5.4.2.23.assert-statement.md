# 5.4.2.23 Assert Statement

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.4.2.23 Assert Statement**](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/4deae985-1f0b-4a95-9b69-7c69a91ea7a1).

## Syntax

|AST node|Instruction kind|Notes|
|---|---|---|
|[DebugAssertStatementNode](../api/RDCore.SDK.Model.AST.Statements.DebugAssertStatementNode.html)|`Simple`|Represents `Debug.Assert`.|

## Static Semantics

> [!NOTE]
> Reserved. This section has no content yet.

## Runtime Semantics

A failed `Assert` call is a semantic break: it enters `Break` mode
([**RD-VBAL §2.3.2** Mode / State](rd-vbal.2.3.2.mode-state.md)).

---
> ⏮️ [**RD-VBAL §5.4.2.22** End Statement](rd-vbal.5.4.2.22.end-statement.md) | ⏭️ [**RD-VBAL §5.4.3** Data Manipulation Statements](rd-vbal.5.4.3.data-manipulation-statements.md)
