# 2.3.1.2 Session Services

An _execution session_'s services are rooted at an
[IRuntimeSession](../api/RDCore.SDK.Runtime.Abstract.Execution.IRuntimeSession.html).

## IRuntimeSession

`IRuntimeSession` exposes:

|Exposes|Description|
|---|---|
|`Environment.Is64Bit`|The environment bitness, on the session's [IRuntimeEnvironmentProfile](../api/RDCore.SDK.Runtime.Abstract.Execution.IRuntimeEnvironmentProfile.html). It also determines the value of the `#If Win64` pre-compiler directive. `#If VBA7` does not depend on it: the environment host defines `VBA7` as true in either bitness.|
|`References`|The workspace's project and library references ([References](#references), below).|
|The three session services|`ISessionMemoryAllocator`, `ISessionSymbols` and `ISessionObjects` ([Services](#services), below).|

## Services

|Service|Responsibility|Members|
|---|---|---|
|[ISessionMemoryAllocator](../api/RDCore.SDK.Runtime.Abstract.Execution.ISessionMemoryAllocator.html)|Allocates and frees blocks in the session's memory space, and reports allocation / fragmentation statistics.|`TryAllocate`, `TryDeallocate`, `Info`|
|[ISessionSymbols](../api/RDCore.SDK.Runtime.Abstract.Execution.ISessionSymbols.html)|The session's symbol table.|`TryDefine` defines a [Symbol](../api/RDCore.SDK.Model.Symbols.Abstract.Symbol.html) in a scope. `TryResolveValue` resolves a name visible from a scope; `TryResolveType` is its type binding context counterpart. `Resolver` is the session's symbol resolver.|
|[ISessionObjects](../api/RDCore.SDK.Runtime.Abstract.Execution.ISessionObjects.html)|Object lifetime.|`CreateObject`; `AddRef` / `RemoveRef` (reference counting); `TryRemoveObject` removes an instance whose reference count has reached zero.|

`ISessionMemoryAllocator` is an _accounting_ layer: it tracks sizes and addresses, MSVBVM-style. It does not hold
the values themselves.

`ISessionSymbols` mirrors the `ISymbolResolver.ResolveValue` / `ResolveType` pair as `TryResolveValue` and
`TryResolveType`. `ISessionSymbols.Resolver` is a `ScopeTreeSymbolResolver` over the session's own symbols, rebuilt
as symbols are defined. See [**RD-VBAL §2.3.1.3** Name Resolution](rd-vbal.2.3.1.3.name-resolution.md).

For object values and their lifetime, see
[**RD-VBAL §2.5.2.1.4** Object Values](rd-vbal.2.5.2.1.4.object-values.md).

## References

`IRuntimeSession.References` is the workspace's project and library references, as an ordered
`IReadOnlyList<ReferencePriorityInfo>`. It is the runtime-facing view of the `.rdproj` _RDCoreReference_ list
([**RD-VBAL §2.2.3.2** RDCoreReference](rd-vbal.2.2.3.projectfile.md)).

Each [ReferencePriorityInfo](../api/RDCore.SDK.Model.Symbols.ReferencePriorityInfo.html) entry carries only:

|Member|Description|
|---|---|
|`Name`|The reference's source-visible name.|
|`Priority`|The reference's rank in the `IRuntimeSession.References` list.|

The list is the _reference priority_ order defined for global-scope name resolution
([**RD-VBAL §2.3.1.3** Name Resolution](rd-vbal.2.3.1.3.name-resolution.md)). It preserves that order exactly as the
language server provides it.

A referenced library's own members are not taken from this list: they are contributed by an
[ISymbolProvider](../api/RDCore.SDK.Runtime.Abstract.Execution.ISymbolProvider.html) (see [below](#isymbolprovider))
and resolved through `ISymbolResolver`.

Name resolution across referenced projects and libraries consults the ordering to disambiguate a global-scope name.
The name-resolution algorithm itself is a separate concern from the list
([**RD-VBAL §2.3.1.3** Name Resolution](rd-vbal.2.3.1.3.name-resolution.md)).

> [!NOTE]
> **Not implemented.** Reference-priority ordering within the global scope is not implemented. The ordering is
> carried on `IRuntimeSession.References`, but nothing consults it.

## ISymbolProvider

`ISymbolProvider` exposes a single `ProvideSymbols` method, which yields the `Symbol`s its source defines. The
composition root ([**RD-VBAL §2.3.1** Composition Root](rd-vbal.2.3.1.composition-root.md)) then defines each provided `Symbol`:

|Context|Each provided `Symbol` is defined into|
|---|---|
|Static|The semantic layer.|
|Runtime|The session symbol table, through `ISessionSymbols.TryDefine`.|

`ISymbolProvider` is the abstraction behind the several _symbol providers_ a session is composed from:

|Symbol provider source|
|---|
|Configuration flags.|
|AST declarations.|
|Reflected referenced libraries.|
|The environment host's own runtime and standard library ([**RD-VBAL §6.0** Standard Library](rd-vbal.6.0.standard-library.md)).|

## Heaps

The session's `ISessionSymbols` and `ISessionObjects` implementations should maintain the following internal
structures:

|Internal structure|Holds|
|---|---|
|_Global heap_|An [IBindingHandle](../api/RDCore.SDK.Model.Values.Bindings.IBindingHandle.html) for any globally-scoped `Symbol`.|
|_Workspace heap_|An `IBindingHandle` for any workspace-scoped `Symbol`.|
|_Static locals heap_|An `IBindingHandle` for any module-scoped `Symbol`.|
|_Object heap_|The `Symbol` references and their associated bindings for any [VBObjectValue](../api/RDCore.SDK.Model.Values.Intrinsic.VBObjectValue.html).|
|_Symbol table_|A map of a `Uri` to its associated `Symbol`.|
|_Name table_|The current representation (casing) of all loaded symbols.|

A `Symbol`'s scope kind determines exactly how, and whether, the symbol is allocated in memory: a
`ScopeKind.Global` symbol lives in the globals heap, a `ScopeKind.Module` symbol in the workspace statics heap, and
a `ScopeKind.Instance` symbol in the object heap. A `Static` local's storage lives in the same heap tier a module
field uses ([**RD-VBAL §5.4.3.1** Local Variable Declarations](rd-vbal.5.4.3.1.local-variable-declarations.md)).

See [**RD-VBAL §2.5.1** Runtime Entities](rd-vbal.2.5.1.runtime-entities.md) for `ScopeKind` and the allocation scopes.

The order in which names are looked up in these heaps is specified in
[**RD-VBAL §2.3.1.3** Name Resolution](rd-vbal.2.3.1.3.name-resolution.md).

## Memory addressing

`ISessionMemoryAllocator` maintains an internal _address pointer_ that tracks the current _memory offset_.

The current memory address pointer should be incremented by a _host-defined_ `IntPtrSize`, which represents the
size of a pointer in the current environment (32 or 64 bits).

The _memory map_ (`MemoryAddress` → `IBindingHandle`) is held by the session storage (`SessionStorage`, below):
it binds the `IBindingHandle` of a value to the
[MemoryAddress](../api/RDCore.SDK.Runtime.Shared.MemoryAddress.html) reserved for it.

> [!NOTE]
> **Not implemented.** The _raw address map_ (`MemoryAddress` → `Uri`) is not implemented.

## Session storage

> [!NOTE]
> **Not implemented.** A bound value is not stored as a byte block at its address: an `IBindingHandle` is itself
> the value bound to a `Symbol`.

A symbol's storage allocation is performed by `SymbolAddressTable`, which
[ICallStackFrame](../api/RDCore.SDK.Runtime.Abstract.Execution.ICallStackFrame.html) and the module/global resolver
share ([**RD-VBAL §2.5.2.1.2** Array Values](rd-vbal.2.5.2.1.2.array-values.md)).
`SymbolAddressTable.FreshBinding` re-boxes a `Variant`'s wrapped value into a fresh
[VBRuntimeVariantValue](../api/RDCore.SDK.Model.Values.Runtime.VBRuntimeVariantValue.html) on every store
([**RD-VBAL §2.5.2.1.5** Variant Values](rd-vbal.2.5.2.1.5.variant-values.md)).

### Zero-size storage

`ISessionMemoryAllocator` refuses a zero-size allocation; this is a hardened invariant. A 0-byte
bump-pointer/free-list allocation would hand the same address to the next caller.

`SessionStorage.TryAllocate` mints its own address for a non-positive size, instead of passing the request to the
session memory allocator: the allocator is never reached for a non-positive size. The following values have a
non-positive storage size, and all of them take this path alike:

|Value with a non-positive storage size|
|---|
|`Nothing`|
|`Null`|
|`Empty`|
|An uninitialized array|
|An empty `ParamArray` array|

Such a value still gets a binding resolvable by name, but never real memory.

The addresses `SessionStorage.TryAllocate` mints for non-positive sizes are negative, so that they can never collide
with, or be mistaken for, an allocator address.

Zero-size storage is supported because a `ParamArray` call with nothing left over is the single most common
`ParamArray` call shape: `Call Callee(100)` against a `ParamArray rest()` parameter with no arguments left over
collects an empty array whose `Size` is 0
([**RD-VBAL §5.3.1.11** Procedure Invocation Argument Processing](rd-vbal.5.3.1.11.procedure-invocation-argument-processing.md)).

## Call stack and file channels

The session's call stack, `RuntimeCallStack`, enforces a call-depth limit, in `OnBeforeTryPush`
([**RD-VBAL §5.3.1.11** Procedure Invocation Argument Processing](rd-vbal.5.3.1.11.procedure-invocation-argument-processing.md)).
Of the GoSub Resumption List, the SDK interface `ICallStackFrame` exposes only `GoSubDepth`, a count of its entries, and not the push/pop mutators
([**RD-VBAL §3.5.4** Execution](rd-vbal.3.5.4.execution.md)).

All file statements run through one session-level shim, the
[IFileChannels](../api/RDCore.SDK.Runtime.Abstract.Execution.IFileChannels.html) /
[IFileChannel](../api/RDCore.SDK.Runtime.Abstract.Execution.IFileChannel.html) interfaces
([**RD-VBAL §5.4.5** File Statements](rd-vbal.5.4.5.file-statements.md)).

## Thread safety

The **RDCore** implementations (⚖️ GPLv3) of the session services are intended to be _thread-safe_.

RD-VBA normally executes on a single thread, but the RD-VBA runtime implementation is not _inherently_
single-threaded. Whether an RD-VBA _environment host_ supports the concurrent execution of RD-VBA execution
threads is _host-dependent_.

---
> ⏮️ [**RD-VBAL §2.3.1.1** Execution Session](rd-vbal.2.3.1.1.execution-session.md) | ⏭️ [**RD-VBAL §2.3.1.3** Name Resolution](rd-vbal.2.3.1.3.name-resolution.md)
