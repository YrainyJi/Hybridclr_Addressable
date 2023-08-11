using UnityEngine;

namespace HotFix
{
    public class HotfixMono : MonoBehaviour
    {

        void Start()
        {
            HotfixUpdata.Main();
            Debug.Log("add hotfix Mono");
            Cct();
        }

        private void Cct()
        {
            Debug.Log("start hotfix!!!!!!!");
        }
    }
}

