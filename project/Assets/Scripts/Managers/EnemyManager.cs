using UnityEngine;

//hayakawa edit_file
public class EnemyManager : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public GameObject enemy;
    public float spawnTime = 3f;
    public Transform[] spawnPoints;

    //グリッドのパラメータ
    const int GRID_X = 5;
    const int GRID_Z = 5;
    int[,] gridDens = new int[GRID_X, GRID_Z];

    void Awake()
    {
        //グリッドの初期化
        gridInit();
    }

    void Start ()
    {
        InvokeRepeating ("Spawn", spawnTime, spawnTime);
    }

    void Spawn ()
    {
        if(playerHealth.currentHealth <= 0f)
        {
            return;
        }

        int spawnPointIndex = Random.Range (0, spawnPoints.Length);

        Instantiate (enemy, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
    }


    //グリッドはEnemyの場所把握，分布を求めるために使う
    //グリッドはEnemyMovementからアクセスされるため，吐き出す関数
    public void gridInit()
    {
        for (int i = 0; i < GRID_X; i++)
            for (int j = 0; j < GRID_Z; j++)
                gridDens[i, j] = 0;
    }

    public int getGridDens(int x, int z)
    {
        return gridDens[x, z];
    }

    public int getGridSize(bool xz)
    {
        return (xz) ? GRID_X : GRID_Z;
    }

    public void addGridDens(int x, int z, int i)
    {
        gridDens[x, z] += i;
    }

    public void subGridDens(int x, int z, int i)
    {
        gridDens[x, z] -= i;
    }
}
