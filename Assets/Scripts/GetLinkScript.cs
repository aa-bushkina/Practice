using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

//"https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/export?exportFormat=csv";
//"https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/export?exportFormat=csv&gid=1912615539";
//https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/edit#gid=0
//https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/edit#gid=1912615539

public class GetLinkScript : MonoBehaviour
{
    [SerializeField] private InputField _inputValues;
    [SerializeField] private InputField _inputSuffixes;

    [SerializeField] private Button _parseButton;
    [SerializeField] private Text _resultText;


    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnClick();
        }
    }

    public void OnClick()
    {
        var linkVal = _inputValues.text;
        var isLinkOk = ParseInput(linkVal);
        if (!isLinkOk)
        {
            PrintLinkError();
            return;
        }

        var linkSuff = _inputSuffixes.text;
        isLinkOk = ParseInput(linkSuff);
        if (!isLinkOk)
        {
            PrintLinkError();
            return;
        }

        var folderPath = "Assets/Text";
        var valPath = Path.Combine(folderPath, "values.csv");
        var suffPath = Path.Combine(folderPath, "suffixes.csv");

        var rowsSuff = ParseSpreadsheets(linkSuff, suffPath);
        var suffixes = rowsSuff[0].Split(',');
        var langNum = suffixes.Length - 2;


        var rowsValues = ParseSpreadsheets(linkVal, valPath);

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

    private string[] ParseSpreadsheets(string link, string path)
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

    private bool ParseInput(in string inputText)
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
        _resultText.text = "Not Ok";
    }

    public void PrintOkMessage()
    {
        //выводим окошко, что все ок
        _resultText.text = "Ok";
    }
}