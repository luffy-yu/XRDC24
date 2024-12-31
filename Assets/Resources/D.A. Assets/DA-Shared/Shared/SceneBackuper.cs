using DA_Assets.Shared.Extensions;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DA_Assets.Shared
{
    public class SceneBackuper
    {
        public static bool TryBackupActiveScene()
        {
            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                bool sceneFileExists = File.Exists(activeScene.path);

                string sceneName = activeScene.name;

                if (sceneName.IsEmpty())
                {
                    sceneName = "Untitled";
                }

                int backupNumber = 0;
                string nameWithoutNumber = sceneName;

                if (sceneName.Contains("-"))
                {
                    string[] parts = sceneName.Split('-');

                    if (parts.Length > 1)
                    {
                        if (int.TryParse(parts.Last(), out int _backupNumber))
                        {
                            backupNumber = _backupNumber;
                            nameWithoutNumber = string.Join("-", parts.Take(parts.Length - 1));
                        }
                    }
                }

                string backupsPath = GetBackupsPath();
                backupsPath.CreateFolderIfNotExists();

                string[] backupFiles = Directory.GetFiles(backupsPath, $"{nameWithoutNumber}-*.unity");
                if (backupFiles.Length > 0)
                {
                    foreach (string file in backupFiles)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);

                        if (fileName.Contains("-"))
                        {
                            string[] parts = fileName.Split('-');

                            if (parts.Length > 1)
                            {
                                if (int.TryParse(parts.Last(), out int existingBackupNumber))
                                {
                                    if (existingBackupNumber > backupNumber)
                                    {
                                        backupNumber = existingBackupNumber;
                                    }
                                }
                            }
                        }           
                    }
                }

                backupNumber++;

                string newName = $"{nameWithoutNumber}-{backupNumber}.unity";
                string filePath = Path.Combine(backupsPath, newName);

                if (sceneFileExists)
                {
                    File.Copy(activeScene.path, filePath);
                }
                else
                {
#if UNITY_EDITOR
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene, filePath);
#endif
                }

                Debug.Log($"A backup of your scene is created at the path:\r\n{filePath}");
                return true;
            }
            catch (Exception ex)
            {
                DALogger.LogError($"Error creating scene backup:\r\n{ex.Message}");
                return false;
            }
        }

        private static string GetProjectAbsolutePath()
        {
            string[] parts = Application.dataPath.Split('/');
            string path = "";

            for (int i = 0; i < parts.Length - 1; i++) //Iterate through all parts except the last one.
            {
                if (i > 0) path += "\\"; //Add a path separator for all but the first element.
                path += parts[i];
            }

            return path;
        }

        public static string GetBackupsPath()
        {
            string @base = GetProjectAbsolutePath();
            string path = Path.Combine(@base, "Library", "Backup", "Scenes");
            return path;
        }
    }
}