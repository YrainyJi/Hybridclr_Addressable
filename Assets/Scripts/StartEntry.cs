using AOT;
using HybridCLR;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class StartEntry : MonoBehaviour
{
    [SerializeField]
    private Slider _ProgressSlider;
    [SerializeField]
    private Text _FileSizeText;
    [SerializeField]
    private Text _ProgressText;

    private void Start()
    {
        EntryManager.HotfixStartEvent += HotFix;
        EntryManager.ResourceSizeEvent += SetFileSize;
        EntryManager.DowonloadPercentEvent += DownloadPercent;

        EntryManager.StartHandle?.Invoke();
    }

    private void HotFix()
    {
        Addressables.LoadAssetAsync<GameObject>("Cube").Completed += obj =>
        {
            Instantiate(obj.Result);

            AOTUpdata.Main();
        };
    }

    private void SetFileSize(float size)
    {
        _FileSizeText.text = $"资源更新总大小: {size.ToString("0.00")} M";
    }

    private void DownloadPercent(float percent)
    {
        _ProgressSlider.value = percent;
        _ProgressText.text = $"资源更新进度: {percent * 100} %";
    }
}



