using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EntryManager : MonoBehaviour
{
    [SerializeField]
    private AssetLabelReference _AotDll;
    [SerializeField]
    private AssetLabelReference _HotfixDll;

    // 先进先出的队列
    private List<object> _UpdateKeys = new List<object>();

    /// <summary>
    /// 热更新开始的事件回调
    /// </summary>
    public static event Action HotfixStartEvent = null;

    /// <summary>
    /// 得到下载大小的事件回调
    /// </summary>
    public static event Action<float> ResourceSizeEvent = null;
    /// <summary>
    /// 下载进度的事件回调
    /// </summary>
    public static event Action<float> DowonloadPercentEvent = null;

    public static Action StartHandle = null;

    void Awake()
    {
        DontDestroyOnLoad(this);
        StartHandle = EntryStart;
    }

    void EntryStart()
    {
        StartCoroutine(Entry());
    }

    IEnumerator Entry()
    {
        // 更新检查任务
        yield return CheckUpdata();

        HotfixStartEvent?.Invoke();
    }

    IEnumerator CheckUpdata()
    {
        // 检查资源更新
        yield return UpdataAddress();
        // 加载补充元数据DLL
        yield return LoadMetaDataForAOTDLL();
        // 加载热更新DLL
        yield return LoadHotFixDLL();
    }

    #region 检测更新

    /// <summary>
    /// 检查资源更新
    /// </summary>
    IEnumerator UpdataAddress()
    {
        // 初始化 Addressables 的检测更新
        yield return Addressables.InitializeAsync();

        // 检查更新
        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false); //false是手动释放异步结果对象
        yield return checkHandle;

        if (checkHandle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("版本检测失败:" + checkHandle.OperationException.ToString());
            yield break;
        }

        List<string> catalogs = checkHandle.Result;

        if (catalogs.Count > 0)
        {
            // 更新目录列表
            AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogs, false);
            yield return updateHandle;

            if (updateHandle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError("版本更新失败:" + updateHandle.OperationException.ToString());
                yield break;
            }

            // 更新列表迭代器
            List<IResourceLocator> locators = updateHandle.Result;
            foreach (var locator in locators)
                _UpdateKeys.AddRange(locator.Keys);

            Addressables.Release(checkHandle);
            Addressables.Release(updateHandle);
        }
        else //版本已经更新过的，采用这种方式
        {
            IEnumerable<IResourceLocator> locators = Addressables.ResourceLocators;
            foreach (var locator in locators)
            {
                _UpdateKeys.AddRange(locator.Keys);
            }
        }

        yield return GetUpdateSizeProgress(_UpdateKeys);
    }

    /// <summary>
    /// 获取更新内容的大小和进度
    /// </summary>
    IEnumerator GetUpdateSizeProgress(IEnumerable<object> key)
    {
        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(key);
        yield return sizeHandle;

        if (sizeHandle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("资源大小更新失败: " + sizeHandle.OperationException.ToString());
            yield break;
        }

        // 显示加载文件的大小
        float size = sizeHandle.Result / (1024.0f * 1024.0f);

        ResourceSizeEvent?.Invoke(size);

        if (sizeHandle.Result > 0)
        {
            // 下载
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(key, Addressables.MergeMode.Union, false);
            while (!downloadHandle.IsDone)
            {
                float percentage = downloadHandle.GetDownloadStatus().Percent;
                DowonloadPercentEvent?.Invoke(percentage);
                yield return null;
            }

            Addressables.Release(downloadHandle);
        }

        Addressables.Release(sizeHandle);
    }

    #endregion

    #region 加载AOT元数据DLL

    /// <summary>
    /// 加载补充元数据DLL
    /// </summary>
    IEnumerator LoadMetaDataForAOTDLL()
    {
        //这一步实际上是为了解决AOT 泛型类的问题 
        HomologousImageMode mode = HomologousImageMode.SuperSet;

        AsyncOperationHandle<IList<TextAsset>> aots = Addressables.LoadAssetsAsync<TextAsset>(_AotDll, null);
        yield return aots;

        foreach (var asset in aots.Result)
        {
            LoadImageErrorCode errorCode = RuntimeApi.LoadMetadataForAOTAssembly(asset.bytes, mode);

            if (errorCode == LoadImageErrorCode.OK)
            {
                Debug.Log($"加载AOT元数据DLL:{asset.name} 成功");
                continue;
            }

            Debug.LogWarning(($"加载AOT元数据DLL:{asset.name} 失败,错误码:{errorCode}"));
        }

        Addressables.Release(aots);
    }

    #endregion

    #region 加载热更DLL

    /// <summary>
    /// 加载热更新DLL
    /// </summary>
    IEnumerator LoadHotFixDLL()
    {
        AsyncOperationHandle<IList<TextAsset>> hotfixs = Addressables.LoadAssetsAsync<TextAsset>(_HotfixDll, null);
        yield return hotfixs;

        foreach (var hotfix in hotfixs.Result)
        {
            Debug.Log($"加载热更DLL: {hotfix.name}");
            Assembly.Load(hotfix.bytes);
            Debug.Log($"加载热更DLL: {hotfix.name} 完成");
        }

        Addressables.Release(hotfixs);
    }

    #endregion
}
