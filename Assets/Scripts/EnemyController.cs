using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private MazeGenerator maze;
    [SerializeField] private float moveSpeed = 1.2f;     //match this to the walk animation so feet don't slide
    [SerializeField] private float turnSpeed = 360f;     //degrees per second
    [SerializeField] private float catchDistance = 0.5f; //stop this close to the player instead of walking into them
    
    private Transform player;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private void Start()
    {
        if (maze == null || !maze.IsGenerated)
        {
            Debug.LogError("Enemy needs a generated maze assigned.", this);
            enabled = false;
            return;
        }

        //start in the finish corner, far away from the player
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
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
    }

    private void Update()
    {
        //find the spawned player; wait until it exists
        if (player == null)
        {
            PlayerController found = FindFirstObjectByType<PlayerController>();
            if (found == null)
                return;
            player = found.transform;
        }
        
        //re-plan every frame from where we actually are, so we can turn back straight away
        Vector2Int myCell = maze.WorldToCell(transform.position);
        Vector2Int playerCell = maze.WorldToCell(player.position);

        Vector3 target;
        if (myCell == playerCell)
            target = player.position; //same cell, no walls in between, go straight at them
        else
            target = maze.CellToWorld(NextStepToward(myCell, playerCell)); //centre of the next cell on the path

        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance < 0.0001f)
            return;

        //turn smoothly to face where we're going
        Quaternion lookRotation = Quaternion.LookRotation(toTarget);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);

        //don't walk into the player
        bool closeEnough = myCell == playerCell && distance <= catchDistance;

        //only walk once we're roughly facing the target, so we turn around instead of moonwalking
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
}