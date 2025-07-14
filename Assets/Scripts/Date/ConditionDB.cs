using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ConditionDB : MonoBehaviour
{
    // �L�[, value
    public static Dictionary<ConditionID, Condition> Conditions { get; set; }
        = new Dictionary<ConditionID, Condition>()
        {
            {
                ConditionID.Poison, new Condition()
                {
                    Name = "�ǂ�",
                    StartMessage = "�͓łɂȂ����I",
                    OnAfterTurn = (Monster monster) =>
                    {
                        monster.UpdateHP(monster.MaxHp / 8); // �Ń_���[�W
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}�͓łŃ_���[�W���󂯂��I"); // �Ń_���[�W�̃��b�Z�[�W
                    }
                }
            },
            {
                ConditionID.Burn, new Condition()
                {
                    Name = "�₯��",
                    StartMessage = "�͂₯�ǂɂȂ����I",
                    OnAfterTurn = (Monster monster) =>
                    {
                        monster.UpdateHP(monster.MaxHp / 16); // �₯�ǃ_���[�W
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}�͂₯�ǂŃ_���[�W���󂯂��I"); // �₯�ǃ_���[�W�̃��b�Z�[�W
                    }
                }
            }
        };

}

public enum ConditionID
{
    None = 0, // �����Ȃ�
    Poison, // ��
    Paralysis, // ���
    Sleep, // ����
    Burn, // �₯��
    Freeze, // ����
    Confusion, // ����
}
