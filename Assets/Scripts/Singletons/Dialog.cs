using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : Singleton<Dialog>
{
    private static readonly string BeginAnimation = "Dialog_Begin";

    [SerializeField] private float _WriteDelay = 0.1f;
    
    [SerializeField] private TMPro.TextMeshProUGUI _LogText;
    [SerializeField] private TMPro.TextMeshProUGUI _NameText;
    [SerializeField] private Image _ImageOfCharacter;

    [SerializeField] private Animator _Animator;


    [Header("Init Property")]
    [SerializeField] private GameObject _RootOfCharacter;
    [SerializeField] private GameObject _RootOfDialogBox;
    
    [Space()]

    [SerializeField] private Vector3 _InitPosOfCharacter;
    [SerializeField] private Vector3 _InitPosOfDialogBox;


    [Header("Test Property")]
    [SerializeField] [TextArea(3, 6)] private string _TestOfTextField;
    
    private int _AnimControlKey;
    private Action _WriteLogCallback;
    private Coroutine _WriteLogCoroutine;

    private Queue<string> _TextQueue = new Queue<string>();

    private StringBuilder _QueueBuilder = new StringBuilder();
    private StringBuilder _WriteBuilder = new StringBuilder();

    private void Awake()
    {
        _WriteLogCoroutine = new Coroutine(this);

        _AnimControlKey = _Animator.GetParameter(0).nameHash;
        _Animator.enabled = false;

        _RootOfCharacter.transform.localPosition = _InitPosOfCharacter;
        _RootOfDialogBox.transform.localPosition = _InitPosOfDialogBox;

        _RootOfCharacter.SetActive(false);
        _RootOfDialogBox.SetActive(false);
    }

    public void UpdateLog(string name, Sprite image)
    {
        _NameText.text = name;
        _ImageOfCharacter.sprite = image;
    }

    public void WriteLog(string text, Action callBack)
    {
        SetTextQueue(text);
        _WriteLogCallback = callBack;

        if (_Animator.enabled)
        {
            _WriteLogCoroutine.StartRoutine(WriteLogRoutine());
            return;
        }
        _RootOfCharacter.SetActive(true);
        _RootOfDialogBox.SetActive(true);

        _Animator.enabled = true;
        _Animator.Play(BeginAnimation);
    }

    public void SkipLog()
    {
        if (_WriteLogCoroutine.IsFinished())
        {
            CloseLog();
        }
        else if (_TextQueue.Count != 0)
        {
            while (_TextQueue.Count != 0) {
                _WriteBuilder.Append(_TextQueue.Dequeue());
            }
            _LogText.text = _WriteBuilder.ToString();
        }
    }

    public void CloseLog()
    {
        _WriteLogCoroutine.StopRoutine();
        _Animator.SetBool(_AnimControlKey, true);
        
        _TextQueue.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TestOfWriteLog();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            SkipLog();
        }
    }

#region Test Handle

    [ContextMenu("TestOfWriteLog")]
    private void TestOfWriteLog()
    {
        _NameText.text = "훈련교관";
        WriteLog(_TestOfTextField, null);
    }

    [ContextMenu("TestOfCloseLog")]
    private void TestOfCloseLog()
    {
        CloseLog();
    }

#endregion Test Handle    

    private void SetTextQueue(string text)
    {
        _QueueBuilder.Clear();

        for (int i = 0; i < text.Length; ++i)
        {
            char character = text[i];
            _QueueBuilder.Append(character);

            if (character.Equals(' '))
            {
                if (_QueueBuilder.ToString() == "  ")
                {
                    _QueueBuilder.Clear();
                    _QueueBuilder.AppendLine();
                }
                continue;
            }
            _TextQueue.Enqueue(_QueueBuilder.ToString());
            _QueueBuilder.Clear();
        }
    }

    private IEnumerator WriteLogRoutine()
    {
        _WriteBuilder.Clear();

        while (_TextQueue.Count != 0)
        {
            _WriteBuilder.Append(_TextQueue.Dequeue());
            _LogText.text = _WriteBuilder.ToString();

            yield return new WaitForSeconds(_WriteDelay);
        }
        _WriteLogCoroutine.Finish();
        _WriteLogCallback?.Invoke();
    }

#region Animation Event

    private void AE_PlayOverBegin()
    {
        _WriteLogCoroutine.StartRoutine(WriteLogRoutine());
    }
    private void AE_PlayOverEnd()
    {
        _Animator.enabled = false;
        _Animator.SetBool(_AnimControlKey, false);

        _RootOfCharacter.SetActive(false);
        _RootOfDialogBox.SetActive(false);

         _LogText.text = string.Empty;
        _NameText.text = string.Empty;
    }

#endregion Animation Event
}