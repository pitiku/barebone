using System;
using System.Linq;
using System.IO.Compression;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit.nintendoswitch
{
    class SwitchDirectoryArchive : AbstractArchive
    {
        protected readonly SwitchSaveFileSystem m_FileSystem;
        protected readonly bool m_Writable;
        protected bool m_DirectoryNotCreated = false;

        protected string DirectoryName => $"{Name}_data";

        public SwitchDirectoryArchive(SwitchSaveFileSystem fileSystem, string name, bool writable)
             : base(doDataBuffering: true)
        {
            m_FileSystem = fileSystem;
            m_Writable = writable;

            Name = name;

            if (!m_FileSystem.EnumerateDirectoriesInDirectory().Contains(DirectoryName))
            {
                if (!m_Writable)
                {
                    throw new FileNotFoundException($"Cannot find folder {DirectoryName} for new readonly archive {Name}.");
                }
                m_DirectoryNotCreated = true;
            }
        }

        public override Task<IReadOnlyList<string>> EnumerateFiles()
        {
            if (m_DirectoryNotCreated)
                return Task.FromResult<IReadOnlyList<string>>(new List<string>());

            return Task.FromResult(m_FileSystem.EnumerateFilesInDirectory(DirectoryName));
        }

        protected override async Task<byte[]> GetDataFromStorage(string name)
        {
            using var fileSystemLock = await m_FileSystem.LockAndMount();
            var result = m_FileSystem.LoadDataFromFile($"{DirectoryName}/{name}");
            return result;
        }

        protected override Task WriteDataToStorage(string name, byte[] data)
        {
            return Task.CompletedTask;
        }

        protected override async Task CommitDataToStorage(IReadOnlyCollection<(string name, byte[] data)> writtenFiles, IReadOnlyCollection<string> deletedFiles)
        {
            using var fileSystemLock = await m_FileSystem.LockAndMount();

            try
            {
                if (m_DirectoryNotCreated)
                {
                    m_FileSystem.CreateDirectory(DirectoryName);
                }

                foreach (var file in writtenFiles)
                {
                    m_FileSystem.WriteDataToFile($"{DirectoryName}/{file.name}", file.data);
                }

                foreach (var fileName in deletedFiles)
                {
                    m_FileSystem.RemoveFile($"{DirectoryName}/{fileName}");
                }

                ExceptionTesting.TriggerException(ExceptionPoint.PreCommit);

                m_FileSystem.Commit();
            }
            catch (Exception)
            {
                m_FileSystem.Unmount();
                throw;
            }
        }

        protected override void RemoveDataFromStorage(string name) { }
    }
}
