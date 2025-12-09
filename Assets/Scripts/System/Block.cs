using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType { Dirt, Grass, Water }
public class Block : MonoBehaviour
{
    [Header("Block Stat")]
    public BlockType type = BlockType.Dirt;
    public int maxHp = 3;
    [HideInInspector] public int hp;

    public int dropCount = 1;
    public bool mineable = true;

    void Awake()
    {
        hp = maxHp;

        if (GetComponent<Collider>() == null)
        {
            var bc = gameObject.AddComponent<BoxCollider>();
            bc.size = Vector3.one;
        }

        try
        {
            gameObject.tag = "Block";
        }
        catch
        {
            Debug.LogWarning("[Block] 'Block' 태그가 프로젝트에 존재하지 않습니다. 에디터 태그 설정을 확인하세요.");
        }
    }

    public void Hit(int damage, Inventory inven)
    {
        if (!mineable) return;

        hp -= damage;

        if (hp < 0)
        {
            if (inven != null && dropCount > 0)
                inven.Add(type, dropCount);

            Destroy(gameObject);
        }
    }
}