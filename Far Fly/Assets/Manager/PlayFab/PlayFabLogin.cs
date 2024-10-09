using GooglePlayGames;
using GooglePlayGames.BasicApi;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayFabLogin : MonoBehaviour
{
    void Start()
    {
        LoginStart();
        //PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    public void LoginStart()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        CheckDisplayName();

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
                    // DisplayName이 없으면 MakeUserName 씬으로 이동
                    SceneManager.LoadScene("MakeUserName");
                }
                else
                {
                    // DisplayName이 있으면 MenuScene으로 이동
                    SceneManager.LoadScene("MenuScene");
                }
            },
            error => 
            {
                Debug.LogError("Failed to get account info: " + error.GenerateErrorReport());
                // 에러 발생 시 기본적으로 MakeUserName 씬으로 이동
                SceneManager.LoadScene("MakeUserName");
            }
        );
    }
}