using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class GetLinkScript : MonoBehaviour
{
    [SerializeField] private InputField _inputField;

    [SerializeField] private Button _parseButton;
    [SerializeField] private Text _resultText;
    private string _linkText = "";


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
        bool isLinkOk = ParseInput(_inputField.text);
        if (!isLinkOk)
        {
            PrintLinkError();
        }

        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var fullPath = Path.Combine(folderPath, "values.csv");
        string uri = string.Concat(_linkText.Substring(0, _linkText.LastIndexOf("/edit")), "/export?exportFormat=csv");

        //"https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/export?exportFormat=csv";
        //https://docs.google.com/spreadsheets/d/1VXKvao9SVMp6O4REm0i8dvQNwKti5CshEj2yW2ExWzE/edit#gid=0
        using (var client = new HttpClient())
        {
            using (var s = client.GetStreamAsync(uri))
            {
                using (var fs = new FileStream(fullPath, FileMode.OpenOrCreate))
                {
                    s.Result.CopyTo(fs);
                }
            }
        }

        PrintOkMessage();
    }

    private bool ParseInput(in string inputText)
    {
        //проверка норм ли ссылка
        if (1 == 0 /*не норм*/)
        {
            return false;
        }

        _linkText = inputText;
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