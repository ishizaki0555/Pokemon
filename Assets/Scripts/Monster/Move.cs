using UnityEngine;

public class Move : MonoBehaviour
{
    //Monster�����ۂɎg���Ƃ��̋Z�f�[�^
    //�Z�̃}�X�^�[�f�[�^������
    //�g���₷���悤�ɂ��邽�߂ɂo�o������

    //Monster���Q�Ƃ���̂�public�ɂ��Ă���
    public MoveBase Base { get;set; }
    public int PP { get; set; }

    //�����ݒ�
    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP;
    }
}

