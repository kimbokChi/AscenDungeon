using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotsBossSword : MonoBehaviour, IObject, ICombatable
{
    private const int Idle   = 0;
    private const int Move   = 1;
    private const int Attack = 2;
    private const int Death  = 3;

    [SerializeField] private ItemDropper _ItemDropper;

    [Header("Ability")]
    [SerializeField] private AbilityTable _AbilityTable;
    [SerializeField] private Animator _Animator;
    [SerializeField] private GameObject _HealthBar;
    [SerializeField] private Image _HealthBarImage;

    [Header("Attack Property")]
    [SerializeField] private float _DownFallLength;

    [Header("Move Property")]
    [SerializeField] private Vector2 _MoveRange;
    [Space()]
    [SerializeField] private float _MoveWaitMin;
    [SerializeField] private float _MoveWaitMax;
    [Space()]
    [SerializeField] private float _MoveTimeMin;
    [SerializeField] private float _MoveTimeMax;

    private Player _Player;
    private int _AnimControlKey;

    [ContextMenu("MoveOrder")]
    private void MoveOrder()
    {
        _Animator.SetInteger(_AnimControlKey, Move);
    }
    [ContextMenu("IdleOrder")]
    private void IdleOrder()
    {
        _Animator.SetInteger(_AnimControlKey, Idle);
    }
    [ContextMenu("AttackOrder")]
    private void AttackOrder()
    {
        _Animator.SetInteger(_AnimControlKey, Attack);
    }

    private void Awake()
    {
        IInit();
    }
    public void IInit()
    {
        _AnimControlKey = _Animator.GetParameter(0).nameHash;

        StartCoroutine(UpdateRoutine());
    }
    public void IUpdate()
    {
        
    }
    public void PlayerEnter(MESSAGE message, Player enterPlayer)
    {
        if (_AbilityTable.CanRecognize(message))
        {
            _Player = enterPlayer;
        }
    }
    public void PlayerExit(MESSAGE message)
    {
        if (_AbilityTable.CantRecognize(message))
        {
            _Player = null;
        }
    }
    public void Damaged(float damage, GameObject attacker)
    {
        if ((_AbilityTable.Table[Ability.CurHealth] -= damage) <= 0f)
        {
            _ItemDropper.CoinDrop(40);
            _ItemDropper.TryPotionDrop(PotionName.SHealingPotion, PotionName.LHealingPotion);
        }
    }
    #region
    public void CastBuff(Buff buffType, IEnumerator castedBuff)
    {
        StartCoroutine(castedBuff);
    }
    public bool IsActive()
    {
        return gameObject.activeSelf;
    }
    public GameObject ThisObject()
    {
        return gameObject;
    }
    public AbilityTable GetAbility => _AbilityTable;
    #endregion;

    private void AE_SetIdleState()
    {
        _Animator.SetInteger(_AnimControlKey, Idle);
    }
    private void AE_DownFall()
    {
        StartCoroutine(DownFallRoutine());
        EffectLibrary.Instance.UsingEffect(EffectKind.SwordAfterImage, transform.position + Vector3.down * 4.5f);
    }
    private void AE_MoveAction()
    {
        StartCoroutine(MoveRoutine());
    }
    private IEnumerator DownFallRoutine()
    {
        var targetPos = transform.localPosition + Vector3.down * _DownFallLength;

        for (float i = 0; i < 0.167f; i += Time.deltaTime * Time.timeScale)
        {
            float rate = Mathf.Min(i / 0.167f, 1f);
            transform.localPosition = Vector2.Lerp(transform.localPosition, targetPos, rate);

            yield return null;
        }
    }
    private IEnumerator UpdateRoutine()
    {
        while (_AbilityTable[Ability.CurHealth] > 0f)
        {
            float moveWait = Random.Range(_MoveTimeMin, _MoveWaitMax);
            for (float i = 0f; i < moveWait; i += Time.deltaTime * Time.timeScale)
                yield return null;

            _Animator.SetInteger(_AnimControlKey, Move);
            while (_Animator.GetInteger(_AnimControlKey) != Idle) yield return null;
        }
    }
    private IEnumerator MoveRoutine()
    {
        Vector3 direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        {
            float xAbsSub = Mathf.Abs(transform.localPosition.x - _MoveRange.x);
            float xAbsAdd = Mathf.Abs(transform.localPosition.x + _MoveRange.x);

            direction.x *=
                ((xAbsSub > xAbsAdd) && direction.x < 0) ||
                ((xAbsSub < xAbsAdd) && direction.x > 0) ? -1 : 1;

            float yAbsSub = Mathf.Abs(transform.localPosition.y - _MoveRange.y);
            float yAbsAdd = Mathf.Abs(transform.localPosition.y + _MoveRange.y);

            direction.y *=
                ((yAbsSub > yAbsAdd) && direction.y < 0) ||
                ((yAbsSub < yAbsAdd) && direction.y > 0) ? -1 : 1;
        }
        float moveTime = Random.Range(_MoveTimeMin, _MoveTimeMax);

        for (float i = 0f; i < moveTime; i += Time.deltaTime * Time.timeScale)
        {
            Vector3 pos = transform.localPosition;
            pos += direction * _AbilityTable.MoveSpeed * Time.deltaTime * Time.timeScale;

            if ((pos.x > _MoveRange.x) || (pos.x < -_MoveRange.x) ||
                (pos.y > _MoveRange.y) || (pos.y < -_MoveRange.y))
            {
                break;
            }
            transform.localPosition = pos;
            yield return null;
        }
        _Animator.SetInteger(_AnimControlKey, Idle);
    }
}
