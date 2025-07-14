using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Monster> wildMonsters; //野生モンスターのリスト

    public Monster GetRandomWildMonster()
    {
        var wildMonster = wildMonsters[Random.Range(0, wildMonsters.Count)];
        wildMonster.Init(); //モンスターの初期化
        return wildMonster; //ランダムに選ばれた野生モンスターを返す
    }
}
