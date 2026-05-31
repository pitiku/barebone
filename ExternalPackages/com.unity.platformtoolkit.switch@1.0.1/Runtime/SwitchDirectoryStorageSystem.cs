using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Unity.PlatformToolkit.nintendoswitch
{
    class SwitchDirectoryStorageSystem : AbstractStorageSystem
    {
        private static int SaveDataId = 1;

        private const string kSaveDataName = "ptsave";
        private readonly SwitchSaveFileSystem m_FileSystem;

        public SwitchDirectoryStorageSystem(nn.account.Uid account)
        {
            m_FileSystem = new SwitchSaveFileSystem(account, $"{kSaveDataName}{SaveDataId}");
            m_FileSystem.Mount();
            SaveDataId++;
        }

        public override async Task<IReadOnlyList<string>> EnumerateArchives()
        {
            using var fileSystemLock = await m_FileSystem.LockAndMount();
            var dirs = m_FileSystem.EnumerateDirectoriesInDirectory();
            var filteredDirs = dirs.Where(name => name.EndsWith("_data"));
            var resultDirs = filteredDirs.Select((name, index) => name.Substring(0, name.Length - 5)).ToList();
            return resultDirs;
        }

        public override async Task<IGenericArchive> GetReadOnlyArchive(string name)
        {
            using var fileSystemLock = await m_FileSystem.LockAndMount();
            return new SwitchDirectoryArchive(m_FileSystem, name, false);
        }

        public override async Task<IGenericArchive> GetWriteOnlyArchive(string name)
        {
            using var fileSystemLock = await m_FileSystem.LockAndMount();
            return new SwitchDirectoryArchive(m_FileSystem, name, true);
        }

        public override async Task DeleteArchive(string name)
        {
            using var fileSystemLock = await m_FileSystem.LockAndMount();
            try
            {
                m_FileSystem.RemoveDirectory($"{name}_data");
                m_FileSystem.Commit();
            }
            catch (Exception)
            {
                m_FileSystem.Unmount();
                throw;
            }
        }

        public override async ValueTask DisposeAsync()
        {
            using (var fileSystemLock = await m_FileSystem.LockAndMount())
            {
                m_FileSystem.Unmount();
            }
            await base.DisposeAsync();
        }
    }
}
