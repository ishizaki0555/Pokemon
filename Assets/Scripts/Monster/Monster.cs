using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//レベルに応じたステータスの違うモンスターの生成するクラス
//注意：データのみ扱う：純粋なC#のクラス
public class Monster
{
    [SerializeField] MonsterBase _base;
    [SerializeField] int level;


    //ベースとなるデータ
    public MonsterBase Base
    {
        get{
            return _base;
        }
    }
    public int Level {
        get {
            return level;
        }
    }

    public int HP { get;set; }

    //使える技
    public List<Move> Moves { get; set; }

    //コンストラクター：生成時の初期設定
    public void Init()
    {
        HP = MaxHp;

        Moves = new List<Move>();   

        //使える技の設定：覚える技のレベル以上なら、Movesに追加
        foreach(LearnableMove learnableMove in Base.LearnableMoves)
        {
            if(Level >= learnableMove.Lavel)
            {
                Moves.Add(new Move(learnableMove.Base));
            }

            //４つ以上の技は使えない
            if(Moves.Count >= 4)
            {
                break;
            }
        }
    }

    //levelに応じたステータスを返すもの:プロパティ(処理を加えることが出来る)
    //プロパティ
    public int Attack{get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }}
    public int Defense{get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; }}
    public int SpAttack{get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5; }}
    public int SpDefense{get { return Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5; }}
    public int Speed{get { return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5; }}
    public int MaxHp{get { return Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10; }}

    //3つの情報を渡す
    //・戦闘不能
    //・クリティカル
    //・相性

    public DamageDetails TakeDamage(Move move, Monster attaker)
    {
        //クリティカル
        float critical = 1f;
        //6.25%でクリティカル
        if(Random.value * 100 <= 6.25f) critical = 2f;
        //相性
        float type = TypeChart.GetEffectivenss(move.Base.Type, Base.Type1) * TypeChart.GetEffectivenss(move.Base.Type, Base.Type2);
        DamageDetails damageDetails = new DamageDetails
        {
            Fainted = false,
            Critical = critical,
            TypeEffectiveness = type,
        };

        float attack = (move.Base.IsPhysical) ?  attaker.Attack : attaker.SpAttack; //物理技か特殊技かで攻撃力を変える
        float defense = (move.Base.IsPhysical) ? SpDefense : Defense; //物理技か特殊技かで防御力を変える

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attaker.Level + 10) / 250f;
        float b = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(b * modifiers);
        HP -= damage;
        if(HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }
        return damageDetails;
    }

    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
}

public class DamageDetails
{
    public bool Fainted { get;set; } //戦闘不能かどうか
    public float Critical { get;set; }
    public float TypeEffectiveness { get;set; }
}

