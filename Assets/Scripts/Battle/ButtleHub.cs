using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ButtleHub : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] HPbar hpBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Monster _monster;

    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Monster monster)
    {
        _monster = monster;
        nameText.text = monster.Base.Name;
        levelText.text = "Lv" + monster.Level.ToString();
        hpBar.SetHP((float)monster.HP / monster.MaxHp);

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.PSN, psnColor },
            {ConditionID.BRN, brnColor },
            {ConditionID.SLP, slpColor },
            {ConditionID.PAR, parColor },
            {ConditionID.FRZ, frzColor },
        };

        SetStatusText();
        _monster.OnStatusChanged += SetStatusText;
    }

    void SetStatusText()
    {
        if(_monster.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _monster.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_monster.Status.Id];
        }
    }

    public IEnumerator UpdateHP()
    {
        if(_monster.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_monster.HP / _monster.MaxHp);
            _monster.HpChanged = false; //HP‚ª•Ï‰»‚µ‚½‚±‚Æ‚ðƒŠƒZƒbƒg
        }
    }
}
