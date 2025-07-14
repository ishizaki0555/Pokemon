using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ConditionDB : MonoBehaviour
{
    // キー, value
    public static Dictionary<ConditionID, Condition> Conditions { get; set; }
        = new Dictionary<ConditionID, Condition>()
        {
            {
                ConditionID.Poison, new Condition()
                {
                    Name = "どく",
                    StartMessage = "は毒になった！",
                    OnAfterTurn = (Monster monster) =>
                    {
                        monster.UpdateHP(monster.MaxHp / 8); // 毒ダメージ
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}は毒でダメージを受けた！"); // 毒ダメージのメッセージ
                    }
                }
            },
            {
                ConditionID.Burn, new Condition()
                {
                    Name = "やけど",
                    StartMessage = "はやけどになった！",
                    OnAfterTurn = (Monster monster) =>
                    {
                        monster.UpdateHP(monster.MaxHp / 16); // やけどダメージ
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}はやけどでダメージを受けた！"); // やけどダメージのメッセージ
                    }
                }
            }
        };

}

public enum ConditionID
{
    None = 0, // 条件なし
    Poison, // 毒
    Paralysis, // 麻痺
    Sleep, // 睡眠
    Burn, // やけど
    Freeze, // 凍結
    Confusion, // 混乱
}
