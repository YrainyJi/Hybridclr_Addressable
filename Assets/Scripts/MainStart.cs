using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HotFix;
using HybridCLR;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using System.Reflection;

public class MainStart : MonoBehaviour
{
    AsyncOperationHandle handle;


    //IEnumerator Start()
    //{
    //    Addressables.InitializeAsync();

    //    string key = "prefab";

    //    AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(key);
    //    yield return getDownloadSize;
    //    Debug.Log(getDownloadSize.Result);


    //    if (getDownloadSize.Result > 0)
    //    {
    //        handle = Addressables.DownloadDependenciesAsync(key, false);
    //        yield return handle;
    //    }
    //}

    private void Start()
    {
        handle = Addressables.DownloadDependenciesAsync("dll");
        Addressables.DownloadDependenciesAsync("prefab");
        Debug.Log(Addressables.RuntimePath);
    }

    void Update()
    {
        if (handle.IsValid())
        {
            if (handle.PercentComplete == 1)
            {
                handle.Completed += LoadDLL;
                Addressables.Release(handle);
            }
        }
    }

    private void HotFix()
    {
        Addressables.LoadAssetAsync<GameObject>("Cube").Completed += obj =>
        {
            Instantiate(obj.Result);
        };
    }

    Assembly currAssembly;

    private void LoadDLL(AsyncOperationHandle handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            //LoadMetadataForAOTAssemblies();
            Addressables.LoadAssetAsync<TextAsset>("Hotfix.dll").Completed += hotfix =>
            {
                currAssembly = Assembly.Load(hotfix.Result.bytes);

                HotFix();
            };
        }
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
