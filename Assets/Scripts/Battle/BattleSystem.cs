using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;

public enum BattleState
{
    Start,
    PlayerAction, //行動選択
    PlayerMove, //技選択
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
    int currentMove; //0:左上, 1:右上, 2:左下, 3:右下　の技
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

        //HUDの描画
        playerHub.SetData(playerUnit.Monster);
        enemyHub.SetData(enemyUnit.Monster);
        dialogBox.SetMoveNames(playerUnit.Monster.Moves);
        dialogBox.EnableActionSelector(false);
        yield return dialogBox.TypeDialog($"やせいの {enemyUnit.Monster.Base.Name} あらわれた！");

        // プレイヤーの行動選択へ
        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.EnableActionSelector(true);
        StartCoroutine(dialogBox.TypeDialog("どうする？"));
    }
    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(true);
    }
    //PlayerMoveの実行
    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;

        //技の決定
        Move move = playerUnit.Monster.Moves[currentMove];
        move.PP--;

        yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name}は{move.Base.Name}をつかった");
        playerUnit.PlayerAttackAnimation();
        audio.PlayOneShot(move.Base.AttackSE);
        yield return new WaitForSeconds(0.7f);
        enemyUnit.PlayerHitanimation();

        //ダメージ計算
        DamageDetails damageDetails = enemyUnit.Monster.TakeDamage(move, playerUnit.Monster);
        //HP反映
        yield return enemyHub.UpdateHP();
        //相性/クリティカルのメッセージ
        yield return ShowDamageDetails(damageDetails);
        //相手が戦闘不能ならメッセージを出して、経験値の更新
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} はたおれた！");
            enemyUnit.PlayerFaintanimation();
            yield return new WaitForSeconds(0.7f);
            float exp = GetExp();
            yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name}は経験値 " + exp + "を手に入れた！");
            yield return StartCoroutine(expBar.SetEXPSmooth(Mathf.Clamp((float)playerUnit.Monster.Base.Exp, 0, playerUnit.Monster.Base.NecessaryExp) / playerUnit.Monster.Base.NecessaryExp, exp));
            if(playerUnit.Monster.Base.NecessaryExp <= playerUnit.Monster.Base.Exp) // レベルが上がった時
            {
                float overExp = OverGetExp();
                yield return StartCoroutine(LevelUp());
                //あふれた経験値：レベルが上がったらループする
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

    //レベルの加算と必要経験値の再計算/データの適用
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
        yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name}は{playerUnit.Monster.Base.Level}に上がった！");
    }

    //溢れた経験値の計算
    float OverGetExp()
    {
        float overExp = playerUnit.Monster.Base.Exp - playerUnit.Monster.Base.NecessaryExp;
        return overExp;
    }

    //経験値の加算
    float GetExp()
    {
        playerUnit.Monster.Base.Exp += enemyUnit.Monster.Base.BasicExp / 1 * enemyUnit.Monster.Base.Level / 7 * 1 * 1 / 1;
        return enemyUnit.Monster.Base.BasicExp / 1 * enemyUnit.Monster.Base.Level / 7 * 1 * 1 / 1;
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        //技の決定：ランダム
        Move move = enemyUnit.Monster.GetRandomMove();
        move.PP--;
        yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name}は{move.Base.Name}をつかった");

        //敵の攻撃アニメーション
        enemyUnit.PlayerAttackAnimation();
        audio.PlayOneShot(move.Base.AttackSE);
        yield return new WaitForSeconds(0.7f);
        playerUnit.PlayerHitanimation();

        //ダメージ計算
        DamageDetails damageDetails = playerUnit.Monster.TakeDamage(move, enemyUnit.Monster);
        //HP反映
        yield return playerHub.UpdateHP();

        //相性/クリティカルのメッセージ
        yield return ShowDamageDetails(damageDetails);
        //戦闘不能ならメッセージ
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} はたおれた！");
            playerUnit.PlayerFaintanimation();

            yield return new WaitForSeconds(0.7f);
            var nextMonster = playerParty.GetHealthyMonster();
            if(nextMonster != null)
            {
                state = BattleState.Start;
                //モンスターの生成と描画
                playerUnit.Setup(nextMonster);

                //HUDの描画
                playerHub.SetData(nextMonster);

                dialogBox.SetMoveNames(nextMonster.Moves);
                dialogBox.EnableActionSelector(false);

                yield return dialogBox.TypeDialog($"いけ！{nextMonster.Base.name}!");

                // プレイヤーの行動選択へ
                PlayerAction();
            }
            else
            {
                BattleOver();
            }
        }
        else
        {
            // それ以外なら
            PlayerAction();
        }
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
        if(state == BattleState.PlayerAction)
        {
            HandleActionSelect();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelect();
        }
    }
    //プレイヤーの行動を処理する
    void HandleActionSelect()
    {
        //下を入力するとRun,上を入力するとFightになる
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

        //色でどちらを選択しているのか分かるようにする
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
        yield return dialogBox.TypeDialog($"うまく逃げられた！");
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
            //技決定
            //・技選択のUIは非表示
            dialogBox.EnableMoveSelector(false);
            //・メッセージの復活
            dialogBox.EnableDialogText(true);
            //・技決定の処理
            // 速度の速い順に処理を行う
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
