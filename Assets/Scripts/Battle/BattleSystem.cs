using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Collections.Generic;

public enum BattleState
{
    Start,
    ActionSelection, //�s���I��
    MoveSelection, //�Z�I��
    RunningTurn, //�G�̋Z�I��
    Busy, //������
    partyScreen, //�����X�^�[�I��
    BattleOver //�o�g���I��
}
public enum BattleAction
{
    Move,
    SwitchMonster,
    UseItem,
    Run
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] buttleUnit playerUnit;
    [SerializeField] buttleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;

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
        yield return dialogBox.TypeDialog($"�₹����{enemyUnit.Monster.Base.Name}�����ꂽ�I");

        // �v���C���[�̍s���I����
        ActionSelection();
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

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if(playerAction == BattleAction.Move)
        {
            playerUnit.Monster.CurrentMove = playerUnit.Monster.Moves[currentMove];
            enemyUnit.Monster.CurrentMove = enemyUnit.Monster.GetRandomMove();

            int playerMovePriority = playerUnit.Monster.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Monster.CurrentMove.Base.Priority;

            bool playerGoseFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoseFirst = false;
            else if(enemyMovePriority == playerMovePriority)
                playerGoseFirst = playerUnit.Monster.Speed >= enemyUnit.Monster.Speed;

            var firstUnit = (playerGoseFirst) ? playerUnit: enemyUnit;
            var secondUnit = (playerGoseFirst) ? enemyUnit: playerUnit;

            var seconfMonster = secondUnit.Monster;

            yield return RunMove(firstUnit, secondUnit, firstUnit.Monster.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if(state == BattleState.BattleOver) yield break;

            if (seconfMonster.HP > 0)
            {
                yield return RunMove(secondUnit, firstUnit, secondUnit.Monster.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if(playerAction == BattleAction.SwitchMonster)
            {
                var selectedMonster = playerParty.Monsters[currentMember];
                state = BattleState.Busy;
                yield return SwitchMonster(selectedMonster);
            }

            var enemtMove = enemyUnit.Monster.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemtMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
            ActionSelection();
    }
    IEnumerator EnemyMove()
    {
        state = BattleState.RunningTurn;

        //�Z�̌���F�����_��
        Move move = enemyUnit.Monster.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.RunningTurn)
            ActionSelection();
    }    

    IEnumerator RunMove(buttleUnit sourceUnit, buttleUnit targetUnit, Move move  )
    {
        bool canRunMove = sourceUnit.Monster.OnBeforeMove(); //�Z���g���O�̏���
        if(!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Monster);
            yield return sourceUnit.Hub.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Monster);


        move.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}��{move.Base.Name}��������");

        if (CheckIfMoveHits(move, sourceUnit.Monster, targetUnit.Monster))
        {

            sourceUnit.PlayerAttackAnimation();
            audio.PlayOneShot(move.Base.AttackSE);

            yield return new WaitForSeconds(0.7f);

            targetUnit.PlayerHitanimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Monster, targetUnit.Monster, move.Base.Target);
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

            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Monster.HP > 0)
            {
                foreach(var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if(rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Monster, targetUnit.Monster, secondary.Target);
                }
            }

            //���肪�퓬�s�\�Ȃ烁�b�Z�[�W���o���āA�o���l�̍X�V
            if (targetUnit.Monster.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Monster.Base.Name}�͂����ꂽ�I");
                targetUnit.PlayerFaintanimation();
                yield return new WaitForSeconds(0.7f);
                CheckForBattleOver(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}�͍U�����O�����I");
        }

        
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Monster source, Monster target, Movetarget moveTarget)
    {
        // �Z�̌��ʂ�K�p
        if (effects.Boosts != null)
        {
            //�Z�̌��ʂ�K�p
            if (moveTarget == Movetarget.Self) // �����ɂ�����Z
                source.ApplyBoost(effects.Boosts);
            else
                target.ApplyBoost(effects.Boosts); // ����ɂ�����Z
        }

        // ��Ԉُ�̓K�p
        if (effects.Status != ConditionID.None)
        {
            target.SetStatus(effects.Status); // ��Ԉُ��ݒ�
        }

        if (effects.VolatileStatus != ConditionID.None)
        {
            target.SetVolatileStatus(effects.VolatileStatus); // ��Ԉُ��ݒ�
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(buttleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        //�Z�̌��ʂ�K�p
        sourceUnit.Monster.OnAfterTurn(); //�Z���g������̏���
        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return sourceUnit.Hub.UpdateHP();

        //���肪�퓬�s�\�Ȃ烁�b�Z�[�W���o���āA�o���l�̍X�V
        if (sourceUnit.Monster.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}�͂����ꂽ�I");
            sourceUnit.PlayerFaintanimation();
            yield return new WaitForSeconds(0.7f);
            CheckForBattleOver(sourceUnit);
        }
    }

    bool CheckIfMoveHits(Move move, Monster source, Monster target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if(accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
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
                prevState = state;
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
            var move = playerUnit.Monster.Moves[currentMove];
            if (move.PP == 0) return;

            //�Z����
            //�E�Z�I����UI�͔�\��
            dialogBox.EnableMoveSelector(false);
            //�E���b�Z�[�W�̕���
            dialogBox.EnableDialogText(true);
            //�E�Z����̏���
            // ���x�̑������ɏ������s��
            StartCoroutine(RunTurns(BattleAction.Move));
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

            if(prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(SwitchMonster(selectedMenber));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchMonster(selectedMenber));
            }
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
        if(playerUnit.Monster.HP > 0)
        {
            yield return dialogBox.TypeDialog($"�߂��Ă����I{playerUnit.Monster.Base.Name}");
            playerUnit.PlayerFaintanimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newMonster);
        dialogBox.SetMoveNames(playerUnit.Monster.Moves);
        yield return dialogBox.TypeDialog($"�����I{newMonster.Base.Name}");

        state = BattleState.RunningTurn;
    }
}
