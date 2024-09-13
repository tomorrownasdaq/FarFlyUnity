using UnityEngine;
using System;
using System.Threading.Tasks;
using Google;
using System.Collections.Generic;

public class GoogleSignInManager : MonoBehaviour
{
    private const string WebClientId = "1022473305988-5296auhlpgau6vhlo02pmhi4csh3l1ol.apps.googleusercontent.com"; // 실제 Web Client ID로 교체하세요
    private GoogleSignInConfiguration configuration;

    public event Action<string, string> OnSignInSuccess;
    public event Action<string> OnSignInFailed;

    private void Start()
    {
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = WebClientId,
            RequestIdToken = true,
            RequestEmail = true,
            UseGameSignIn = false
        };
    }

    public void SignIn()
    {
        try
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
        }
        catch (Exception e)
        {
            Debug.LogError($"SignIn failed: {e}");
            OnSignInFailed?.Invoke($"SignIn failed: {e.Message}");
        }
    }

    public void SignOut()
    {
        GoogleSignIn.DefaultInstance.SignOut();
    }

    private void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    OnSignInFailed?.Invoke($"Got Error: {error.Status} {error.Message}");
                }
                else
                {
                    OnSignInFailed?.Invoke("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            OnSignInFailed?.Invoke("Canceled");
        }
        else
        {
            OnSignInSuccess?.Invoke(task.Result.Email, task.Result.IdToken);
        }
    }

    public void OnSignInSilently()
    {
        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }
}