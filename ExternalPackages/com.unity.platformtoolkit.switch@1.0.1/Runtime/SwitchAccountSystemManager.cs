using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit.nintendoswitch
{
    partial class SwitchAccountSystemManager : IAccountPickerSystemProvider
    {
        private GenericAccountSystem<SwitchAccount> m_AccountSystem;

        public IAccountSystem AccountSystem => m_AccountSystem;

        private readonly object m_LockObject = new();
        private readonly AccountAttributeProvider<SwitchAccount> accountAttributeProvider;

        public SwitchAccountSystemManager(AttributeStore attributeStore)
        {
            accountAttributeProvider = new(attributeStore.Attributes);
            accountAttributeProvider.RegisterAttributeGetter("Nickname", SwitchAccount.GetNicknameAttribute);
            accountAttributeProvider.RegisterAttributeGetter("ProfileImage", SwitchAccount.GetProfileImageAttribute);
            accountAttributeProvider.RegisterAttributeGetter("UserID", SwitchAccount.GetUserIDAttribute);
            accountAttributeProvider.RegisterAttributeGetter("UserHandle", SwitchAccount.GetUserHandleAttribute);
            accountAttributeProvider.FinalizeRegistration();
        }

        public bool Initialize()
        {
            nn.account.Account.Initialize();

            // TODO: Look into the issues this flow decision causes more
            // We couldn't initialize a startup account, swap to a picker only system.
            lock (m_LockObject)
            {
                if (!InitializeStartupAccount())
                {
                    m_AccountSystem = new GenericAccountSystem<SwitchAccount>(accountPickerSystemProvider: this);
                    return false;
                }
            }

            return true;
        }

        public async Task<IAccount> Show()
        {
            await Awaitable.BackgroundThreadAsync();

            await using var mod = await m_AccountSystem.BeginAccountSystemModification();

            nn.account.Uid uid = new();
            var result = nn.account.Account.ShowUserSelector(ref uid);
            if (!result.IsSuccess())
            {
                throw new UserRefusalException();
            }

            var existingAccount = mod.SignedInAccounts.Where(x => uid.Equals(x.m_UserID)).FirstOrDefault();
            if (existingAccount is null)
            {
                SwitchAccount account = new(uid, accountAttributeProvider);
                account.InitializeAccount(m_AccountSystem);
                mod.Add(account);

                return account;
            }

            return existingAccount;
        }
    }

    partial class SwitchAccountSystemManager : IPrimaryAccountSystemProvider
    {
        private bool InitializeStartupAccount()
        {
            // Make sure we don't run TryOpenPreselectedUser if it's already open, will abort instead of returning false
            if (m_AccountSystem == null)
            {
                SwitchAccount account = null;
                nn.account.UserHandle handle = new();
                if (nn.account.Account.TryOpenPreselectedUser(ref handle))
                {
                    nn.account.Uid uid = new();
                    if (nn.account.Account.GetUserId(ref uid, handle).IsSuccess())
                    {
                        account = new(handle, uid, accountAttributeProvider);
                        m_AccountSystem = new GenericAccountSystem<SwitchAccount>(new List<SwitchAccount> { account }, this, 0, this);
                        account.InitializeAccount(m_AccountSystem);

                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<IAccount> Establish()
        {
            await Awaitable.BackgroundThreadAsync();

            await using var mod = await m_AccountSystem.BeginAccountSystemModification();
            if (mod.CurrentPrimaryAccount == null)
            {
                nn.account.Uid uid = new();
                var result = nn.account.Account.ShowUserSelector(ref uid);
                if (!result.IsSuccess())
                {
                    throw new UserRefusalException();
                }

                SwitchAccount account = new(uid, accountAttributeProvider);
                account.InitializeAccount(m_AccountSystem);

                mod.Add(account);
                mod.MakePrimary(account);

                return account;
            }
            else
            {
                return mod.CurrentPrimaryAccount;
            }
        }
    }
}
