using System;
using System.IO;
using System.Diagnostics;

using Spring.Social.OAuth1;
using Spring.Social.Dropbox.Api;
using Spring.Social.Dropbox.Connect;
using Spring.IO;

class DropboxExample
{
    // Register your own Dropbox app at https://www.dropbox.com/developers/apps
    // with "Full Dropbox" access level and set your app keys and app secret below
    private const string DropboxAppKey = "1vbpi114qvtkczd";
    private const string DropboxAppSecret = "8iicdrpbw3wxthl";

    private const string OAuthTokenFileName = "OAuthTokenFileName.txt";
    private static IDropbox dropbox;
    static void DropBox()
    {
        DropboxServiceProvider dropboxServiceProvider =
            new DropboxServiceProvider(DropboxAppKey, DropboxAppSecret, AccessLevel.AppFolder);

        // Authenticate the application (if not authenticated) and load the OAuth token
        if (!File.Exists(OAuthTokenFileName))
        {
            AuthorizeAppOAuth(dropboxServiceProvider);
        }
        OAuthToken oauthAccessToken = LoadOAuthToken();

        // Login in Dropbox
        dropbox = dropboxServiceProvider.GetApi(oauthAccessToken.Value, oauthAccessToken.Secret);
        // Upload a file
        //UploadFile("..\\..\\netninja.jpg");
        //DownloadFile("/netninja.jpg");
    }
    private static string GetShareLink(string fileName)
    {
        DropboxLink sharedUrl = dropbox.GetShareableLinkAsync("/" + fileName).Result;
        return sharedUrl.Url;
    }
    private static void DownloadFile(string fileaPath)
    {
        DropboxFile file = dropbox.DownloadFileAsync(fileaPath).Result;
        StreamWriter writer = new StreamWriter("..\\..\\DownloadedFile.jpg");
        writer.Write(file.Content);
        writer.Close();
    }

    private static void UploadFile(string filePath)
    {
        Entry uploadFileEntry = dropbox.UploadFileAsync(
            new FileResource(filePath), "/netninja.jpg").Result;
    }
    private static OAuthToken LoadOAuthToken()
    {
        string[] lines = File.ReadAllLines(OAuthTokenFileName);
        OAuthToken oauthAccessToken = new OAuthToken(lines[0], lines[1]);
        return oauthAccessToken;
    }

    private static void AuthorizeAppOAuth(DropboxServiceProvider dropboxServiceProvider)
    {
        // Authorization without callback url
        Console.Write("Getting request token...");
        OAuthToken oauthToken = dropboxServiceProvider.OAuthOperations.FetchRequestTokenAsync(null, null).Result;
        Console.WriteLine("Done.");

        OAuth1Parameters parameters = new OAuth1Parameters();
        string authenticateUrl = dropboxServiceProvider.OAuthOperations.BuildAuthorizeUrl(
            oauthToken.Value, parameters);
        Console.WriteLine("Redirect the user for authorization to {0}", authenticateUrl);
        Process.Start(authenticateUrl);
        Console.Write("Press [Enter] when authorization attempt has succeeded.");
        Console.ReadLine();

        Console.Write("Getting access token...");
        AuthorizedRequestToken requestToken = new AuthorizedRequestToken(oauthToken, null);
        OAuthToken oauthAccessToken =
            dropboxServiceProvider.OAuthOperations.ExchangeForAccessTokenAsync(requestToken, null).Result;
        Console.WriteLine("Done.");

        string[] oauthData = new string[] { oauthAccessToken.Value, oauthAccessToken.Secret };
        File.WriteAllLines(OAuthTokenFileName, oauthData);
    }
}