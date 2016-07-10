using UnityEngine;
using System.Collections;

//hayakawa 既存のファイルをたくさん編集しました…
public class EnemyMovement : MonoBehaviour
{
    Transform player;
    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;
    NavMeshAgent nav;

    //制御のための定数
    const double INSIGHT_RANGE_ZOMBUNNY = 0.6;//ZOMBUNNYの視界
    const double INSIGHT_LENGTH_ZOMBUNNY = 12.0;
    const double SOUND_LENGTH_ZOMBUNNY = 10.0;
    const double INSIGHT_RANGE_ZOMBEAR = 0.95;
    const double INSIGHT_LENGTH_ZOMBEAR = 20.0;
    const double SOUND_LENGTH_ZOMBEAR = 40.0;
    const int TRACK_TIME = 100;
    const int LOITER_TIME = 300;
    const int PREVPOS_TIME = 80;
    const int SEARCH_PLAYER = 0x0001;
    const int FIND_PLAYER = 0x0002;
    const int LOITER = 0x0004;
    const int ZOMBEAR_FL = 0x0001;
    const int ZOMBEAR_FR = 0x0002;
    const int ZOMBEAR_SR = 0x0004;
    const int ZOMBEAR_SL = 0x0008;
    const int ZOMBEAR_B = 0x00016;

    const int ZOMBUNNY = 0;
    const int ZOMBEAR = 1;
    const int HELLEPHANT = 2;

    Transform gunEnd;
    PlayerShooting playerShooting;
    Transform floor;
    public int enemyReconnoiter;
    public int findCount;
    public int loiterCount;
    public Vector3 loiterPosition;
    Vector3 floorLen;
    EnemyManager enemyManager;
    int gridX, gridZ;
    int prevGridX, prevGridZ;
    int gridsizeX, gridsizeZ;
    Vector3 prevPos;
    int prevCount;
    int enemyID;
    Vector3 raidGoal;

    void Awake ()
    {
        player = GameObject.FindGameObjectWithTag ("Player").transform;
        playerHealth = player.GetComponent <PlayerHealth> ();
        enemyHealth = GetComponent <EnemyHealth> ();
        nav = GetComponent <NavMeshAgent> ();
        gunEnd = player.FindChild("GunBarrelEnd").transform;
        playerShooting = gunEnd.GetComponent<PlayerShooting>();
        floor = GameObject.Find("Environment").transform.FindChild("Floor").transform;

        //Floorの座標，初期化
        floorLen = Quaternion.Euler(0f, floor.eulerAngles.y, 0f) * floor.transform.position;

        //グリッド，初期化
        enemyManager = GameObject.Find("EnemyManager").transform.GetComponent<EnemyManager>();
        gridsizeX = enemyManager.getGridSize(true);
        gridsizeZ = enemyManager.getGridSize(false);
        prevGridX = -1;
        prevGridZ = -1;

        //enemyの状態（徘徊，索敵，発見）初期化
        enemyReconnoiter = SEARCH_PLAYER;

        //徘徊時の移動座標，初期化
        loiterCount = 0;
        loiterPosition = new Vector3(0.0f, 0.0f, 0.0f);

        //徘徊時同じ場所で停留させないための変数，初期化
        prevPos = new Vector3(0.0f, 0.0f, 0.0f);
        prevCount = 0;

        //
        raidGoal = new Vector3(0.0f, 0.0f, 0.0f);
        //Enemyの種類判別，取得
        if (transform.gameObject.name == "Zombunny(Clone)")
            enemyID = ZOMBUNNY;
        else if (transform.gameObject.name == "ZomBear(Clone)")
            enemyID = ZOMBEAR;
        else if (transform.gameObject.name == "Hellephant(Clone)")
            enemyID = HELLEPHANT;
    }

    void Update ()
    {
        //位置をグリッドに格納
        inputGrid();
        
        //enemyとplayerが生きているか判定
        bool health = enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0 && playerHealth.currentNemuke > 0;
        if (health)
        {
            //索敵
            if ((enemyReconnoiter & SEARCH_PLAYER) != 0)
                searchPlayer();
            
            //徘徊
            if ((enemyReconnoiter & LOITER) != 0)
                loiter();

            //発見
            if ((enemyReconnoiter & FIND_PLAYER) != 0)
                findPlayer();
        }
        else
        {
            //ナビによる追跡を無効化する
            nav.enabled = false;
        }
    }

