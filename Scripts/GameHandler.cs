using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;

public class GameHandler : MonoBehaviour
{
    public AudioSource mainCameraAudioSource;

    private static GameHandler instance;

    // private static int score; 
    private static int score;
    private static int level;
    private static int scoreThreshold = 500;

  public static event Action<int> OnLevelChanged;

    private LevelGrid levelGrid;
    [SerializeField]private Snake snake;

    
    public GameObject enemySnakep;
    public int enemySnakeCount = 1; // Number of enemy snakes on the map


    private void Awake(){
      instance = this;
      InitializeStatic();
    }

    void Start(){
      Debug.Log("Starting GameHandler");
      level = 1;
      mainCameraAudioSource = Camera.main.GetComponent<AudioSource>();

      levelGrid = new LevelGrid(15, 10);

      levelGrid.Setup(snake);

      snake.Setup(levelGrid);

      for (int i = 0; i < enemySnakeCount -1; i++){
        GameObject enemySnake = Instantiate(enemySnakep);
        EnemySnake enemySnakeScript = enemySnake.GetComponent<EnemySnake>();
        enemySnakeScript.Setup(levelGrid); // Pass the level grid to each enemy
      }


      Debug.Log("GameHandler setup complete");
    }

      public static int GetLevel() {
        return level;
    }

    public static void InitializeStatic(){
      score = 0;
    }

    public static int GetScore(){
      return score;
    }

    public static void AddScore(){
      score += 100;

      if (score >= level * scoreThreshold) {
        Debug.Log(level * scoreThreshold);
            LevelUp();
        }
    }

    private static void LevelUp() {
        level++;
        OnLevelChanged?.Invoke(level);
        
    }

    public static void SnakeDied(){
      GameOverWindow.ShowStatic();

      if (instance.mainCameraAudioSource != null) {
        instance.mainCameraAudioSource.Stop();
    } else {
        Debug.LogWarning("Main camera audio source not found!");
    }
      
    }

}
