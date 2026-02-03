// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using UnityEditor;
// using UnityEngine;
//  
// public class PathTools
// {
//     // 打包AB包根路径
//     public const string AB_RESOURCES = "StreamingAssets"; 
//     
//     // 得到 AB 资源的输入目录
//     public static string GetABResourcesPath()
//     {
//         return Application.dataPath + "/" + AB_RESOURCES;
//     }
//
//     // 获得 AB 包输出路径
//     public static string GetABOutPath()
//     {
//         return GetPlatformPath() + "/" + GetPlatformName();
//     }
//     
//     //获得平台路径
//     private static string GetPlatformPath()
//     {
//         string strReturenPlatformPath = string.Empty;
//
// #if UNITY_STANDALONE_WIN
//         strReturenPlatformPath = Application.streamingAssetsPath;
// #elif UNITY_IPHONE
//             strReturenPlatformPath = Application.persistentDataPath;
// #elif UNITY_ANDROID
//             strReturenPlatformPath = Application.persistentDataPath;
// #endif
//         
//         return strReturenPlatformPath;
//     }
//     
//     // 获得平台名称
//     public static string GetPlatformName()
//     {
//         string strReturenPlatformName = string.Empty;
//
// #if UNITY_STANDALONE_WIN
//         strReturenPlatformName = "Windows";
// #elif UNITY_IPHONE
//             strReturenPlatformName = "IPhone";
// #elif UNITY_ANDROID
//             strReturenPlatformName = "Android";
// #endif
//
//         return strReturenPlatformName;
//     }
//     
//     // 返回 WWW 下载 AB 包加载路径
//     public static string GetWWWAssetBundlePath()
//     {
//         string strReturnWWWPath = string.Empty;
//
// #if UNITY_STANDALONE_WIN
//         strReturnWWWPath = "file://" + GetABOutPath();
// #elif UNITY_IPHONE
//             strReturnWWWPath = GetABOutPath() + "/Raw/";
// #elif UNITY_ANDROID
//             strReturnWWWPath = "jar:file://" + GetABOutPath();
// #endif
//
//         return strReturnWWWPath;
//     }
// }
//
//
//
// /// <summary>
// /// AssetBundle 打包工具
// /// </summary>
// public class ABBuilder 
// {
//     private static string abOutPath;
//     private static List<AssetBundleBuild> listassets = new List<AssetBundleBuild>();
//     private static List<DirectoryInfo> listfileinfo = new List<DirectoryInfo>();
//     private static bool isover = false; //是否检查完成，可以打包
//     static private string selectPath;
//
//     public static bool GetState()
//     {
//         return isover;
//     }
//
//     public static AssetBundleBuild[] GetAssetBundleBuilds()
//     {
//         return listassets.ToArray();
//     }
//
//     [MenuItem("ABTools/Build", false)]
//     public static void Build()
//     {
//         abOutPath = Application.streamingAssetsPath;
//
//         if (!Directory.Exists(abOutPath))
//             Directory.CreateDirectory(abOutPath);
//
//         UnityEngine.Object obj = Selection.activeObject;
//         selectPath = AssetDatabase.GetAssetPath(obj);
//         SearchFileAssetBundleBuild(selectPath);
//         
//         BuildPipeline.BuildAssetBundles(abOutPath,
//             GetAssetBundleBuilds(), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
//         Debug.Log("AssetBundle打包完毕");
//     }
//
//     //是文件，继续向下
//     public static void SearchFileAssetBundleBuild(string path) 
//     {
//         DirectoryInfo directory = new DirectoryInfo(@path);
//         FileSystemInfo[] fileSystemInfos = directory.GetFileSystemInfos();
//         listfileinfo.Clear();
//         //遍历所有文件夹中所有文件
//         foreach (var item in fileSystemInfos)
//         {
//             int idx = item.ToString().LastIndexOf(@"\");
//             string name = item.ToString().Substring(idx + 1);
//             //item为文件夹，添加进listfileinfo，递归调用
//             if ((item as DirectoryInfo) != null)
//                 listfileinfo.Add(item as DirectoryInfo);
//             
//             //剔除meta文件，其他文件都创建AssetBundleBuild,添加进listassets；
//             if (!name.Contains(".meta"))
//             {
//                 CheckFileOrDirectoryReturnBundleName(item, path + "/" + name);
//             }
//         }
//
//         if (listfileinfo.Count == 0)
//             isover = true;
//         else
//         {
//             Debug.LogError(listfileinfo.Count);
//         }
//     }
//
//     //判断是文件还是文件夹
//     public static string CheckFileOrDirectoryReturnBundleName(FileSystemInfo fileSystemInfo, string path) 
//     {
//         FileInfo fileInfo = fileSystemInfo as FileInfo;
//         if (fileInfo != null)
//         {
//             string[] strs = path.Split('.');
//             string[] dictors = strs[0].Split('/');
//             string name = "";
//             for (int i = 1; i < dictors.Length; i++)
//             {
//                 if (i < dictors.Length - 1)
//                 {
//                     name += dictors[i] + "/";
//                 }
//                 else
//                 {
//                     name += dictors[i];
//                 }
//             }
//
//             string[] strName = selectPath.Split('/');
//             AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
//             assetBundleBuild.assetBundleName = strName[strName.Length - 1];
//             assetBundleBuild.assetBundleVariant = "ab";
//             assetBundleBuild.assetNames = new string[] {path};
//             listassets.Add(assetBundleBuild);
//             return name;
//         }
//         else
//         {
//             //递归调用
//             SearchFileAssetBundleBuild(path);
//             return null;
//         }
//     }
//
//     [MenuItem("ABTools/Remove AB Label")]
//     public static void RemoveABLabel()
//     {
//         // 需要移除标记的根目录
//         string strNeedRemoveLabelRoot = string.Empty;
//         // 目录信息（场景目录信息数组，表示所有根目录下场景目录）
//         DirectoryInfo[] directoryDIRArray = null;
//         
//         // 定义需要移除AB标签的资源的文件夹根目录
//         strNeedRemoveLabelRoot = PathTools.GetABResourcesPath();   
//         DirectoryInfo dirTempInfo = new DirectoryInfo(strNeedRemoveLabelRoot);
//         JudgeDirOrFileByRecursive(dirTempInfo);
//         directoryDIRArray = dirTempInfo.GetDirectories();
//         
//         // 遍历本场景目录下所有的目录或者文件
//         foreach (DirectoryInfo currentDir in directoryDIRArray)
//         {
//             // 递归调用方法，找到文件，则使用 AssetImporter 类，标记"包名"与 "后缀名"
//             JudgeDirOrFileByRecursive(currentDir);
//         }
//
//         // 清空无用的 AB 标记
//         AssetDatabase.RemoveUnusedAssetBundleNames();
//         // 刷新
//         AssetDatabase.Refresh();
//
//         // 提示信息，标记包名完成
//         Debug.Log("AssetBundle 本次操作移除标记完成");
//     }
//
//     /// <summary>
//     /// 递归判断判断是否是目录或文件
//     /// 是文件，修改 Asset Bundle 标记
//     /// 是目录，则继续递归
//     /// </summary>
//     /// <param name="fileSystemInfo">当前文件信息（文件信息与目录信息可以相互转换）</param>
//     public static void JudgeDirOrFileByRecursive(FileSystemInfo fileSystemInfo)
//     {
//         if (fileSystemInfo.Exists == false)
//         {
//             Debug.LogError("文件或者目录名称：" + fileSystemInfo + " 不存在，请检查");
//             return;
//         }
//
//         // 得到当前目录下一级的文件信息集合
//         DirectoryInfo directoryInfoObj = fileSystemInfo as DirectoryInfo; 
//         // 文件信息转为目录信息
//         FileSystemInfo[] fileSystemInfoArray = directoryInfoObj.GetFileSystemInfos();
//
//         foreach (FileSystemInfo fileInfo in fileSystemInfoArray)
//         {
//             FileInfo fileInfoObj = fileInfo as FileInfo;
//             // 文件类型
//             if (fileInfoObj != null)
//             {
//                 // 修改此文件的 AssetBundle 标签
//                 RemoveFileABLabel(fileInfoObj);
//             }
//             // 目录类型
//             else
//             {
//                 // 如果是目录，则递归调用
//                 JudgeDirOrFileByRecursive(fileInfo);
//             }
//         }
//     }
//
//     /// <summary>
//     /// 给文件移除 Asset Bundle 标记
//     /// </summary>
//     /// <param name="fileInfoObj">文件（文件信息）</param>
//     static void RemoveFileABLabel(FileInfo fileInfoObj)
//     {
//         // AssetBundle 包名称
//         string strABName = string.Empty;
//         // 文件路径（相对路径）
//         string strAssetFilePath = string.Empty;
//
//         // 参数检查（*.meta 文件不做处理）
//         if (fileInfoObj.Extension == ".meta")
//         {
//             return;
//         }
//
//         // 得到 AB 包名称
//         strABName = string.Empty;
//         // 获取资源文件的相对路径
//         int tmpIndex = fileInfoObj.FullName.IndexOf("Assets");
//         // 得到文件相对路径
//         strAssetFilePath = fileInfoObj.FullName.Substring(tmpIndex); 
//         
//         // 给资源文件移除 AB 名称
//         AssetImporter tmpImportObj = AssetImporter.GetAtPath(strAssetFilePath);
//         tmpImportObj.assetBundleName = strABName;
//     }
// }