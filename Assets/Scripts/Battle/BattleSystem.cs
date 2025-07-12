using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;

public enum BattleState
{
    Start,
    PlayerAction, //�s���I��
    PlayerMove, //�Z�I��
    EnemyMove,
    Busy,
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] buttleUnit playerUnit;
    [SerializeField] buttleUnit enemyUnit;
    [SerializeField] ButtleHub playerHub;
    [SerializeField] ButtleHub enemyHub;
    [SerializeField] BattleDialogBox dialogBox;
    public UnityAction BattleOver;

    [SerializeField] EXPbar expBar;


    BattleState state;
    int currentAction; //0:Fight, 1:Run
    int currentMove; //0:����, 1:�E��, 2:����, 3:�E���@�̋Z
    [SerializeField] AudioSource audio;
    [SerializeField] AudioClip RunSE;
    [SerializeField] AudioClip LevelUpSE;

    MonsterParty playerParty; //�v���C���[�̃����X�^�[�̃p�[�e�B�[
    Monster wildMonster; //�쐶�̃����X�^�[


    public void StartBattle(MonsterParty monsterParty, Monster wildMonster)
    {   
        this.playerParty = monsterParty;
        this.wildMonster = wildMonster;
        StartCoroutine(SetupBattle());
        if(playerUnit.Monster.Base.Level == 1)
        {
            playerUnit.Monster.Base.NecessaryExp = 2;
            playerUnit.Monster.Base.Exp = 0;
        }
        else
        {
            playerUnit.Monster.Base.NecessaryExp = Mathf.Floor(Mathf.Pow(playerUnit.Monster.Base.Level, 3) * (24 + Mathf.Floor((playerUnit.Monster.Base.Level + 1) / 3)) / 50);
        }
        expBar.SetEXP((float)playerUnit.Monster.Base.Exp / (float)playerUnit.Monster.Base.NecessaryExp);
    }
    IEnumerator SetupBattle()
    {
        state = BattleState.Start;
        //�����X�^�[�̐����ƕ`��
        playerUnit.Setup(playerParty.GetHealthyMonster());
        enemyUnit.Setup(wildMonster);

        //HUD�̕`��
        playerHub.SetData(playerUnit.Monster);
        enemyHub.SetData(enemyUnit.Monster);
        dialogBox.SetMoveNames(playerUnit.Monster.Moves);
        dialogBox.EnableActionSelector(false);
        yield return dialogBox.TypeDialog($"�₹���� {enemyUnit.Monster.Base.Name} �����ꂽ�I");

        // �v���C���[�̍s���I����
        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.EnableActionSelector(true);
        StartCoroutine(dialogBox.TypeDialog("�ǂ�����H"));
    }
    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(true);
    }
    //PlayerMove�̎��s
    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;

        //�Z�̌���
        Move move = playerUnit.Monster.Moves[currentMove];
        move.PP--;

        yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name}��{move.Base.Name}��������");
        playerUnit.PlayerAttackAnimation();
        audio.PlayOneShot(move.Base.AttackSE);
        yield return new WaitForSeconds(0.7f);
        enemyUnit.PlayerHitanimation();

        //�_���[�W�v�Z
        DamageDetails damageDetails = enemyUnit.Monster.TakeDamage(move, playerUnit.Monster);
        //HP���f
        yield return enemyHub.UpdateHP();
        //����/�N���e�B�J���̃��b�Z�[�W
        yield return ShowDamageDetails(damageDetails);
        //���肪�퓬�s�\�Ȃ烁�b�Z�[�W���o���āA�o���l�̍X�V
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} �͂����ꂽ�I");
            enemyUnit.PlayerFaintanimation();
            yield return new WaitForSeconds(0.7f);
            float exp = GetExp();
            yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name}�͌o���l " + exp + "����ɓ��ꂽ�I");
            yield return StartCoroutine(expBar.SetEXPSmooth(Mathf.Clamp((float)playerUnit.Monster.Base.Exp, 0, playerUnit.Monster.Base.NecessaryExp) / playerUnit.Monster.Base.NecessaryExp, exp));
            if(playerUnit.Monster.Base.NecessaryExp <= playerUnit.Monster.Base.Exp) // ���x�����オ������
            {
                float overExp = OverGetExp();
                yield return StartCoroutine(LevelUp());
                //���ӂꂽ�o���l�F���x�����オ�����烋�[�v����
                while(overExp > 0)
                {
                    playerUnit.Monster.Base.Exp += overExp;
                    yield return StartCoroutine(expBar.SetEXPSmooth(Mathf.Clamp((float)playerUnit.Monster.Base.Exp, 0, playerUnit.Monster.Base.NecessaryExp) / playerUnit.Monster.Base.NecessaryExp, exp));
                    if (playerUnit.Monster.Base.Exp >= playerUnit.Monster.Base.NecessaryExp)
                    {
                        overExp = OverGetExp();
                        Debug.Log(overExp);
                        yield return StartCoroutine(LevelUp());
                    }
                    else
                    {
                        overExp = 0;
                    }
                }
            }
            yield return new WaitForSeconds(0.7f);
            BattleOver();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    //���x���̉��Z�ƕK�v�o���l�̍Čv�Z/�f�[�^�̓K�p
    IEnumerator LevelUp()
    {
        yield return new WaitForSeconds(0.7f);
        playerUnit.Monster.Base.Level++;
        playerUnit.Monster.Base.NecessaryExp = Mathf.Floor(Mathf.Pow(playerUnit.Monster.Base.Level, 3) * (24 + Mathf.Floor((playerUnit.Monster.Base.Level + 1) / 3)) / 50);
        playerUnit.Monster.Base.Exp = 0;
        expBar.SetEXP(playerUnit.Monster.Base.Exp / playerUnit.Monster.Base.NecessaryExp);
        playerHub.SetData(playerUnit.Monster);
        //playerUnit.UpdatAwake();
        playerHub.SetData(playerUnit.Monster);
        audio.PlayOneShot(LevelUpSE);
        yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name}��{playerUnit.Monster.Base.Level}�ɏオ�����I");
    }

    //��ꂽ�o���l�̌v�Z
    float OverGetExp()
    {
        float overExp = playerUnit.Monster.Base.Exp - playerUnit.Monster.Base.NecessaryExp;
        return overExp;
    }

    //�o���l�̉��Z
    float GetExp()
    {
        playerUnit.Monster.Base.Exp += enemyUnit.Monster.Base.BasicExp / 1 * enemyUnit.Monster.Base.Level / 7 * 1 * 1 / 1;
        return enemyUnit.Monster.Base.BasicExp / 1 * enemyUnit.Monster.Base.Level / 7 * 1 * 1 / 1;
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        //�Z�̌���F�����_��
        Move move = enemyUnit.Monster.GetRandomMove();
        move.PP--;
        yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name}��{move.Base.Name}��������");

        //�G�̍U���A�j���[�V����
        enemyUnit.PlayerAttackAnimation();
        audio.PlayOneShot(move.Base.AttackSE);
        yield return new WaitForSeconds(0.7f);
        playerUnit.PlayerHitanimation();

        //�_���[�W�v�Z
        DamageDetails damageDetails = playerUnit.Monster.TakeDamage(move, enemyUnit.Monster);
        //HP���f
        yield return playerHub.UpdateHP();

        //����/�N���e�B�J���̃��b�Z�[�W
        yield return ShowDamageDetails(damageDetails);
        //�퓬�s�\�Ȃ烁�b�Z�[�W
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} �͂����ꂽ�I");
            playerUnit.PlayerFaintanimation();

            yield return new WaitForSeconds(0.7f);
            var nextMonster = playerParty.GetHealthyMonster();
            if(nextMonster != null)
            {
                state = BattleState.Start;
                //�����X�^�[�̐����ƕ`��
                playerUnit.Setup(nextMonster);

                //HUD�̕`��
                playerHub.SetData(nextMonster);

                dialogBox.SetMoveNames(nextMonster.Moves);
                dialogBox.EnableActionSelector(false);

                yield return dialogBox.TypeDialog($"�����I{nextMonster.Base.name}!");

                // �v���C���[�̍s���I����
                PlayerAction();
            }
            else
            {
                BattleOver();
            }
        }
        else
        {
            // ����ȊO�Ȃ�
            PlayerAction();
        }
    }

    //�}���Ȃǂ̃��b�Z�[�W�o��
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if(damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog($"�}���ɂ��������I");
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog($"���ʃo�c�O���I");
        }
        else if(damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog($"���ʂ͂��܂ЂƂ��E�E�E");
        }
    }

    public  void HandleUpdate()
    {
        if(state == BattleState.PlayerAction)
        {
            HandleActionSelect();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelect();
        }
    }
    //�v���C���[�̍s������������
    void HandleActionSelect()
    {
        //������͂����Run,�����͂����Fight�ɂȂ�
        //0:Fight
        //1:Run
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
            {
                currentAction++;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
            {
                currentAction--;
            }
        }

        //�F�łǂ����I�����Ă���̂�������悤�ɂ���
        dialogBox.UpdateActionicon(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                audio.PlayOneShot(RunSE);
                dialogBox.EnableMoveSelector(false);
                StartCoroutine(RunMove());
            }
        }
    }

    IEnumerator RunMove()
    {
        yield return dialogBox.TypeDialog($"���܂�������ꂽ�I");
        yield return new WaitForSeconds(0.7f);
        BattleOver();
    }


    void HandleMoveSelect()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Monster.Moves.Count - 1)
            {
                currentMove++;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
            {
                currentMove--;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Monster.Moves.Count - 2)
            {
                currentMove += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
            {
                currentMove -= 2;
            }
        }
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Monster.Moves[currentMove]);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            //�Z����
            //�E�Z�I����UI�͔�\��
            dialogBox.EnableMoveSelector(false);
            //�E���b�Z�[�W�̕���
            dialogBox.EnableDialogText(true);
            //�E�Z����̏���
            // ���x�̑������ɏ������s��
            if (playerUnit.Monster.Speed > enemyUnit.Monster.Speed)
            {
                StartCoroutine(PerformPlayerMove());
            }
            else
            {
                StartCoroutine(EnemyMove());
            }
        }
    }
}
