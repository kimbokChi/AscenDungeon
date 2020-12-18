﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronShield : Item
{
    public enum AnimState
    {
        Defaulf, Begin, Duration, End
    }
    [SerializeField] private Animator Animator;
    [SerializeField] private float DurationTime;
    [SerializeField] private float DemandCharge;

    private int _AnimControlKey;
    private bool _IsAlreadyInit;

    protected override void AttackAnimationPlayOver()
    {
        Animator.SetInteger(_AnimControlKey, (int)AnimState.Defaulf);

        base.AttackAnimationPlayOver();
    }

    public override bool CanAttackState
    {
        get
        {
            return Animator.GetInteger(_AnimControlKey) == (int)AnimState.Defaulf && 
                Finger.Instance.Gauge.Charge >= DemandCharge;
        }
    }

    public override void OffEquipThis(SlotType offSlot)
    {
        if (offSlot == SlotType.Weapon)
        {
            Inventory.Instance.ChargeAction -= ChargeAction;
        }
    }

    public override void OnEquipThis(SlotType onSlot)
    {
        Init();

        if (onSlot == SlotType.Weapon)
        {
            Inventory.Instance.ChargeAction += ChargeAction;
        }
    }

    private void AnimationBeginOver()
    {
        Animator.SetInteger(_AnimControlKey, (int)AnimState.Duration);

        StartCoroutine(DurateShield());
    }

    private void Init()
    {
        if (!_IsAlreadyInit)
        {
            _AnimControlKey = Animator.GetParameter(0).nameHash;

            _IsAlreadyInit = true;
        }
    }

    private void ChargeAction(float charge)
    {
        if (charge >= DemandCharge)
        {
            Animator.SetInteger(_AnimControlKey, (int)AnimState.Begin);
        }
    }

    private IEnumerator DurateShield()
    {
        Player player;

        if (transform.parent.parent.TryGetComponent(out player))
        {
            player.CanHaveDamage = false;
        }
        yield return new WaitForSeconds(DurationTime);

        Animator.SetInteger(_AnimControlKey, (int)AnimState.End);

        if (transform.parent.parent.TryGetComponent(out player))
        {
            player.CanHaveDamage = true;
        }
    }
}