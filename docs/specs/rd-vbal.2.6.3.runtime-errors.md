# 2.6.3 Runtime Errors

A **runtime error** is raised by the runtime semantics layer and left unhandled by workspace code. Examples are a subscript out of range, a type-mismatch coercion, and division by zero.

|||
|---|---|
|Code family|`VBR`; the numeric portion matches the corresponding MS-VBA run-time error code|
|Title|_Run-time error_|
|Raised by|the runtime semantics layer|
|Source metadata|[VBRuntimeErrorInfo](../api/RDCore.SDK.Model.Errors.VBRuntimeErrorInfo.html); its identifier is a [VBRuntimeErrorId](../api/RDCore.SDK.Model.Errors.VBRuntimeErrorId.html) whose numeric value corresponds to its MS-VBAL specified error code|
|Severity|`Error`|

Regardless of the error message content, RD-VBA must still raise the MS-VBA equivalent error code in the relevant contexts (e.g. `VBR00461` `MethodOrDataMemberNotFound`, whose message uses the term "data member"; see [**RD-VBAL §2.4.2** Non-intrinsic Types](rd-vbal.2.4.2.non-intrinsic-types.md)).

`InternalError` is never reported for a language-level condition that a well-formed program meets; such a condition raises a run-time error.

On a run-time error, the interactive shell renders an icon, the title with the program's own line number, the diagnostic code and description, `Err.Source`, and the stack trace; see [**RD-VBAL §2.0.2** Client/Server Capabilities](rd-vbal.2.0.2.client-server-capabilities.md).

## Run-time Errors Referenced by Statement Semantics

|Error|`VBRuntimeErrorId`|Raised when|See|
|---|---|---|---|
|13|`TypeMismatch`|`For Each` over anything other than an array or an object (a scalar)|[**RD-VBAL §5.4.2.4** For Each Statement](rd-vbal.5.4.2.4.for-each-statement.md)|
|91|`ObjectVariableOrWithBlockVariableNotSet`|`For Each` over `Nothing` (invoking `_NewEnum` on an unset reference)|[**RD-VBAL §5.4.2.4** For Each Statement](rd-vbal.5.4.2.4.for-each-statement.md)|
|92, "For loop not initialized"|`ForLoopNotInitialized`|a `ForNext` or `ForEachNext` with no stored loop state (a `GoTo` landing directly on the closer), or a `For Each` over an array that was never dimensioned|[**RD-VBAL §5.4.2.3** For Statement](rd-vbal.5.4.2.3.for-statement.md), [**RD-VBAL §5.4.2.4** For Each Statement](rd-vbal.5.4.2.4.for-each-statement.md)|
|438|`ObjectDoesntSupportThisPropertyOrMethod`|`For Each` over a live object with no `VB_UserMemId = -4` member|[**RD-VBAL §5.4.2.4** For Each Statement](rd-vbal.5.4.2.4.for-each-statement.md)|

The verbose message for `ForLoopNotInitialized` is the resx entry `VBForLoopNotInitialized_Verbose` when a `Next` closer runs without its opener, and `VBForEach_ArrayNotInitialized_Verbose` when a `For Each` enumerates a never-dimensioned array; both are provided in both languages.

## Application Errors

An **application error** is a custom run-time error explicitly raised from workspace source code with `Error` or `Err.Raise`; see [**RD-VBAL §5.4.4.3** Error Statement](rd-vbal.5.4.4.3.error-statement.md) and [**RD-VBAL §6.1.3.2** Err Class](rd-vbal.6.1.3.2.err-class.md). MS-VBAL does not distinguish an application error from a semantic run-time error.

|||
|---|---|
|Code family|`VBA`, a pseudo-code; the numeric portion matches the application-supplied error code|
|Title|_Application error_|
|Raised by|workspace source code (`Error` or `Err.Raise`)|
|Source metadata|[VBApplicationErrorInfo](../api/RDCore.SDK.Model.Errors.VBApplicationErrorInfo.html)|
|Severity|`Error`|

RDCore removes the need for the `vbObjectError` constant by internally representing run-time errors and application errors as different error metadata types; see [**RD-VBAL §1.1.4** Core Diagnostics](rd-vbal.1.1.4.core-diagnostics.md).

---
> ⏮️ [**RD-VBAL §2.6.2** Semantic Compilation Errors](rd-vbal.2.6.2.semantic-compilation-errors.md) | ⏭️ [**RD-VBAL §2.6.4** Rubberduck Core Diagnostics](rd-vbal.2.6.4.rubberduck-core-diagnostics.md)
