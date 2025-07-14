using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro.EditorUtilities;

public class buttleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] ButtleHub hub;
    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }

    public Monster Monster { get; set; }
    public ButtleHub Hub { get => hub;}

    Vector3 originalPos;
    Color originalColor;
    Image image;

    //バトルで使うモンスターを保持
    //モンスターの画像を反映する

    private void Awake()
    {
        //level = _base.Level;
        image = GetComponent<Image>();
        originalPos = transform.localPosition;
        originalColor = image.color;
    }
    //public void UpdatAwake()
    //{
    //    level = _base.Level;
    //    Monster.Level = level;
    //}

    public void Setup(Monster monster)
    {
        //_baseからレベルに応じたモンスターを生成する
        //BattleSyestemで使うからプロパティに入れる
        Monster = monster;

        if(isPlayerUnit)
        {
            image.sprite = Monster.Base.BackSprite;
        }
        else
        {
            image.sprite = Monster.Base.FrontSprite;
        }

        hub.SetData(monster);


        image.color = originalColor;
        PlayEnterAnimation();
    }

    //登場Anim
    public void PlayEnterAnimation()
    {
        if(isPlayerUnit)
        {
            //左端に配置
            transform.localPosition = new Vector3(-520f, originalPos.y, 0);
        }
        else
        {
            //右端に配置
            transform.localPosition = new Vector3(520f, originalPos.y, 0);
        }
        //戦闘時の位置までアニメーション
        transform.DOLocalMoveX(originalPos.x, 0.5f);
    }
    //攻撃Anim
    public void PlayerAttackAnimation()
    {
        //シーケンス
        //一度右に動いた後、元の位置に戻る
        Sequence sequence = DOTween.Sequence();
        if(isPlayerUnit)
        {
            sequence.Append(transform.DOLocalMoveX(originalPos.x + 50, 0.25f)); //後ろに追加
        }
        else
        {
            sequence.Append(transform.DOLocalMoveX(originalPos.x - 50, 0.25f)); //後ろに追加
        }
        sequence.Append(transform.DOLocalMoveX(originalPos.x, 0.2f)); //後ろに追加
    }
    //ダメージAnim
    public void PlayerHitanimation()
    {
        //色を一度グレーにしてから戻す
        Sequence sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }
    //戦闘不能Anim
    public void PlayerFaintanimation()
    {
        //下に下がりながら、薄くなる
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0, 0.5f));
    }
}
