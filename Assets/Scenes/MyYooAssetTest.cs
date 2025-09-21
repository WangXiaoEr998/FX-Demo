using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using YooAsset;


public class MyYooAssetTest : MonoBehaviour
{

    public EPlayMode ePlayMode = EPlayMode.HostPlayMode;
    public string hostServerIP = "http://192.168.1.101/WebServer";
    public string packagePath = "CDN/PC";
    public string appVersion = "v1.0";
    public string packageName = "DefaultPackage";

    ResourcePackage package;
    InitializationOperation initOperation;
    RequestPackageVersionOperation requestOperation;
    UpdatePackageManifestOperation updateOperation;
    ResourceDownloaderOperation downloader;

    private void Awake()
    {
        YooAssets.Initialize();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (package == null)
        {
            StartCoroutine(RunYooAsset());
        }
    }


    IEnumerator RunYooAsset()
    {
        package = YooAssets.TryGetPackage(packageName);
        if (package == null)
        {
            package = YooAssets.CreatePackage(packageName);
        }
        YooAssets.SetDefaultPackage(package);

        initOperation = null;
        if (ePlayMode == EPlayMode.EditorSimulateMode)
        {
            PackageInvokeBuildResult buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
            string packageRoot = buildResult.PackageRootDirectory;
            FileSystemParameters fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

            EditorSimulateModeParameters initParams = new EditorSimulateModeParameters();
            initParams.EditorFileSystemParameters = fileSystemParams;
        }
        if (ePlayMode == EPlayMode.HostPlayMode)
        {
            string defaultHostServer = $"{hostServerIP}/{packagePath}/{appVersion}";
            string fallbackHostServer = $"{hostServerIP}/{packagePath}/{appVersion}";
            IRemoteServices remoteServices = new YooAssetRemoteServices(defaultHostServer, fallbackHostServer);

            FileSystemParameters cacheFileSystemParam = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            //FileSystemParameters buildinFileSystemParam = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

            HostPlayModeParameters initParams = new HostPlayModeParameters();
            initParams.CacheFileSystemParameters = cacheFileSystemParam;
            //initParams.BuildinFileSystemParameters = buildinFileSystemParam;

            initOperation = package.InitializeAsync(initParams);
        }
        yield return initOperation;
        if (initOperation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("InitPackage success");
        }
        else
        {
            Debug.LogWarning($"InitPackage error: {initOperation.Error}");
            yield break;
        }

        requestOperation = package.RequestPackageVersionAsync();
        yield return requestOperation;
        if (requestOperation.Status == EOperationStatus.Succeed)
        {
            appVersion = requestOperation.PackageVersion;
            Debug.Log($"UpdatePackageVersion success: {requestOperation.PackageVersion}");
        }
        else
        {
            Debug.LogWarning($"UpdatePackageVersion error: {requestOperation.Error}");
            yield break;
        }

        updateOperation = package.UpdatePackageManifestAsync(appVersion);
        yield return updateOperation;
        if (updateOperation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("UpdatePackageManifest success");
        }
        else
        {
            Debug.LogWarning($"UpdatePackageManifest error: {updateOperation.Error}");
            yield break;
        }

        downloader = package.CreateResourceDownloader(10, 3);
        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("ResourceDownloadCount: 0");
            GetSpriteAsset(GameObject.Find("Image"), "Assets/Scenes/test.jpg", "test");
            GetAudioAsset(GameObject.Find("Audio Source"), "Assets/Scenes/Yelp");
            GetPrefabAsset(GameObject.Find("Canvas"), "Assets/Scenes/Image");
            yield break;
        }
        else
        {
            int count = downloader.TotalDownloadCount;
            long bytes = downloader.TotalDownloadBytes;
            Debug.Log($"ResourceDownloadInfo: {count} files - {bytes / 1024 / 1024}Mb");
        }

        yield return downloadPackage();

