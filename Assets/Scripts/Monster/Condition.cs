using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public ConditionID Id {  get; set; }
    public string Name { get;  set; } // �����̖��O
    public string Description { get; set; } // �����̐���

    // ��Ԉُ펞�̃��b�Z�[�W
    public string StartMessage { get; set; } 

    public Action<Monster> OnStart { get; set; } // ��Ԉُ�J�n���̏���
    public Func<Monster, bool> OnBeforMove { get; set; } // �Z���g���O�̏���

    public Action<Monster> OnAfterTurn { get; set; } // �^�[���I����̏���

}
