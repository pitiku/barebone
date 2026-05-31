#if UNITY_PS4

using System;
using System.IO;

#region Write
public class PS4WriteFileRequest : Sony.PS4.SaveData.FileOps.FileOperationRequest
{
    public byte[] largeData = new byte[1024 * 1024 * 2];

    public override void DoFileOperations(Sony.PS4.SaveData.Mounting.MountPoint _oMountPoint, Sony.PS4.SaveData.FileOps.FileOperationResponse _oResponse)
    {
        PS4WriteFileResponse oFileResponse = _oResponse as PS4WriteFileResponse;

        string sOutpath = _oMountPoint.PathName.Data + "/Data.dat";

        int iTotalWritten = 0;

        using (FileStream oFileStream = File.OpenWrite(sOutpath))
        {
            // Add some information to the file.
            while (iTotalWritten < largeData.Length)
            {
                int writeSize = Math.Min(largeData.Length - iTotalWritten, 1000000); // Write up to 1000 bytes
                oFileStream.Write(largeData, iTotalWritten, writeSize);
                iTotalWritten += writeSize;

                // Update progress value during saving
                _oResponse.UpdateProgress((float)iTotalWritten / (float)largeData.Length);
            }
        }

        FileInfo oInfo = new FileInfo(sOutpath);
        oFileResponse.lastWriteTime = oInfo.LastWriteTime;
        oFileResponse.totalFileSizeWritten = oInfo.Length;
    }
}

public class PS4WriteFileResponse : Sony.PS4.SaveData.FileOps.FileOperationResponse
{
    public DateTime lastWriteTime;
    public long totalFileSizeWritten;
}
#endregion

#region Read
public class PS4ReadFileRequest : Sony.PS4.SaveData.FileOps.FileOperationRequest
{
    public override void DoFileOperations(Sony.PS4.SaveData.Mounting.MountPoint _oMountPoint, Sony.PS4.SaveData.FileOps.FileOperationResponse _oResponse)
    {
        PS4ReadFileResponse oFileResponse = _oResponse as PS4ReadFileResponse;

        string sOutpath = _oMountPoint.PathName.Data + "/Data.dat";
        FileInfo oInfo = new FileInfo(sOutpath);
        oFileResponse.largeData = new byte[oInfo.Length];

        int iTotalRead = 0;

        // Example of updating the progress value.
        using (FileStream oFileStream = File.OpenRead(sOutpath))
        {
            // Add some information to the file.
            while (iTotalRead < oInfo.Length)
            {
                int iReadSize = Math.Min((int)oInfo.Length - iTotalRead, 1000); // read up to 1000 bytes
                oFileStream.Read(oFileResponse.largeData, iTotalRead, iReadSize);
                iTotalRead += iReadSize;

                // Update progress value during saving
                _oResponse.UpdateProgress((float)iTotalRead / (float)oInfo.Length);
            }
        }
    }
}

public class PS4ReadFileResponse : Sony.PS4.SaveData.FileOps.FileOperationResponse
{
    public byte[] largeData;
}
#endregion
#endif