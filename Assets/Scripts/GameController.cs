using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    FreeRoam, //マップ移動
    Option, //オプション画面
    Battle, //バトル
}

public class GameController : MonoBehaviour
{
    //ゲームの状態を管理
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state  = GameState.FreeRoam;

    public int rows = 5; // 行数
    public int cols = 5; // 列数
    public GameObject squarePrefab; // Imageのプレハブ
    public float animationDelay = 0.1f; // アニメーション間隔
    public List<GameObject> squareObject = new List<GameObject>();
    [SerializeField] GameObject ParentCanvas;
    [SerializeField] AudioSource audio;
    [SerializeField] AudioClip[] BattleBGM;
    [SerializeField] AudioClip RunBGM;     
 
    private GameObject[,] grid; // UIのグリッドを管理

    private void Start()
    {
        playerController.OnEncounted += StartAnimSet;
        battleSystem.OnBattleOver += EndBattle;
        grid = new GameObject[rows, cols];
        GenerateGrid();
    }

    public void StartAnimSet()
    {
        StartCoroutine(StartBattle());
    }

    //ここでエンカウント用のアニメーションをしたい(DOTween)
    public IEnumerator StartBattle()
    {
        state = GameState.Battle;
        audio.clip = BattleBGM[Random.Range(0, BattleBGM.Length)];
        audio.Play();
        StartCoroutine(AnimateSpiral());
        yield return new WaitForSeconds(3.5f);
        ParentCanvas.SetActive(false);
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<MonsterParty>();
        var wildMonster = FindAnyObjectByType<MapArea>().GetComponent<MapArea>().GetRandomWildMonster();

        battleSystem.StartBattle(playerParty, wildMonster);
        yield return null;
    }

    void GenerateGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                GameObject newSquare = Instantiate(squarePrefab, new Vector2(i * 100, j * 100), Quaternion.identity);
                newSquare.SetActive(false); // 初めは非表示
                squareObject.Add(newSquare);
                newSquare.transform.parent = ParentCanvas.transform;
                grid[i, j] = newSquare;
                
            }
        }
    }

    public IEnumerator AnimateSpiral()
    {
        int left = 0, right = cols - 1, top = 0, bottom = rows - 1;
        while (left <= right && top <= bottom)
        {
            for (int i = left; i <= right; i++) yield return AnimateCell(top, i);
            top++;
            for (int i = top; i <= bottom; i++) yield return AnimateCell(i, right);
            right--;
            for (int i = right; i >= left; i--) yield return AnimateCell(bottom, i);
            bottom--;
            for (int i = bottom; i >= top; i--) yield return AnimateCell(i, left);
            left++;
        }
    }

    IEnumerator AnimateCell(int i, int j)
    {
        grid[i, j].SetActive(true);
        yield return new WaitForSeconds(animationDelay);
    }

    public void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        for(int i = 0; i < squareObject.Count; i++)
        {
            squareObject[i].SetActive(false);
        }
        ParentCanvas.SetActive(true);
        audio.clip = RunBGM;
        audio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if(state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if(state == GameState.Battle) 
        {
            battleSystem.HandleUpdate();
        }
    }
}
