using UnityEngine;

namespace AOT
{
    public class AOTUpdata
    {
        public static void Main()
        {
            Debug.Log("开始更新");
            Start();
        }

        private static void Start()
        {
            Debug.Log("更新内容");
            End();
        }

        private static void End()
        {
            Debug.Log("更新完成!");
        }

        public static void StartHotFix()
        {
            Debug.Log("热更新脚本调用了");
        }

    }
}


