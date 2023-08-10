using UnityEngine;
using UnityEditor;
using System.IO;

public class HybridCLRExtension
{
    /// <summary>
    /// 给 DLL 添加后缀
    /// </summary>
    [MenuItem("HybridCLR/Modify Suffix")]
    public static void ModifySuffix()
    {
        string aotDLLFile = $"{Application.dataPath}/AotDLL";
        if (!Directory.Exists(aotDLLFile)) return;

        DirectoryInfo dir = new DirectoryInfo(aotDLLFile);

        FileInfo[] files = dir.GetFiles();

        for (int i = 0; i < files.Length; i++)
        {
            // 文件的扩展名
            string fileExtensionName = files[i].Extension;

            if (fileExtensionName.Equals(".meta")) continue;

            if (!fileExtensionName.Equals(".bytes") && fileExtensionName.Equals(".dll"))
            {
                string newFileExtensionName = $"{fileExtensionName}.bytes";
                string newFilePath = Path.ChangeExtension(files[i].FullName, newFileExtensionName);
                files[i].MoveTo(newFilePath);
            }
        }

        AssetDatabase.Refresh();
    }

}
