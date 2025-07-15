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

    // キー, value
    public static Dictionary<ConditionID, Condition> Conditions { get; set; }
        = new Dictionary<ConditionID, Condition>()
        {
            {
                ConditionID.PSN, new Condition()
                {
                    Name = "どく",
                    StartMessage = "は毒になった！",
                    OnAfterTurn = (Monster monster) =>
                    {
                        monster.UpdateHP(monster.MaxHp / 8);
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}は毒のダメージを受けた！");
                    }

                }
            },
            {
                ConditionID.BRN, new Condition()
                {
                    Name = "やけど",
                    StartMessage = "はやけどになった！",
                    OnAfterTurn = (Monster monster) =>
                    {
                        monster.UpdateHP(monster.MaxHp / 16);
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}はやけどのダメージを受けた！");
                    }
                }
            },
            {
                ConditionID.PAR, new Condition()
                {
                    Name = "麻痺",
                    StartMessage = "は麻痺になった！",
                    OnBeforMove = (Monster monster) =>
                    {
                        if(Random.Range(1, 5) == 1)
                        {
                            monster.StatusChanges.Enqueue($"{monster.Base.Name}は麻痺で動けない");
                            return false; // 麻痺の効果で行動できない
                        }
                        return true; // 麻痺の効果はない
                    }
                }
            },
            {
                ConditionID.FRZ, new Condition()
                {
                    Name = "凍結",
                    StartMessage = "は凍り付いた!",
                    OnBeforMove = (Monster monster) =>
                    {
                        if(Random.Range(1, 5) == 1)
                        {
                            monster.CureStatus(); // 凍結を治す
                            monster.StatusChanges.Enqueue($"{monster.Base.Name}は凍り付いて動けない");
                            return true; // 麻痺の効果で行動できない
                        }
                        return false; // 麻痺の効果はない
                    }
                }
            },
            {
                ConditionID.SLP, new Condition()
                {
                    Name = "睡眠",
                    StartMessage = "は眠りに付いた!",
                    OnStart = (Monster monster) =>
                    {
                        monster.StatusTime = Random.Range(1, 4); // 1～3ターンの間、眠る
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}は{monster.StatusTime}ターン眠りに付いた");
                    },
                    OnBeforMove = (Monster monster) =>
                    {
                        if(monster.StatusTime <= 0)
                        {
                            monster.CureStatus(); // 睡眠を治す
                            monster.StatusChanges.Enqueue($"{monster.Base.Name}は目を覚ました");
                            return true; // 睡眠から目覚めたので行動できる
                        }
                        monster.StatusTime--; // ターンを減らす
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}は眠っている");
                        return false; // 麻痺の効果はない
                    }
                }
            },
            {
                ConditionID.confusion, new Condition()
                {
                    Name = "混乱",
                    StartMessage = "は混乱した",
                    OnStart = (Monster monster) =>
                    {
                        monster.VolatileStatusTime = Random.Range(1, 4); // 1～3ターンの間、眠る
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}は{monster.StatusTime}ターン眠りに付いた");
                    },
                    OnBeforMove = (Monster monster) =>
                    {
                        if(monster.VolatileStatusTime <= 0)
                        {
                            monster.CureVolatileStatus();
                            monster.StatusChanges.Enqueue($"{monster.Base.Name}は混乱した！");
                            return true;
                        }
                        monster.VolatileStatusTime--;

                        if(Random.Range(1, 3) == 1)
                            return true;

                        monster.StatusChanges.Enqueue($"{monster.Base.Name}は混乱している");
                        monster.UpdateHP(monster.MaxHp / 8);
                        monster.StatusChanges.Enqueue($"混乱して自分を攻撃してしまった！");
                        return false;
                    }
                }
            }

        };

}

public enum ConditionID
{
    None, // 条件なし
    PSN, // 毒
    PAR, // 麻痺
    SLP, // 睡眠
    BRN, // やけど
    FRZ, // 凍結
    confusion // 混乱
}
