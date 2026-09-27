# 5.3.1.2 Static Procedures

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §5.3.1.2** Static Procedures](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/125068ec-a57e-4296-843b-5d009169de2f).

In a procedure declared `Static`, every one of its local variables has module extent, not just the ones
declared with an explicit `Static` keyword.

> [!NOTE]
> **Not implemented.** A whole procedure declared `Static` is not modeled. A local variable has module extent
> only when it is declared with an explicit `Static` keyword; see
> [**RD-VBAL §5.4.3.1** Local Variable Declarations](rd-vbal.5.4.3.1.local-variable-declarations.md).

Without whole-procedure `Static`, RD-VBA's `Static` handling is narrower than MS-VBAL, but it is not wrong
for what it covers.

---
> ⏮️ [**RD-VBAL §5.3.1.1** Procedure Scope](rd-vbal.5.3.1.1.procedure-scope.md) | ⏭️ [**RD-VBAL §5.3.1.3** Procedure Names](rd-vbal.5.3.1.3.procedure-names.md)
