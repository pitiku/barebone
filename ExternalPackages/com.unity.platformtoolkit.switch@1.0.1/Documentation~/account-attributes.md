# Nintendo Switch™ account attributes reference

Understand the attributes available on Nintendo Switch™ accounts when using the Platform Toolkit package.

## Usage

You can use `HasAttribute` and `GetAttribute` on any IAccount object to make sure the attribute exists for the platform.

## Attributes available on Nintendo Switch™ and Nintendo Switch™ 2

| **Attribute Name** | **Type** | **Description** | **Platform Documentation** |
| -------------- | ---- | ----------- | ---------------------- |
| **Nickname**       | string | The nickname for the user profile. | [Nickname documentation](https://developer.nintendo.com/o/online-docs/g1kr9vj6-en/Packages/SDK/NintendoSDK/Documents/Api/HtmlNX/structnn_1_1account_1_1_nickname.html) |
| **ProfileImage**   | Texture2D | The profile image for the user. | [LoadProfileImage documentation](https://developer.nintendo.com/html/online-docs/g1kr9vj6-en/Packages/SDK/NintendoSDK/Documents/Api/HtmlNX/namespacenn_1_1account.html#af2e5235f7466dc91fb2ec19c3a0f2437) |
| **UserID**         | UID | The user identification retrieved from the system. | [UID documentation](https://developer.nintendo.com/html/online-docs/g1kr9vj6-en/Packages/SDK/NintendoSDK/Documents/Api/HtmlNX/structnn_1_1account_1_1_uid.html) |
| **UserHandle**     | UserHandle | The opaque handle associated with a user. Used for other Nintendo account operations on open users. | [UserHandle documentation](https://developer.nintendo.com/html/online-docs/g1kr9vj6-en/Packages/SDK/NintendoSDK/Documents/Api/HtmlNX/structnn_1_1account_1_1_user_handle.html) |
