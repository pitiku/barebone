#if UNITY_PS5

using System;
using System.IO;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;

#region Write
public class PS5WriteFileRequest : FileOps.FileOperationRequest
{
    public byte[] m_aiLargeData = new byte[1024 * 1024 * 40];

    public override void DoFileOperations(Mounting.MountPoint _oMountPoint, FileOps.FileOperationResponse _oResponse)
    {
        PS5WriteFileResponse oFileResponse = _oResponse as PS5WriteFileResponse;

        string sOutpath = _oMountPoint.PathName.Data + "/Data.dat";
        int iTotalWritten = 0;

        int iChunkSize = m_aiLargeData.Length / 5;

        using (FileStream oFileStream = new FileStream(sOutpath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1024 * 1024 * 5))
        {
            while (iTotalWritten < m_aiLargeData.Length)
            {
                int iWriteSize = Math.Min(m_aiLargeData.Length - iTotalWritten, iChunkSize);
                oFileStream.Write(m_aiLargeData, iTotalWritten, iWriteSize);
                iTotalWritten += iWriteSize;

                // Update progress value during saving
                _oResponse.UpdateProgress((float)iTotalWritten / (float)m_aiLargeData.Length);
            }
        }

        FileInfo oFileInfo = new FileInfo(sOutpath);
        oFileResponse.lastWriteTime = oFileInfo.LastWriteTime;
        oFileResponse.totalFileSizeWritten += oFileInfo.Length;
    }
}

public class PS5WriteFileResponse : FileOps.FileOperationResponse
{
    public DateTime lastWriteTime;
    public long totalFileSizeWritten;
}
#endregion

#region Read
public class PS5ReadFileRequest : FileOps.FileOperationRequest
{
    public override void DoFileOperations(Mounting.MountPoint _oMountPoint, FileOps.FileOperationResponse _oResponse)
    {
        PS5ReadFileResponse oFileResponse = _oResponse as PS5ReadFileResponse;

        string sOutpath = _oMountPoint.PathName.Data + "/Data.dat";

        FileInfo oFileInfo = new FileInfo(sOutpath);

        oFileResponse.m_aiLargeData = new byte[oFileInfo.Length];

        int iTotalRead = 0;

        using (FileStream oFileStream = new FileStream(sOutpath, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 1024 * 5)) // File.OpenRead(outpath3))
        {
            while (iTotalRead < oFileInfo.Length)
            {
                int iReadSize = Math.Min((int)oFileInfo.Length - iTotalRead, 1024 * 1024 * 2); // read up to 2 mb in a single write

                oFileStream.Read(oFileResponse.m_aiLargeData, iTotalRead, iReadSize);

                iTotalRead += iReadSize;

                // Update progress value during saving
                _oResponse.UpdateProgress((float)iTotalRead / (float)oFileInfo.Length);
            }
        }
    }
}

public class PS5ReadFileResponse : FileOps.FileOperationResponse
{
    public byte[] m_aiLargeData;
}
#endregion
#endif
