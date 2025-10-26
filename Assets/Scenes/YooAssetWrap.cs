using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class YooAssetWrap: MonoBehaviour
{
    // 读取配置
    readonly string defaultHostServer = "http://192.168.1.100/WebServer/CDN/PC/v1.0";
    readonly string fallbackHostServer = "http://192.168.1.100/WebServer/CDN/PC/v1.0";
    readonly int downloadingMaxNumber = 10;
    readonly int failedTryAgain = 3;


    public delegate void YooAssetWrapCallback(YooAssetWrapCallbackMsg _callbackMsg);
    public YooAssetWrapCallbackMsg callbackMsg = new YooAssetWrapCallbackMsg();

    private EPlayMode ePlayMode = EPlayMode.HostPlayMode;
    private string appVersion = "v1.0";
    private Dictionary<string, bool> packageState = new Dictionary<string, bool>();
    private List<Dictionary<string, YooAssetWrapCallback>> packageCallbackList = new List<Dictionary<string, YooAssetWrapCallback>>();


    #region 单例模式

    private static YooAssetWrap _instance;

    public static YooAssetWrap Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<YooAssetWrap>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("YooAssetWrap");
                    _instance = go.AddComponent<YooAssetWrap>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion


    public void CheckPackage(string _packageName, YooAssetWrapCallback _callback, bool _isUpdate = false)
    {
        if (YooAssets.Initialized == false)
        {
            YooAssets.Initialize();
        }

        ResourcePackage package = YooAssets.TryGetPackage(_packageName);
        if (package != null && package.PackageValid == true && _isUpdate == false)
        {
            // 需要的ab包已经下载更新完成
            string msg = $"yooassetwrap: {_packageName} is ok.";
            callbackMsg.SetMsg(0, msg);
            _callback(callbackMsg);
            return;
        }
        if (package == null)
        {
            package = YooAssets.CreatePackage(_packageName);
            packageState[_packageName] = false;
        }
        if (_isUpdate == true && packageState[_packageName] != true)
        {
            // 需要重新下载更新ab包
            packageState[_packageName] = false;
        }

        Dictionary<string, YooAssetWrapCallback> newPackageCallback = new Dictionary<string, YooAssetWrapCallback>();
        newPackageCallback.Add(_packageName, _callback);
        packageCallbackList.Add(newPackageCallback);
        if (packageState[_packageName] != true)
        {
            // 进入ab包下载更新流程
            packageState[_packageName] = true;
            StartCoroutine(CheckABPackage(_packageName));
        }
    }

    void UpdatePackage(string _packageName)
    {
        StartCoroutine(UpdateAbPackage(_packageName));
    }

    void HandlePackageCallback(string _packageName, YooAssetWrapCallbackMsg _callbackMsg)
    {
        for (int i = 0; i < packageCallbackList.Count; i++)
        {
            if (packageCallbackList[i].ContainsKey(_packageName))
            {
                packageCallbackList[i][_packageName](_callbackMsg);
            }
        }
        packageCallbackList.Clear();
    }

    IEnumerator CheckABPackage(string _packageName)
    {
        ResourcePackage package = YooAssets.TryGetPackage(_packageName);
        InitializationOperation op_InitYooAsset = null;
        if (ePlayMode == EPlayMode.HostPlayMode)
        {
            IRemoteServices remoteServices = new YooAssetWrapRemoteServices(defaultHostServer, fallbackHostServer);

            FileSystemParameters cacheFileSystemParam = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            //FileSystemParameters buildinFileSystemParam = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

            HostPlayModeParameters initParams = new HostPlayModeParameters();
            initParams.CacheFileSystemParameters = cacheFileSystemParam;
            //initParams.BuildinFileSystemParameters = buildinFileSystemParam;

            op_InitYooAsset = package.InitializeAsync(initParams);
        }
        yield return op_InitYooAsset;
        if (op_InitYooAsset.Status != EOperationStatus.Succeed)
        {
            packageState[_packageName] = false;
            string msg = $"yooassetwrap: yooasset {_packageName} init failed. error: {op_InitYooAsset.Error}";
            callbackMsg.SetMsg(-1, msg);
            HandlePackageCallback(_packageName, callbackMsg);
            yield break;
        }

        RequestPackageVersionOperation op_RequestAppVersion = package.RequestPackageVersionAsync();
        yield return op_RequestAppVersion;
        if (op_RequestAppVersion.Status != EOperationStatus.Succeed)
        {
            packageState[_packageName] = false;
            string msg = $"yooassetwrap: yooasset {_packageName} request app version failed. error: {op_RequestAppVersion.Error}";
            callbackMsg.SetMsg(-1, msg);
            HandlePackageCallback(_packageName, callbackMsg);
            yield break;
        }
        else
        {
            appVersion = op_RequestAppVersion.PackageVersion;
        }

        UpdatePackageManifestOperation op_UpdatePackageManifest = package.UpdatePackageManifestAsync(appVersion);
        yield return op_UpdatePackageManifest;
        if (op_UpdatePackageManifest.Status != EOperationStatus.Succeed)
        {
            packageState[_packageName] = false;
            string msg = $"yooassetwrap: yooasset {_packageName} update package manifest failed. error: {op_UpdatePackageManifest.Error}";
            callbackMsg.SetMsg(-1, msg);
            HandlePackageCallback(_packageName, callbackMsg);
            yield break;
        }

        ResourceDownloaderOperation op_DownloadResource = package.CreateResourceDownloader(downloadingMaxNumber, failedTryAgain);
        if (op_DownloadResource.TotalDownloadCount == 0)
        {
            packageState[_packageName] = true;
            string msg = $"yooassetwrap: yooasset {_packageName} need to download resources count: 0";
            callbackMsg.SetMsg(0, msg);
            HandlePackageCallback(_packageName, callbackMsg);
            yield break;
        }
        else
        {
            int fileCount_update = op_DownloadResource.TotalDownloadCount;
            long fileSize_update = op_DownloadResource.TotalDownloadBytes;

            string msg = $"yooassetwrap: yooasset {_packageName} need to download resources count: {fileCount_update} -  bytes: {fileSize_update / 1024 / 1024}Mb";

            // sure to download resources
            TipWindows.Instance.SetClickCallback((state) =>
            {
                if (state == true)
                {
                    TipWindows.Instance.Display2View();
                    UpdatePackage(_packageName);
                }
            });
            TipWindows.Instance.Display1View();
            TipWindows.Instance.SetTipsContent($"有{fileCount_update}个新文件待更新");
            TipWindows.Instance.SetTipWindowActive(true);
            yield break;
        }
    }

    IEnumerator UpdateAbPackage(string _packageName)
    {
        ResourcePackage package = YooAssets.TryGetPackage(_packageName);
        ResourceDownloaderOperation op_DownloadResource = package.CreateResourceDownloader(downloadingMaxNumber, failedTryAgain);
        if (op_DownloadResource.TotalDownloadCount == 0)
        {
            packageState[_packageName] = true;
            string msg = $"yooassetwrap: yooasset {_packageName} need to download resources count: 0";
            callbackMsg.SetMsg(0, msg);
            HandlePackageCallback(_packageName, callbackMsg);
            yield break;
        }

        op_DownloadResource.DownloadUpdateCallback = (DownloadUpdateData data) =>
        {
            int totalCount = data.TotalDownloadCount;
            int curCount = data.CurrentDownloadCount;
            long totalBytes = data.TotalDownloadBytes;
            long curBytes = data.CurrentDownloadBytes;

            string msg = $"yooassetwrap: yooasset {_packageName} download resources count: {curCount}/{totalCount}";

            // display download progress
            TipWindows.Instance.SetTipsProgress($"{curCount} / {totalCount}");
            TipWindows.Instance.SetSliderValue((curCount * 100 / totalCount));
        };
        
        op_DownloadResource.BeginDownload();
        yield return op_DownloadResource;
        if (op_DownloadResource.Status == EOperationStatus.Succeed)
        {
            packageState[_packageName] = true;
            string msg = $"yooassetwrap: yooasset {_packageName} resources download success.";
            callbackMsg.SetMsg(0, msg);
            HandlePackageCallback(_packageName, callbackMsg);


            TipWindows.Instance.SetTipWindowActive(false);
            yield break;
        }
        else
        {
            packageState[_packageName] = false;
            //string msg = $"yooassetwrap: yooasset {_packageName} resources download failed. error: {op_DownloadResource.Error}";
            //callbackMsg.SetMsg(-1, msg);
            //HandlePackageCallback(_packageName, callbackMsg);

            TipWindows.Instance.Display3View();
            TipWindows.Instance.SetTipsContent($"下载失败，请检查网络");
            yield break;
        }
    }



    public void GetSpriteAsset(string _packageName, string _atlasPath, string _spriteName, YooAssetWrapCallback _callback)
    {
        ResourcePackage package = YooAssets.TryGetPackage(_packageName);
        if (package == null || package.PackageValid != true)
        {
            string msg = $"yooassetwrap: yooasset {_packageName} is Unavailable.";
            callbackMsg.SetMsg(-1, msg);
            _callback(callbackMsg);
            return;
        }

        SubAssetsHandle handle = package.LoadSubAssetsAsync<Sprite>(_atlasPath);
        handle.Completed += (SubAssetsHandle handle) =>
        {
            string msg = $"yooassetwrap: yooasset {_packageName} sprite asset is ok.";
            if (handle.SubAssetObjects != null)
            {
                Sprite sprite = handle.GetSubAssetObject<Sprite>(_spriteName);
                if (sprite != null)
                {
                    callbackMsg.SetMsg(0, msg, sprite);
                    _callback(callbackMsg);
                    handle.Release();
                    return;
                }
            }

            msg = $"yooassetwrap: yooasset {_packageName} sprite asset is null.";
            callbackMsg.SetMsg(-1, msg);
            _callback(callbackMsg);
            handle.Release();
        };
    }

    public void GetAudioAsset(string _packageName, string _assetPath, YooAssetWrapCallback _callback)
    {
        ResourcePackage package = YooAssets.TryGetPackage(_packageName);
        if (package == null || package.PackageValid != true)
        {
            string msg = $"yooassetwrap: yooasset {_packageName} is Unavailable.";
            callbackMsg.SetMsg(-1, msg);
            _callback(callbackMsg);
            return;
        }

        AssetHandle handle = package.LoadAssetAsync<AudioClip>(_assetPath);
        handle.Completed += (AssetHandle handle) =>
        {
            string msg = $"yooassetwrap: yooasset {_packageName} audio asset is ok.";
            if (handle.AssetObject != null)
            {
                AudioClip clip = handle.AssetObject as AudioClip;
                if (clip != null)
                {
                    callbackMsg.SetMsg(0, msg, clip);
                    _callback(callbackMsg);
                    handle.Release();
                    return;
                }
            }

            msg = $"yooassetwrap: yooasset {_packageName} audio asset is null.";
            callbackMsg.SetMsg(-1, msg);
            _callback(callbackMsg);
            handle.Release();
        };
    }

    public void GetPrefabAsset(string _packageName, string _assetPath, YooAssetWrapCallback _callback)
    {
        ResourcePackage package = YooAssets.TryGetPackage(_packageName);
        if (package == null || package.PackageValid != true)
        {
            string msg = $"yooassetwrap: yooasset {_packageName} is Unavailable.";
            callbackMsg.SetMsg(-1, msg);
            _callback(callbackMsg);
            return;
        }

        AssetHandle handle = package.LoadAssetAsync<GameObject>(_assetPath);
        handle.Completed += (AssetHandle handle) =>
        {
            string msg = $"yooassetwrap: yooasset {_packageName} prefab asset is ok.";
            if (handle.AssetObject != null)
            {
                GameObject go = handle.InstantiateSync();
                if (go != null)
                {
                    callbackMsg.SetMsg(0, msg, go);
                    _callback(callbackMsg);
                    handle.Release();
                    return;
                }
            }

            msg = $"yooassetwrap: yooasset {_packageName} prefab asset is null.";
            callbackMsg.SetMsg(-1, msg);
            _callback(callbackMsg);
            handle.Release();
        };
    }


    private void OnDestroy()
    {

    }


    IEnumerator ClearPackageUnusedCacheBundleFiles(string _packageName)
    {
        ResourcePackage package = YooAssets.TryGetPackage(_packageName);
        var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
        yield return operation;
        if (operation.Status == EOperationStatus.Succeed)
        {

        }
        else
        {

        }
    }

    IEnumerator ClearPackageAllCacheBundleFiles(string _packageName)
    {
        ResourcePackage package = YooAssets.TryGetPackage(_packageName);
        var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearAllBundleFiles);
        yield return operation;
        if (operation.Status == EOperationStatus.Succeed)
        {

        }
        else
        {

        }
    }

    IEnumerator ClearPackageUnusedCacheManifestFiles(string _packageName)
    {
        ResourcePackage package = YooAssets.TryGetPackage(_packageName);
        var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedManifestFiles);
        yield return operation;
        if (operation.Status == EOperationStatus.Succeed)
        {

        }
        else
        {

        }
    }

    IEnumerator ClearPackageAllCacheManifestFiles(string _packageName)
    {
        ResourcePackage package = YooAssets.TryGetPackage(_packageName);
        var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearAllManifestFiles);
        yield return operation;
        if (operation.Status == EOperationStatus.Succeed)
        {

        }
        else
        {

        }
    }

    IEnumerator DestroyPackage(string _packageName)
    {
        ResourcePackage package = YooAssets.TryGetPackage(_packageName);
        DestroyOperation operation = package.DestroyAsync();
        yield return operation;
        if (YooAssets.RemovePackage(package))
        {

        }
    }



    public struct YooAssetWrapCallbackMsg
    {
        public int retCode;
        public string retMsg;
        public object retAsset;

        public void SetMsg(int _code, string _msg, object _asset = null)
        {
            retCode = _code;
            retMsg = _msg;
            retAsset = _asset;
        }

    }


    public class YooAssetWrapRemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public YooAssetWrapRemoteServices(string defaultHostServer, string fallbackHostServer)
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


}
