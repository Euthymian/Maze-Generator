using UnityEngine;

public class Win : MonoBehaviour
{
    [SerializeField] private GameObject winText; 

    private void Start()
    {
        winText.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerController.ResetPressed += HideWinText;
    }

    private void OnDisable()
    {
        PlayerController.ResetPressed -= HideWinText;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
            winText.SetActive(true);
    }

    private void HideWinText()
    {
        winText.SetActive(false);
    }
}
