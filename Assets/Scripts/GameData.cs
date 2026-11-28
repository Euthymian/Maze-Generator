using System;
using UnityEngine;

[Serializable]
public class GameData
{
    // General
    public int score;
    public int mazeSeed;
    
    // Player
    public Vector3 playerPos;
    public float yaw;
    public float pitch;
    
    // Enemy
    public Vector3 enemyPos;
    public Quaternion rotation;
    public Vector3 spawnPos;
    public int hitsRemaining;
    public float respawnIn;
}
