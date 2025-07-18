using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Color highlightColor;
    //dialogのTextを取得して、変更する
    [SerializeField] int letterPerSeconds;
    [SerializeField] TextMeshProUGUI dialogText;

    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;

    [SerializeField] List<TextMeshProUGUI> actionTexts;
    [SerializeField] List<TextMeshProUGUI> moveTexts;


    [SerializeField] TextMeshProUGUI ppText;
    [SerializeField] TextMeshProUGUI typeText;

    //Textを変更するための関数
    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    //タイプ形式で文字を出力する
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach(char letter in dialog)
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / letterPerSeconds); //1/30 * 30 => 1行
        }
        yield return new WaitForSeconds(0.7f);
    }

    //UIの表示/非表示
    //DialogTextの表示管理
    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    //ActionSelectorの表示管理
    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    //MoveSelectorの表示管理
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled); 
    }

    //選択中のアクションの色を変える
    public void UpdateActionicon(int selectAction)
    {
        //selectActionが0の時はactionTexts[0]の色を青にする、それ以外は黒
        //selectActionが1の時はactionTexts[1]の色を青にする、それ以外は黒
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

    //選択中のMoveの色を変える
    public void UpdateMoveSelection(int selectMove, Move move)
    {
        //selectMoveが0の時はmoveTexts[0]の色を青にする、それ以外は黒
        //selectMoveが1の時はmoveTexts[1]の色を青にする、それ以外は黒
        //selectMoveが2の時はmoveTexts[2]の色を青にする、それ以外は黒
        //selectMoveが3の時はmoveTexts[3]の色を青にする、それ以外は黒
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

    //目標
    //・技名の反映
    //・選択中の技の色を変える
    //・PPとTYPEの反映
    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            //覚えている数だけ反映
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
