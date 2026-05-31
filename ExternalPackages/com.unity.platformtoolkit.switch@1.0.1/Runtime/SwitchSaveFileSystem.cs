using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit.nintendoswitch
{
    internal class SwitchSaveFileSystem
    {
        public bool IsMounted { get; private set; }

        private readonly nn.account.Uid m_Account;
        private readonly string m_SaveDataName;

        private AsyncLock m_FileSystemLock = new AsyncLock();

        public SwitchSaveFileSystem(nn.account.Uid account, string saveDataName)
        {
            IsMounted = false;
            m_Account = account;
            m_SaveDataName = saveDataName;
        }

        public async Task<IDisposable> LockAndMount()
        {
            var lockObject = await m_FileSystemLock.LockAsync();
            try
            {
                Mount();
                return lockObject;
            }
            catch (Exception)
            {
                lockObject.Dispose();
                throw;
            }
        }

        public void Mount()
        {
            if (!IsMounted)
            {
                var name = m_Account.ToString();
                var result = nn.fs.SaveData.Ensure(m_Account);
                if (!result.IsSuccess())
                {
                    throw new IOException($"Could not ensure save data for account {name}.");
                }
                result = nn.fs.SaveData.Mount(m_SaveDataName, m_Account);
                if (!result.IsSuccess())
                {
                    if (!nn.fs.FileSystem.ResultMountNameAlreadyExists.Includes(result))
                    {
                        throw new IOException($"Could not mount save data for account {name}.");
                    }
                }
                IsMounted = true;
            }
        }

        public void Unmount()
        {
            if (IsMounted)
            {
                nn.fs.FileSystem.Unmount(m_SaveDataName);
                IsMounted = false;
            }
        }

        public IReadOnlyList<string> EnumerateDirectoriesInDirectory(string name = "")
        {
            var dirName = $"{m_SaveDataName}:/{name}";

            if (!IsMounted)
            {
                throw new InvalidOperationException($"Filesystem not mounted when enumerating directories in directory {dirName}.");
            }

            return EnumerateDirectory(dirName, true);
        }

        public IReadOnlyList<string> EnumerateFilesInDirectory(string name = "")
        {
            var dirName = $"{m_SaveDataName}:/{name}";

            if (!IsMounted)
            {
                throw new InvalidOperationException($"Filesystem not mounted when enumerating files in directory {dirName}.");
            }

            return EnumerateDirectory(dirName, false);
        }

        /// <summary>
        /// Returns data read from the given file.
        /// </summary>
        /// <param name="name">The name of the file/stream to load</param>
        /// <returns>A non-null array of data read from the file/stream</returns>
        /// <exception cref="T:System.IO.InvalidOperationException">Filesystem is not mounted.</exception>
        /// <exception cref="T:System.IO.IOException">There was an error reading the data.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">Data for name is not found.</exception>
        public byte[] LoadDataFromFile(string name)
        {
            var fileName = $"{m_SaveDataName}:/{name}";

            if (!IsMounted)
            {
                throw new InvalidOperationException($"Filesystem not mounted when reading from file {fileName}.");
            }

            nn.fs.FileHandle fileHandle = new();
            var result = nn.fs.File.Open(ref fileHandle, fileName, nn.fs.OpenFileMode.Read);
            if (!result.IsSuccess())
            {
                throw new FileNotFoundException($"Failed to open file {name}");
            }

            try
            {
                long fileSize = 0;
                result = nn.fs.File.GetSize(ref fileSize, fileHandle);
                if (!result.IsSuccess())
                {
                    throw new IOException($"Error reading file size for file {name}");
                }

                if (fileSize <= 0)
                {
                    return Array.Empty<byte>();
                }

                var data = new byte[fileSize];
                result = nn.fs.File.Read(fileHandle, 0, data, fileSize);
                if (!result.IsSuccess())
                {
                    throw new IOException($"Didn't read enough bytes for file {name}");
                }
                return data;
            }
            finally
            {
                nn.fs.File.Close(fileHandle);
            }
        }

        public void WriteDataToFile(string name, byte[] data)
        {
            var fileName = $"{m_SaveDataName}:/{name}";

            if (!IsMounted)
            {
                throw new InvalidOperationException($"Filesystem not mounted when writing to file {fileName}.");
            }

            nn.fs.FileHandle fileHandle = new();
            var result = nn.fs.File.Open(ref fileHandle, fileName, nn.fs.OpenFileMode.Write);
            if (result.IsSuccess())
            {
                result = nn.fs.File.SetSize(fileHandle, data.Length);
                if (!result.IsSuccess())
                {
                    nn.fs.File.Close(fileHandle);
                    throw new IOException($"Error resizing file with name {name} : {result.ToString()}");
                }
            }
            else
            {
                result = nn.fs.File.Create(fileName, data.Length);
                if (result.IsSuccess())
                {
                    result = nn.fs.File.Open(ref fileHandle, fileName, nn.fs.OpenFileMode.Write);
                }
                else
                {
                    throw new IOException($"Error creating file with name {name} : {result.ToString()}");
                }
            }

            result = nn.fs.File.Write(fileHandle, 0, data, data.Length, nn.fs.WriteOption.Flush);
            nn.fs.File.Close(fileHandle);
            if (!result.IsSuccess())
            {
                throw new IOException($"Error writing to file with name {name} : {result.ToString()}");
            }
        }

        public void RemoveFile(string name)
        {
            var fileName = $"{m_SaveDataName}:/{name}";

            if (!IsMounted)
            {
                throw new InvalidOperationException($"Filesystem not mounted when trying to remove file {fileName}.");
            }

            // We are ignoring the result because we do not want to throw an error if the file does not exist.
            // Because if the file doesn't exist and we attempt to delete it, the result is the same if the file has existed.
            // https://github.cds.internal.unity3d.com/unity/SystemCapabilities/pull/788
            nn.fs.File.Delete(fileName);
        }

        public void CreateDirectory(string name)
        {
            var dirName = $"{m_SaveDataName}:/{name}";

            if (!IsMounted)
            {
                throw new InvalidOperationException($"Filesystem not mounted when trying to create directory {dirName}.");
            }

            var result = nn.fs.Directory.Create(dirName);
            if (!result.IsSuccess())
            {
                throw new IOException($"Error creating directory {name} : {result.ToString()}");
            }
        }

        public void RemoveDirectory(string name)
        {
            var dirName = $"{m_SaveDataName}:/{name}";

            if (!IsMounted)
            {
                throw new InvalidOperationException($"Filesystem not mounted when trying to remove directory {dirName}.");
            }

            var result = nn.fs.Directory.DeleteRecursively(dirName);
            if (!result.IsSuccess())
            {
                throw new IOException($"Could not remove directory {dirName}.");
            }
        }

        public void Commit()
        {
            if (!IsMounted)
            {
                throw new InvalidOperationException($"Filesystem {m_SaveDataName} not mounted when trying to commit.");
            }
            nn.fs.FileSystem.Commit(m_SaveDataName);
        }

        private IReadOnlyList<string> EnumerateDirectory(string dirName, bool directories = false)
        {
            nn.fs.DirectoryHandle directory = new();
            var mode = directories ? nn.fs.OpenDirectoryMode.Directory : nn.fs.OpenDirectoryMode.File;
            var state = nn.fs.Directory.Open(ref directory, dirName, mode);
            if (!state.IsSuccess())
            {
                throw new IOException($"Could not open directory {dirName}.");
            }

            long entryCount = 0;
            state = nn.fs.Directory.GetEntryCount(ref entryCount, directory);
            if (!state.IsSuccess())
            {
                throw new IOException($"Could not get directory entry count for directory {dirName}.");
            }

            long entriesRead = 0;
            nn.fs.DirectoryEntry[] entryBuffer = new nn.fs.DirectoryEntry[entryCount];
            state = nn.fs.Directory.Read(ref entriesRead, entryBuffer, directory, entryCount);
            nn.fs.Directory.Close(directory);
            if (!state.IsSuccess())
            {
                throw new IOException($"Could not read directory entries.");
            }

            List<string> result = new();
            foreach (var file in entryBuffer)
            {
                result.Add(file.name);
            }

            return result;
        }
    }
}
