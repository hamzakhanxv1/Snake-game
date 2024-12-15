using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;

public class Snake : MonoBehaviour
{
    private enum Direction{
        Left,
        Right,
        Up,
        Down
    }

    private enum State { 
        Alive,
        Dead
    }

    private AudioSource audioSource;
    
    private State state;
    private LevelGrid levelGrid;
    private Direction gridMoveDirection;
    private Vector2Int gridPosition;
    private float gridMoveTimer;
    private float gridMoveTimerMax;
    private int snakeBodySize;
    private int score;
    private List<SnakeMovePosition> snakeMovePositionList;
    private List<SnakeBodyPart> snakeBodyPartList;
    

    public void Setup(LevelGrid levelGrid){
        this.levelGrid = levelGrid;
    }

    private void Start() {
    GameHandler.OnLevelChanged += HandleLevelChanged;
}

private void OnDestroy() {
    GameHandler.OnLevelChanged -= HandleLevelChanged;
}
private void HandleLevelChanged(int newLevel) {
    gridMoveTimerMax = Mathf.Max(0.02f, gridMoveTimerMax - 0.05f); // Reduce timer max, making the snake faster
}

    private void Awake(){

       audioSource = GetComponent<AudioSource>();
        gridPosition = new Vector2Int(-6, 0);
        gridMoveTimerMax = .25f;
        gridMoveTimer = gridMoveTimerMax;
        gridMoveDirection = Direction.Right;

        snakeMovePositionList = new List<SnakeMovePosition>();
        snakeBodySize = 1;

        snakeBodyPartList = new List<SnakeBodyPart>();
        state = State.Alive;

        score = 0;
    }

    private void Update(){
        switch (state) {
        case State.Alive:
            HandleInput();
            HandleGridMovement();
            break;
        case State.Dead:
            break;
        }
    }

    private void HandleInput(){
                if(Input.GetKeyDown(KeyCode.UpArrow)){
                if(gridMoveDirection != Direction.Down){
                    gridMoveDirection = Direction.Up;
                }
                
            }
            if(Input.GetKeyDown(KeyCode.DownArrow)){
                if(gridMoveDirection != Direction.Up){
                    gridMoveDirection = Direction.Down;
                }
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow)){
                if(gridMoveDirection != Direction.Right){
                    gridMoveDirection = Direction.Left;
                }
            }
            if(Input.GetKeyDown(KeyCode.RightArrow)){
                if(gridMoveDirection != Direction.Left){
                    gridMoveDirection = Direction.Right;
                }
            }
    }

    private void HandleGridMovement(){
        gridMoveTimer += Time.deltaTime;
        if(gridMoveTimer >= gridMoveTimerMax){
            gridMoveTimer -= gridMoveTimerMax;

            
            SnakeMovePosition previousSnakeMovePosition = null;
            if(snakeMovePositionList.Count > 0){
                previousSnakeMovePosition = snakeMovePositionList[0];
            }
            SnakeMovePosition snakeMovePosition = new SnakeMovePosition(previousSnakeMovePosition, gridPosition, gridMoveDirection);
            snakeMovePositionList.Insert(0, snakeMovePosition);

            Vector2Int gridMoveDirectionVector;
            switch(gridMoveDirection){
                default:
                case Direction.Right: gridMoveDirectionVector = new Vector2Int(+1, 0); break;
                case Direction.Left: gridMoveDirectionVector = new Vector2Int(-1, 0); break;
                case Direction.Up: gridMoveDirectionVector = new Vector2Int(0, +1); break;
                case Direction.Down: gridMoveDirectionVector = new Vector2Int(0, -1); break;

            }
            
            gridPosition += gridMoveDirectionVector;
            
            gridPosition = levelGrid.ValidateGridPosition(gridPosition);
            
            bool snakeAteFood = levelGrid.TrySnakeEatFood(gridPosition);
            if(snakeAteFood){
                snakeBodySize++;
                CreateSnakeBody(); 
                
            }


            if(snakeMovePositionList.Count >= snakeBodySize +1){
                snakeMovePositionList.RemoveAt(snakeMovePositionList.Count - 1);
            }

         
            UpdateSnakeBodyParts();

            foreach (SnakeBodyPart snakeBodyPart in snakeBodyPartList){
                Vector2Int snakeBodyPartGridPosition = snakeBodyPart.GetGridPosition();
                if(gridPosition == snakeBodyPartGridPosition){
                    CMDebug.Text("Dead", transform.position);
                    state = State.Dead;
                    PlayDeathSound();
                    GameHandler.SnakeDied();
                }
            }

            transform.position = new Vector3(gridPosition.x, gridPosition.y);
            transform.eulerAngles = new Vector3(0, 0, GetAngleFromVector(gridMoveDirectionVector) - 90);
         
            // UpdateSnakeBodyParts();
            
        }
    }
    private void PlayDeathSound()
    {
        if (audioSource != null)
        {
            audioSource.Play(); // Play death sound
        }
    }
    public void PlayerDied(){
        if (state == State.Alive)
        {
            state = State.Dead;
            PlayDeathSound();
            CMDebug.TextPopup("Game Over!", transform.position);
            GameHandler.SnakeDied(); // Notify the GameHandler about player death
        }
    }



    private float GetAngleFromVector(Vector2Int dir){
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if(n < 0){
            n += 360;
        }
        return n;
    }

    private void CreateSnakeBody(){
        snakeBodyPartList.Add(new SnakeBodyPart(snakeBodyPartList.Count));
    }

    private void UpdateSnakeBodyParts(){
        for(int i = 0; i < snakeBodyPartList.Count; i++){
                snakeBodyPartList[i].SetSnakeMovePosition(snakeMovePositionList[i]);
        }
    }

    public Vector2Int GetGridPosition(){
        return gridPosition;
    }


