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

    //�R���X�g���N�^�[�F�������̏����ݒ�
    public void Init()
    {
        HP = MaxHp;

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
    }

    //level�ɉ������X�e�[�^�X��Ԃ�����:�v���p�e�B(�����������邱�Ƃ��o����)
    //�v���p�e�B
    public int Attack{get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }}
    public int Defense{get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; }}
    public int SpAttack{get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5; }}
    public int SpDefense{get { return Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5; }}
    public int Speed{get { return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5; }}
    public int MaxHp{get { return Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10; }}

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

        float attack = (move.Base.IsPhysical) ?  attaker.Attack : attaker.SpAttack; //�����Z������Z���ōU���͂�ς���
        float defense = (move.Base.IsPhysical) ? SpDefense : Defense; //�����Z������Z���Ŗh��͂�ς���

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
    public bool Fainted { get;set; } //�퓬�s�\���ǂ���
    public float Critical { get;set; }
    public float TypeEffectiveness { get;set; }
}

