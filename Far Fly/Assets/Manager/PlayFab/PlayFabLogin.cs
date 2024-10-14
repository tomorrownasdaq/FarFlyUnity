using GooglePlayGames;
using GooglePlayGames.BasicApi;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class PlayFabLogin : MonoBehaviour
{
    public string entityId; // Id representing the logged in player
    public string entityType; // entityType representing the logged in player

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
            Debug.Log("Google Play Games authentication successful.");
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, ProcessServerAuthCode);
        }
        else
        {
            Debug.Log("Google Play Games authentication failed.");
            // Fallback to custom ID login
            LoginWithCustomID();
        }
    }

    private void ProcessServerAuthCode(string serverAuthCode)
    {
        Debug.Log("Server Auth Code: " + serverAuthCode);
        var request = new LoginWithGooglePlayGamesServicesRequest
        {
            ServerAuthCode = serverAuthCode,
            CreateAccount = true,
            TitleId = PlayFabSettings.staticSettings.TitleId
        };
        PlayFabClientAPI.LoginWithGooglePlayGamesServices(request, OnLoginSuccess, OnLoginWithGooglePlayGamesServicesFailure);
    }

    private void LoginWithCustomID()
    {
        var request = new LoginWithCustomIDRequest { CustomId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("PlayFab Login Success");
        entityId = result.EntityToken.Entity.Id;
        entityType = result.EntityToken.Entity.Type;
        CheckDisplayName();
    }

    private void OnLoginWithGooglePlayGamesServicesFailure(PlayFabError error)
    {
        Debug.Log("PlayFab Login Failure with Google Play Games Services: " + error.GenerateErrorReport());
        // Fallback to custom ID login
        LoginWithCustomID();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }

    private void CheckDisplayName()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
            result =>
            {
                if (string.IsNullOrEmpty(result.AccountInfo.TitleInfo.DisplayName))
                {
                    // DisplayName is empty, move to MakeUserName scene
                    SceneManager.LoadScene("MakeUserName");
                }
                else
                {
                    // DisplayName exists, move to MenuScene
                    SceneManager.LoadScene("MenuScene");
                }
            },
            error =>
            {
                Debug.LogError("Failed to get account info: " + error.GenerateErrorReport());
                // On error, default to MakeUserName scene
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
        // Implement SetUserData logic here if needed
    }
}