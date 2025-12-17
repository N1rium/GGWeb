using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager
{
    private async Task<UnityWebRequest> SendAsync(UnityWebRequest request)
    {
        var op = request.SendWebRequest();
    
        while (!op.isDone)
            await Task.Yield();
    
        return request;
    }
    
    public async Task<string> Get(string url)
    {
        using var req = UnityWebRequest.Get(url);
        var res = await SendAsync(req);
    
        if (res.result != UnityWebRequest.Result.Success)
            throw new Exception(res.error);
    
        return res.downloadHandler.text;
    }
    
    public async Task<T> GetJson<T>(string url)
    {
        using var req = UnityWebRequest.Get(url);
        var res = await SendAsync(req);

        if (res.result != UnityWebRequest.Result.Success)
            throw new Exception(res.error);

        return JsonUtility.FromJson<T>(res.downloadHandler.text);
    }
    
    public async Task<Texture2D> GetTexture(string url)
    {
        Debug.Log($"GetTexture: {url}");
        using var req = UnityWebRequestTexture.GetTexture(url);
        var op = req.SendWebRequest();

        while (!op.isDone)
            await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception(req.error);

        return DownloadHandlerTexture.GetContent(req);
    }

    public async Task<UserAvatarAsset> GetUserAvatarAsset(string userId)
    {
        return await GetJson<UserAvatarAsset>($"https://www.geoguessr.com/api/v4/avatar/user/{userId}");
        /*https://www.geoguessr.com/api/v4/avatar/user*/
    }
}
