using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets instance;
    public Sprite snakeHeadSprite;
    public Sprite foodSprite;
    public Sprite snakeBodySprite;
    public Sprite enemySnakep;
    
    public  void Awake(){
        instance = this;
    }

   
}
