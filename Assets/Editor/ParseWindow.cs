using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

//https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/export?exportFormat=csv;
//https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/export?exportFormat=csv&gid=1912615539;
//https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/edit#gid=0
//https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/edit#gid=1912615539

public class ParseWindow : EditorWindow
{
    public string inputBottomText;
    public string inputTopText;

    private void OnGUI()
    {
        inputTopText = EditorGUILayout.TextField("Link with values", inputTopText);
        inputBottomText = EditorGUILayout.TextField("Link with suffixes", inputBottomText);

        if (GUILayout.Button("Parse"))
        {
            ParseTable();
        }
    }

    [MenuItem("Window/Parse Window")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ParseWindow));
    }

    public void ParseTable()
    {
        var linkVal = inputTopText;
        var isLinkOk = CheckInput(linkVal);
        if (!isLinkOk)
        {
            PrintLinkError();
            return;
        }

        var linkSuff = inputBottomText;
        isLinkOk = CheckInput(linkSuff);
        if (!isLinkOk)
        {
            PrintLinkError();
            return;
        }

        var folderPath = "Assets/Text";
        var valPath = Path.Combine(folderPath, "values.csv");
        var suffPath = Path.Combine(folderPath, "suffixes.csv");

        var rowsSuff = DownloadSpreadsheets(linkSuff, suffPath);
        var suffixes = rowsSuff[0].Split(',');
        var langNum = suffixes.Length - 2;


        var rowsValues = DownloadSpreadsheets(linkVal, valPath);

        for (var i = 2; i < langNum; i++)
        {
            var csv = new StringBuilder();
            for (var j = 0; j < rowsValues.Length; j++)
            {
                if (!String.IsNullOrEmpty(rowsValues[j]))
                {
                    var translatesArray = Regex.Split(rowsValues[j], ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                    var first = translatesArray[1];
                    var second = translatesArray[i];
                    var newLine = string.Format("{0},{1}", first, second);
                    csv.AppendLine(newLine);
                }
            }

            var resFolder = "Assets/Text/Events";
            var resFileName = String.Concat("textEvent_", suffixes[i], ".csv");
            var resPath = Path.Combine(resFolder, resFileName);
            File.WriteAllText(resPath, csv.ToString());
        }

        PrintOkMessage();
    }

    private string[] DownloadSpreadsheets(string link, string path)
    {
        var gid = link.Substring(link.LastIndexOf("#gid"));
        var uri = string.Concat(
            link.Substring(0, link.LastIndexOf("/edit")),
            "/export?exportFormat=csv",
            "&",
            gid.Substring(1));
        using (var client = new HttpClient())
        {
            using (var stream = client.GetStreamAsync(uri))
            {
                using (var fstream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    stream.Result.CopyTo(fstream);
                }
            }
        }

        return File.ReadAllLines(path);
    }

    private bool CheckInput(in string inputText)
    {
        if (!inputText.StartsWith("https://docs.google.com/spreadsheets/d/"))
        {
            return false;
        }

        if (!Uri.IsWellFormedUriString(inputText, UriKind.Absolute))
        {
            return false;
        }

        return true;
    }

    public void PrintLinkError()
    {
        //выводим окошко с ошибкой
        GUILayout.Label("Not ok");
        //_resultText.text = "Not Ok";
    }

    public void PrintOkMessage()
    {
        //выводим окошко, что все ок
        GUILayout.Label("Ok");
        // _resultText.text = "Ok";
    }
}