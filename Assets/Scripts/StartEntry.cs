using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HotFix;
using HybridCLR;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using System.Reflection;

public class StartEntry : MonoBehaviour
{
    void Awake()
    {
        LoadDepend();
    }

    private void Start()
    {
        LoadDLL();
    }

    private void LoadDepend()
    {
        Addressables.DownloadDependenciesAsync("dll");
        Addressables.DownloadDependenciesAsync("prefab");
        LoadMetadataForAOTAssemblies();
    }

    private void LoadDLL()
    {
        Addressables.LoadAssetAsync<TextAsset>("Hotfix.dll").Completed += hotfix =>
        {
            Assembly.Load(hotfix.Result.bytes);
            HotFix();
        };
    }

    private void HotFix()
    {
        Addressables.LoadAssetAsync<GameObject>("Cube").Completed += obj =>
        {
            Instantiate(obj.Result);
        };
    }

    private void LoadMetadataForAOTAssemblies()
    {
        List<string> aotDllList = new List<string>
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
            "Unity.Addressables.dll",
            "Unity.ResourceManager.dll"
        };
        foreach (var aotDllName in aotDllList)
        {
            Addressables.LoadAssetAsync<TextAsset>(aotDllName).Completed += AotDLL =>
            {
                if (AotDLL.Status == AsyncOperationStatus.Succeeded)
                {
                    byte[] dllBytes = AotDLL.Result.bytes;
                    var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
                    Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. ret:{err}");
                }
            };
        }
    }
}
