﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Ability
{
    MoveSpeed, IMoveSpeed, CurHealth, MaxHealth,
    AttackPower, IAttackPower, End
}
public class AbilityTable : MonoBehaviour
{
    public  Dictionary<Ability, float>  Table
    {
        get
        {
            if (mTable == null) { Init(); }

            return mTable;
        }
    }
    private Dictionary<Ability, float> mTable;

    private void Init()
    {
        mTable = new Dictionary<Ability, float>();

        for (Ability i = 0; i < Ability.End; ++i)
        {
            switch (i)
            {
                case Ability.CurHealth:
                case Ability.MaxHealth:
                    mTable.Add(i, MaxHealth);
                    break;
                case Ability.AttackPower:
                    mTable.Add(i, AttackPower);
                    break;
                default:
                    mTable.Add(i, default);
                    break;
            }
        }
    }
    [SerializeField] private float MoveSpeed;
    [SerializeField] private float MaxHealth;
    [SerializeField] private float AttackPower;
}
