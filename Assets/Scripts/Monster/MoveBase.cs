using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp; //�Z�̎g�p�񐔁iPP�j: 0�ȉ��͖���
    [SerializeField] int prioity;
    [SerializeField] MoveCategory category; //�Z�̃J�e�S���i�����A����A�ω��Z�j: �����Z�͍U���͂��g���A����Z�͓��U���g���A�ω��Z�̓X�e�[�^�X��ς��� 
    [SerializeField] MoveEffects effects; //�Z�̌���: �X�e�[�^�X�̏㏸�l�Ȃǂ��܂�
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] Movetarget target; //�Z�̑Ώ�: �����A����Ȃǂ��w�肷�邽�߂̗񋓌^�iMovetarget�j

    [SerializeField] AudioClip attackSE; //�Z�̌��ʉ�



    //���̃t�@�C������Q�Ƃ��邽�߂Ƀv���p�e�B���g��
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
    [SerializeField] List<StatBoost> boosts; //�X�e�[�^�X�̏㏸�l
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
    public Stat stat; //�X�e�[�^�X�̎��
    public int boost; //�㏸�l
}

public enum MoveCategory
{
    Physical, //�����Z
    Special, //����Z
    Status //�ω��Z
}

public enum Movetarget
{
    Foe, //����
    Self //����
}