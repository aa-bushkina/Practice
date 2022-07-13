using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

//Spreadsheet of values
//https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/edit#gid=0
//Spreadsheet of suffixes
//https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/edit#gid=1912615539

public class ParserWindow : EditorWindow
{
    private string inputTopText;
    private string inputBottomText;

    [MenuItem("Parse/Parse Window")]
    public static void ShowWindow()
    {
        var width = 500;
        var height = 300;
        GetWindow<ParserWindow>().maxSize = new Vector2(width, height);
        GetWindow<ParserWindow>().minSize = new Vector2(width, height);
    }

    private void OnGUI()
    {
        
        var textAreaStyle = new GUIStyle(GUI.skin.textField)
        {
            
            wordWrap = true,
            normal =
            {
                background = Texture2D.grayTexture
            }
        };

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Enter link with values...");
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        inputTopText = GUILayout.TextArea(inputTopText, textAreaStyle, GUILayout.Height(80),
            GUILayout.Width(400));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.Label("Enter link with suffixes...");
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        inputBottomText =
            GUILayout.TextArea(inputBottomText, textAreaStyle, GUILayout.Height(80),
                GUILayout.Width(400));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Parse", GUILayout.Width(80), GUILayout.Height(40)))
        {
            ParseTable();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }


    private void ParseTable()
    {
        var linkVal = inputTopText;
        var isLinkOk = CheckInput(linkVal);
        if (!isLinkOk)
        {
            ShowPopUp("Error! Incorrect links have been entered!");
            return;
        }

        var linkSuff = inputBottomText;
        isLinkOk = CheckInput(linkSuff);
        if (!isLinkOk)
        {
            ShowPopUp("Error!Incorrect links have been entered!");
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

        ShowPopUp("Ok");
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
        return !string.IsNullOrEmpty(inputText) &&
               inputText.StartsWith("https://docs.google.com/spreadsheets/d/") &&
               inputText.Contains("/edit") &&
               inputText.Contains("#gid") &&
               Uri.IsWellFormedUriString(inputText, UriKind.Absolute);
    }

    private void ShowPopUp(string mess)
    {
        var rect = new Rect(150, 50, 0, 0);
        PopupWindow.Show(rect, new ResultPopup(mess));
    }
}

public class ResultPopup : PopupWindowContent
{
    private string _mess;

    public ResultPopup(string mess)
    {
        _mess = mess;
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(200, 200);
    }

    public override void OnGUI(Rect rect)
    {
        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            alignment = TextAnchor.MiddleCenter
            
        };
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.Label(_mess, labelStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }
}