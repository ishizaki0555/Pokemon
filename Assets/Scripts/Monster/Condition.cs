using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public ConditionID Id {  get; set; }
    public string Name { get;  set; } // 条件の名前
    public string Description { get; set; } // 条件の説明

    // 状態異常時のメッセージ
    public string StartMessage { get; set; } 

    public Action<Monster> OnStart { get; set; } // 状態異常開始時の処理
    public Func<Monster, bool> OnBeforMove { get; set; } // 技を使う前の処理

    public Action<Monster> OnAfterTurn { get; set; } // ターン終了後の処理

}
