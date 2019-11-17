using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject
{
    public int playerDamage;
    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;

    private Animator animator;
    private Transform target;
    private bool skipMove;
    private int xAttempDir;
    private int yAttempDir;
    private int Count;

    // Start is called before the first frame update
    protected override void Start()
    {
        GameManager.instance.AddEnemyToList(this);
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
    }

    protected override void AttempMove<T>(int xDir, int yDir)
    {
        Count += 1;
        if (skipMove)
        {
            skipMove = false;
            return;
        }

        base.AttempMove<T>(xDir, yDir);
        skipMove = true;
    }

    public void MoveEnemy()
    {
        int xDir = 0;
        int yDir = 0;
        Count = 0;
        xAttempDir = 0;
        yAttempDir = 0;

        if ((transform.position.x - target.position.x) > Mathf.Epsilon)
            xAttempDir = -1;
        else if ((transform.position.x - target.position.x) < -Mathf.Epsilon)
            xAttempDir = 1;

        if ((transform.position.y - target.position.y) > Mathf.Epsilon)
            yAttempDir = -1;
        else if ((transform.position.y - target.position.y) < -Mathf.Epsilon)
            yAttempDir = 1;


        if (Mathf.Abs(transform.position.x - target.position.x) < Mathf.Abs(transform.position.y - target.position.y))
        {

            yDir = transform.position.y > target.position.y ? -1 : 1;

            yAttempDir = 0;
        }
        else
        {
            xDir = transform.position.x > target.position.x ? -1 : 1;

            xAttempDir = 0;
        }

        AttempMove<Player>(xDir,yDir);
    }

    protected override void OnCantMove<T>(T component)
    {
        Debug.Log(xAttempDir + "\n" + yAttempDir + "\n" + "\n");
        if (component != null)
        {
            Player hitPlayer = component as Player;

            hitPlayer.LoseFood(playerDamage);

            animator.SetTrigger("enemyAttack");

            SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);
        }
        else if((xAttempDir==0 && yAttempDir == 0)||Count==2)
        {

        }
        else
        {
            AttempMove<Player>(xAttempDir, yAttempDir);
            xAttempDir = 0;
            yAttempDir = 0;
        }
    }
}
