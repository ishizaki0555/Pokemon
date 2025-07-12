using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    //壁判定のLayer
    [SerializeField] LayerMask solidObjectsLayer;
    //くさむら判定のLayer
    [SerializeField] LayerMask longGrassLayer;
    //オプション画面用のキャンバス
    [SerializeField] GameObject OptionCanvas;

    //相互依存を解消：UnityAction(関数を登録する)
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

        //オプションメニューを開く
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

    //１マス徐々に近づける
    IEnumerator Move(Vector3 direction)
    {
        isMove = true;
        Vector3 targetPos = transform.position + direction;
        if(isWalkable(targetPos) == false)
        {
            isMove = false;
            yield break;
        }
        //　現在のターゲットの場所が違うなら、近づけ続ける
        while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            //近づける
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5f * Time.deltaTime); //(現在地, 目標値, 速度) :目標値に近づける
            yield return null;
        }

        transform.position = targetPos;
        isMove = false;
        CheckForEncounters();
    }

    // 特定の位置に移動できるのか判定する関数
    bool isWalkable(Vector3 targetPos)
    {
        // targetPosを中心に円形のRayを作る :　SolidObjectLayerにぶつかったらtureが帰ってくる.だからfalse
        return Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) == false;
    }
    //自分の場所から、円形のRayを飛ばして、草むらのLayerに当たったら、ランダムエンカウント
    void CheckForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position, 0.2f, longGrassLayer))
        {
            // ランダムエンカウント
            if(Random.Range(0, 100) < 10)
            {
                animator.SetBool("isMoving", false);
                OnEncounted();
            }
        }
    }
}
