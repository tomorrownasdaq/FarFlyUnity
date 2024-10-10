using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    [Serializable]
    public class StageInfo
    {
        public string stageName;
        public string sceneName;
        public Button stageButton;
    }

    public List<StageInfo> stages = new List<StageInfo>();

    void Start()
    {
        InitializeStageButtons();
    }

    void InitializeStageButtons()
    {
        foreach (var stage in stages)
        {
            if (stage.stageButton != null)
            {
                stage.stageButton.onClick.AddListener(() => LoadStage(stage.sceneName));
            }
        }
    }

    void LoadStage(string sceneName)
    {
        Debug.Log($"Changing to scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}