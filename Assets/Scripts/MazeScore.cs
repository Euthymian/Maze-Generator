using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public sealed class MazeScore : MonoBehaviour
{
    private TextMeshProUGUI label;

    public int Score { get; private set; }

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
        label.SetText("Score {0}", Score);
    }

    public void AddHit()
    {
        Score++;
        label.SetText("Score {0}", Score);
    }
}
