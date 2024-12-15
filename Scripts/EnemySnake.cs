using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySnake : MonoBehaviour
{
    private enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    private enum EnemyState
    {
        Alive,
        Dead
    }

    private Direction moveDirection;
    private Vector2Int gridPosition;
    private float moveTimer;
    private float moveTimerMax;
    private LevelGrid levelGrid;
    private Snake snake;
    private EnemyState enemyState;

    

    private void Start()
    {
        snake = FindObjectOfType<Snake>();
        enemyState = EnemyState.Alive;  // Initialize enemy snake as alive
    }

    private void Awake()
    {
        gridPosition = new Vector2Int(Random.Range(-10, 10), Random.Range(-10, 10)); // Random start position
        moveTimerMax = 0.5f; // Speed of the enemy snake
        moveTimer = moveTimerMax;
        moveDirection = Direction.Right;
    }

    public void Setup(LevelGrid levelGrid)
    {
        this.levelGrid = levelGrid;
    }

    private void Update()
    {
        HandleMovement();
        CheckCollisionWithPlayer();
    }

    private void HandleMovement()
    {
        if (enemyState == EnemyState.Dead) return;

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveTimerMax)
        {
            moveTimer -= moveTimerMax;

            // Find the closest position (head or body) of the player snake
            List<Vector2Int> playerPositions = snake.GetFullSnakeGridPositionList();
            Vector2Int closestPosition = GetClosestPosition(playerPositions, gridPosition);

            // Calculate direction towards the closest position
            Vector2Int directionToTarget = closestPosition - gridPosition;

            // Choose the axis with the largest difference to move closer to the target
            if (Mathf.Abs(directionToTarget.x) > Mathf.Abs(directionToTarget.y))
            {
                moveDirection = directionToTarget.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                moveDirection = directionToTarget.y > 0 ? Direction.Up : Direction.Down;
            }

            Vector2Int moveDirVector = GetDirectionVector(moveDirection);
            gridPosition += moveDirVector;

            // Update rotation based on move direction
            UpdateRotation();

            // Boundary check to keep within play area
            gridPosition.x = Mathf.Clamp(gridPosition.x, -10, 10);
            gridPosition.y = Mathf.Clamp(gridPosition.y, -10, 10);

            transform.position = new Vector3(gridPosition.x, gridPosition.y);

            // Check for collision with player's head or body
            if (gridPosition == snake.GetGridPosition())
            {
                Debug.Log("Player hit by enemy snake!");
                snake.PlayerDied(); // Trigger player death
                GameHandler.SnakeDied();
                enemyState = EnemyState.Dead;
                HandleDeath();
            }

            // Check for collision with player snake's body
            foreach (Vector2Int bodyPartPosition in playerPositions)
            {
                if (gridPosition == bodyPartPosition)
                {
                    Debug.Log("Enemy hit player snake's body!");
                    snake.PlayerDied(); // Trigger player death
                    GameHandler.SnakeDied();
                    enemyState = EnemyState.Dead;
                    HandleDeath();
                    return;
                }
            }
        }
    }

    private Vector2Int GetClosestPosition(List<Vector2Int> positions, Vector2Int currentPosition)
    {
        Vector2Int closestPosition = positions[0];
        float closestDistance = Vector2.Distance(currentPosition, closestPosition);

        foreach (Vector2Int position in positions)
        {
            float distance = Vector2.Distance(currentPosition, position);
            if (distance < closestDistance)
            {
                closestPosition = position;
                closestDistance = distance;
            }
        }

        return closestPosition;
    }



// Method to update rotation based on movement direction
    private void UpdateRotation()
    {
        switch (moveDirection)
        {
            case Direction.Up:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction.Down:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case Direction.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case Direction.Right:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
    }



    private void CheckCollisionWithPlayer()
    {
        // Ensure this check happens every frame
        if (enemyState == EnemyState.Alive && gridPosition == snake.GetGridPosition())
        {
            Debug.Log("Player hit by enemy snake!");
            snake.PlayerDied();
            GameHandler.SnakeDied();

            enemyState = EnemyState.Dead;
            HandleDeath();
        }
    }

    private Vector2Int GetDirectionVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return new Vector2Int(0, +1);
            case Direction.Down: return new Vector2Int(0, -1);
            case Direction.Left: return new Vector2Int(-1, 0);
            case Direction.Right: return new Vector2Int(+1, 0);
            default: return new Vector2Int(0, 0);
        }
    }

    private void HandleDeath()
    {
        GetComponent<SpriteRenderer>().color = Color.red; // Change color to indicate death
        enabled = false; // Stop further movement
    }
}
