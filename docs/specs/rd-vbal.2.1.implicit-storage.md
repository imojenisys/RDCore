# 2.1 Implicit Storage

MS-VBAL explicitly presumes an _implementation-dependent_ storage mechanism outside the scope of its own specification. **RD-VBAL explicitly specifies these mechanisms**. This decouples the _language semantics_ from _implementation-dependent storage_.

The _environment host_ is responsible for configuring the implicit storage of the host VBA environment, and for loading any _application settings_ and additional _workspace resources_ into the runtime environment (see [**RD-VBAL §2.3.1** Composition Root](rd-vbal.2.3.1.composition-root.md)).

## 2.1.1 Application Settings

MS-VBAL addresses legitimate **application configuration** concerns through a _get-only_ API exposed in the _standard library_:

|Function|Specification|SDK member|
|---|---|---|
|`GetAllSettings`|[**MS-VBAL §6.1.2.8.1.7** GetAllSettings](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/a2b5707d-9a11-4cca-9920-3a2b7e7ff518)|[`IStdInteractionModule.StdInteraction__GetAllSettings`](../api/RDCore.SDK.Runtime.Abstract.StdLib.IStdInteractionModule.html#RDCore_SDK_Runtime_Abstract_StdLib_IStdInteractionModule_StdInteraction__GetAllSettings_RDCore_SDK_Model_Values_Intrinsic_VBStringValue_RDCore_SDK_Model_Values_Intrinsic_VBStringValue_)|
|`GetSetting`|[**MS-VBAL §6.1.2.8.1.10** GetSetting](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/7e55c0a6-2fc2-425c-9f4c-be88cff4f629)|[`IStdInteractionModule.StdInteraction__GetSetting`](../api/RDCore.SDK.Runtime.Abstract.StdLib.IStdInteractionModule.html#RDCore_SDK_Runtime_Abstract_StdLib_IStdInteractionModule_StdInteraction__GetSetting_RDCore_SDK_Model_Values_Intrinsic_VBStringValue_RDCore_SDK_Model_Values_Intrinsic_VBStringValue_RDCore_SDK_Model_Values_Intrinsic_VBStringValue_RDCore_SDK_Model_Values_Intrinsic_VBVariantValue_)|

RD-VBA keeps backward compatibility by keeping an implementation of this API backed by the _Windows Registry_. RD-VBA application settings are not inherently constrained to the _Windows Registry_, however.

RD-VBA therefore adds functions managing _workspace application settings_, using a similar API:

|Function|Specification|SDK member|
|---|---|---|
|🧩 `GetJsonSettings`|**RD-VBAL §6.1.2.8.1.7.1** (see [**RD-VBAL §6.1.2.8** Interaction](rd-vbal.6.1.2.8.interaction.md))|[`IStdInteractionModule.StdInteraction__GetJsonSettings`](../api/RDCore.SDK.Runtime.Abstract.StdLib.IStdInteractionModule.html#RDCore_SDK_Runtime_Abstract_StdLib_IStdInteractionModule_StdInteraction__GetJsonSettings_RDCore_SDK_Model_Values_Intrinsic_VBStringValue_RDCore_SDK_Model_Values_Intrinsic_VBStringValue_)|
|🧩 `GetJsonSetting`|**RD-VBAL §6.1.2.8.1.10.1** (see [**RD-VBAL §6.1.2.8** Interaction](rd-vbal.6.1.2.8.interaction.md))|[`IStdInteractionModule.StdInteraction__GetJsonSetting`](../api/RDCore.SDK.Runtime.Abstract.StdLib.IStdInteractionModule.html#RDCore_SDK_Runtime_Abstract_StdLib_IStdInteractionModule_StdInteraction__GetJsonSetting_RDCore_SDK_Model_Values_Intrinsic_VBStringValue_RDCore_SDK_Model_Values_Intrinsic_VBStringValue_RDCore_SDK_Model_Values_Intrinsic_VBVariantValue_)|

Whether **any** _standard library_ calls implicate actual or simulated _Windows Registry_ reads is entirely **implementation-dependent**. Such calls may behave differently on different platforms. This remains entirely compliant with the relevant MS-VBAL sections as specified.

> [!IMPORTANT]
> The _host environment_ **may** expose configuration settings that set the implicit storage of `GetAllSettings` and `GetSetting` to _workspace application settings_. These functions then work exactly as if they were invoking `GetJsonSettings` and `GetJsonSetting`, respectively.

### 2.1.1.1 Workspace Application Settings

The MS-VBAL-specified settings API (`GetSetting`, `GetAllSettings`) would work perfectly fine as-is for workspace application settings. Distinctly separate functions (`GetJsonSettings`, `GetJsonSetting`) were nevertheless introduced in RD-VBAL, to maintain backward compatibility without modifying any existing signatures.

As a result:

- the _legacy_ `GetSetting`/`GetAllSettings` API maintains its MS-VBA behavior;
- RD-VBA applications can use the `GetJsonSettings` API, which brings application configuration on par with any other managed (.NET) configuration scheme.

|Rule|Description|
|---|---|
|Location|A _workspace_ may include one or more `appsettings.json` file(s), at its root or under any of its subfolders.|
|File name|A configuration file may be named differently: `appsettings.json` is a language platform default, and this default is configurable.|
|Binding|The _application host_ (`rdc.exe`) is responsible for binding the workspace configuration as the application is _composed_, before it starts executing.|

Workspace application settings have the full power and flexibility of a .NET managed `IConfigurationBuilder` underneath.

The `ErlLineNumbering` environment setting is bound from `appsettings.json`, with the rest of the runtime profile (see **RD-VBAL §6.1.2.7.1.14** `Erl`, in [**RD-VBAL §6.1.2.7** Information](rd-vbal.6.1.2.7.information.md)).

---
> ⏮️ [**RD-VBAL §2.0.2** Client/Server Capabilities](rd-vbal.2.0.2.client-server-capabilities.md) | ⏭️ [**RD-VBAL §2.2** RDPROJ Structure](rd-vbal.2.2.rdproj-structure.md)
