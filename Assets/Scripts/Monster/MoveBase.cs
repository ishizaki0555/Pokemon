using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp; //技の使用回数（PP）: 0以下は無効
    [SerializeField] int prioity;
    [SerializeField] MoveCategory category; //技のカテゴリ（物理、特殊、変化技）: 物理技は攻撃力を使う、特殊技は特攻を使う、変化技はステータスを変える 
    [SerializeField] MoveEffects effects; //技の効果: ステータスの上昇値などを含む
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] Movetarget target; //技の対象: 自分、相手などを指定するための列挙型（Movetarget）

    [SerializeField] AudioClip attackSE; //技の効果音



    //他のファイルから参照するためにプロパティを使う
    public string Name { get => name; } 
    public string Description { get => description; }
    public MonsterType Type { get => type; }
    public int Power { get => power; }
    public int Accuracy { get => accuracy; }

    public bool AlwaysHits{ get => alwaysHits; }
    public int PP { get => pp; }
    public int Priority { get => prioity; }
    public AudioClip AttackSE { get => attackSE; }

    public MoveCategory Category { get => category; }
    public MoveEffects Effects { get => effects;  }
    public Movetarget Target { get => target; }
    public List<SecondaryEffects> Secondaries { get => secondaries; }
}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts; //ステータスの上昇値
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;

    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }
    public ConditionID Status
    {
        get { return status; }
    }

    public ConditionID VolatileStatus
    { 
        get { return volatileStatus; }  
    }
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] Movetarget target;

    public int Chance { get => chance; }
    public Movetarget Target { get => target; }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat; //ステータスの種類
    public int boost; //上昇値
}

public enum MoveCategory
{
    Physical, //物理技
    Special, //特殊技
    Status //変化技
}

public enum Movetarget
{
    Foe, //相手
    Self //自分
}