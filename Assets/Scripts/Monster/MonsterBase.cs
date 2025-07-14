using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// モンスターのマスターデータ : 外部から変更しない（インスペクターだけ変更可能）
[CreateAssetMenu]
public class MonsterBase : ScriptableObject
{
    // 名前、説明、画像、タイプ、ステータス
    [SerializeField] new string name;
    [TextArea]
    [SerializeField] string description;

    [SerializeField] int level; // モンスターの今のレベル
    [SerializeField] float exp; //今取得している経験値
    [SerializeField] int basicExp; // 倒された時の基礎経験値
    [SerializeField] float necessaryExp;

    //画像
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    //タイプ
    [SerializeField] MonsterType type1;
    [SerializeField] MonsterType type2;

    //ステータス: hp,at,df,sAT,sDF,sp
    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves;

    public int MaxHP { get => maxHP; }
    public int Attack { get => attack; }
    public int Defense { get => defense;}
    public int SpAttack { get => spAttack; }
    public int SpDefense { get => spDefense;}
    public int Speed { get => speed;}

    public List<LearnableMove> LearnableMoves { get => learnableMoves; }
    public string Name { get => name;}
    public string Description { get => description;}
    public Sprite FrontSprite { get => frontSprite;}
    public Sprite BackSprite { get => backSprite; }
    public MonsterType Type1 { get => type1; }
    public MonsterType Type2 { get => type2; }
    public int Level { get => level; set => level = value; }
    public float Exp { get => exp; set => exp = value; }
    public int BasicExp { get => basicExp; }
    public float NecessaryExp { get => necessaryExp; set => necessaryExp = value; }
}


//覚える技：どのレベルで何を覚えるのか
[System.Serializable]
public class LearnableMove
{
    //ヒエラルキーで設定する
    [SerializeField] MoveBase _base;
    [SerializeField] int lavel;
    public MoveBase Base { get => _base;}
    public int Lavel { get => lavel;}
}

public enum MonsterType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
}

public enum Stat
{
    Attack, //物理攻撃
    Defense, //物理防御
    SpAttack, //特殊攻撃
    SpDefense, //特殊防御
    Speed, //素早さ
}


//やりたいこと
//・技の愛称計算
//・クリティカルヒット
//・上のことをメッセージに表示
public class TypeChart
{
    static float[][] chart =
    {
        //攻撃\防御          NOR  FIR  WAT  ELE  GRS  ICE  FIG  POI
        /* NOR */new float[]{1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
        /* FIR */new float[]{1f,0.5f,0.5f,  1f,  2f,  2f,  1f,  1f},
        /* WAT */new float[]{1f,  2f,0.5f,  1f,0.5f,  1f,  1f,  1f},
        /* ELE */new float[]{1f,  1f,  2f,0.5f,0.5f,  1f,  1f,  1f},
        /* GRS */new float[]{1f,0.5f,  2f,  1f,0.5f,  1f,  1f,0.5f},
        /* ICE */new float[]{1f,0.5f,0.5f,  1f,  2f,0.5f,  1f,  1f},
        /* FIG */new float[]{2f,  1f,  1f,  1f,  1f,  2f,  1f,0.5f},
        /* POI */new float[]{1f,  1f,  1f,  1f,  2f,  1f,  1f,0.5f},
    };

    public static float GetEffectivenss(MonsterType attackType, MonsterType defenseType)
    {
        if(attackType == MonsterType.None || defenseType == MonsterType.None)
        {
            return 1f;
        }
        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;
        return chart[row][col];
    }
}