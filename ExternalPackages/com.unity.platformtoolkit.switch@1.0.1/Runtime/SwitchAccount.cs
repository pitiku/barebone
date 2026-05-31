using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit.nintendoswitch
{
    class SwitchAccount : IAccount, IDoubleSignOut
    {
        private AccountAttributeProvider<SwitchAccount> m_AttributeProvider;
        internal nn.account.Uid m_UserID { get; private set; } = nn.account.Uid.Invalid;
        private nn.account.UserHandle m_Handle = new();
        private GenericSavingSystem m_SavingSystem = null;
        private SwitchDirectoryStorageSystem m_StorageSystem = null;
        private GenericAccountSystem<SwitchAccount> m_AccountSystem;
        private readonly GenericLifetimeToken m_LifetimeToken = new GenericLifetimeToken();

        // Inherited from IAccount
        public AccountState State { get; private set; }

        internal static Task<string> GetNicknameAttribute(SwitchAccount account)
        {
            return account.GetNameHelper();
        }

        internal static Task<Texture2D> GetProfileImageAttribute(SwitchAccount account)
        {
            return account.GetProfileImage();
        }

        internal static Task<nn.account.Uid> GetUserIDAttribute(SwitchAccount account)
        {
            return Task.FromResult(account.m_UserID);
        }

        internal static Task<nn.account.UserHandle> GetUserHandleAttribute(SwitchAccount account)
        {
            return Task.FromResult(account.m_Handle);
        }

        public SwitchAccount(nn.account.UserHandle handle, nn.account.Uid uid, AccountAttributeProvider<SwitchAccount> provider)
        {
            m_Handle = handle;
            m_UserID = uid;
            m_StorageSystem = new(m_UserID);
            m_AttributeProvider = provider;
            State = AccountState.SignedIn;
        }

        public SwitchAccount(nn.account.Uid uid, AccountAttributeProvider<SwitchAccount> provider)
        {
            var result = nn.account.Account.OpenUser(ref m_Handle, uid);
            if (result.IsSuccess())
            {
                m_UserID = uid;
                State = AccountState.SignedIn;
            }
            else
            {
                throw new InvalidAccountException($"The account ({uid}) cannot be opened.");
            }
            m_StorageSystem = new(m_UserID);
            m_AttributeProvider = provider;
        }

        public void InitializeAccount(GenericAccountSystem<SwitchAccount> accountSystem)
        {
            m_AccountSystem = accountSystem;
        }

        async Task<bool> IAccount.SignOut()
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            if (State == AccountState.SignedIn)
            {
                await using var mod = await m_AccountSystem.BeginAccountSystemModification();

                nn.account.Account.CloseUser(m_Handle);

                State = AccountState.SignedOut;

                mod.Remove(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        Task<IAchievementSystem> IAccount.GetAchievementSystem()
        {
            throw new InvalidOperationException("Achievement System is not supported on this platform");
        }

        Task<ISavingSystem> IAccount.GetSavingSystem()
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            if (m_SavingSystem == null)
            {
                m_SavingSystem = new(m_StorageSystem);
            }
            return Task.FromResult<ISavingSystem>(m_SavingSystem);
        }

        private Task<string> GetNameHelper()
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            nn.account.Nickname name = new();
            var result = nn.account.Account.GetNickname(ref name, m_UserID);
            if (result.IsSuccess())
            {
                return Task.FromResult(name.name);
            }
            else
            {
                throw new InvalidAccountException();
            }
        }

        private async Task<Texture2D> GetProfileImage()
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            long actualSize = 0;
            byte[] profileBuffer = new byte[nn.account.Account.ProfileImageBytesMax];
            var result = nn.account.Account.LoadProfileImage(ref actualSize, profileBuffer, m_UserID);
            if (result.IsSuccess())
            {
                await Awaitable.MainThreadAsync();
                Texture2D profileTexture = new(256, 256);
                if (profileTexture.LoadImage(profileBuffer))
                {
                    return profileTexture;
                }
            }

            throw new InvalidAccountException();
        }

        public bool HasAttribute<T>(string attributeName)
        {
            return m_AttributeProvider.HasAttribute<T>(attributeName);
        }

        public Task<T> GetAttribute<T>(string attributeName)
        {
            return m_AttributeProvider.GetAttribute<T>(this, attributeName);
        }

        public bool TrySignOut()
        {
            if (!m_LifetimeToken.TryAtomicDispose(DisposeReason.InvalidAccount))
                return false;

            State = AccountState.SignedOut;
            return true;
        }

        public async Task CleanUpAfterSignOut()
        {
            if (m_SavingSystem != null)
            {
                await m_SavingSystem.DisposeAsync();
                m_SavingSystem = null;
                m_StorageSystem = null;
            }

            if (m_StorageSystem != null)
            {
                await m_StorageSystem.DisposeAsync();
                m_StorageSystem = null;
            }
        }

        public Task<string> GetName()
        {
            try
            {
                return GetNameHelper();
            }
            catch (Exception e)
            {
                return Task.FromResult(AccountErrorHandling.HandleGetNameException(e));
            }
        }

        public async Task<Texture2D> GetPicture()
        {
            try
            {
                return await GetProfileImage();
            }
            catch (Exception e)
            {
                return AccountErrorHandling.HandleGetPictureException(e);
            }
        }
    }
}
