﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronShield : Item
{
    public enum AnimState
    {
        Defaulf, Begin, End, Attack
    }
    [SerializeField] private Area _CollisionArea;
    [SerializeField] private Animator Animator;
    [SerializeField] private Vector2 EffectOffset;

    [Header("Ability")]
    [SerializeField] private float DurationTime;
    [SerializeField] private float DemandCharge;

    private int _AnimControlKey;
    private bool _IsAlreadyInit;

    private GameObject _Player;

    protected override void AttackAnimationPlayOver()
    {
        Animator.SetInteger(_AnimControlKey, (int)AnimState.Defaulf);

        base.AttackAnimationPlayOver();
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

            _Player = transform.parent.parent.gameObject;
        }
    }

    private void AnimationBeginOver()
    {
        StartCoroutine(DurateShield());
    }

    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        Animator.SetInteger(_AnimControlKey, (int)AnimState.Attack);
    }

    private void Init()
    {
        if (!_IsAlreadyInit)
        {
            _CollisionArea?.SetEnterAction(o =>
            {
                if (o.TryGetComponent(out ICombatable combatable))
                {
                    combatable.Damaged(StatTable[ItemStat.AttackPower], _Player);
                    MainCamera.Instance.Shake(0.2f, 1.2f);
                }
            });
            _AnimControlKey = Animator.GetParameter(0).nameHash;

            _IsAlreadyInit = true;
        }
    }

    private void ChargeAction(float charge)
    {
        if (charge >= DemandCharge)
        {
            // AttackButtonController.Instance.HideButton();

            Animator.SetInteger(_AnimControlKey, (int)AnimState.Begin);
        }
    }

    private IEnumerator DurateShield()
    {
        Inventory.Instance.BeDamagedAction += DamagedAction;
        yield return new WaitForSeconds(DurationTime);

        // AttackButtonController.Instance.ShowButton();

        Animator.SetInteger(_AnimControlKey, (int)AnimState.End);
        Inventory.Instance.BeDamagedAction -= DamagedAction;
    }

    private void DamagedAction(ref float damage, GameObject attacker, GameObject victim)
    {
        if (attacker.TryGetComponent(out ICombatable combatable))
        {
            combatable.Damaged(damage / 2, _Player);
        }
        damage = 0f;

        Vector2 point = (Vector2)transform.position + EffectOffset;

        for (int i = 0; i < 4; ++i)
        {
            EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, point + Random.insideUnitCircle * 0.9f);
        }
        EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, point);

        SoundManager.Instance.PlaySound(SoundName.ShieldDefence);
    }

    public override void AttackCancel()
    {
        Animator.SetInteger(_AnimControlKey, (int)AnimState.Defaulf);
    }
}
