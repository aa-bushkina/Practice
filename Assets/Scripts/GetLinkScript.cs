using System.Collections;
using System.Collections.Generic;
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
        _resultText.text = _linkText;
    }
}