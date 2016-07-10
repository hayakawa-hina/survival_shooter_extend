using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public PlayerHealth playerNemuke;


    Animator anim;


    void Awake()
    {
        anim = GetComponent<Animator>();
    }


    void Update()
    {
        if (playerHealth.currentHealth <= 0 || playerHealth.currentNemuke <= 0)
        {
            anim.SetTrigger("GameOver");
        }
    }
}
