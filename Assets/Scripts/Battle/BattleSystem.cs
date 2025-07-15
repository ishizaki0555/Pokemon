using System.Collections;
using UnityEngine;
using System;


public enum BattleState
{
    Start,
    ActionSelection, //�s���I��
    MoveSelection, //�Z�I��
    PerformMove, //�G�̋Z�I��
    Busy, //������
    partyScreen, //�����X�^�[�I��
    BattleOver //�o�g���I��
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] buttleUnit playerUnit;
    [SerializeField] buttleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    [SerializeField] EXPbar expBar;


    BattleState state;

    int currentAction; //0:Fight, 1:Run
    int currentMove; //0:����, 1:�E��, 2:����, 3:�E���@�̋Z
    int currentMember; //�I�𒆂̃����X�^�[�̃C���f�b�N�X

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

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Monster.Moves);
        dialogBox.EnableActionSelector(false);
        yield return dialogBox.TypeDialog($"�₹���� {enemyUnit.Monster.Base.Name} �����ꂽ�I");

        // �v���C���[�̍s���I����
        ChooseFierTurn();
    }

    void ChooseFierTurn()
    {
        if (playerUnit.Monster.Speed >= enemyUnit.Monster.Speed)
            ActionSelection();
        else
            StartCoroutine(EnemyMove());
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Monsters.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.EnableActionSelector(true);
        dialogBox.SetDialog("�ǂ�����H");
    }

    void OpenMonsterScreen()
    {
        state = BattleState.partyScreen;
        partyScreen.SetPartyData(playerParty.Monsters);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(true);
    }
    //PlayerMove�̎��s
    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        //�Z�̌���
        Move move = playerUnit.Monster.Moves[currentMove];
        yield return StartCoroutine(RunMove(playerUnit, enemyUnit, move));

        // �����퓬��Ԃ�PerformMove�Ȃ�΁A�G�̋Z�����s����
        if (state == BattleState.PerformMove)
            StartCoroutine(EnemyMove());

    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        //�Z�̌���F�����_��
        Move move = enemyUnit.Monster.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.PerformMove)
            ActionSelection();
    }    

    IEnumerator RunMove(buttleUnit sourceUnit, buttleUnit targetUnit, Move move  )
    {
        move.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}��{move.Base.Name}��������");

        sourceUnit.PlayerAttackAnimation();
        audio.PlayOneShot(move.Base.AttackSE);

        yield return new WaitForSeconds(0.7f);
        
        targetUnit.PlayerHitanimation();

        if(move.Base.Category == MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit.Monster, targetUnit.Monster);
        }
        else
        {
            //�_���[�W�v�Z
            DamageDetails damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster);
            //HP���f
            yield return targetUnit.Hub.UpdateHP();
            //����/�N���e�B�J���̃��b�Z�[�W
            yield return ShowDamageDetails(damageDetails);
        } 
        //���肪�퓬�s�\�Ȃ烁�b�Z�[�W���o���āA�o���l�̍X�V
        if (targetUnit.Monster.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Monster.Base.Name} �͂����ꂽ�I");
            targetUnit.PlayerFaintanimation();
            yield return new WaitForSeconds(0.7f);
            CheckForBattleOver(targetUnit);
        }
        sourceUnit.Monster.OnAfterTurn(); //�Z���g������̏���
        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return sourceUnit.Hub.UpdateHP();

        //���肪�퓬�s�\�Ȃ烁�b�Z�[�W���o���āA�o���l�̍X�V
        if (sourceUnit.Monster.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name} �͂����ꂽ�I");
            sourceUnit.PlayerFaintanimation();
            yield return new WaitForSeconds(0.7f);
            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerable RunMoveEffects(Move move, Monster source, Monster target)
    {
        //�Z�̌��ʂ�K�p
        var effects = move.Base.Effects;
        if (effects.Boosts != null)
        {
            if (move.Base.Target == Movetarget.Self)
                source.ApplyBoost(effects.Boosts);
            else
                target.ApplyBoost(effects.Boosts);
        }

        //�X�e�[�^�X�ُ�̓K�p
        if (effects.Status != ConditionID.None)
        {
            target.SetStatus(effects.Status);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Monster monster)
    {
        while(monster.StatusChanges.Count > 0)
        {
            var message = monster.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(buttleUnit faintedUnit)
    {
        if(faintedUnit.IsPlayerUnit)
        {
            var nextMonster = playerParty.GetHealthyMonster();
            if (nextMonster != null)
            {
                OpenMonsterScreen();
            }
            else
                BattleOver(false);
        }
        else
            BattleOver(true);
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
        if(state == BattleState.ActionSelection)
        {
            HandleActionSelect();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelect();
        }
        else if (state == BattleState.partyScreen)
        {
            HandlePartySelection();
        }
    }

    /// <summary>
    /// �s���I���̏���
    /// </summary>
    void HandleActionSelect()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow))
            currentAction++;
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
            currentAction--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3); 

        //�F�łǂ����I�����Ă���̂�������悤�ɂ���
        dialogBox.UpdateActionicon(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                MoveSelection();
            }
            else if (currentAction == 1) // ����
            {

            }
            else if (currentAction == 2) // �|�P����������
            {
                OpenMonsterScreen();
            }
            else if (currentAction == 3) // ������
            {
                audio.PlayOneShot(RunSE);
                dialogBox.EnableMoveSelector(false);
                StartCoroutine(MoveRun());
            }
        }
    }

    IEnumerator MoveRun()
    {
        yield return dialogBox.TypeDialog($"���܂�������ꂽ�I");
        yield return new WaitForSeconds(0.7f);
        OnBattleOver(false);
    }

    /// <summary>
    /// �Z�I���̏���
    /// </summary>
    void HandleMoveSelect()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMove++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMove--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentAction = Mathf.Clamp(currentMove, 0, playerUnit.Monster.Moves.Count - 1);

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
                StartCoroutine(PlayerMove());
            }
            else
            {
                StartCoroutine(EnemyMove());
            }
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            //�Z�I����UI���\��
            dialogBox.EnableMoveSelector(false);
            //���b�Z�[�W�̕���
            dialogBox.EnableDialogText(true);
            //�s���I���֖߂�
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMember++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMember--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;

        currentAction = Mathf.Clamp(currentMember, 0, playerParty.Monsters.Count - 1);

        partyScreen.UpdataMenberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMenber = playerParty.Monsters[currentMember];
            if (selectedMenber.HP <= 0)
            {
                partyScreen.SetMassageText("���̃|�P�����͂����킦�Ȃ��I");
                return;
            }
            if(selectedMenber == playerUnit.Monster)
            {
                partyScreen.SetMassageText("���̃|�P�����͂����킦�Ȃ��I");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchMonster(selectedMenber));
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            //�����X�^�[�I����UI���\��
            partyScreen.gameObject.SetActive(false);
            //�s���I���֖߂�
            ActionSelection();
        }
    }   
    
    IEnumerator SwitchMonster(Monster newMonster)
    {
        bool curentMonsterFainted = true;
        if (playerUnit.Monster.HP > 0)
        {
            curentMonsterFainted = false;
            yield return dialogBox.TypeDialog($"�߂��Ă����A{playerUnit.Monster.Base.name}!");
            playerUnit.PlayerFaintanimation();
            yield return new WaitForSeconds(2f);
        }

        //�����X�^�[�̐����ƕ`��
        playerUnit.Setup(newMonster);
        dialogBox.SetMoveNames(newMonster.Moves);
        yield return dialogBox.TypeDialog($"�����I{newMonster.Base.name}!");

        if(curentMonsterFainted)
            //�����I�����������X�^�[���퓬�s�\�Ȃ�΁A�G�̋Z�����s����
            ChooseFierTurn();
        else
            //�����I�����������X�^�[���퓬�s�\�łȂ���΁A�v���C���[�̋Z�����s����
            StartCoroutine(EnemyMove());        
    }
}
