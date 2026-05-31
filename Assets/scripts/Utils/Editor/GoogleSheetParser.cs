using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine.Networking;

#if UNITY_EDITOR
public class GoogleSheetParser : Editor
{
    UnityWebRequest m_oWebRequest;
    [NonSerialized] public string GSheet_ID = "";
    public Dictionary<string, string[,]> m_aoSheets = new Dictionary<string, string[,]>(); // sheetname with the CSV in it

    public void sendRequest()
    {
        m_oWebRequest = UnityWebRequest.Get("https://script.google.com/macros/s/" + GSheet_ID + "/exec");
        m_oWebRequest.SendWebRequest();

        EditorApplication.update += checkForImportRequestEnd; // the only way to wait for a process to finish is with this
    }

    void checkForImportRequestEnd()
    {
        string sDriveSheetParsed;
        if (m_oWebRequest != null && m_oWebRequest.isDone)
        {
            EditorApplication.update -= checkForImportRequestEnd;
            var bytes = m_oWebRequest.downloadHandler.data;
            sDriveSheetParsed = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            m_aoSheets = sheetCreator(sDriveSheetParsed);

            checkSheets();
        }
    }

    Dictionary<string, string[,]> sheetCreator(string sDriveSheet)
    {
        Dictionary<string, string[,]> aoSheets = new Dictionary<string, string[,]>();

        string[] asCSVstring = sDriveSheet.Split("%");

        for (int iSheetNumber = 0; iSheetNumber < Int32.Parse(asCSVstring[0]); iSheetNumber += 4)
        {
            string sheetName = asCSVstring[iSheetNumber + 1]; // name of the sheet
            string[,] asImportedCSV = new string[Int32.Parse(asCSVstring[iSheetNumber + 3]), Int32.Parse(asCSVstring[iSheetNumber + 2])];  // length of rows and columns of the sheet

            string[] asAllRows = asCSVstring[iSheetNumber + 4].Split("º");
            for (int iRow = 0; iRow < asAllRows.Length; iRow++)
            {
                string[] asColInRows = asAllRows[iRow].Split("#");
                for (int iCol = 0; iCol < asColInRows.Length; iCol++)
                {
                    asImportedCSV[iRow, iCol] = asAllRows[iRow];
                }
            }

            aoSheets[sheetName] = asImportedCSV;
        }

        return aoSheets;
    }

    public virtual void checkSheets() { }
}
#endif
