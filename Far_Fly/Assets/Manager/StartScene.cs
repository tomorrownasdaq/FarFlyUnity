using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    [SerializeField] private PowerManager powerManager;

    public void StageshowScene()
    {
        Debug.Log("Changing to main scene");
        SceneManager.LoadScene("StageScene");
    }

    public void DecreasePowerAndSync()
    {
        if (powerManager != null)
        {
            powerManager.DecreasePower();
        }
        else
        {
            Debug.LogError("PowerManager reference is missing!");
        }
    }
}