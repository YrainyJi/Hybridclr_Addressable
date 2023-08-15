using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        // 初始化 Addressables
        yield return Addressables.InitializeAsync();

        // 检查更新
        AsyncOperationHandle<List<string>> handle = Addressables.CheckForCatalogUpdates(false);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            List<string> catalogs = handle.Result;

            if (catalogs != null && catalogs.Count > 0)
            {
                for (int i = 0; i < catalogs.Count; i++)
                {
                    Debug.Log($"catalog:  {catalogs[i]}");
                }

                _LoadingShowText.text = "下载更新catalog";

                var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                yield return updateHandle;

                foreach (var item in updateHandle.Result)
                {
                    _UpdateKeys.AddRange(item.Keys);
                }
                _LoadingShowText.text = $"更新catalog完成: {updateHandle.Status}";

                // 获取下载文件的大小
                var sizeHandle = Addressables.GetDownloadSizeAsync(_UpdateKeys);
                yield return sizeHandle;

                long totalDownloadSize = sizeHandle.Result;
                _LoadingFileSizeText.text = $"下载文件的大小: {totalDownloadSize}";

                if (totalDownloadSize > 0)
                {
                    // 下载
                    var downloadHandle = Addressables.DownloadDependenciesAsync(_UpdateKeys, Addressables.MergeMode.Union, false);
                    while (!downloadHandle.IsDone)
                    {
                        if (downloadHandle.Status == AsyncOperationStatus.Failed) break;

                        float percentage = downloadHandle.PercentComplete;
                        _LoadingShowText.text = $"资源加载进度: {MathF.Round(1 - percentage) * 100}%";
                        _loadingSlider.value = 1 - percentage;

                        if (percentage >= 0.9f)
                        {
                            _loadingSlider.value = 1;
                        }

                        yield return null;
                    }

                    yield return downloadHandle;
                }
            }
            else
            {
                _LoadingShowText.text = "没有需要更新的catalogs信息";
            }
        }

        Addressables.Release(handle);
    }

}
