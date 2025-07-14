using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Monster> wildMonsters; //�쐶�����X�^�[�̃��X�g

    public Monster GetRandomWildMonster()
    {
        var wildMonster = wildMonsters[Random.Range(0, wildMonsters.Count)];
        wildMonster.Init(); //�����X�^�[�̏�����
        return wildMonster; //�����_���ɑI�΂ꂽ�쐶�����X�^�[��Ԃ�
    }
}
