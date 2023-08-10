using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotFix
{
    public class HotfixMono : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("add hotfix Mono");
            cct();
            HotfixUpdata.Main();
        }


        private void cct()
        {
            Debug.Log("start hotfix!!!!!!!");
        }
    }
}

