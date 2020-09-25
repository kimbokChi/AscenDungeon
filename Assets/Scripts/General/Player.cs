﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, ICombatable
{
    [SerializeField]
    private GameObject mGameOverWindow;

    [SerializeField]
    private float mBlinkTime;
    private Timer mBlinkTimer;

    [SerializeField]
    private float WaitTimeATK;
    private Timer mWaitATK;

    [SerializeField]
    private float mMoveSpeed;

    [SerializeField] 
    private Area RangeArea;

    [SerializeField]
    private AbilityTable AbilityTable;

    [SerializeField]
    private float mDefense;

    [SerializeField]
    private DIRECTION9 mLocation9;

    private IEnumerator mEMove;

    private List<Collider2D> mCollidersOnMove;

    private bool mCanElevation;

    private bool mIsMoveToUpDown;

    private CircleCollider2D mRnageCollider;

    public  bool IsDeath => mIsDeath;

    public AbilityTable GetAbility => AbilityTable;

    private bool mIsDeath;

    public LPOSITION3 GetLPOSITION3()
    {
        switch (mLocation9)
        {
            case DIRECTION9.TOP_LEFT:
            case DIRECTION9.TOP:
            case DIRECTION9.TOP_RIGHT:
                return LPOSITION3.TOP;

            case DIRECTION9.MID_LEFT:
            case DIRECTION9.MID:
            case DIRECTION9.MID_RIGHT:
                return LPOSITION3.MID;

            case DIRECTION9.BOT_LEFT:
            case DIRECTION9.BOT:
            case DIRECTION9.BOT_RIGHT:
                return LPOSITION3.BOT;

            default:
                break;
        }
        Debug.Log("Value Error");
        return LPOSITION3.NONE;
    }

    public TPOSITION3 GetTPOSITION3()
    {
        switch (mLocation9)
        {
            case DIRECTION9.TOP_LEFT:
            case DIRECTION9.MID_LEFT:
            case DIRECTION9.BOT_LEFT:
                return TPOSITION3.LEFT;

            case DIRECTION9.TOP:
            case DIRECTION9.MID:
            case DIRECTION9.BOT:
                return TPOSITION3.MID;

            case DIRECTION9.TOP_RIGHT:
            case DIRECTION9.MID_RIGHT:
            case DIRECTION9.BOT_RIGHT:
                return TPOSITION3.RIGHT;

            default:
                break;
        }
        Debug.Log("Value Error");
        return TPOSITION3.NONE;
    }


    private void Start()
    {
        mCanElevation = false;
        mIsDeath      = false;

        mIsMoveToUpDown = false;

        mWaitATK    = new Timer();
        mBlinkTimer = new Timer();

        mWaitATK.Start(WaitTimeATK);

        mCollidersOnMove = new List<Collider2D>();

        Debug.Assert(RangeArea.TryGetComponent(out mRnageCollider));
    }

    private void InputAction()
    {
        DIRECTION9 moveRIR9 = DIRECTION9.END;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (GetLPOSITION3() == LPOSITION3.TOP)
            {
                mCanElevation = Castle.Instance.CanNextPoint();
            }
            if (!mCanElevation)
            {
                DIRECTION9 prevLocation9 = mLocation9;

                moveRIR9 = ((int)mLocation9 - 3) < 0 ? mLocation9 : mLocation9 - 3;

                mIsMoveToUpDown = (prevLocation9 != moveRIR9);
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            DIRECTION9 prevLocation9 = mLocation9;

            moveRIR9 = ((int)mLocation9 + 3) > 8 ? mLocation9 : mLocation9 + 3;

            mIsMoveToUpDown = (prevLocation9 != moveRIR9);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveRIR9 = (int)mLocation9 % 3 == 0 ? mLocation9 : mLocation9 - 1;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            moveRIR9 = (int)mLocation9 % 3 == 2 ? mLocation9 : mLocation9 + 1;
        }

        MoveAction(moveRIR9);
    }

    private void CheckToDeath()
    {
        mIsDeath = (AbilityTable.Table[Ability.CurHealth]) <= 0f;

        if (mIsDeath)
        {
            RangeArea.enabled = false;

            if (TryGetComponent(out SpriteRenderer renderer))
            {
                renderer.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);

                if (renderer.flipX)
                {
                     transform.rotation = Quaternion.Euler(0f, 0f, 90.0f);
                }
                else transform.rotation = Quaternion.Euler(0f, 0f, -90.0f);
            }
            transform.position += (Vector3.up * -0.4f);

            mGameOverWindow.SetActive(true);
        }
    }

    private void Update()
    {
        mWaitATK.Update();

        if (!mBlinkTimer.IsOver()) 
        {
            mBlinkTimer.Update(); 
        }
        mRnageCollider.radius = Inventory.Instance.GetWeaponRange();

        if (RangeArea.TryEnterTypeT(out GameObject challenger))
        {
            if (mWaitATK.IsOver() && challenger.TryGetComponent(out ICombatable combat))
            {
                Inventory.Instance.OnAttack(gameObject, combat);

                mWaitATK.Start(WaitTimeATK);
            }
            if (TryGetComponent(out SpriteRenderer renderer))
            {
                renderer.flipX = (challenger.transform.position.x > transform.position.x);
            }            
        }

        if (!mIsDeath)
        {
            InputAction();

            CheckToDeath();
        }        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (mEMove != null) mCollidersOnMove.Add(collision);
    }

    private void MoveAction(DIRECTION9 moveDIR9)
    {
        if (mEMove == null && moveDIR9 != mLocation9)
        {
            // Move To Next Floor
            if (mCanElevation)
            {
                if (Castle.Instance.CanNextPoint(out Vector2 nextPoint))
                {
                    switch (mLocation9)
                    {
                        case DIRECTION9.TOP_LEFT:
                            moveDIR9 = DIRECTION9.BOT_LEFT;
                            break;
                        case DIRECTION9.TOP:
                            moveDIR9 = DIRECTION9.BOT;
                            break;
                        case DIRECTION9.TOP_RIGHT:
                            moveDIR9 = DIRECTION9.BOT_RIGHT;
                            break;
                    }
                    StartCoroutine(mEMove = EMove(nextPoint, moveDIR9));
                }
            }

            // Move To MovePoint
            else if (moveDIR9 != DIRECTION9.END)
            {
                StartCoroutine(mEMove = EMove(Castle.Instance.GetMovePoint(moveDIR9), moveDIR9));
            }
        }
    }

    private IEnumerator EMove(Vector2 movePoint, DIRECTION9 moveDIR9)
    {
        Inventory.Instance.OnMoveBegin(movePoint.normalized);

        float lerpAmount = 0;

        while (lerpAmount < 1)
        {
            lerpAmount = Mathf.Min(1, lerpAmount + Time.deltaTime * Time.timeScale * AbilityTable.MoveSpeed);

            transform.position = Vector2.Lerp(transform.position, movePoint, lerpAmount);

            yield return null;
        }
        Inventory.Instance.OnMoveEnd(mCollidersOnMove.ToArray());

        mCollidersOnMove.Clear();

        if (mCanElevation)
        {
            // Inventory.Instance.UseItem(ITEM_KEYWORD.ENTER);

            mCanElevation = false;
        }
        mLocation9 = moveDIR9; mEMove = null;

        mIsMoveToUpDown = false;

        yield break;
    }

    #region READ
    /// <summary>
    /// 플레이어 개체가 위/아래로 이동중이라면 false를, 그렇지 않다면 true를 반환합니다.
    /// </summary>
    /// <param name="playerPos">
    /// 함수의 반환값과는 관계없이 일관되게 플레이어의 위치를 전달합니다.
    /// </param>
    /// <returns></returns>
    #endregion
    public bool Position(out Vector2 playerPos)
    {
        playerPos = transform.position;

        return !mIsMoveToUpDown;
    }

    public void Damaged(float damage, GameObject attacker)
    {
        if (!mBlinkTimer.IsOver()) { return; }

        Inventory.Instance.OnDamaged(ref damage, attacker, gameObject);

        AbilityTable.Table[Ability.CurHealth] -= damage / mDefense;

        mBlinkTimer.Start(mBlinkTime);
    }

    public void CastBuff(BUFF buffType, IEnumerator castedBuff)
    {
        StartCoroutine(castedBuff);
    }
}