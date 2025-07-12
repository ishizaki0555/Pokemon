using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class MoveBase : ScriptableObject
{
    //�Z�̃}�X�^�[�f�[�^

    //���O�A�ڍׁA�^�C�v�A�З́A���m���APP(�Z���g���Ƃ��ɏ����|�C���g)
    [SerializeField] new string name; //�Z�̖��O

    [TextArea]
    [SerializeField] string description; //�Z�̐���

    [SerializeField] MonsterType type; //�Z�̃^�C�v
    [SerializeField] int power; //�Z�̈З�
    [SerializeField] int accuracy; //���m��
    [SerializeField] int pp; //�Z�̎g�p�񐔁iPP�j: 0�ȉ��͖���
    [SerializeField] AudioClip attackSE; //�Z�̌��ʉ�
    [SerializeField] private bool isPhysical; //�����Z������Z��


    [SerializeField] MoveEffects effects; //�Z�̌���

    //���̃t�@�C������Q�Ƃ��邽�߂Ƀv���p�e�B���g��
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