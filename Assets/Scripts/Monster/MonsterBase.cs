using System.Collections.Generic;
using UnityEngine;

// �����X�^�[�̃}�X�^�[�f�[�^ : �O������ύX���Ȃ��i�C���X�y�N�^�[�����ύX�\�j
[CreateAssetMenu]
public class MonsterBase : ScriptableObject
{
    // ���O�A�����A�摜�A�^�C�v�A�X�e�[�^�X
    [SerializeField] new string name;
    [TextArea]
    [SerializeField] string description;

    [SerializeField] int level; // �����X�^�[�̍��̃��x��
    [SerializeField] float exp; //���擾���Ă���o���l
    [SerializeField] int basicExp; // �|���ꂽ���̊�b�o���l
    [SerializeField] float necessaryExp;

    //�摜
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    //�^�C�v
    [SerializeField] MonsterType type1;
    [SerializeField] MonsterType type2;

    //�X�e�[�^�X: hp,at,df,sAT,sDF,sp
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


//�o����Z�F�ǂ̃��x���ŉ����o����̂�
[System.Serializable]
public class LearnableMove
{
    //�q�G�����L�[�Őݒ肷��
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
    Dark,
    Steel,
    Fairy, 
}

public enum Stat
{
    Attack, //�����U��
    Defense, //�����h��
    SpAttack, //����U��
    SpDefense, //����h��
    Speed, //�f����

    // ���̓�͎��ۂ̃X�e�[�^�X�ł͂Ȃ�
    Accuracy,
    Evasion
}


//��肽������
//�E�Z�̈��̌v�Z
//�E�N���e�B�J���q�b�g
//�E��̂��Ƃ����b�Z�[�W�ɕ\��
public class TypeChart
{
    static float[][] chart =
    {
        //�U��\�h��          NOR  FIR  WAT  ELE  GRS  ICE  FIG  POI  GRO  FLY  PSY  BUG  CRG  GST  DRG  DRK  STL  FRL
        /* NOR */new float[]{1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  0.5f, 0f,  1f,  1f, 0.5f, 1f},  
        /* FIR */new float[]{1f,0.5f,0.5f,  1f,  2f,  2f,  1f,  1f,  1f,  1f,  1f,  2f,  0.5f, 1f, 0.5f, 1f,  2f,  1f},
        /* WAT */new float[]{1f,  2f,0.5f,  1f,0.5f,  1f,  1f,  1f,  2f,  1f,  1f,  1f,  2f,  1f,  0.5f, 1f,  1f,  1f},
        /* ELE */new float[]{1f,  1f,  2f,0.5f,0.5f,  1f,  1f,  1f,  0f,  2f,  1f,  1f,  1f,  1f,  0.5f, 1f,  1f,  1f},
        /* GRS */new float[]{1f,0.5f,  2f,  1f,0.5f,  1f,  1f,0.5f,  2f, 0.5f, 1f, 0.5f, 2f,  1f, 0.5f,  1f, 0.5f, 1f},
        /* ICE */new float[]{1f,0.5f,0.5f,  1f,  2f,0.5f,  1f,  1f,  1f,  2f,  1f,  1f,  1f,  1f,  2f, 1f,  0.5f, 1f},
        /* FIG */new float[]{2f,  1f,  1f,  1f,  1f,  2f,  1f,0.5f, 1f, 0.5f,0.5f, 0.5f,  2f, 0f,  1f, 2f, 2f, 0.5f},
        /* POI */new float[]{1f,  1f,  1f,  1f,  2f,  1f,  1f,0.5f, 0.5f, 1f, 1f, 1f,  0.5f,0.5f, 1f, 1f, 0f, 1f},
        /* GRO */new float[]{1f,  2f,  1f,  2f,  0.5f, 1f, 1f, 2f,  1f, 0f, 1f, 0.5f,  1f, 1f, 1f, 1f, 2f, 1f},
        /* FLY */new float[]{1f,  1f,  1f,  1f,  2f, 1f, 2f, 1f,  1f, 1f, 1f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f},
        /* PSY */new float[]{1f,  1f,  1f,  1f,  1f, 1f, 2f, 1f, 1f, 1f,0.5f,1f,  1f, 1f, 1f, 0f,0.5f, 1f},
        /* BUG */new float[]{1f,  0.5f, 1f,  1f,  2f, 1f, 0.5f, 0.5f, 1f, 0.5f, 2f, 1f,  1f, 0.5f, 1f, 2f,0.5f, 1f},
        /* CRG */new float[]{1f,  2f, 1f,  1f,  1f, 2f, 0.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,0.5f, 2f},
        /* GST */new float[]{0f,  1f, 1f,  1f,  1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f,0.5f, 1f},
        /* DRG */new float[]{1f,  1f, 1f,  1f,  1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f,0.5f, 0f},
        /* DRK */new float[]{1f,  1f, 1f,  1f,  1f, 1f,0.5f,1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f,0.5f,1f, 1f},
        /* STL */new float[]{1f,  0.5f,0.5f,0.5f,1f,2f,2f,1f,1f,1f,1f,1f,1f,1f,1f,1f,0.5f,2f},
        /* FRL */new float[]{1f,  0.5f, 1f,  1f,  1f, 1f, 1f, 1f, 1f, 1f,1f,1f,1f, 1f, 1f, 1f,0.5f,1f},



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