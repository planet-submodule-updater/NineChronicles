using System;
using System.Collections;
using System.IO;
using Nekoyume.BlockChain;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Planetarium.Nekoyume.Editor
{
    public static class StoreDownloader
    {
        private const string MainNetPartitionFullSnapshotURL =
            "https://9c-snapshots.s3.ap-northeast-2.amazonaws.com/main/partition/full/9c-main-snapshot.zip";

        [MenuItem("Tools/Store/Download Main-net Snapshot", true)]
        public static bool DownloadMainNetStoreValidation() => !Application.isPlaying;

        [MenuItem("Tools/Store/Download Main-net Snapshot")]
        public static void DownloadMainNetStore()
        {
            if (!EditorUtility.DisplayDialog("Question", "This job takes a very long time. Do you want to continue?",
                "Yes", "No"))
            {
                Debug.Log("Downloading main-net snapshot canceled");
                return;
            }

            var fileName = $"main-net-snapshot-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
            var downloadFilePath = EditorUtility.SaveFilePanel(
                "Select Download Path",
                Application.temporaryCachePath,
                fileName,
                "zip");
            if (string.IsNullOrEmpty(downloadFilePath))
            {
                Debug.Log("Downloading main-net snapshot canceled");
                return;
            }

            EditorCoroutineUtility.StartCoroutineOwnerless(DownloadFileAsync(
                downloadFilePath,
                MainNetPartitionFullSnapshotURL));
        }

        [MenuItem("Tools/Store/Download and Extract Main-net Snapshot", true)]
        public static bool DownloadAndExtractMainNetStoreValidation() => !Application.isPlaying;

        [MenuItem("Tools/Store/Download and Extract Main-net Snapshot")]
        public static void DownloadAndExtractMainNetStore()
        {
            if (!EditorUtility.DisplayDialog("Question", "This job takes a very long time. Do you want to continue?",
                "Yes", "No"))
            {
                Debug.Log("Downloading main-net snapshot canceled");
                return;
            }

            var selectedFolder = EditorUtility.SaveFolderPanel(
                "Select Extract Path",
                StorePath.GetPrefixPath(),
                "9c_dev");
            if (string.IsNullOrEmpty(selectedFolder))
            {
                Debug.Log("Downloading main-net snapshot canceled");
                return;
            }

            EditorCoroutineUtility.StartCoroutineOwnerless(
                DownloadZipFileAndExtractAsync(
                    MainNetPartitionFullSnapshotURL,
                    selectedFolder));
        }

        private static IEnumerator DownloadFileAsync(string downloadFilePath, string url)
        {
            using var downloadHandler = new DownloadHandlerFile(downloadFilePath);
            downloadHandler.removeFileOnAbort = true;
            using var request = UnityWebRequest.Get(url);
            request.downloadHandler = downloadHandler;
            var asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                if (EditorUtility.DisplayCancelableProgressBar(
                    "Download",
                    $"url: {MainNetPartitionFullSnapshotURL}\ndownload to: {downloadFilePath}",
                    asyncOperation.progress))
                {
                    request.Abort();
                    Debug.Log("Downloading store canceled");
                    EditorUtility.ClearProgressBar();
                    yield break;
                }

                yield return new WaitForSeconds(.1f);
            }

            yield return asyncOperation;
            EditorUtility.ClearProgressBar();

            if (request.result != UnityWebRequest.Result.Success)
            {
                EditorUtility.DisplayDialog("Error",
                    $"Failed to download the Main-net Store at \"{MainNetPartitionFullSnapshotURL}\"", "ok");
                yield break;
            }

            if (!File.Exists(downloadFilePath))
            {
                EditorUtility.DisplayDialog("Error", $"Zip file not exist at \"{downloadFilePath}\"", "ok");
                yield break;
            }

            Debug.Log($"Download the Main-net store finished. Downloaded at \"{downloadFilePath}\"");
        }

        private static IEnumerator DownloadZipFileAndExtractAsync(string url, string extractPath)
        {
            var fileName = $"main-net-snapshot-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip";
            var downloadFilePath = Path.Combine(Application.temporaryCachePath, fileName);
            yield return EditorCoroutineUtility.StartCoroutineOwnerless(DownloadFileAsync(
                downloadFilePath,
                url));

            ZipUnzip.Unzip(downloadFilePath, extractPath);
            if (EditorUtility.DisplayDialog("Delete zip file", "Do you want to delete the zip file?", "Yes", "No"))
            {
                File.Delete(downloadFilePath);
            }

            Debug.Log($"Download and extract the Main-net snapshot finished. Extracted at \"{extractPath}\"");
        }
    }
}
