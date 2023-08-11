using HybridCLR;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StartEntry : MonoBehaviour
{
    static Assembly hotfixAss;

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
            hotfixAss = Assembly.Load(hotfix.Result.bytes);
            HotFix();
        };
    }

    private void HotFix()
    {
        Addressables.LoadAssetAsync<GameObject>("Cube").Completed += obj =>
        {
            Instantiate(obj.Result);
        };

        //HotfixUpdata.Main();
    }

    private void LoadMetadataForAOTAssemblies()
    {
        List<string> aotDllList = new List<string>
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll"
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



