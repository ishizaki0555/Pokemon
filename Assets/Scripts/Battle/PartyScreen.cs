using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    PartyMemberUI[] memberSlots;
    List<Monster> monsters;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Monster> monsters)
    {
        this.monsters = monsters;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if(i < monsters.Count)
                memberSlots[i].SetData(monsters[i]);
            else
                memberSlots[i].gameObject.SetActive(false);
        }
        messageText.text = "モンスターを選択";
    }

    public void UpdataMenberSelection(int selectMember)
    {
        for(int i = 0; i < monsters.Count; i++)
        {
            if(i == selectMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void SetMassageText(string message)
    {
        messageText.text = message;
    }
}
