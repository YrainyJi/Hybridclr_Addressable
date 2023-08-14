using UnityEngine;

namespace HotFix
{
    public class HotfixUpdata
    {
        public static void Main()
        {
            Debug.Log("开始热更");
            Start();
        }

        private static void Start()
        {
            Debug.Log("热更内容");
            End();
        }

        private static void End()
        {
            Debug.Log("更新完成!");
        }
    }
}


