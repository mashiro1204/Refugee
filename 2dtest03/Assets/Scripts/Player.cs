using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject
{
    // 1 point dmg per hit to wall
    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;
    public Image hp;
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    private Animator animator;
    // player的当前food点数
    private int food;

    // Start is called before the first frame update
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        
        food = GameManager.instance.playerFoodPoints;

        //foodText.text = "food: " + food;
        hp.rectTransform.localScale = new Vector3(food / 120f, 1, 1);

        base.Start();
    }

    private void OnDisable()
    {
        GameManager.instance.playerFoodPoints = food;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.playersTurn)
            return;

        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal != 0)
            vertical = 0;

        if (horizontal != 0 || vertical != 0)
            AttempMove<Wall>(horizontal,vertical);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Exit")
        {
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        }
        else if(other.tag == "Food")
        {
            food += pointsPerFood;
            if (food > 120)
                food = 120;
            //foodText.text = "+" + pointsPerFood + " food: " + food;
            hp.rectTransform.localScale = new Vector3(food / 120f, 1, 1);
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            food += pointsPerSoda;
            if (food > 120)
                food = 120;
            //foodText.text = "+" + pointsPerSoda + " food: " + food;
            hp.rectTransform.localScale = new Vector3(food / 120f, 1, 1);
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false);
        }
    }

    protected override void OnCantMove<T>(T component)
    {
        if (component != null)
        {
            Wall hitWall = component as Wall;
            hitWall.DamageWall(wallDamage);
            animator.SetTrigger("playerChop");
        }

    }

    protected override void AttempMove<T>(int xDir, int yDir)
    {
        food--;
        if (food < 0)
            food = 0;
        //foodText.text = "food: " + food;
        hp.rectTransform.localScale = new Vector3(food / 120f, 1, 1);

        base.AttempMove<T>(xDir, yDir);

        RaycastHit2D hit;
        if (Move(xDir, yDir, out hit))
        {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }

        CheckIfGameOver();

        GameManager.instance.playersTurn = false;
    }

    private void Restart()
    {
        //Application.LoadLevel(Application.loadedLevel);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoseFood(int loss)
    {
        animator.SetTrigger("playerHit");
        food -= loss;
        if (food < 0)
            food = 0;
        //foodText.text = "-" + loss + " food: " + food;
        hp.rectTransform.localScale = new Vector3(food / 120f, 1, 1);
        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if (food <= 0)
        {
            animator.SetTrigger("playerDeath");
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.musicSource.Stop();
            StartCoroutine(Dying());
            //GameManager.instance.GameOver();
            //SoundManager.instance.PlaySingle(gameOverSound);
            //SoundManager.instance.musicSource.Stop();
        }
    }

    IEnumerator Dying()
    {
        yield return new WaitForSeconds(3.0f);    //只用函数类型为IEnumerator才能使用该语句，延迟两秒
        GameManager.instance.GameOver();
    }
}
