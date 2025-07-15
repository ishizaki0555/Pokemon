using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

public class ConditionDB : MonoBehaviour
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    // �L�[, value
    public static Dictionary<ConditionID, Condition> Conditions { get; set; }
        = new Dictionary<ConditionID, Condition>()
        {
            {
                ConditionID.PSN, new Condition()
                {
                    Name = "�ǂ�",
                    StartMessage = "�͓łɂȂ����I",
                    OnAfterTurn = (Monster monster) =>
                    {
                        monster.UpdateHP(monster.MaxHp / 8);
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}�͓ł̃_���[�W���󂯂��I");
                    }

                }
            },
            {
                ConditionID.BRN, new Condition()
                {
                    Name = "�₯��",
                    StartMessage = "�͂₯�ǂɂȂ����I",
                    OnAfterTurn = (Monster monster) =>
                    {
                        monster.UpdateHP(monster.MaxHp / 16);
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}�͂₯�ǂ̃_���[�W���󂯂��I");
                    }
                }
            },
            {
                ConditionID.PAR, new Condition()
                {
                    Name = "���",
                    StartMessage = "�͖�ჂɂȂ����I",
                    OnBeforMove = (Monster monster) =>
                    {
                        if(Random.Range(1, 5) == 1)
                        {
                            monster.StatusChanges.Enqueue($"{monster.Base.Name}�͖�Ⴢœ����Ȃ�");
                            return false; // ��Ⴢ̌��ʂōs���ł��Ȃ�
                        }
                        return true; // ��Ⴢ̌��ʂ͂Ȃ�
                    }
                }
            },
            {
                ConditionID.FRZ, new Condition()
                {
                    Name = "����",
                    StartMessage = "�͓���t����!",
                    OnBeforMove = (Monster monster) =>
                    {
                        if(Random.Range(1, 5) == 1)
                        {
                            monster.CureStatus(); // ����������
                            monster.StatusChanges.Enqueue($"{monster.Base.Name}�͓���t���ē����Ȃ�");
                            return true; // ��Ⴢ̌��ʂōs���ł��Ȃ�
                        }
                        return false; // ��Ⴢ̌��ʂ͂Ȃ�
                    }
                }
            },
            {
                ConditionID.SLP, new Condition()
                {
                    Name = "����",
                    StartMessage = "�͖���ɕt����!",
                    OnStart = (Monster monster) =>
                    {
                        monster.StatusTime = Random.Range(1, 4); // 1�`3�^�[���̊ԁA����
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}��{monster.StatusTime}�^�[������ɕt����");
                    },
                    OnBeforMove = (Monster monster) =>
                    {
                        if(monster.StatusTime <= 0)
                        {
                            monster.CureStatus(); // ����������
                            monster.StatusChanges.Enqueue($"{monster.Base.Name}�͖ڂ��o�܂���");
                            return true; // ��������ڊo�߂��̂ōs���ł���
                        }
                        monster.StatusTime--; // �^�[�������炷
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}�͖����Ă���");
                        return false; // ��Ⴢ̌��ʂ͂Ȃ�
                    }
                }
            },
            {
                ConditionID.confusion, new Condition()
                {
                    Name = "����",
                    StartMessage = "�͍�������",
                    OnStart = (Monster monster) =>
                    {
                        monster.VolatileStatusTime = Random.Range(1, 4); // 1�`3�^�[���̊ԁA����
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}��{monster.StatusTime}�^�[������ɕt����");
                    },
                    OnBeforMove = (Monster monster) =>
                    {
                        if(monster.VolatileStatusTime <= 0)
                        {
                            monster.CureVolatileStatus();
                            monster.StatusChanges.Enqueue($"{monster.Base.Name}�͍��������I");
                            return true;
                        }
                        monster.VolatileStatusTime--;

                        if(Random.Range(1, 3) == 1)
                            return true;

                        monster.StatusChanges.Enqueue($"{monster.Base.Name}�͍������Ă���");
                        monster.UpdateHP(monster.MaxHp / 8);
                        monster.StatusChanges.Enqueue($"�������Ď������U�����Ă��܂����I");
                        return false;
                    }
                }
            }

        };

}

public enum ConditionID
{
    None, // �����Ȃ�
    PSN, // ��
    PAR, // ���
    SLP, // ����
    BRN, // �₯��
    FRZ, // ����
    confusion // ����
}
