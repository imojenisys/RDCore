# 6.1.2.7 Information

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §6.1.2.7** Information](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/fd00fc8f-ea7d-4c3b-b7a5-5e55377551e4).

The `Information` module is represented in the SDK by the interface
[IStdInformationModule](../api/RDCore.SDK.Runtime.Abstract.StdLib.IStdInformationModule.html).

The `Information` module is declared in full: every member of **MS-VBAL §6.1.2.7** has a declaration in
`IStdInformationModule`, and so does each of the two RD-VBA additions, `Erl` and `Err`. The module's symbols are read
off those declarations ([**RD-VBAL §6.0** Standard Library](rd-vbal.6.0.standard-library.md)).

> [!NOTE]
> **Not implemented.** Of the `Information` members, only `Erl` has a runtime implementation. Calling any other
> member raises the run-time error "Application-defined or object-defined error".


## 6.1.2.7.1 Public Functions

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §6.1.2.7.1** Public Functions](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/f0bab2d6-d9af-4bbc-9cad-18011c755afb).

`Erl` and `Err` are not members MS-VBAL lists under this module. They are numbered after the last MS-VBAL member of
this section.

|§|Member|Notes|
|---|---|---|
|6.1.2.7.1.1|[IMEStatus](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/4743ffad-82af-4f99-9335-de011458bb0c)||
|6.1.2.7.1.2|[IsArray](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/777dec25-8548-4603-873b-ec6fbdc66ad3)||
|6.1.2.7.1.3|[IsDate](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/adbaa664-a4b6-4a08-a1f0-8e3d7ea306b3)||
|6.1.2.7.1.4|[IsEmpty](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/f8585baa-abdc-42a0-a100-44e868ea49b5)||
|6.1.2.7.1.5|[IsError](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/9de72087-60d9-4ec8-ab52-b473760a83ed)||
|6.1.2.7.1.6|[IsMissing](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/9ec6f6f1-14a6-458e-9024-05dd0d9afb26)|Always `False` for a non-`Variant` parameter. See [**RD-VBAL §5.3.1.11** Procedure Invocation Argument Processing](rd-vbal.5.3.1.11.procedure-invocation-argument-processing.md).|
|6.1.2.7.1.7|[IsNull](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/7f9d8aa8-15cd-471b-be0a-4a889295fb42)||
|6.1.2.7.1.8|[IsNumeric](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/d920bc45-c366-4151-97dc-58c6a5b82780)||
|6.1.2.7.1.9|[IsObject](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/bcd5cbad-9d04-4b64-a098-c6031a6135d5)||
|6.1.2.7.1.10|[QBColor](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/4b7087c6-20fc-4d90-8d83-730a0c6a4aad)||
|6.1.2.7.1.11|[RGB](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/2a99cfd6-54fd-4e91-abd5-2aab8ee4b7c4)||
|6.1.2.7.1.12|[TypeName](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/713495ae-6a54-46f4-ad76-a7e7697803af)||
|6.1.2.7.1.13|[VarType](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/2deec703-c2ef-4bf1-a246-1d51c60895da)|Reports the values of `VbVarType`. See [**RD-VBAL §6.1.1** Predefined Enums](rd-vbal.6.1.1.predefined-enums.md).|
|6.1.2.7.1.14|🧩 `Erl`|RD-VBA addition. See [§6.1.2.7.1.14](#6127114-erl).|
|6.1.2.7.1.15|🧩 `Err`|RD-VBA addition. See [§6.1.2.7.1.15](#6127115-err).|

### 6.1.2.7.1.14 Erl

🧩 `Information.Erl` is an RD-VBA addition to MS-VBAL. `Erl` returns the line number at which the most recent
run-time error was raised.

MS-VBAL does not document `Erl` at all. MS-VBA hides `Erl` (it is a hidden member), but nonetheless exposes it from
the `Information` module.

🎯 `Erl` is a deliberate RD-VBA departure: RD-VBA widens a member MS-VBA got wrong. MS-VBA's answer is not a useful
one, in two separate ways (what it counts, and its resolution), and RD-VBA's `Erl` departs from MS-VBA's in both.

#### What Erl counts

MS-VBA's `Erl` counts the wrong thing: it reports the last line-number label execution passed. Hardly any code numbers
every line, so under MS-VBA a fault in an unnumbered statement is reported at whichever numbered line came before
it, however far back that is.

Under MS-VBA, a program that numbers no lines is told by `Erl` that every error happened at line `0`. MS-VBA's `Erl` is
truthful only where every single line is numbered, which is to say a BASIC program.

What RD-VBA's `Erl` counts is a setting of the environment, `ErlLineNumbering`, a member of
[IRuntimeEnvironmentProfile](../api/RDCore.SDK.Runtime.Abstract.Execution.IRuntimeEnvironmentProfile.html).
`ErlLineNumbering` is bound from `appsettings.json`, like the rest of the runtime profile
([**RD-VBAL §2.1** Implicit Storage](rd-vbal.2.1.implicit-storage.md)).

|`ErlLineNumbering` mode ([VBErlLineNumbering](../api/RDCore.SDK.Runtime.Abstract.Execution.VBErlLineNumbering.html))|`Erl` reports|
|---|---|
|`DocumentLine` (the default)|The line the faulting statement is really on, counted from `1` as an editor counts it.|
|`LineLabel`|The last line-number label at or before the faulting statement, or `0` when no line-number label precedes it. This is MS-VBA's answer, bug for bug.|

A named label never sets `Erl`, in either `ErlLineNumbering` mode. Only a label spelled as decimal digits sets `Erl`
([**RD-VBAL §5.4.1.1** Statement Labels](rd-vbal.5.4.1.1.statement-labels.md)).

`LineLabel` is the right answer for a numbered BASIC program, where the line-number label is the document line.
`LineLabel` mode exists for a workspace whose own code depends on MS-VBA's `Erl` behaviour.

#### Width

MS-VBA reports `Erl` too narrowly: with `ushort` resolution. MS-VBA's `Erl` wraps around on any line number that does
not fit, so a program numbered past line 65535 is told it faulted at a line it does not have.

🎯 RD-VBA's `Erl` returns a `Long`, for this reason. Because it returns a `Long`, every legal line number is
representable.

#### Implementation

The line number `Erl` reports is carried on
[ISessionErrorState](../api/RDCore.SDK.Runtime.Abstract.Execution.ISessionErrorState.html)`.LineNumber`. It is
captured at the one point every run-time error already reaches the session error state: the interpreter's
error-interception point ([**RD-VBAL §3.5.4** Execution](rd-vbal.3.5.4.execution.md)).

In `LineLabel` mode, [InstructionList](../api/RDCore.SDK.Semantics.Instructions.InstructionList.html)`.TryGetLineNumber`
answers the line number `Erl` reports ([**RD-VBAL §3.5.1** InstructionList](rd-vbal.3.5.1.instructionlist.md)).

### 6.1.2.7.1.15 Err

🧩 `Err` is not a member MS-VBAL lists under this module. RD-VBA implements the MS-VBA shape of `Err`: a zero-argument
`Function` of the `Information` module, returning an instance of the class `ErrObject`.

MS-VBAL describes the error object as the default instance of a global class module named `Err` instead. The two
shapes are indistinguishable from source: a standard module's members are promoted to the project scope, so a bare
`Err` yields the error object in either shape ([**RD-VBAL §2.3.1.3** Name Resolution](rd-vbal.2.3.1.3.name-resolution.md)).

The MS-VBA shape leaves `ErrObject` nameable in an `As` clause, instead of shadowed by its own default instance. The
`Err` function and the `ErrObject` class are described in [**RD-VBAL §6.1.3.2** Err Class](rd-vbal.6.1.3.2.err-class.md).

---
> ⏮️ [**RD-VBAL §6.1.2.6** Financial](rd-vbal.6.1.2.6.financial.md) | ⏭️ [**RD-VBAL §6.1.2.8** Interaction](rd-vbal.6.1.2.8.interaction.md)
