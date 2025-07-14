using System.Collections;
using UnityEngine;
using TMPro;

public class ButtleHub : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPbar hpBar;

    Monster _monster;

    public void SetData(Monster monster)
    {
        _monster = monster;
        nameText.text = monster.Base.Name;
        levelText.text = "Lv" + monster.Level.ToString();
        hpBar.SetHP((float)monster.HP / monster.MaxHp);
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
