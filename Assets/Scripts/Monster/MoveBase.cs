using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class MoveBase : ScriptableObject
{
    //技のマスターデータ

    //名前、詳細、タイプ、威力、正確性、PP(技を使うときに消費するポイント)
    [SerializeField] new string name; //技の名前

    [TextArea]
    [SerializeField] string description; //技の説明

    [SerializeField] MonsterType type; //技のタイプ
    [SerializeField] int power; //技の威力
    [SerializeField] int accuracy; //正確性
    [SerializeField] int pp; //技の使用回数（PP）: 0以下は無効
    [SerializeField] AudioClip attackSE; //技の効果音
    [SerializeField] private bool isPhysical; //物理技か特殊技か


    [SerializeField] MoveEffects effects; //技の効果

    //他のファイルから参照するためにプロパティを使う
    public string Name { get => name; } 
    public string Description { get => description; }
    public MonsterType Type { get => type; }
    public int Power { get => power; }
    public int Accuracy { get => accuracy; }
    public int PP { get => pp; }
    public AudioClip AttackSE { get => attackSE; }
    public MoveEffects Effects { get => effects; }
    public bool IsPhysical {
        get
        {
            if(type == MonsterType.Fire || type == MonsterType.Water || type == MonsterType.Grass || type == MonsterType.Electric || type == MonsterType.Dragon)
            {
                return true; 
            }
            else
            {
                return false;
            }
        }
    }
}

[System.Serializable]
public class MoveEffects
{

    [SerializeField] ConditionID status;

    public ConditionID Status { get => status; }
}

[Serializable]
public class  StatBoost
{
    
}