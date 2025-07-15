using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Color highlightColor;
    //dialog��Text���擾���āA�ύX����
    [SerializeField] int letterPerSeconds;
    [SerializeField] TextMeshProUGUI dialogText;

    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;

    [SerializeField] List<TextMeshProUGUI> actionTexts;
    [SerializeField] List<TextMeshProUGUI> moveTexts;


    [SerializeField] TextMeshProUGUI ppText;
    [SerializeField] TextMeshProUGUI typeText;

    //Text��ύX���邽�߂̊֐�
    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    //�^�C�v�`���ŕ������o�͂���
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach(char letter in dialog)
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / letterPerSeconds); //1/30 * 30 => 1�s
        }
        yield return new WaitForSeconds(0.7f);
    }

    //UI�̕\��/��\��
    //DialogText�̕\���Ǘ�
    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    //ActionSelector�̕\���Ǘ�
    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    //MoveSelector�̕\���Ǘ�
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled); 
    }

    //�I�𒆂̃A�N�V�����̐F��ς���
    public void UpdateActionicon(int selectAction)
    {
        //selectAction��0�̎���actionTexts[0]�̐F��ɂ���A����ȊO�͍�
        //selectAction��1�̎���actionTexts[1]�̐F��ɂ���A����ȊO�͍�
        for(int i = 0; i < actionTexts.Count; i++)
        {
            if(selectAction == i)
            {
                actionTexts[i].color = highlightColor;
            }
            else
            {
                actionTexts[i].color = Color.black;
            }
        }
    }

    //�I�𒆂�Move�̐F��ς���
    public void UpdateMoveSelection(int selectMove, Move move)
    {
        //selectMove��0�̎���moveTexts[0]�̐F��ɂ���A����ȊO�͍�
        //selectMove��1�̎���moveTexts[1]�̐F��ɂ���A����ȊO�͍�
        //selectMove��2�̎���moveTexts[2]�̐F��ɂ���A����ȊO�͍�
        //selectMove��3�̎���moveTexts[3]�̐F��ɂ���A����ȊO�͍�
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (selectMove == i)
            {
                moveTexts[i].color = highlightColor;
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }
        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if (move.PP == 0)
            ppText.color = Color.red;
        else
            ppText.color = Color.black;
    }

    //�ڕW
    //�E�Z���̔��f
    //�E�I�𒆂̋Z�̐F��ς���
    //�EPP��TYPE�̔��f
    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            //�o���Ă��鐔�������f
            if(i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = ".";
            }
        }
    }
}
