using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionDB
{
    // �L�[, value
    public static Dictionary<ConditionID, Condition> Conditions { get; set; }
        = new Dictionary<ConditionID, Condition>()
        {
            {
                ConditionID.Poison, new Condition()
                {
                    Name = "�ǂ�",
                    StartMessage = "�͓łɂȂ����I"
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
