using UnityEngine;
using UnityEngine.SceneManagement;

public class PongDoor : MonoBehaviour
{
    [SerializeField] private string pongSceneName;
    
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
        {
            return;
        }
        
        Vector3 pushBack = (player.transform.position - transform.position).normalized * 0.6f;
        pushBack.y = 0f;
        
        player.Teleport(player.transform.position + pushBack, player.Yaw, player.Pitch);
        
        SaveManager saveManager = FindFirstObjectByType<SaveManager>();
        if (saveManager != null)
        {
            saveManager.Save();
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(pongSceneName);
        
        SceneManager.LoadScene(pongSceneName);
    }

}
