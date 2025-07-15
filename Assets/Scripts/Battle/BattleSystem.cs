using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Collections.Generic;

public enum BattleState
{
    Start,
    ActionSelection, //行動選択
    MoveSelection, //技選択
    RunningTurn, //敵の技選択
    Busy, //処理中
    partyScreen, //モンスター選択
    BattleOver //バトル終了
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
    int currentMove; //0:左上, 1:右上, 2:左下, 3:右下　の技
    int currentMember; //選択中のモンスターのインデックス

    [SerializeField] AudioSource audio;
    [SerializeField] AudioClip RunSE;
    [SerializeField] AudioClip LevelUpSE;

    MonsterParty playerParty; //プレイヤーのモンスターのパーティー
    Monster wildMonster; //野生のモンスター


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
        //モンスターの生成と描画
        playerUnit.Setup(playerParty.GetHealthyMonster());
        enemyUnit.Setup(wildMonster);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Monster.Moves);
        dialogBox.EnableActionSelector(false);
        yield return dialogBox.TypeDialog($"やせいの{enemyUnit.Monster.Base.Name}あらわれた！");

        // プレイヤーの行動選択へ
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
        dialogBox.SetDialog("どうする？");
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

        //技の決定：ランダム
        Move move = enemyUnit.Monster.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.RunningTurn)
            ActionSelection();
    }    

    IEnumerator RunMove(buttleUnit sourceUnit, buttleUnit targetUnit, Move move  )
    {
        bool canRunMove = sourceUnit.Monster.OnBeforeMove(); //技を使う前の処理
        if(!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Monster);
            yield return sourceUnit.Hub.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Monster);


        move.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}は{move.Base.Name}をつかった");

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
                //ダメージ計算
                DamageDetails damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster);
                //HP反映
                yield return targetUnit.Hub.UpdateHP();
                //相性/クリティカルのメッセージ
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

            //相手が戦闘不能ならメッセージを出して、経験値の更新
            if (targetUnit.Monster.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Monster.Base.Name}はたおれた！");
                targetUnit.PlayerFaintanimation();
                yield return new WaitForSeconds(0.7f);
                CheckForBattleOver(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}は攻撃を外した！");
        }

        
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Monster source, Monster target, Movetarget moveTarget)
    {
        // 技の効果を適用
        if (effects.Boosts != null)
        {
            //技の効果を適用
            if (moveTarget == Movetarget.Self) // 自分にかける技
                source.ApplyBoost(effects.Boosts);
            else
                target.ApplyBoost(effects.Boosts); // 相手にかける技
        }

        // 状態異常の適用
        if (effects.Status != ConditionID.None)
        {
            target.SetStatus(effects.Status); // 状態異常を設定
        }

        if (effects.VolatileStatus != ConditionID.None)
        {
            target.SetVolatileStatus(effects.VolatileStatus); // 状態異常を設定
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(buttleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        //技の効果を適用
        sourceUnit.Monster.OnAfterTurn(); //技を使った後の処理
        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return sourceUnit.Hub.UpdateHP();

        //相手が戦闘不能ならメッセージを出して、経験値の更新
        if (sourceUnit.Monster.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}はたおれた！");
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


    //急所などのメッセージ出力
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if(damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog($"急所にあたった！");
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog($"効果バツグン！");
        }
        else if(damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog($"効果はいまひとつだ・・・");
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
    /// 行動選択の処理
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

        //色でどちらを選択しているのか分かるようにする
        dialogBox.UpdateActionicon(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                MoveSelection();
            }
            else if (currentAction == 1) // 道具
            {

            }
            else if (currentAction == 2) // ポケモンを交換
            {
                prevState = state;
                OpenMonsterScreen();
            }
            else if (currentAction == 3) // 逃げる
            {
                audio.PlayOneShot(RunSE);
                dialogBox.EnableMoveSelector(false);
                StartCoroutine(MoveRun());
            }
        }
    }

    IEnumerator MoveRun()
    {
        yield return dialogBox.TypeDialog($"うまく逃げられた！");
        yield return new WaitForSeconds(0.7f);
        OnBattleOver(false);
    }

    /// <summary>
    /// 技選択の処理
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

            //技決定
            //・技選択のUIは非表示
            dialogBox.EnableMoveSelector(false);
            //・メッセージの復活
            dialogBox.EnableDialogText(true);
            //・技決定の処理
            // 速度の速い順に処理を行う
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            //技選択のUIを非表示
            dialogBox.EnableMoveSelector(false);
            //メッセージの復活
            dialogBox.EnableDialogText(true);
            //行動選択へ戻る
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
                partyScreen.SetMassageText("このポケモンはもう戦えない！");
                return;
            }
            if(selectedMenber == playerUnit.Monster)
            {
                partyScreen.SetMassageText("このポケモンはもう戦えない！");
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
            //モンスター選択のUIを非表示
            partyScreen.gameObject.SetActive(false);
            //行動選択へ戻る
            ActionSelection();
        }
    }   
    
    IEnumerator SwitchMonster(Monster newMonster)
    {
        if(playerUnit.Monster.HP > 0)
        {
            yield return dialogBox.TypeDialog($"戻ってこい！{playerUnit.Monster.Base.Name}");
            playerUnit.PlayerFaintanimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newMonster);
        dialogBox.SetMoveNames(playerUnit.Monster.Moves);
        yield return dialogBox.TypeDialog($"いけ！{newMonster.Base.Name}");

        state = BattleState.RunningTurn;
    }
}
