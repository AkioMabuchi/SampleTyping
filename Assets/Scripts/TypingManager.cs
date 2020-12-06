using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UniRx;
using UnityEngine;

[Serializable]
public class Question
{
    public string title;
    public string roman;
}

public class TypingManager : SingletonMonoBehaviour<TypingManager>
{
    [SerializeField] Question[] _questions = new Question[12];
    List<char> _roman = new List<char>();
    private int _romanIndex = 0;

    private bool _isWindows;
    private bool _isMac;
    private bool _isActive;
    
    ReactiveProperty<string> _titleText = new ReactiveProperty<string>("");
    ReactiveProperty<string> _romanText = new ReactiveProperty<string>("");
    ReactiveProperty<string> _osText = new ReactiveProperty<string>("");
    ReactiveProperty<int> _count = new ReactiveProperty<int>(0);
    ReactiveProperty<int> _miss = new ReactiveProperty<int>(0);

    public ReactiveProperty<string> TitleText => _titleText;
    public ReactiveProperty<string> RomanText => _romanText;
    public ReactiveProperty<string> OsText => _osText;
    public ReactiveProperty<int> Count => _count;
    public ReactiveProperty<int> Miss => _miss;

    // Start is called before the first frame update
    void Start()
    {
        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            _osText.Value = "Windows";
            _isWindows = true;
            _isActive = true;
        }

        if (SystemInfo.operatingSystem.Contains("Mac"))
        {
            _osText.Value = "Mac";
            _isMac = true;
            _isActive = true;
        }

