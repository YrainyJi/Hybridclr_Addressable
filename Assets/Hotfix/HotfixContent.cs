using UnityEngine;

namespace HotFix
{
    public class HotfixContent : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("热更脚本");
            AOT.AOTUpdata.StartHotFix();
        }

    }
}

