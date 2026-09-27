# 6.1.3.1 Collection Object

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §6.1.3.1** Collection Object](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/31ec9e63-f71e-4521-863b-a7d12007b7cc).

The `Collection` class is represented in the SDK by the interface
[IStdCollectionClass](../api/RDCore.SDK.Runtime.Abstract.StdLib.IStdCollectionClass.html).

A `For Each` statement finds an object's enumeration member through
[VBReturningMemberSymbol](../api/RDCore.SDK.Model.Symbols.Abstract.VBReturningMemberSymbol.html) /
[SymbolProperties](../api/RDCore.SDK.Model.Symbols.Abstract.SymbolProperties.html)`.UserMemId` /
[WellKnownDispIds](../api/RDCore.SDK.Model.WellKnownDispIds.html)`.NewEnum`. This is the same lookup the constructor of
[VBCollectionType](../api/RDCore.SDK.Model.Types.Complex.VBCollectionType.html) uses
([**RD-VBAL §5.4.2.4** For Each Statement](rd-vbal.5.4.2.4.for-each-statement.md)).


## 6.1.3.1.1 Public Functions

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §6.1.3.1.1** Public Functions](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/3f2c31e1-524e-415c-823c-dc9d51bf9b7e).

|§|Member|Notes|
|---|---|---|
|6.1.3.1.1.1|[Count](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/6cfa9558-1337-4871-be64-1dcef32fde8d)||
|6.1.3.1.1.2|[Item](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/2cc7e3b8-a9b6-4f1e-9b42-960e67c45abe)||


## 6.1.3.1.2 Public Subroutines

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §6.1.3.1.2** Public Subroutines](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/8f97f8a0-e0a8-430a-8601-330d589b8e3b).

|§|Member|Notes|
|---|---|---|
|6.1.3.1.2.1|[Add](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/07e970d8-0fb5-461c-bb6f-7960e8c45699)||
|6.1.3.1.2.2|[Remove](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/93d5dcdd-91a5-4fae-9ba3-4472c55faf69)||

---
> ⏮️ [**RD-VBAL §6.1.3** Predefined Class Modules](rd-vbal.6.1.3.predefined-class-modules.md) | ⏭️ [**RD-VBAL §6.1.3.2** Err Class](rd-vbal.6.1.3.2.err-class.md)
