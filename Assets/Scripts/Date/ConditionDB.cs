using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionDB
{
    // ÉLÅ[, value
    public static Dictionary<ConditionID, Condition> Conditions { get; set; }
        = new Dictionary<ConditionID, Condition>()
        {
            {
                ConditionID.Poison, new Condition()
                {
                    Name = "Ç«Ç≠",
                    StartMessage = "ÇÕì≈Ç…Ç»Ç¡ÇΩÅI"
                }
            }
        };

}

public enum ConditionID
{
    None = 0, // èåèÇ»Çµ
    Poison, // ì≈
    Paralysis, // ñÉ·É
    Sleep, // êáñ∞
    Burn, // Ç‚ÇØÇ«
    Freeze, // ìÄåã
    Confusion, // ç¨óê
}
