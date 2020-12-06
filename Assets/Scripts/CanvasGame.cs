using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using TMPro;
using UnityEngine;

public class CanvasGame : SingletonMonoBehaviour<CanvasGame>
{
    private GameObject _gameObjectTextMeshProOs;

    private GameObject _gameObjectTextMeshProCount;

    private GameObject _gameObjectTextMeshProMiss;

    private GameObject _gameObjectPanelMain;

    private GameObject _gameObjectTextMeshProTitle;

    private GameObject _gameObjectTextMeshProRoman;

    private TextMeshProUGUI _textMeshProOs;

    private TextMeshProUGUI _textMeshProCount;

    private TextMeshProUGUI _textMeshProMiss;

    private TextMeshProUGUI _textMeshProTitle;

    private TextMeshProUGUI _textMeshProRoman;
    
    // Start is called before the first frame update
    void Start()
    {
        _gameObjectTextMeshProOs = gameObject.transform.Find("TextMeshProOS").gameObject;
        _gameObjectTextMeshProCount = gameObject.transform.Find("TextMeshProCount").gameObject;
        _gameObjectTextMeshProMiss = gameObject.transform.Find("TextMeshProMiss").gameObject;
        _gameObjectPanelMain = gameObject.transform.Find("PanelMain").gameObject;
        _gameObjectTextMeshProTitle = _gameObjectPanelMain.transform.Find("TextMeshProTitle").gameObject;
        _gameObjectTextMeshProRoman = _gameObjectPanelMain.transform.Find("TextMeshProRoman").gameObject;

        _textMeshProOs = _gameObjectTextMeshProOs.GetComponent<TextMeshProUGUI>();
        _textMeshProCount = _gameObjectTextMeshProCount.GetComponent<TextMeshProUGUI>();
        _textMeshProMiss = _gameObjectTextMeshProMiss.GetComponent<TextMeshProUGUI>();
        _textMeshProTitle = _gameObjectTextMeshProTitle.GetComponent<TextMeshProUGUI>();
        _textMeshProRoman = _gameObjectTextMeshProRoman.GetComponent<TextMeshProUGUI>();

        TypingManager.Instance.OsText.Subscribe(osText =>
        {
            _textMeshProOs.text = $"OS：{osText}";
        });

        TypingManager.Instance.Count.Subscribe(count =>
        {
            _textMeshProCount.text = $"総タイプ数：{count}";
        });

        TypingManager.Instance.Miss.Subscribe(miss =>
        {
            _textMeshProMiss.text = $"ミスタイプ数：{miss}";
        });

        TypingManager.Instance.TitleText.Subscribe(titleText =>
        {
            _textMeshProTitle.text = titleText;
        });

        TypingManager.Instance.RomanText.Subscribe(romanText =>
        {
            _textMeshProRoman.text = romanText;
        });
    }
}



