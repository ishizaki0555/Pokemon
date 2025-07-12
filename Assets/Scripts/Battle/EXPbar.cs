using System.Collections;
using UnityEngine;

public class EXPbar : MonoBehaviour
{
    [SerializeField] GameObject exp;

    public void SetEXP(float EXP)
    {
        exp.transform.localScale = new Vector3(Mathf.Clamp(EXP, 0, 1), 1, 1);
    }

    public IEnumerator SetEXPSmooth(float newEXP, float maxEXP)
    {
        float currentEXP = exp.transform.localScale.x;
        float changeAmout = Mathf.Abs(newEXP - currentEXP); // 変化量を絶対値に修正

        while (Mathf.Abs(newEXP - currentEXP) > 0.01f) // 変化が小さくなるまでループ
        {
            currentEXP = Mathf.MoveTowards(currentEXP, newEXP, changeAmout * Time.deltaTime);
            exp.transform.localScale = new Vector3(Mathf.Clamp(currentEXP, 0, 1), 1, 1);
            yield return null;
        }
        exp.transform.localScale = new Vector3(Mathf.Clamp(newEXP, 0, 1), 1, 1); // 最終値の補正
    }
}
