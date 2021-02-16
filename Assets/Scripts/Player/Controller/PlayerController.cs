﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 플레이어에게 이동을 지시할 수 있어야 한다.
// 플레이어의 이동은 플레이어 내부적으로 처리한다.
// 플레이어의 이동 방식을 변경해야한다. MovePoint는 움직일 수 있는 지점을 제한하는 용도로 사용하자.
// 또한 위/아래로 이동할 때에는 y값만 MovePoint로 부터 불러오도록..
// 일단 공격 버튼부터 만들자. 이동은 좀 걸릴것 같으니까.

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SubscribableButton _AttackButton;

    [Header("___Scaling Prop___")]
    [SerializeField] private float _ScalingTime;
    [SerializeField] private float _DefaultScale;
    [SerializeField] private float _MaxScale;

    [Header("___MoveButton Prop___")]
    [SerializeField] private SubscribableButton _UMoveButton;
    [SerializeField] private SubscribableButton _DMoveButton;
    [SerializeField] private SubscribableButton _LMoveButton;
    [SerializeField] private SubscribableButton _RMoveButton;

    public SubscribableButton this[Direction dir]
    {
        get
        {
            switch (dir)
            {
                case Direction.Up:
                    return _UMoveButton;
                case Direction.Down:
                    return _DMoveButton;
                case Direction.Right:
                    return _RMoveButton;
                case Direction.Left:
                    return _LMoveButton;

                default:
                    return _AttackButton;
            }
        }
    }
    public SubscribableButton AttackButton => _AttackButton;

    private Player _Player;

    private bool _IsAlreadyInit = false;
    private Coroutine _WaitCharge;
    private Coroutine _WaitScaling;

    private Image[] _AllButtonImages;

    private void Start()
    {
        if (!_IsAlreadyInit)
        {
            _WaitCharge  = new Coroutine(this);
            _WaitScaling = new Coroutine(this);

            var find = GameObject.FindGameObjectWithTag("Player");
            if (find != null)
            {
                if (find.TryGetComponent(out _Player))
                {
                    _AttackButton.ButtonAction += state =>
                    {
                        switch (state)
                        {
                            case ButtonState.Down:

                                _WaitCharge.StartRoutine(WaitForStartCharging());
                                break;

                            case ButtonState.Up:

                                if (!_WaitCharge.IsFinished())
                                {
                                    _Player.AttackOrder();
                                }
                                else
                                {
                                    Finger.Instance.EndCharging();

                                    _WaitScaling.StopRoutine();
                                    _WaitScaling.StartRoutine(AttackBtnScaling(true));
                                }
                                _WaitCharge.StopRoutine();
                                break;
                        }
                    };

                    _UMoveButton.ButtonAction += state => MoveOrderToPlayer(state, Direction.Up);
                    _DMoveButton.ButtonAction += state => MoveOrderToPlayer(state, Direction.Down);
                    _LMoveButton.ButtonAction += state => MoveOrderToPlayer(state, Direction.Left);
                    _RMoveButton.ButtonAction += state => MoveOrderToPlayer(state, Direction.Right);
                }
            }
            _IsAlreadyInit = true;

            SetButtonScale(GameLoger.Instance.ControllerDefScale, GameLoger.Instance.ControllerMaxScale);
            SetButtonOffset(GameLoger.Instance.ControllerOffset);

            // ====== _AllButtonImages Init ===== //
            if (_AllButtonImages == null)
            {
                var buttons = GetAllButtons();
                _AllButtonImages = new Image[buttons.Length];

                for (int i = 0; i < _AllButtonImages.Length; i++)
                {
                    buttons[i].TryGetComponent(out _AllButtonImages[i]);
                }
            }
            // ====== _AllButtonImages Init ===== //
            SetButtonAlpha(GameLoger.Instance.ControllerAlpha);

            transform.localPosition = GameLoger.Instance.ControllerPos;
        }

        void MoveOrderToPlayer(ButtonState state, Direction direction)
        {
            switch (state)
            {
                case ButtonState.Down: _Player.MoveOrder(direction);
                    break;
                case ButtonState.Up  :
                    {
                        switch (direction)
                        {
                            case Direction.Right:
                            case Direction.Left:
                                _Player.MoveStop();
                                break;
                        }
                    }
                    break;
            }
        }
    }
    
    public SubscribableButton[] GetAllButtons()
    {
        return new SubscribableButton[5]
        {
            _UMoveButton, _DMoveButton,
            _LMoveButton, _RMoveButton, _AttackButton
        };
    }

    public void SetButtonScale(float defaultScale, float maxScale)
    {
        _DefaultScale = defaultScale;
        _MaxScale = maxScale;

        Vector3 scale = Vector3.one * defaultScale;
        _UMoveButton.transform.localScale = scale;
        _DMoveButton.transform.localScale = scale;
        _LMoveButton.transform.localScale = scale;
        _RMoveButton.transform.localScale = scale;

        _AttackButton.transform.localScale = scale;

        _WaitScaling.StopRoutine();
    }
    public void SetButtonOffset(float offset)
    {
        _UMoveButton.transform.localPosition = Vector3.up    * offset;
        _DMoveButton.transform.localPosition = Vector3.down  * offset;
        _LMoveButton.transform.localPosition = Vector3.left  * offset;
        _RMoveButton.transform.localPosition = Vector3.right * offset;
    }
    public void SetButtonAlpha(float alpha)
    {
        var newColor = _AllButtonImages[0].color;
            newColor.a = alpha;

        for (int i = 0; i < _AllButtonImages.Length; ++i)
        {
            _AllButtonImages[i].color = newColor;
        }
    }

    private IEnumerator WaitForStartCharging()
    {
        for (float i = 0f; i < Finger.PRESS_TIME; i += Time.deltaTime)
        {
            yield return null;
        }
        _WaitCharge.Finish();

        Finger.Instance.StartCharging();

        _WaitScaling.StopRoutine();
        _WaitScaling.StartRoutine(AttackBtnScaling(false));
    }
    private IEnumerator AttackBtnScaling(bool toDeafult)
    {
        Vector3 targetScale = Vector3.one * (toDeafult ? _DefaultScale : _MaxScale);

        for (float i = 0; i < _ScalingTime; i += Time.deltaTime)
        {
            float ratio = Mathf.Min(i / _ScalingTime, 1f);

            _AttackButton.transform.localScale = 
                Vector3.Lerp(_AttackButton.transform.localScale, targetScale, ratio);

            yield return null;
        }
        _WaitScaling.Finish();
    }
}
