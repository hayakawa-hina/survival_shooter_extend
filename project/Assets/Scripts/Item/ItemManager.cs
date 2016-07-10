using UnityEngine;
using System.Collections;

//hayakawa 
public class ItemManager : MonoBehaviour {

    //パラメータ
    public GameObject item;
    Vector3 floorLen;
    Transform floor;
    const int MAX_ITEM_NUM = 3;
    public int nowItemNUM;
    int repopCount;

    public struct RepopTime {
        public int baseTime;
        public int plusTime;

        public RepopTime(int baseTime, int plusTime)
        {
            this.baseTime = baseTime;
            this.plusTime = plusTime;
        }
    };
    RepopTime[] repopTime = new RepopTime[5];

    void Awake()
    {
        //床の座標，長さ計算
        floor = GameObject.Find("Environment").transform.FindChild("Floor").transform;
        floorLen = Quaternion.Euler(0f, floor.eulerAngles.y, 0f) * floor.transform.position;

        //現在のアイテム数，アイテムの上限値
        nowItemNUM = 0;
        repopCount = 200;
        repopTime[0] = new RepopTime(200, 100);
        repopTime[1] = new RepopTime(200, 50);
        repopTime[2] = new RepopTime(175, 50);
        repopTime[3] = new RepopTime(150, 50);
        repopTime[4] = new RepopTime(0, 0);
    }

    void Update()
    {
        itemRepopLoop();
    }

    void itemRepopLoop()
    {
        if (nowItemNUM >= MAX_ITEM_NUM)
        {
            return;
        }

        if(--repopCount < 0)
        {
            itemRepop();
            nowItemNUM++;
            repopCount = randRepopTime();
        }
    
    }

    public void itemRepop()
    {
        Vector3 spawnPoint = randSpawnPoint();
        //インスタンス生成
        Instantiate(item, spawnPoint, item.transform.rotation);
    }

    public Vector3 randSpawnPoint()
    {
        //floorの内側に対してランダムに位置を決める
        float spX = Random.Range(floorLen.x * 0.9f, -floorLen.x * 0.9f);
        float spZ = Random.Range(-floorLen.z * 0.9f, floorLen.z * 0.9f);
        Vector3 spawnPoint = new Vector3(spX, item.transform.position.y, spZ);

        //floor基準からからワールド座標に戻す
        spawnPoint = Quaternion.Euler(0f, -floor.eulerAngles.y, 0f) * spawnPoint;

        return spawnPoint;
    }
   
    int randRepopTime()
    {
        int len = repopTime.Length;
        RepopTime rt = repopTime[(int)Random.Range(0, len)];
        return rt.baseTime + Random.Range(0, rt.plusTime);
    }
}
