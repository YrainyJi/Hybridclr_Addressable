using System;
using UnityEngine;

namespace HotFix
{
    public class HotfixContent : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("热更脚本");
            AOT.AOTUpdata.StartHotFix();
            Finishing();
        }

        void Finishing()
        {
            Debug.Log("热更新完成");
        }
    }
}

