using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public Move CurrentMove { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; } 

    public Dictionary<Stat, int > StatBoosts { get; private set; } //ステータスの上昇値
    public Condition Status { get; private set; } //状態異常

    public int StatusTime { get; set; } //状態異常の持続時間

    public Condition VolatileStatus { get; private set; } //状態異常
    public int VolatileStatusTime { get; set; } //状態異常の持続時間

    public Queue<string> StatusChanges { get; private set; } = new Queue<string>(); //ステータスの変化を記録するキュー 
    

    public bool HpChanged { get; set; } //HPが変化したかどうか
    public event System.Action OnStatusChanged;


    //コンストラクター：生成時の初期設定
    public void Init()
    {
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
        CalculateStats();

        HP = MaxHp;

        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    void CalculateStats()
    {
        //ステータスの計算
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense* Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);


        MaxHp = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + Level; //最大HPの計算
    }

    void ResetStatBoost()
    {
        //ステータスの上昇値をリセット
        StatBoosts = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 },
            { Stat.Accuracy, 0 },
            { Stat.Evasion, 0 },
        };
    }

    int GetStat(Stat stat)
    {
        //ステータスの取得
        int statVal = Stats[stat];

        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f}; //ステータスの上昇値に応じた倍率

        if(boost >= 0)
            statVal = Mathf.FloorToInt( statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[boost]);

        return statVal;
    }

    public void ApplyBoost(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;   
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            //ステータスの上昇値を表示
            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}の{stat}が上がった！");
            else
                StatusChanges.Enqueue($"{Base.Name}の{stat}が下がった！");

            Debug.Log($"{Base.Name}の{stat}が{StatBoosts[stat]}段階上昇した。");
        }
    }


    //levelに応じたステータスを返すもの:プロパティ(処理を加えることが出来る)
    //プロパティ
    public int Attack{ get { return GetStat(Stat.Attack); } }
    public int Defense { get { return GetStat(Stat.Defense); } }
    public int SpAttack{get { return GetStat(Stat.SpAttack); ; }}
    public int SpDefense{get { return GetStat(Stat.SpDefense); } }
    public int Speed{get { return GetStat(Stat.Speed); } }
    public int MaxHp{ get; private set; }

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

        float attack = (move.Base.Category == MoveCategory.Special) ?  attaker.Attack : attaker.SpAttack; //物理技か特殊技かで攻撃力を変える
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense; //物理技か特殊技かで防御力を変える

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attaker.Level + 10) / 250f;
        float b = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(b * modifiers);

        UpdateHP(damage); //HPを減らす

        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp); //HPを減らす。0未満にならないようにする
        HpChanged = true; //HPが変化したことを記録
    }

    public void SetStatus(ConditionID conditionId)
    {
        if (Status != null) return;

        Status = ConditionDB.Conditions[conditionId]; // どの状態異常になるのか
        Status?.OnStart?.Invoke(this); //状態異常の開始時の処理を呼び出す
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}"); //状態異常の開始メッセージをキューに追加
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null; //状態異常を解除
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionDB.Conditions[conditionId]; // どの状態異常になるのか
        VolatileStatus?.OnStart?.Invoke(this); //状態異常の開始時の処理を呼び出す
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}"); //状態異常の開始メッセージをキューに追加
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null; //状態異常を解除
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return Moves[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        //麻痺の効果を適用
        if (Status?.OnBeforMove != null)
        {
            if(!Status.OnBeforMove(this))
                canPerformMove = false;
        }

        if (VolatileStatus?.OnBeforMove != null)
        {
            if (!VolatileStatus.OnBeforMove(this))
                canPerformMove = false;
        }
        return canPerformMove; //麻痺の効果がない場合はtrueを返す
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;  
        ResetStatBoost();
    }
}

public class DamageDetails
{
    public bool Fainted { get;set; } //戦闘不能かどうか
    public float Critical { get;set; }
    public float TypeEffectiveness { get;set; }
}

