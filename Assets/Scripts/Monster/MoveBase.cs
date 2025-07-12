using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class MoveBase : ScriptableObject
{
    //�Z�̃}�X�^�[�f�[�^

    //���O�A�ڍׁA�^�C�v�A�З́A���m���APP(�Z���g���Ƃ��ɏ����|�C���g)
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] MonsterType type;
    [SerializeField] int power;
    [SerializeField] int accuracy; //���m��
    [SerializeField] int pp;
    [SerializeField] AudioClip attackSE;

    //���̃t�@�C������Q�Ƃ��邽�߂Ƀv���p�e�B���g��
    public string Name { get => name; } 
    public string Description { get => description; }
    public MonsterType Type { get => type; }
    public int Power { get => power; }
    public int Accuracy { get => accuracy; }
    public int PP { get => pp; }
    public AudioClip AttackSE { get => attackSE; }
}