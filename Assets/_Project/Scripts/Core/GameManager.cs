using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int TotalPoints { get; private set; }
    public string CurrentStoryId { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectStory(string storyId)
    {
        CurrentStoryId = storyId;
    }

    public void AddPoints(int points)
    {
        TotalPoints += points;
        Debug.Log($"[GameManager] +{points} poin. Total: {TotalPoints}");
    }
}