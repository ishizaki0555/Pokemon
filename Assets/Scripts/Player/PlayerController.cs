using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    //�ǔ����Layer
    [SerializeField] LayerMask solidObjectsLayer;
    //�����ނ画���Layer
    [SerializeField] LayerMask longGrassLayer;
    //�I�v�V������ʗp�̃L�����o�X
    [SerializeField] GameObject OptionCanvas;

    //���݈ˑ��������FUnityAction(�֐���o�^����)
    public UnityAction OnEncounted;

    Animator animator;
    bool isMove;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        OptionCanvas.SetActive(false);
    }
    public void HandleUpdate()
    {
        if(!isMove)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            if(x != 0)
            {
                y = 0;
            }

            if(x != 0 || y != 0)
            {
                animator.SetFloat("InputX", x);
                animator.SetFloat("InputY", y);

                StartCoroutine(Move(new Vector3(x, y, 0)));
            }            
        }
        animator.SetBool("isMoving", isMove);

        //�I�v�V�������j���[���J��
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(Time.timeScale == 0)
            {
                isMove = false;
                OptionCanvas.SetActive(false);
                Time.timeScale = 1.0f;
            }
            else
            {
                isMove = true;
                OptionCanvas.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }

    //�P�}�X���X�ɋ߂Â���
    IEnumerator Move(Vector3 direction)
    {
        isMove = true;
        Vector3 targetPos = transform.position + direction;
        if(isWalkable(targetPos) == false)
        {
            isMove = false;
            yield break;
        }
        //�@���݂̃^�[�Q�b�g�̏ꏊ���Ⴄ�Ȃ�A�߂Â�������
        while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            //�߂Â���
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5f * Time.deltaTime); //(���ݒn, �ڕW�l, ���x) :�ڕW�l�ɋ߂Â���
            yield return null;
        }

        transform.position = targetPos;
        isMove = false;
        CheckForEncounters();
    }

    // ����̈ʒu�Ɉړ��ł���̂����肷��֐�
    bool isWalkable(Vector3 targetPos)
    {
        // targetPos�𒆐S�ɉ~�`��Ray����� :�@SolidObjectLayer�ɂԂ�������ture���A���Ă���.������false
        return Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) == false;
    }
    //�����̏ꏊ����A�~�`��Ray���΂��āA���ނ��Layer�ɓ���������A�����_���G���J�E���g
    void CheckForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position, 0.2f, longGrassLayer))
        {
            // �����_���G���J�E���g
            if(Random.Range(0, 100) < 10)
            {
                animator.SetBool("isMoving", false);
                OnEncounted();
            }
        }
    }
}