        if (_isActive)
        {
            InitializeQuestion();
        }
        else
        {
            _osText.Value = "未対応";
            _titleText.Value = "このOSには対応していません";
            _romanText.Value = "This operating system is not supported.";
        }
    }

    private void OnGUI()
    {
        if (_isActive && Event.current.type == EventType.KeyDown)
        {
            switch (InputKey(GetCharFromKeyCode(Event.current.keyCode)))
            {
                case 1: // 正解文字を入力した時
                case 2: // 柔軟な入力をした時
                    _romanIndex++;
                    _count.Value++;
                    if (_roman[_romanIndex] == '@')
                    {
                        InitializeQuestion();
                    }
                    else
                    {
                        _romanText.Value = GenerateRomanText();
                    }
                    break;
                case 3: // ミスタイプ
                    _miss.Value++;
                    break;
            }
        }
    }

    void InitializeQuestion()
    {
        Question question = _questions[UnityEngine.Random.Range(0, _questions.Length)];

        _romanIndex = 0;
        
        _roman.Clear();

        char[] characters = question.roman.ToCharArray();

        foreach (char character in characters)
        {
            _roman.Add(character);
        }
        
        _roman.Add('@');

        _titleText.Value = question.title;
        _romanText.Value = GenerateRomanText();
    }

    string GenerateRomanText()
    {
        string text = "<style=typed>";
        for (int i = 0; i < _roman.Count; i++)
        {
            if (_roman[i] == '@')
            {
                break;
            }
            if (i == _romanIndex)
            {
                text += "</style><style=untyped>";
            }

            text += _roman[i];
        }
        text += "</style>";
        return text;
    }
    int InputKey(char inputChar)
    {
        char prevChar3 = _romanIndex >= 3 ? _roman[_romanIndex - 3] : '\0';
        char prevChar2 = _romanIndex >= 2 ? _roman[_romanIndex - 2] : '\0';
        char prevChar = _romanIndex >= 1 ? _roman[_romanIndex - 1] : '\0';
        char currentChar = _roman[_romanIndex];
        char nextChar = _roman[_romanIndex + 1];
        char nextChar2 = nextChar == '@' ? '@' : _roman[_romanIndex + 2];

        if (inputChar == '\0')
        {
            return 0;
        }

        if (inputChar == currentChar)
        {
            return 1;
        }
        
        //「い」の曖昧入力判定（Windowsのみ）

        if (_isWindows && inputChar == 'y' && currentChar == 'i' &&
            (prevChar == '\0' || prevChar == 'a' || prevChar == 'i' || prevChar == 'u' || prevChar == 'e' ||
             prevChar == 'o')) 
        {
            _roman.Insert(_romanIndex, 'y');
            return 2;
        }

        if (_isWindows && inputChar == 'y' && currentChar == 'i' && prevChar == 'n' && prevChar2 == 'n' &&
            prevChar3 != 'n')
        {
            _roman.Insert(_romanIndex, 'y');
            return 2;
        }

        if (_isWindows && inputChar == 'y' && currentChar == 'i' && prevChar == 'n' && prevChar2 == 'x')
        {
            _roman.Insert(_romanIndex, 'y');
            return 2;
        }

        //「う」の曖昧入力判定（「whu」はWindowsのみ）
        if (inputChar == 'w' && currentChar == 'u' && (prevChar == '\0' || prevChar == 'a' || prevChar == 'i' ||
                                                       prevChar == 'u' || prevChar == 'e' || prevChar == 'o'))
        {
            _roman.Insert(_romanIndex, 'w');
            return 2;
        }

        if (inputChar == 'w' && currentChar == 'u' && prevChar == 'n' && prevChar2 == 'n' && prevChar3 != 'n')
        {
            _roman.Insert(_romanIndex, 'w');
            return 2;
        }

        if (inputChar == 'w' && currentChar == 'u' && prevChar == 'n' && prevChar2 == 'x')
        {
            _roman.Insert(_romanIndex, 'w');
            return 2;
        }

        if (_isWindows && inputChar == 'h' && prevChar2 != 't' && prevChar2 != 'd' && prevChar == 'w' &&
            currentChar == 'u') 
        {
            _roman.Insert(_romanIndex, 'h');
            return 2;
        }

        //「か」「く」「こ」の曖昧入力判定（Windowsのみ）
        if (_isWindows && inputChar == 'c' && prevChar != 'k' &&
            currentChar == 'k' == (nextChar == 'a' || nextChar == 'u' || nextChar == 'o'))
        {
            _roman[_romanIndex] = 'c';
            return 2;
        }

        //「く」の曖昧入力判定（Windowsのみ）
        if (_isWindows && inputChar == 'q' && prevChar != 'k' && currentChar == 'k' && nextChar == 'u')
        {
            _roman[_romanIndex] = 'q';
            return 2;
        }
        
        //「し」の曖昧入力判定
        if (inputChar == 'h' && prevChar == 's' && currentChar == 'i')
        {
            _roman.Insert(_romanIndex, 'h');
            return 2;
        }
        
        //「じ」の曖昧入力判定
        if (inputChar == 'j' && currentChar == 'z' && nextChar == 'i')
        {
            _roman[_romanIndex] = 'j';
            return 2;
        }
        
        //「しゃ」「しゅ」「しぇ」「しょ」の曖昧入力判定
        if (inputChar == 'h' && prevChar == 's' && currentChar == 'y')
        {
            _roman[_romanIndex] = 'h';
            return 2;
        }
        
        //「じゃ」「じゅ」「じぇ」「じょ」の曖昧入力判定
        if (inputChar == 'z' && prevChar != 'j' && currentChar == 'j' &&
            (nextChar == 'a' || nextChar == 'u' || nextChar == 'e' || nextChar == 'o'))
        {
            _roman[_romanIndex] = 'z';
            _roman.Insert(_romanIndex + 1, 'y');
            return 2;
        }

        //「し」「せ」の曖昧入力判定（Windowsのみ）
        if (_isWindows && inputChar == 'c' && prevChar != 's' && currentChar == 's' &&
            (nextChar == 'i' || nextChar == 'e'))
        {
            _roman[_romanIndex] = 'c';
            return 2;
        }
        
        //「ち」の曖昧入力判定
        if (inputChar == 'c' && prevChar != 't' && currentChar == 't' && nextChar == 'i')
        {
            _roman[_romanIndex] = 'c';
            _roman.Insert(_romanIndex + 1, 'h');
            return 2;
        }
        
        //「ちゃ」「ちゅ」「ちぇ」「ちょ」の曖昧入力判定
        if (inputChar == 'c' && prevChar != 't' && currentChar == 't' && nextChar == 'y')
        {
            _roman[_romanIndex] = 'c';
            return 2;
        }
        
        //「cya」=>「cha」
        if (inputChar == 'h' && prevChar == 'c' && currentChar == 'y')
        {
            _roman[_romanIndex] = 'h';
            return 2;
        }
        
        //「つ」の曖昧入力判定
        if (inputChar == 's' && prevChar == 't' && currentChar == 'u')
        {
            _roman.Insert(_romanIndex, 's');
            return 2;
        }
        
        //「つぁ」「つぃ」「つぇ」「つぉ」の分解入力判定
        if (inputChar == 'u' && prevChar == 't' && currentChar == 's' &&
            (nextChar == 'a' || nextChar == 'i' || nextChar == 'e' || nextChar == 'o'))
        {
            _roman[_romanIndex] = 'u';
            _roman.Insert(_romanIndex + 1, 'x');
            return 2;
        }

        if (inputChar == 'u' && prevChar2 == 't' && prevChar == 's' &&
            (currentChar == 'a' || currentChar == 'i' || currentChar == 'e' || currentChar == 'o'))
        {
            _roman.Insert(_romanIndex, 'u');
            _roman.Insert(_romanIndex + 1, 'x');
            return 2;
        }
        
        //「てぃ」の分解入力判定
        if (inputChar == 'e' && prevChar == 't' && currentChar == 'h' && nextChar == 'i')
        {
            _roman[_romanIndex] = 'e';
            _roman.Insert(_romanIndex + 1, 'x');
            return 2;
        }
        
        //「でぃ」の分解入力判定
        if (inputChar == 'e' && prevChar == 'd' && currentChar == 'h' && nextChar == 'i')
        {
            _roman[_romanIndex] = 'e';
            _roman.Insert(_romanIndex + 1, 'x');
            return 2;
        }
        
        //「でゅ」の分解入力判定
        if (inputChar == 'e' && prevChar == 'd' && currentChar == 'h' && nextChar == 'u')
        {
            _roman[_romanIndex] = 'e';
            _roman.Insert(_romanIndex + 1, 'x');
            _roman.Insert(_romanIndex + 2, 'y');
            return 2;
        }
        
        //「とぅ」の分解入力判定
        if (inputChar == 'o' && prevChar == 't' && currentChar == 'w' && nextChar == 'u')
        {
            _roman[_romanIndex] = 'o';
            _roman.Insert(_romanIndex + 1, 'x');
            return 2;
        }
        
        //「どぅ」の分解入力判定
        if (inputChar == 'o' && prevChar == 'd' && currentChar == 'w' && nextChar == 'u')
        {
            _roman[_romanIndex] = 'o';
            _roman.Insert(_romanIndex + 1, 'x');
            return 2;
        }
        
        //「ふ」の曖昧入力判定
        if (inputChar == 'f' && currentChar == 'h' && nextChar == 'u')
        {
            _roman[_romanIndex] = 'f';
            return 2;
        }
        
        //「ふぁ」「ふぃ」「ふぇ」「ふぉ」の分解入力判定（一部Macのみ）
        if (inputChar == 'w' && prevChar == 'f' &&
            (currentChar == 'a' || currentChar == 'i' || currentChar == 'e' || currentChar == 'o'))
        {
            _roman.Insert(_romanIndex,'w');
            return 2;
        }

        if (inputChar == 'y' && prevChar == 'f' && (currentChar == 'i' || currentChar == 'e'))
        {
            _roman.Insert(_romanIndex,'y');
            return 2;
        }

        if (inputChar == 'h' && prevChar != 'f' && currentChar == 'f' &&
            (nextChar == 'a' || nextChar == 'i' || nextChar == 'e' || nextChar == 'o'))
        {
            if (_isMac)
            {
                _roman[_romanIndex] = 'h';
                _roman.Insert(_romanIndex + 1, 'w');
            }
            else
            {
                _roman[_romanIndex] = 'h';
                _roman.Insert(_romanIndex + 1, 'u');
                _roman.Insert(_romanIndex + 2, 'x');
            }

            return 2;
        }

        if (inputChar == 'u' && prevChar == 'f' &&
            (currentChar == 'a' || currentChar == 'i' || currentChar == 'e' || currentChar == 'o'))
        {
            _roman.Insert(_romanIndex, 'u');
            _roman.Insert(_romanIndex + 1, 'x');
            return 2;
        }

        if (_isMac && inputChar == 'u' && prevChar == 'h' && currentChar == 'w' &&
            (nextChar == 'a' || nextChar == 'i' || nextChar == 'e' || nextChar == 'o'))
        {
            _roman[_romanIndex] = 'u';
            _roman.Insert(_romanIndex + 1, 'x');
            return 2;
        }

        //「ん」の曖昧入力判定（「n'」には未対応）
        if (inputChar == 'n' && prevChar2 != 'n' && prevChar == 'n' && currentChar != 'a' && currentChar != 'i' &&
            currentChar != 'u' && currentChar != 'e' && currentChar != 'o' && currentChar != 'y') 
        {
            _roman.Insert(_romanIndex, 'n');
            return 2;
        }
        
        if (inputChar == 'x' && prevChar != 'n' && currentChar == 'n' && nextChar != 'a' && nextChar != 'i' &&
            nextChar != 'u' && nextChar != 'e' && nextChar != 'o' && nextChar != 'y')
        {
            if (nextChar == 'n')
            {
                _roman[_romanIndex] = 'x';
            }
            else
            {
                _roman.Insert(_romanIndex, 'x');
            }

            return 2;
        }
        
        //「きゃ」「にゃ」などを分解する
        if (inputChar == 'i' && currentChar == 'y' &&
            (prevChar == 'k' || prevChar == 's' || prevChar == 't' || prevChar == 'n' || prevChar == 'h' ||
             prevChar == 'm' || prevChar == 'r' || prevChar == 'g' || prevChar == 'z' || prevChar == 'd' ||
             prevChar == 'b' || prevChar == 'p') &&
            (nextChar == 'a' || nextChar == 'u' || nextChar == 'e' || nextChar == 'o'))
        {
            if (nextChar == 'e')
            {
                _roman[_romanIndex] = 'i';
                _roman.Insert(_romanIndex + 1, 'x');
            }
            else
            {
                _roman.Insert(_romanIndex, 'i');
                _roman.Insert(_romanIndex + 1, 'x');
            }

            return 2;
        }
        
        //「しゃ」「ちゃ」などを分解する
        if (inputChar == 'i' &&
            (currentChar == 'a' || currentChar == 'u' || currentChar == 'e' || currentChar == 'o') &&
            (prevChar2 == 's' || prevChar2 == 'c') && prevChar == 'h')
        {
            if (nextChar == 'e')
            {
                _roman.Insert(_romanIndex,'i');
                _roman.Insert(_romanIndex + 1, 'x');
            }
            else
            {
                _roman.Insert(_romanIndex, 'i');
                _roman.Insert(_romanIndex + 1, 'x');
                _roman.Insert(_romanIndex + 2, 'y');
            }

            return 2;
        }
        
        //「しゃ」を「c」で分解する（Windows限定）
        if (_isWindows && inputChar == 'c' && currentChar == 's' && prevChar != 's' && nextChar == 'y' &&
            (nextChar2 == 'a' || nextChar2 == 'u' || nextChar2 == 'e' || nextChar2 == 'o'))
        {
            if (nextChar2 == 'e')
            {
                _roman[_romanIndex] = 'c';
                _roman[_romanIndex + 1] = 'i';
                _roman.Insert(_romanIndex + 1, 'x');
            }
            else
            {
                _roman[_romanIndex] = 'c';
                _roman.Insert(_romanIndex + 1, 'i');
                _roman.Insert(_romanIndex + 2, 'x');
            }

            return 2;
        }
        
        //「っ」の分解入力判定
        if ((inputChar == 'x' || inputChar == 'l') &&
            (currentChar == 'k' && nextChar == 'k' || currentChar == 's' && nextChar == 's' ||
             currentChar == 't' && nextChar == 't' || currentChar == 'g' && nextChar == 'g' ||
             currentChar == 'z' && nextChar == 'z' || currentChar == 'j' && nextChar == 'j' ||
             currentChar == 'd' && nextChar == 'd' || currentChar == 'b' && nextChar == 'b' || 
             currentChar == 'p' && nextChar == 'p'))
        {
            _roman[_romanIndex] = inputChar;
            _roman.Insert(_romanIndex + 1, 't');
            _roman.Insert(_romanIndex + 2, 'u');
            return 2;
        }
        
        //「っか」「っく」「っこ」の特殊入力（Windows限定）
        if (_isWindows && inputChar == 'c' && currentChar == 'k' && nextChar == 'k' &&
            (nextChar2 == 'a' || nextChar2 == 'u' || nextChar == 'o'))
        {
            _roman[_romanIndex] = 'c';
            _roman[_romanIndex + 1] = 'c';
            return 2;
        }
        
        //「っく」の特殊入力（Windows限定）
        if (_isWindows && inputChar == 'q' && currentChar == 'k' && nextChar == 'k' && nextChar2 == 'u')
        {
            _roman[_romanIndex] = 'q';
            _roman[_romanIndex + 1] = 'q';
            return 2;
        }
        
        //「っし」「っせ」の特殊入力（Windows限定）
        if (_isWindows && inputChar == 'c' && currentChar == 's' && nextChar == 's' &&
            (nextChar2 == 'i' || nextChar2 == 'e'))
        {
            _roman[_romanIndex] = 'c';
            _roman[_romanIndex + 1] = 'c';
            return 2;
        }
        //「っちゃ」「っちゅ」「っちぇ」「っちょ」の曖昧入力判定
        if (inputChar == 'c' && currentChar == 't' && nextChar == 't' && nextChar2 == 'y')
        {
            _roman[_romanIndex] = 'c';
            _roman[_romanIndex + 1] = 'c';
            return 2;
        }
        //「っち」の曖昧入力判定
        if (inputChar == 'c' && currentChar == 't' && nextChar == 't' && nextChar2 == 'i')
        {
            _roman[_romanIndex] = 'c';
            _roman[_romanIndex + 1] = 'c';
            _roman.Insert(_romanIndex + 2, 'h');
            return 2;
        }
        
        //「l」と「x」の完全互換性
        if (inputChar == 'x' && currentChar == 'l')
        {
            _roman[_romanIndex] = 'x';
            return 2;
        }

        if (inputChar == 'l' && currentChar == 'x')
        {
            _roman[_romanIndex] = 'l';
            return 2;
        }
        
        return 3;
    }
    char GetCharFromKeyCode(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.A:
                return 'a';
            case KeyCode.B:
                return 'b';
            case KeyCode.C:
                return 'c';
            case KeyCode.D:
                return 'd';
            case KeyCode.E:
                return 'e';
            case KeyCode.F:
                return 'f';
            case KeyCode.G:
                return 'g';
            case KeyCode.H:
                return 'h';
            case KeyCode.I:
                return 'i';
            case KeyCode.J:
                return 'j';
            case KeyCode.K:
                return 'k';
            case KeyCode.L:
                return 'l';
            case KeyCode.M:
                return 'm';
            case KeyCode.N:
                return 'n';
            case KeyCode.O:
                return 'o';
            case KeyCode.P:
                return 'p';
            case KeyCode.Q:
                return 'q';
            case KeyCode.R:
                return 'r';
            case KeyCode.S:
                return 's';
            case KeyCode.T:
                return 't';
            case KeyCode.U:
                return 'u';
            case KeyCode.V:
                return 'v';
            case KeyCode.W:
                return 'w';
            case KeyCode.X:
                return 'x';
            case KeyCode.Y:
                return 'y';
            case KeyCode.Z:
                return 'z';
            case KeyCode.Alpha0:
                return '0';
            case KeyCode.Alpha1:
                return '1';
            case KeyCode.Alpha2:
                return '2';
            case KeyCode.Alpha3:
                return '3';
            case KeyCode.Alpha4:
                return '4';
            case KeyCode.Alpha5:
                return '5';
            case KeyCode.Alpha6:
                return '6';
            case KeyCode.Alpha7:
                return '7';
            case KeyCode.Alpha8:
                return '8';
            case KeyCode.Alpha9:
                return '9';
            case KeyCode.Minus:
                return '-';
            case KeyCode.Caret:
                return '^';
            case KeyCode.Backslash:
                return '\\';
            case KeyCode.At:
                return '@';
            case KeyCode.LeftBracket:
                return '[';
            case KeyCode.Semicolon:
                return ';';
            case KeyCode.Colon:
                return ':';
            case KeyCode.RightBracket:
                return ']';
            case KeyCode.Comma:
                return ',';
            case KeyCode.Period:
                return '_';
            case KeyCode.Slash:
                return '/';
            case KeyCode.Underscore:
                return '_';
            case KeyCode.Backspace:
                return '\b';
            case KeyCode.Return:
                return '\r';
            case KeyCode.Space:
                return ' ';
            default:
                return '\0';
        }
    }
}

