using UnityEngine;

public class Move : MonoBehaviour
{
    //Monsterが実際に使うときの技データ
    //技のマスターデータを持つ
    //使いやすいようにするためにＰＰも持つ

    //Monsterが参照するのでpublicにしておく
    public MoveBase Base { get;set; }
    public int PP { get; set; }

    //初期設定
    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP;
    }
}

