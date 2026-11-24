using UnityEngine;
using UnityEngine.UI;

public class StoryListController : MonoBehaviour
{
    [SerializeField] private Button btnStartAR;
    [SerializeField] private ARBridge arBridge;
    [SerializeField] private string dummyStoryId = "story_001";

    private void OnEnable() => btnStartAR.onClick.AddListener(HandleStartAR);
    private void OnDisable() => btnStartAR.onClick.RemoveListener(HandleStartAR);

    private void HandleStartAR()
    {
        GameManager.Instance.SelectStory(dummyStoryId);
        arBridge.OpenAR(dummyStoryId);
    }
}