using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//���x���ɉ������X�e�[�^�X�̈Ⴄ�����X�^�[�̐�������N���X
//���ӁF�f�[�^�݈̂����F������C#�̃N���X
public class Monster
{
    [SerializeField] MonsterBase _base;
    [SerializeField] int level;


    //�x�[�X�ƂȂ�f�[�^
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

    //�g����Z
    public List<Move> Moves { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; } 

    public Dictionary<Stat, int > StatBoosts { get; private set; } //�X�e�[�^�X�̏㏸�l

    public Queue<string> StatusChanges { get; private set; } = new Queue<string>(); //�X�e�[�^�X�̕ω����L�^����L���[ 
    public Condition Status { get; private set; } //��Ԉُ�

    public bool HpChanged { get; set; } //HP���ω��������ǂ���


    //�R���X�g���N�^�[�F�������̏����ݒ�
    public void Init()
    {
        Moves = new List<Move>();   

        //�g����Z�̐ݒ�F�o����Z�̃��x���ȏ�Ȃ�AMoves�ɒǉ�
        foreach(LearnableMove learnableMove in Base.LearnableMoves)
        {
            if(Level >= learnableMove.Lavel)
            {
                Moves.Add(new Move(learnableMove.Base));
            }

            //�S�ȏ�̋Z�͎g���Ȃ�
            if(Moves.Count >= 4)
            {
                break;
            }
        }
        CalculateStats();

        HP = MaxHp;

        ResetStatBoost();
    }

    void CalculateStats()
    {
        //�X�e�[�^�X�̌v�Z
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense* Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);


        MaxHp = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10; //�ő�HP�̌v�Z
    }

    void ResetStatBoost()
    {
        //�X�e�[�^�X�̏㏸�l�����Z�b�g
        StatBoosts = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 }
        };
        Debug.Log("�X�e�[�^�X�̏㏸�l�����Z�b�g���܂����B");
    }

    int GetStat(Stat stat)
    {
        //�X�e�[�^�X�̎擾
        int statVal = Stats[stat];

        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f}; //�X�e�[�^�X�̏㏸�l�ɉ������{��

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

            if(boost > 0)
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            else
                StatusChanges.Enqueue($"{Base.Name}�� {stat} ���������I");
        }
    }


    //level�ɉ������X�e�[�^�X��Ԃ�����:�v���p�e�B(�����������邱�Ƃ��o����)
    //�v���p�e�B
    public int Attack{ get { return GetStat(Stat.Attack); } }
    public int Defense{get { return GetStat(Stat.Defense); } }
    public int SpAttack{get { return GetStat(Stat.SpAttack); ; }}
    public int SpDefense{get { return GetStat(Stat.SpDefense); } }
    public int Speed{get { return GetStat(Stat.Speed); } }
    public int MaxHp{ get; private set; }

    //3�̏���n��
    //�E�퓬�s�\
    //�E�N���e�B�J��
    //�E����

    public DamageDetails TakeDamage(Move move, Monster attaker)
    {
        //�N���e�B�J��
        float critical = 1f;
        //6.25%�ŃN���e�B�J��
        if(Random.value * 100 <= 6.25f) critical = 2f;
        //����
        float type = TypeChart.GetEffectivenss(move.Base.Type, Base.Type1) * TypeChart.GetEffectivenss(move.Base.Type, Base.Type2);
        DamageDetails damageDetails = new DamageDetails
        {
            Fainted = false,
            Critical = critical,
            TypeEffectiveness = type,
        };

        float attack = (move.Base.Category == MoveCategory.Special) ?  attaker.Attack : attaker.SpAttack; //�����Z������Z���ōU���͂�ς���
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense; //�����Z������Z���Ŗh��͂�ς���

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attaker.Level + 10) / 250f;
        float b = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(b * modifiers);

        UpdateHP(damage); //HP�����炷

        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp); //HP�����炷�B0�����ɂȂ�Ȃ��悤�ɂ���
        HpChanged = true; //HP���ω��������Ƃ��L�^
    }

    public void SetStatus(ConditionID conditionId)
    {
        Status = ConditionDB.Conditions[conditionId];
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}"); //��Ԉُ�̊J�n���b�Z�[�W���L���[�ɒǉ�
    }

    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        ResetStatBoost();
    }
}

public class DamageDetails
{
    public bool Fainted { get;set; } //�퓬�s�\���ǂ���
    public float Critical { get;set; }
    public float TypeEffectiveness { get;set; }
}

