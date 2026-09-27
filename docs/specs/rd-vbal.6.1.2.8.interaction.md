# 6.1.2.8 Interaction

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §6.1.2.8** Interaction](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/9a7e0f79-3164-41dd-beda-bcc02dc79d1f).

The `Interaction` module is represented in the SDK by the interface
[IStdInteractionModule](../api/RDCore.SDK.Runtime.Abstract.StdLib.IStdInteractionModule.html).

The application settings members (`GetAllSettings`, `GetSetting`, and the RD-VBA variants `GetJsonSettings` and
`GetJsonSetting`) are described with the implicit storage they read, in
[**RD-VBAL §2.1** Implicit Storage](rd-vbal.2.1.implicit-storage.md). RD-VBA keeps the MS-VBA behaviour of
`GetAllSettings` and `GetSetting`, backed by the Windows Registry, for backward compatibility.


## 6.1.2.8.1 Public Functions

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §6.1.2.8.1** Public Functions](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/5253bced-81cd-4a45-a4da-0270224bf0e5).

`GetJsonSettings` and `GetJsonSetting` are RD-VBA variants of `GetAllSettings` and `GetSetting`, and take a child
number of the member they vary.

|§|Member|Notes|
|---|---|---|
|6.1.2.8.1.1|[CallByName](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/b195f168-fb71-428d-b12d-1ab490ba64a4)||
|6.1.2.8.1.2|[Choose](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/2f178971-8a8b-4d07-a21e-d0fa82b9db7c)||
|6.1.2.8.1.3|[Command](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/03942db8-2ce2-46f5-b7dc-d32779b89101)||
|6.1.2.8.1.4|[CreateObject](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/a2040e64-6724-4bf6-a496-7ef01ec9af31)||
|6.1.2.8.1.5|[DoEvents](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/83c90f91-96e7-421e-8faf-a07b1c0cbb68)||
|6.1.2.8.1.6|[Environ / Environ$](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/efa62d49-87ea-4b55-949d-69a7fc7e08bb)||
|6.1.2.8.1.7|[GetAllSettings](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/a2b5707d-9a11-4cca-9920-3a2b7e7ff518)|Backed by the Windows Registry, unless the host sets its implicit storage to _workspace application settings_. See [**RD-VBAL §2.1** Implicit Storage](rd-vbal.2.1.implicit-storage.md).|
|6.1.2.8.1.7.1|🧩 `GetJsonSettings`|RD-VBA variant of `GetAllSettings` over _workspace application settings_. See [**RD-VBAL §2.1** Implicit Storage](rd-vbal.2.1.implicit-storage.md).|
|6.1.2.8.1.8|[GetAttr](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/f4c6a91b-d66d-4eb7-878d-24974d8e80ac)||
|6.1.2.8.1.9|[GetObject](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/74281353-20cb-4a12-8deb-47839d290132)||
|6.1.2.8.1.10|[GetSetting](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/7e55c0a6-2fc2-425c-9f4c-be88cff4f629)|Backed by the Windows Registry, unless the host sets its implicit storage to _workspace application settings_. See [**RD-VBAL §2.1** Implicit Storage](rd-vbal.2.1.implicit-storage.md).|
|6.1.2.8.1.10.1|🧩 `GetJsonSetting`|RD-VBA variant of `GetSetting` over _workspace application settings_. See [**RD-VBAL §2.1** Implicit Storage](rd-vbal.2.1.implicit-storage.md).|
|6.1.2.8.1.11|[IIf](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/3593c073-c770-4060-8416-aaf7545ea5fe)||
|6.1.2.8.1.12|[InputBox](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/64b763ff-9f5f-43cd-b247-c75557e51989)||
|6.1.2.8.1.13|[MsgBox](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/ba0375ee-936a-482c-8a9f-e9fa4515d191)||
|6.1.2.8.1.14|[Partition](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/44e5d3f2-6126-4a0e-b047-de2d546995c4)||
|6.1.2.8.1.15|[Shell](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/70501cdb-0322-4e1f-915f-819dced09534)||
|6.1.2.8.1.16|[Switch](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/7136c7fd-223c-4870-b2ba-9fe2cc9a2baa)||


## 6.1.2.8.2 Public Subroutines

> [!NOTE]
> This section describes the implementation of [**MS-VBAL §6.1.2.8.2** Public Subroutines](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/9af0f4b6-9d30-4c6c-a8f1-fb32903f8142).

|§|Member|Notes|
|---|---|---|
|6.1.2.8.2.1|[AppActivate](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/6e739dc6-7eea-4f95-ba0a-ef632e2608ba)||
|6.1.2.8.2.2|[Beep](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/f154435f-5eb6-4300-a1c7-61b925f0cf0a)||
|6.1.2.8.2.3|[DeleteSetting](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/295c31a2-a9f6-4326-b69e-3d6f5549abec)||
|6.1.2.8.2.4|[SaveSetting](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/98261176-760c-4257-8476-49669bd36053)||
|6.1.2.8.2.5|[SendKeys](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/df1a061c-0f22-4459-93f7-a8ada7148bb7)||

---
> ⏮️ [**RD-VBAL §6.1.2.7** Information](rd-vbal.6.1.2.7.information.md) | ⏭️ [**RD-VBAL §6.1.2.9** KeyCodeConstants](rd-vbal.6.1.2.9.keycodeconstants.md)
