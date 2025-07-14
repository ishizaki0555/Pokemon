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

    //�o�g���Ŏg�������X�^�[��ێ�
    //�����X�^�[�̉摜�𔽉f����

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
        //_base���烌�x���ɉ����������X�^�[�𐶐�����
        //BattleSyestem�Ŏg������v���p�e�B�ɓ����
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

    //�o��Anim
    public void PlayEnterAnimation()
    {
        if(isPlayerUnit)
        {
            //���[�ɔz�u
            transform.localPosition = new Vector3(-520f, originalPos.y, 0);
        }
        else
        {
            //�E�[�ɔz�u
            transform.localPosition = new Vector3(520f, originalPos.y, 0);
        }
        //�퓬���̈ʒu�܂ŃA�j���[�V����
        transform.DOLocalMoveX(originalPos.x, 0.5f);
    }
    //�U��Anim
    public void PlayerAttackAnimation()
    {
        //�V�[�P���X
        //��x�E�ɓ�������A���̈ʒu�ɖ߂�
        Sequence sequence = DOTween.Sequence();
        if(isPlayerUnit)
        {
            sequence.Append(transform.DOLocalMoveX(originalPos.x + 50, 0.25f)); //���ɒǉ�
        }
        else
        {
            sequence.Append(transform.DOLocalMoveX(originalPos.x - 50, 0.25f)); //���ɒǉ�
        }
        sequence.Append(transform.DOLocalMoveX(originalPos.x, 0.2f)); //���ɒǉ�
    }
    //�_���[�WAnim
    public void PlayerHitanimation()
    {
        //�F����x�O���[�ɂ��Ă���߂�
        Sequence sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }
    //�퓬�s�\Anim
    public void PlayerFaintanimation()
    {
        //���ɉ�����Ȃ���A�����Ȃ�
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0, 0.5f));
    }
}