    //索敵の状態
    void searchPlayer()
    {
        //敵からみた正面方向と，プレイヤーへの方向の内積
        double player_ran = Vector3.Dot(transform.forward.normalized, (player.position - transform.position).normalized);
        //プレイヤーと敵の距離
        double player_len = Vector3.Distance(transform.position, player.position);

        //敵とプレイヤーの目の位置を決める
        Vector3 eye_position = transform.position;
        Vector3 player_top_position = player.position;

        //最終的に見つかったかを判定する
        bool find = false;

        //ゾンバニー
        if (enemyID == ZOMBUNNY)
        {
            //目の位置設定
            eye_position.y = 1.0f; //Zombunny_eye
            player_top_position.y = 0.5f; //Player

            //目の位置からの，プレイヤーへの方向
            Vector3 player_dir = (player_top_position - eye_position).normalized;

            //レイキャストで障害物がないか判定
            RaycastHit hit;
            Physics.Raycast(eye_position, player_dir, out hit);

            //一定距離内にいて，一定視野角内にいれば真
            bool insight = player_ran > INSIGHT_RANGE_ZOMBUNNY && player_len < INSIGHT_LENGTH_ZOMBUNNY;

            //プレイヤーと敵を阻む障害物があれば真
            bool shield = hit.transform.tag != "Player";

            //音が鳴って、それが聞こえる範囲にいれば真
            bool sound = playerShooting.gun_volume > 0 && player_len < SOUND_LENGTH_ZOMBUNNY;

            //ダメージを食らえば真
            bool damaged = enemyHealth.emDamaged;

            //発見の条件
            find = (insight && !shield) || sound || damaged;
        }
        else if (enemyID == ZOMBEAR)
        {
            eye_position.y = 1.0f; //ZomBear_eye
            player_top_position.y = 0.5f; //Player

            Vector3 player_dir = (player_top_position - eye_position).normalized;
            RaycastHit hit;
            Physics.Raycast(eye_position, player_dir, out hit);

            bool insight = player_ran > INSIGHT_RANGE_ZOMBEAR && player_len < INSIGHT_LENGTH_ZOMBEAR;
            bool shield = hit.transform.tag != "Player";
            bool sound = playerShooting.gun_volume > 0 && player_len < SOUND_LENGTH_ZOMBEAR;
            bool damaged = enemyHealth.emDamaged;
            find = (insight && !shield) || sound || damaged;
        }
        else if(enemyID == HELLEPHANT)
        {
            ;
        }
        
        //発見したか分岐
        if (find)
        {            
            enemyHealth.emDamaged = false;
            //発見状態に遷移
            enemyReconnoiter = FIND_PLAYER;
            //発見のカウンタセット
            findCount = 0;
        }
        else
        {
            //徘徊をする
            enemyReconnoiter |= LOITER;
        }
    }

    //発見の状態
    void findPlayer()
    {
        //敵ごとに対応を変える
        if (enemyID == ZOMBUNNY)
            zombunnyFind();
        else if (enemyID == ZOMBEAR)
            zombearFind();
    }

    //徘徊の状態
    void loiter()
    {
        //以前の位置からあまり動いてなければ徘徊カウントをゼロにする（壁に向かって進んでる場合）
        double prevDis = Mathf.Abs(prevPos.x - transform.position.x) * Mathf.Abs(prevPos.z - transform.position.z);
        prevCount = (prevDis < 0.00001) ? prevCount + 1 : prevCount;
        prevPos = transform.position;

        if (prevCount > PREVPOS_TIME)
        {
            loiterCount = 0;
            prevCount = 0;
        }

        //徘徊カウンタがゼロなら目的位置を指定
        if (loiterCount == 0)
        {
            //徘徊する方向を制御
            float ran = randomDir();
            //float ran = Random.Range(-180, 180);
            loiterPosition.x = Mathf.Cos(ran * (Mathf.PI / 180)) * 40;
            loiterPosition.z = Mathf.Sin(ran * (Mathf.PI / 180)) * 40;
        }

        //セットされたカウンタの目的位置で一定時間は歩き続ける
        if (loiterCount < LOITER_TIME)
        {
            nav.speed = 2;
            nav.SetDestination(transform.position + loiterPosition);
        }
        else//一定時間徘徊したら次の目的位置を決める（カウンタリセット）
            loiterCount = -1;

        loiterCount++;
    }

    //存在してる敵の位置ををグリッドに格納
    void inputGrid()
    {
        if (!(prevGridX == -1 || prevGridZ == -1))
            enemyManager.subGridDens(prevGridX, prevGridZ, 1);

        //床の回転を考慮して，グリッドに位置を格納（５×５），
        Vector3 posRotate = Quaternion.Euler(0f, floor.eulerAngles.y, 0f) * transform.position;
        gridX = (int)((posRotate.x - floorLen.x) / ( - 2 * floorLen.x / gridsizeX));
        gridZ = (int)((posRotate.z + floorLen.z) / (2 * floorLen.z / gridsizeZ));
        gridX = (gridX >= gridsizeX) ? gridsizeX - 1 : gridX;
        gridZ = (gridZ >= gridsizeZ) ? gridsizeZ - 1 : gridZ;
        gridX = (gridX < 0) ? 0 : gridX;
        gridZ = (gridZ < 0) ? 0 : gridZ;
        enemyManager.addGridDens(gridX, gridZ, 1);
        prevGridX = gridX;
        prevGridZ = gridZ;
    }