//return the full size ocupised by the snake
    public List<Vector2Int> GetFullSnakeGridPositionList(){
        List<Vector2Int> gridPositionList = new List<Vector2Int>(){ gridPosition };
        foreach(SnakeMovePosition snakeMovePosition in snakeMovePositionList){
            gridPositionList.Add(snakeMovePosition.GetGridPosition());
        }
        return gridPositionList;
    }


    private class SnakeBodyPart{
        private SnakeMovePosition snakeMovePosition;
        private Transform transform;

        public SnakeBodyPart(int bodyIndex)
        {
            GameObject snakeBodyGameObject = new GameObject("SnakeBody", typeof(SpriteRenderer));
            snakeBodyGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.instance.snakeBodySprite;
            snakeBodyGameObject.GetComponent<SpriteRenderer>().sortingOrder = -bodyIndex;
            transform = snakeBodyGameObject.transform;
        }



        public void SetSnakeMovePosition(SnakeMovePosition snakeMovePosition)
        {
            this.snakeMovePosition = snakeMovePosition;
            transform.position = new Vector3(snakeMovePosition.GetGridPosition().x, snakeMovePosition.GetGridPosition().y);
            transform.eulerAngles = new Vector3(0, 0, GetBodyPartAngle(snakeMovePosition));
        }

        private float GetBodyPartAngle(SnakeMovePosition snakeMovePosition)
        {
            switch (snakeMovePosition.GetDirection())
            {
                default:
                case Direction.Up:
                switch(snakeMovePosition.GetPreviousDirection()){
                        default:
                            return  0;
                            break;
                        case Direction.Left:
                            return 0 + 45;
                            break;
                        case Direction.Right:
                            return 0 - 45;
                            break;
                    }
                    break;
                case Direction.Down:
                    switch(snakeMovePosition.GetPreviousDirection()){
                        default:
                            return 180;
                            break;
                        case Direction.Left:
                            return 180 - 45;
                            break;
                        case Direction.Right:
                            return 180 + 45;
                            break;
                    }
                    break;
                case Direction.Left:
                    switch(snakeMovePosition.GetPreviousDirection()){
                            default:
                                return -90;
                                break;
                            case Direction.Down:
                                return -45;
                                break;
                            case Direction.Up:
                                return 45;
                                break;
                        }
                    break;
                case Direction.Right: 
                    switch(snakeMovePosition.GetPreviousDirection()){
                            default:
                                return 90; 
                                break;
                            case Direction.Down:
                                return 45;
                                break;
                            case Direction.Up:
                                return -45;
                                break;                            
                        }
                    break;
            }
        }
        

        public Vector2Int GetGridPosition()
        {
            return snakeMovePosition.GetGridPosition();
        }
    }

    private class SnakeMovePosition {

        private SnakeMovePosition previousSnakeMovePosition; 
        private Vector2Int gridPosition;
        private Direction direction;

        public SnakeMovePosition(SnakeMovePosition previousSnakeMovePosition, Vector2Int gridPosition, Direction direction) {
            this.previousSnakeMovePosition = previousSnakeMovePosition;
            this.gridPosition = gridPosition;
            this.direction = direction;
        }

        public Vector2Int GetGridPosition() {
            return gridPosition;
        }

        public Direction GetDirection(){
            return direction;
        }

        public Direction GetPreviousDirection(){
            if(previousSnakeMovePosition == null){
                return Direction.Right;
            }else{
                return previousSnakeMovePosition.direction;
            }
        }
    }
}


