using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public enum BattleState
{
    Start,
    ActionSelection, //行動選択
    MoveSelection, //技選択
    PerformMove, //敵の技選択
    Busy, //処理中
    partyScreen, //モンスター選択
    BattleOver //バトル終了
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
        expBar.SetEXP((float)playerUnit.Monster.Base.Exp / (float)playerUnit.Monster.Base.NecessaryExp);
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
        yield return dialogBox.TypeDialog($"やせいの {enemyUnit.Monster.Base.Name} あらわれた！");

        // プレイヤーの行動選択へ
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
    //PlayerMoveの実行
    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        //技の決定
        Move move = playerUnit.Monster.Moves[currentMove];
        yield return StartCoroutine(RunMove(playerUnit, enemyUnit, move));

        // もし戦闘状態がPerformMoveならば、敵の技を実行する
        if (state == BattleState.PerformMove)
            StartCoroutine(EnemyMove());

    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        //技の決定：ランダム
        Move move = enemyUnit.Monster.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.PerformMove)
            ActionSelection();
    }

    //レベルの加算と必要経験値の再計算/データの適用
    //IEnumerator LevelUp()
    //{
    //    yield return new WaitForSeconds(0.7f);
    //    playerUnit.Monster.Base.Level++;
    //    playerUnit.Monster.Base.NecessaryExp = Mathf.Floor(Mathf.Pow(playerUnit.Monster.Base.Level, 3) * (24 + Mathf.Floor((playerUnit.Monster.Base.Level + 1) / 3)) / 50);
    //    playerUnit.Monster.Base.Exp = 0;
    //    expBar.SetEXP(playerUnit.Monster.Base.Exp / playerUnit.Monster.Base.NecessaryExp);
    //    playerHub.SetData(playerUnit.Monster);
    //    //playerUnit.UpdatAwake();
    //    playerHub.SetData(playerUnit.Monster);
    //    audio.PlayOneShot(LevelUpSE);
    //    yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name}は{playerUnit.Monster.Base.Level}に上がった！");
    //}

    ////溢れた経験値の計算
    //float OverGetExp()
    //{
    //    float overExp = playerUnit.Monster.Base.Exp - playerUnit.Monster.Base.NecessaryExp;
    //    return overExp;
    //}

    ////経験値の加算
    //float GetExp()
    //{
    //    playerUnit.Monster.Base.Exp += enemyUnit.Monster.Base.BasicExp / 1 * enemyUnit.Monster.Base.Level / 7 * 1 * 1 / 1;
    //    return enemyUnit.Monster.Base.BasicExp / 1 * enemyUnit.Monster.Base.Level / 7 * 1 * 1 / 1;
    //}

    

    IEnumerator RunMove(buttleUnit sourceUnit, buttleUnit targetUnit, Move move  )
    {
        move.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}は{move.Base.Name}をつかった");

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
            //ダメージ計算
            DamageDetails damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster);
            //HP反映
            yield return targetUnit.Hub.UpdateHP();
            //相性/クリティカルのメッセージ
            yield return ShowDamageDetails(damageDetails);
        } 
        //相手が戦闘不能ならメッセージを出して、経験値の更新
        if (targetUnit.Monster.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Monster.Base.Name} はたおれた！");
            targetUnit.PlayerFaintanimation();
            yield return new WaitForSeconds(0.7f);
            /*float exp = GetExp();
            yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name}は経験値 " + exp + "を手に入れた！");
            yield return StartCoroutine(expBar.SetEXPSmooth(Mathf.Clamp((float)playerUnit.Monster.Base.Exp, 0, playerUnit.Monster.Base.NecessaryExp) / playerUnit.Monster.Base.NecessaryExp, exp));
            if (playerUnit.Monster.Base.NecessaryExp <= playerUnit.Monster.Base.Exp) // レベルが上がった時
            {
                float overExp = OverGetExp();
                yield return StartCoroutine(LevelUp());
                //あふれた経験値：レベルが上がったらループする
                while (overExp > 0)
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
            }*/
            yield return new WaitForSeconds(0.7f);
            CheckForBattleOver(targetUnit);
        }
    }

    IEnumerable RunMoveEffects(Move move, Monster source, Monster target)
    {
        var effects = move.Base.Effects;
        if (move.Base.Effects.Boosts != null)
        {
            if (move.Base.Target == Movetarget.Self)
                source.ApplyBoost(effects.Boosts);
            else
                target.ApplyBoost(effects.Boosts);
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
                OpenMonsterScreen();
            }
            else if (currentAction == 3) // 逃げる
            {
                audio.PlayOneShot(RunSE);
                dialogBox.EnableMoveSelector(false);
                StartCoroutine(RunMove());
            }
        }
    }

    IEnumerator RunMove()
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
            //技決定
            //・技選択のUIは非表示
            dialogBox.EnableMoveSelector(false);
            //・メッセージの復活
            dialogBox.EnableDialogText(true);
            //・技決定の処理
            // 速度の速い順に処理を行う
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
            state = BattleState.Busy;
            StartCoroutine(SwitchMonster(selectedMenber));
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
        bool curentMonsterFainted = true;
        if (playerUnit.Monster.HP > 0)
        {
            curentMonsterFainted = false;
            yield return dialogBox.TypeDialog($"戻ってこい、{playerUnit.Monster.Base.name}!");
            playerUnit.PlayerFaintanimation();
            yield return new WaitForSeconds(2f);
        }

        //モンスターの生成と描画
        playerUnit.Setup(newMonster);
        dialogBox.SetMoveNames(newMonster.Moves);
        yield return dialogBox.TypeDialog($"いけ！{newMonster.Base.name}!");

        if(curentMonsterFainted)
            //もし選択したモンスターが戦闘不能ならば、敵の技を実行する
            ChooseFierTurn();
        else
            //もし選択したモンスターが戦闘不能でなければ、プレイヤーの技を実行する
            StartCoroutine(EnemyMove());        
    }
}
