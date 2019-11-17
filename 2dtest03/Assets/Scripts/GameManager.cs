using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 2f;
    public float turnDelay = .1f;
    public BoardManager boardScript;
    public static GameManager instance = null;
    public int playerFoodPoints = 120;
    [HideInInspector] public bool playersTurn = true;

    private int level = 1;
    private List<Enemy> enemies;
    private bool enemiesMoving;
    private Text levelText;
    private GameObject levelImage;
    private bool doingSetup;
    
    void Awake()
    {
        Debug.Log("HAHAHAHawake");
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);

        enemies = new List<Enemy>();
        DontDestroyOnLoad(gameObject);
        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    private void OnLevelWasLoaded(int index)
    {
        Debug.Log("HAHAHAHonload");
        level++;
        InitGame();
    }




    void InitGame()
    {
        doingSetup = true;
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "ROUND " + level;
        levelImage.SetActive(true);
        Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();
        boardScript.SetupScene(level);
    }

    private void HideLevelImage()
    {
        levelImage.SetActive(false);
        doingSetup = false;
    }

    public void GameOver()
    {
        levelText.text = "YOU ARE DEAD\n\n" + "SCORE: " + level *10;
        levelText.color = Color.red;
        levelImage.SetActive(true);
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //当这不是playerhuihe且enemy的上一个移动操作已经结束才能执行接下来的协程
        if (playersTurn || enemiesMoving||doingSetup)
            return;
        StartCoroutine(MoveEnemies());
    }

    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
    }

    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;
        yield return new WaitForSeconds(turnDelay);
        if (enemies.Count==0)
        {
            yield return new WaitForSeconds(turnDelay);
        }
        for (int i= 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);
        }

        playersTurn = true;
        enemiesMoving = false;
    }
}
