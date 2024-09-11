using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage_1 : MonoBehaviour
{

    public void Stage1show()
    {
        Debug.Log("Changing to main scene");
        SceneManager.LoadScene("Stage1"); // 메인 씬의 인덱스 또는 이름
    }
    

}


