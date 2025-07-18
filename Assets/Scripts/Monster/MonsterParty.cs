using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterParty : MonoBehaviour
{
    [SerializeField] List<Monster> monsters;


    public List<Monster> Monsters
    {
        get { return monsters; }
    }

    private void Start()
    {
        foreach (Monster monster in monsters)
        {
            monster.Init();
        }
    }

    public Monster GetHealthyMonster()
    {
        return monsters.Where(m => m.HP > 0).FirstOrDefault();
    }
}
