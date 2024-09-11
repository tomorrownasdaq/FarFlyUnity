using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{

    public void StageshowScene()
    {
        Debug.Log("Changing to main scene");
        SceneManager.LoadScene("StageScene"); // 메인 씬의 인덱스 또는 이름
    }
    

}


