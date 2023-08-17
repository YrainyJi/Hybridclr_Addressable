using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [SerializeField]
    private Slider _loadingSlider;
    [SerializeField]
    private Text _LoadingShowText;
    [SerializeField]
    private Text _LoadingFileSizeText;

    // 先进先出的队列
    private List<object> _UpdateKeys = new List<object>();

    IEnumerator Start()
    {
        // 初始化 Addressables 的检测更新
        yield return Addressables.InitializeAsync();

        // 检查更新
        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false); ////false是手动释放异步结果对象
        yield return checkHandle;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            List<string> catalogs = checkHandle.Result;

            if (catalogs != null && catalogs.Count > 0)
            {
                _LoadingShowText.text = "下载更新catalog";

                // 更新目录日志
                AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                yield return updateHandle;

                _LoadingShowText.text = $"需要更新的资源: {updateHandle.Result.Count} 个";

                foreach (var item in updateHandle.Result)
                    _UpdateKeys.AddRange(item.Keys);

                yield return GetUpdateSizeProgress(_UpdateKeys);

                Addressables.Release(updateHandle);
            }
            else
            {
                _LoadingShowText.text = "没有需要更新的catalogs信息";
            }
        }

        Addressables.Release(checkHandle);
    }

    // 获取更新内容的大小和进度
    IEnumerator GetUpdateSizeProgress(IEnumerable key)
    {
        // 获取下载文件的大小
        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(key);
        yield return sizeHandle;

        // 显示加载文件的大小
        float size = sizeHandle.Result / (1024.0f * 1024.0f);
        _LoadingFileSizeText.text = $"下载文件的大小: {size.ToString("0.00")} M";

        if (sizeHandle.Result > 0)
        {
            // 下载
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(key, Addressables.MergeMode.Union, false);
            while (downloadHandle.IsValid())
            {
                float percentage = downloadHandle.GetDownloadStatus().Percent;
                _LoadingShowText.text = $"资源加载进度: {percentage * 100}%";
                _loadingSlider.value = percentage;
                yield return null;

                if (percentage == 1)
                    Addressables.Release(downloadHandle);
            }
        }

        Addressables.Release(sizeHandle);
    }

}