    //ゾンバニーの発見行動
    void zombunnyFind()
    {
        //一定時間はプレイヤーを追い続ける，時間経過後は索敵戻る
        if (findCount < TRACK_TIME)
        {
            nav.speed = 4;
            nav.SetDestination(player.position);
        }
        else
            enemyReconnoiter = SEARCH_PLAYER;

        findCount++;
    }
    //ゾンベアーの発見行動
    void zombearFind()
    {
        //プレイヤーの視点から，プレイヤー周りにどの位置にいるか内積を使い判定，また距離を計算
        double player_eye = Vector3.Dot(player.forward.normalized, (transform.position - player.position).normalized);
        double player_len = Vector3.Distance(transform.position, player.position);
        Vector3 player_side = Vector3.Cross(player.forward.normalized, player.up.normalized);
        double side = Vector3.Dot(player_side.normalized, (transform.position - player.position).normalized);

        //プレイヤーの前方にいると判断する閾値と横にいる閾値の作成
        float front_ran = Random.Range(-0.2f, 0.5f);
        float side_ran = Random.Range(-0.95f, -0.2f);

        //前にいた場合
        if (player_eye > front_ran)
        {
            //一定距離離れていたらゾンビは索敵に戻る
            if (player_len > INSIGHT_LENGTH_ZOMBEAR * 0.75)
                enemyReconnoiter = SEARCH_PLAYER;
            else
            {
                //プレイヤーの横側に回る
                float ran1 = Random.Range(0.0f, 2.0f) * Mathf.PI;
                Vector3 ran2 = new Vector3(Mathf.Cos(ran1), Mathf.Sin(ran1));
                raidGoal = player_side * Random.Range(5.0f, 12.5f) + player.transform.position + ran2.normalized * Random.Range(1.0f, 5.0f);
                //プレイヤーの前方のうち、右寄りなら右側から、左寄りなら左側にまわる
                raidGoal *= (side > 0) ? 1 : -1;

                nav.speed = 3.5f;
                nav.SetDestination(raidGoal);
            }
        }
        else if (player_eye > side_ran)//横に移った場合
        {
            if (player_len > INSIGHT_LENGTH_ZOMBEAR * 0.75)
                enemyReconnoiter = SEARCH_PLAYER;
            else
            {
                //プレイヤーの背後に回る
                float ran1 = Random.Range(0.0f, 2.0f) * Mathf.PI;
                Vector3 ran2 = new Vector3(Mathf.Cos(ran1), Mathf.Sin(ran1));
                raidGoal = (-player.forward.normalized) * Random.Range(5.0f, 12.5f) + player.transform.position + ran2.normalized * Random.Range(1.0f, 5.0f);
                nav.speed = 3.5f;
                nav.SetDestination(raidGoal);
            }
        }
        else if (player_eye > -1)//背後まで来たとき
        {
            if (player_len > INSIGHT_LENGTH_ZOMBEAR * 0.75)
                enemyReconnoiter = SEARCH_PLAYER;
            else
            {
                //プレイヤーに近づいて攻撃する
                nav.speed = 3.5f;
                nav.SetDestination(player.transform.position);
            }
        }

    }
    //グリッドに格納された分布から，敵の少ないところに移動しやすくさせる重み付きランダム
    float randomDir()
    {
        //自身いるグリッドと接している8グリットを調べる，8グリッド分の人数の合計を計算，１次元配列に変換する
        float[] around = new float[9];
        float sum = 0.0f;
        for(int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                bool check = !(gridX + i < 0 || gridZ + j < 0 || gridX + i > 4 || gridZ + j > 4 || (i == 0 && j == 0));
                if (check)
                {
                    //累乗の逆数をとることで，人数が多いところの重みを小さくする
                    float bunsan = 1000 / Mathf.Pow((enemyManager.getGridDens(gridX + i, gridZ + j) + 1), 4);
                    around[3 * i + j + 4] = bunsan;
                    sum += bunsan;
                }
                else
                    around[3 * i + j + 4] = -1;
            }
        }
        //次に進むグリッドを決定
        float ran = Random.Range(0.0f, (float)sum);
        float pivot = 0.0f;
        int index = 0;
        for (int i = 0; i < 9; i++)
        {   
            if(!(around[i] == -1 || i == 4))
            {
                pivot += around[i];
                if(pivot >= ran)
                {
                    index = i;
                    break;
                }
            }
        }
        //グリッドの方向
        int gridAngle = 0;
        if (index == 0) gridAngle = 180;
        if (index == 1) gridAngle = 135;
        if (index == 2) gridAngle = 90;
        if (index == 3) gridAngle = 225;
        if (index == 5) gridAngle = 45;
        if (index == 6) gridAngle = 270;
        if (index == 7) gridAngle = 315;
        if (index == 8) gridAngle = 0;

        return Random.Range(gridAngle - 35, gridAngle + 35);
    }
}
