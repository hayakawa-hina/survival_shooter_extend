using UnityEngine;
using System.Collections;

//hayakawa new_file
public class ItemGet: MonoBehaviour {

    public bool playerInRange;
    GameObject player;
    GameObject ItemN;
    PlayerHealth playerHealth;
    ItemManager itemManager;

    const int ITEM_N_POINT = 20;
    const int ITEM_TIME = 5000;
    const int FIRST_REPOS = 10;
    int itemCount;
    bool posLock;

    void Awake () {
        //各種変数初期化
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
        itemManager = GameObject.Find("ItemManager").transform.GetComponent<ItemManager>();
        ItemN = gameObject;
        itemCount = 0;
        playerInRange = false;
        posLock = false;
    }

	void Update ()
    {
        if (playerInRange)
        {
            playerHealth.currentNemuke += ITEM_N_POINT;
            playerHealth.currentNemuke = (playerHealth.currentNemuke > 100) ? 100 : playerHealth.currentNemuke;

            itemManager.nowItemNUM--;
            Destroy(ItemN);
        }

        //一定時間経過後アイテムを削除
        if (itemCount > ITEM_TIME)
        {
            itemManager.nowItemNUM--;
            Destroy(ItemN);
        }

        //アイテムの制限時間カウント進める
        itemCount++;

        //一定時間経過後アイテムの位置変更許可をロックする
        if (itemCount > FIRST_REPOS)
            posLock = true;
    }

    void OnTriggerEnter(Collider other)
    {
        //コライダーの範囲内にPlayerが入ったか検出
        if (other.gameObject == player)
            playerInRange = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        //プレイヤー以外のオブジェクトと位置が被ったら位置を再び検出し直す
        if (collision.gameObject != player && !posLock)
            gameObject.transform.position = itemManager.randSpawnPoint();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            playerInRange = false;
        }
    }
}
