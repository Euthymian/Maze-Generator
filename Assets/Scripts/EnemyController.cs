using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    private const int HitsToDefeat = 3;
    private const float RespawnDelay = 5f;

    [SerializeField] private MazeGenerator maze;
    [SerializeField] private GameObject model;
    [SerializeField] private Collider hitCollider;
    [SerializeField] private float moveSpeed = 1.2f;     //match this to the walk animation so feet don't slide
    [SerializeField] private float turnSpeed = 360f;     //degrees per second
    [SerializeField] private float catchDistance = 0.5f; //stop this close to the player instead of walking into them
    
    private Transform player;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private int hitsRemaining = HitsToDefeat;
    private float respawnAt;

    private void Start()
    {
        if (maze == null || !maze.IsGenerated)
        {
            Debug.LogError("Enemy needs a generated maze assigned.", this);
            enabled = false;
            return;
        }

        Vector3 start = maze.CellToWorld(new Vector2Int(maze.SizeX - 1, maze.SizeZ - 1));
        start.y = transform.position.y;
        transform.position = start;
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }
    
    private void OnEnable()
    {
        PlayerController.ResetPressed += ResetToSpawn;
    }

    private void OnDisable()
    {
        PlayerController.ResetPressed -= ResetToSpawn;
    }

    private void ResetToSpawn()
    {
        if (hitsRemaining > 0)
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
    }

    public bool TryHit()
    {
        if (!isActiveAndEnabled || hitsRemaining == 0)
            return false;

        hitsRemaining--;
        if (hitsRemaining == 0)
        {
            hitCollider.enabled = false;
            model.SetActive(false);
            respawnAt = Time.time + RespawnDelay;
        }

        return true;
    }

    private void Respawn()
    {
        Vector2Int deathCell = maze.WorldToCell(transform.position);
        Vector2Int? playerCell = player != null ? maze.WorldToCell(player.position) : null;
        Vector2Int cell;
        do
        {
            cell = new Vector2Int(Random.Range(0, maze.SizeX), Random.Range(0, maze.SizeZ));
        }
        while (cell == deathCell || cell == playerCell);

        Vector3 position = maze.CellToWorld(cell);
        position.y = spawnPosition.y;
        spawnPosition = position;
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        hitsRemaining = HitsToDefeat;
        model.SetActive(true);
        hitCollider.enabled = true;
    }

    private void Update()
    {
        if (hitsRemaining == 0)
        {
            if (Time.time >= respawnAt)
                Respawn();
            return;
        }

        if (player == null)
        {
            PlayerController found = FindFirstObjectByType<PlayerController>();
            if (found == null)
                return;
            player = found.transform;
        }
        
        Vector2Int myCell = maze.WorldToCell(transform.position);
        Vector2Int playerCell = maze.WorldToCell(player.position);

        Vector3 target;
        if (myCell == playerCell)
            target = player.position; 
        else
            target = maze.CellToWorld(NextStepToward(myCell, playerCell)); 

        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        
        if (myCell == playerCell && distance <= catchDistance)
        {
            KillPlayer();
            return;
        }

        if (distance < 0.0001f)
            return;
        
        Quaternion lookRotation = Quaternion.LookRotation(toTarget);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
        
        bool closeEnough = myCell == playerCell && distance <= catchDistance;

        bool facingTarget = Vector3.Angle(transform.forward, toTarget) < 45f;

        if (!closeEnough && facingTarget)
        {
            float step = Mathf.Min(moveSpeed * Time.deltaTime, distance);
            transform.position += toTarget.normalized * step;
        }
    }

    //breadth-first search through the maze, returns the first cell on the shortest path to goal
    private Vector2Int NextStepToward(Vector2Int from, Vector2Int goal)
    {
        if (from == goal)
            return from;

        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var queue = new Queue<Vector2Int>();
        cameFrom[from] = from;
        queue.Enqueue(from);

        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            if (cell == goal)
                break;

            foreach (Vector2Int neighbour in maze.GetOpenNeighbours(cell))
            {
                if (!cameFrom.ContainsKey(neighbour))
                {
                    cameFrom[neighbour] = cell;
                    queue.Enqueue(neighbour);
                }
            }
        }

        //walk back from the goal until we reach the cell right after 'from'
        Vector2Int stepCell = goal;
        while (cameFrom[stepCell] != from)
            stepCell = cameFrom[stepCell];

        return stepCell;
    }
    
    private void KillPlayer()
    {
        enabled = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
