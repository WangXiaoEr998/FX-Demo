using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;


public class YooAssetRemoteServices : IRemoteServices
{

    private readonly string _defaultHostServer;
    private readonly string _fallbackHostServer;

    public YooAssetRemoteServices(string defaultHostServer, string fallbackHostServer)
    {
        _defaultHostServer = defaultHostServer;
        _fallbackHostServer = fallbackHostServer;
    }

    string IRemoteServices.GetRemoteMainURL(string fileName)
    {
        return $"{_defaultHostServer}/{fileName}";
    }

    string IRemoteServices.GetRemoteFallbackURL(string fileName)
    {
        return $"{_fallbackHostServer}/{fileName}";
    }

}