        //var operationClear = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
        //operationClear.Completed += (AsyncOperationBase obj) =>
        //{
        //    Sprite bg = package.LoadAssetAsync<Sprite>("").AssetObject as Sprite;
        //    GameObject go = new GameObject();
        //    go.AddComponent<SpriteRenderer>().sprite = bg;
        //};
    }


    IEnumerator downloadPackage()
    {
        downloader.DownloadFileBeginCallback = DownloadFileBeginCallback;
        downloader.DownloadFinishCallback = DownloadFinishCallback;
        downloader.DownloadUpdateCallback = DownloadUpdateCallback;
        downloader.DownloadErrorCallback = DownloadErrorCallback;
        downloader.BeginDownload();

        yield return downloader;
        if (downloader.Status == EOperationStatus.Succeed)
        {
            Debug.Log("ResourceDownload success");
        }
        else
        {
            Debug.LogWarning($"ResourceDownloadInfo error: {downloader.Error}");
            yield break;
        }

    }


    private void DownloadFileBeginCallback(DownloadFileData data)
    {
        Debug.Log($"DownloadFileBegin: {data.FileName}");
    }

    private void DownloadFinishCallback(DownloaderFinishData data)
    {
        Debug.Log($"DownloadFinish: {data.Succeed}");
        if (data.Succeed)
        {
            GetSpriteAsset(GameObject.Find("Image"), "Assets/Scenes/test.jpg", "test");
            GetAudioAsset(GameObject.Find("Audio Source"), "Assets/Scenes/Yelp");
            GetPrefabAsset(GameObject.Find("Canvas"), "Assets/Scenes/Image");
        }
    }

    private void DownloadUpdateCallback(DownloadUpdateData data)
    {
        int totalCount = data.TotalDownloadCount;
        int curCount = data.CurrentDownloadCount;
        long totalBytes = data.TotalDownloadBytes;
        long curBytes = data.CurrentDownloadBytes;
        Debug.Log($"DownloadUpdate: {curCount / totalCount}, {curBytes / 1024}/{totalBytes / 1024}");
    }

    private void DownloadErrorCallback(DownloadErrorData data)
    {
        Debug.LogWarning($"DownloadError: {data.FileName} - {data.ErrorInfo}");
    }




    public void GetSpriteAsset(GameObject obj, string assetPath,string assetName)
    {
        var package = YooAssets.GetPackage(packageName);
        SubAssetsHandle handle = package.LoadSubAssetsAsync<Sprite>(assetPath);
        handle.Completed += (SubAssetsHandle handle) =>
        {
            Sprite sprite = handle.GetSubAssetObject<Sprite>(assetName);
            obj.GetComponent<Image>().sprite = sprite;
            handle.Release();
        };
    }

    public void GetAudioAsset(GameObject obj, string assetName)
    {
        var package = YooAssets.GetPackage(packageName);
        AssetHandle handle = package.LoadAssetAsync<AudioClip>(assetName);
        handle.Completed += (AssetHandle handle) =>
        {
            AudioClip audioClip = handle.AssetObject as AudioClip;
            obj.GetComponent<AudioSource>().clip = audioClip;
            obj.GetComponent<AudioSource>().Play();
            handle.Release();
        };
    }

    public void GetPrefabAsset(GameObject parent,string assetName)
    {
        var package = YooAssets.GetPackage(packageName);
        AssetHandle handle = package.LoadAssetAsync<GameObject>(assetName);
        handle.Completed += (AssetHandle handle) =>
        {
            GameObject go = handle.InstantiateSync();
            //go.transform.parent = parent.transform;
            go.transform.SetParent(parent.transform, true);
            handle.Release();
        };
    }


    IEnumerator ClearPackageUnusedCacheBundleFiles()
    {
        var package = YooAssets.GetPackage(packageName);
        var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);

        yield return operation;
        if (operation.Status == EOperationStatus.Succeed)
        {
            //清理成功
        }
        else
        {
            //清理失败
        }
    }

    IEnumerator ClearPackageAllCacheBundleFiles()
    {
        var package = YooAssets.GetPackage(packageName);
        var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearAllBundleFiles);

        yield return operation;
        if (operation.Status == EOperationStatus.Succeed)
        {
            //清理成功
        }
        else
        {
            //清理失败
        }
    }

    private IEnumerator ClearPackageUnusedCacheManifestFiles()
    {
        var package = YooAssets.GetPackage(packageName);
        var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedManifestFiles);

        yield return operation;
        if (operation.Status == EOperationStatus.Succeed)
        {
            //清理成功
        }
        else
        {
            //清理失败
        }
    }

    private IEnumerator ClearPackageAllCacheManifestFiles()
    {
        var package = YooAssets.GetPackage(packageName);
        var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearAllManifestFiles);
        yield return operation;

        if (operation.Status == EOperationStatus.Succeed)
        {
            //清理成功
        }
        else
        {
            //清理失败
        }
    }

    IEnumerator DestroyPackage()
    {
        var package = YooAssets.GetPackage(packageName);
        DestroyOperation operation = package.DestroyAsync();

        yield return operation;
        if (YooAssets.RemovePackage(package))
        {
            Debug.Log("DestroyPackage success！");
        }
    }

}
