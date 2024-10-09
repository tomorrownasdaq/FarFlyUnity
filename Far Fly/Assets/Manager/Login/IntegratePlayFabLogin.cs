using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.DataModels;
using System.Collections.Generic;
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class IntegratedPlayFabManager : MonoBehaviour
{
    public string entityId; // Id representing the logged in player
    public string entityType; // entityType representing the logged in player
    private readonly Dictionary<string, string> _entityFileJson = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _tempUpdates = new Dictionary<string, string>();
    public string ActiveUploadFileName;
    public string NewFileName;
    public int GlobalFileLock = 0;

    void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            PlayFabSettings.staticSettings.TitleId = "4AD5e";
            PlayFabSettings.staticSettings.DeveloperSecretKey = "TP3GPW6KTFBWRUS7ZEZIJ19DTS7G5NHF3YRQ7RNHSFD7RPRAM6";
        }
        LoginStart();
    }

    public void LoginStart()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, ProcessServerAuthCode);
        }
        else
        {
            Debug.Log("Google Play Games authentication failed.");
        }
    }

    private void ProcessServerAuthCode(string serverAuthCode)
    {
        Debug.Log("Server Auth Code: " + serverAuthCode);
        var request = new LoginWithGooglePlayGamesServicesRequest
        {
            ServerAuthCode = serverAuthCode,
            CreateAccount = true,
            TitleId = PlayFabSettings.TitleId
        };
        PlayFabClientAPI.LoginWithGooglePlayGamesServices(request, OnLoginWithGooglePlayGamesServicesSuccess, OnLoginWithGooglePlayGamesServicesFailure);
    }

    private void OnLoginWithGooglePlayGamesServicesSuccess(LoginResult result)
    {
        Debug.Log("PlayFab Login Success with Google Play Games Services");
        entityId = result.EntityToken.Entity.Id;
        entityType = result.EntityToken.Entity.Type;
        CheckDisplayName();
    }

    private void OnLoginWithGooglePlayGamesServicesFailure(PlayFabError error)
    {
        Debug.Log("PlayFab Login Failure with Google Play Games Services: " + error.GenerateErrorReport());
    }

    private void CheckDisplayName()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
            result =>
            {
                if (string.IsNullOrEmpty(result.AccountInfo.TitleInfo.DisplayName))
                {
                    SceneManager.LoadScene("MakeUserName");
                }
                else
                {
                    SceneManager.LoadScene("MenuScene");
                }
            },
            error =>
            {
                Debug.LogError("Failed to get account info: " + error.GenerateErrorReport());
                SceneManager.LoadScene("MakeUserName");
            }
        );
    }

    public void GetUserData(string myPlayFabId)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = myPlayFabId,
            Keys = null
        }, result => {
            Debug.Log("Got user data:");
            if (result.Data == null || !result.Data.ContainsKey("Ancestor")) Debug.Log("No Ancestor");
            else Debug.Log("Ancestor: " + result.Data["Ancestor"].Value);
        }, (error) => {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
    }

    public void SetUserData()
    {
        // Implement your SetUserData logic here
    }

    void LoadAllFiles()
    {
        if (GlobalFileLock != 0)
            throw new System.Exception("This example overly restricts file operations for safety. Careful consideration must be made when doing multiple file operations in parallel to avoid conflict.");

        GlobalFileLock += 1;
        var request = new GetFilesRequest { Entity = new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType } };
        PlayFabDataAPI.GetFiles(request, OnGetFileMeta, OnSharedFailure);
    }

    void OnGetFileMeta(GetFilesResponse result)
    {
        Debug.Log("Loading " + result.Metadata.Count + " files");

        _entityFileJson.Clear();
        foreach (var eachFilePair in result.Metadata)
        {
            _entityFileJson.Add(eachFilePair.Key, null);
            StartCoroutine(GetActualFile(eachFilePair.Value));
        }
        GlobalFileLock -= 1;
    }

    IEnumerator GetActualFile(GetFileMetadata fileData)
    {
        GlobalFileLock += 1;
        using (UnityWebRequest www = UnityWebRequest.Get(fileData.DownloadUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                _entityFileJson[fileData.FileName] = www.downloadHandler.text;
            }
            else
            {
                Debug.Log($"Error downloading file {fileData.FileName}: {www.error}");
            }
        }
        GlobalFileLock -= 1;
    }

    void OnSharedFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
        GlobalFileLock -= 1;
    }

    // ... (rest of the file upload related methods)
}