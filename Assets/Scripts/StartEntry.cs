using AOT;
using HybridCLR;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StartEntry : MonoBehaviour
{
    private void Start()
    {
        EntryManager.HotfixStartHandle += HotFix;
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

}



