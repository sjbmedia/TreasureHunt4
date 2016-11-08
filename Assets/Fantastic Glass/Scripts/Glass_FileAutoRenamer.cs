#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FantasticGlass
{
	#region File Auto Renamer

	public class Glass_FileAutoRenamer
	{
		public string currentVersionStringFind = "_1_1_0";
		public string currentVersionStringReplace = "";
		public string previousVersionStringFind = "";
		public string previousVersionStringReplace = "_1_0_0";
		public List<string> currentVersionFiles_oldSuffix = new List<string> ();
		public List<string> currentVersionFiles_withoutSuffix = new List<string> ();
		public List<string> previousVersionFiles_oldSuffix = new List<string> ();
		public List<string> previousVersionFiles_withoutSuffix = new List<string> ();
		public Dictionary<string, bool> folders = new Dictionary<string,bool> ();
		public bool backupOldFiles = false;
		public string packagePath = "";
		public bool findCurrent = true;
		public bool findPrevious = true;
		public bool savePrevious = true;
		public bool saveCurrent = true;
		public bool deletePrevious = true;
		public bool deleteCurrent = true;
		public bool ignoreMetaFiles = false;
		public bool printDebug = true;
		public static string metaString = ".meta";

		public Glass_FileAutoRenamer ()
		{
			Init ();
		}

		public void Init ()
		{
			InitPackagePath ();
			InitFolderPaths ();
		}
		
		public void ClearData ()
		{
			currentVersionFiles_oldSuffix.Clear ();
			currentVersionFiles_withoutSuffix.Clear ();
			previousVersionFiles_oldSuffix.Clear ();
			previousVersionFiles_withoutSuffix.Clear ();
		}
		
		public void FindAndRenameAllFiles ()
		{
			ClearData ();

			if (findCurrent)
				Find_CurentVersion ();
			StripSuffix_CurrentVersion ();

			if (findPrevious)
				Find_PreviousVersion ();

			if (savePrevious)
				Save_PreviousVersion_NewSuffix ();
			if (deletePrevious)
				Delete_PreviousVersion_OldSuffix ();

			if (saveCurrent)
				Save_CurrentVersion_NewSuffix ();
			if (deleteCurrent)
				Delete_CurrentVersion_OldSuffix ();
		}

		public void Find_CurentVersion ()
		{
			foreach (string folderPath in folders.Keys) {
				if (folders [folderPath]) {
					Find_CurentVersion (folderPath);
				}
			}
		}

		public void Find_CurentVersion (string path)
		{
			DirectoryInfo pathInfo = new DirectoryInfo (path);

			if (ignoreMetaFiles)
				foreach (FileInfo fileInfo in pathInfo.GetFiles()) {
					if (fileInfo.Name.Contains (currentVersionStringFind)) {
						if (fileInfo.FullName.Contains (".meta"))
							continue;
						currentVersionFiles_oldSuffix.Add (fileInfo.FullName);
						if (printDebug)
							Debug.Log ("Found(New Version):" + fileInfo.FullName);
					}
				}
			else
				foreach (FileInfo fileInfo in pathInfo.GetFiles()) {
					if (fileInfo.Name.Contains (currentVersionStringFind)) {
						currentVersionFiles_oldSuffix.Add (fileInfo.FullName);
						if (printDebug)
							Debug.Log ("Found(New Version):" + fileInfo.FullName);
					}
				}
		}

		public void StripSuffix_CurrentVersion ()
		{
			foreach (string filePath in currentVersionFiles_oldSuffix) {
				currentVersionFiles_withoutSuffix.Add (ReplaceSuffix (filePath, currentVersionStringFind, ""));
			}
		}
	
		public void StripSuffix_PreviousVersion ()
		{
			if (previousVersionStringFind.Length > 0) {
				foreach (string filePath in previousVersionFiles_oldSuffix) {
					previousVersionFiles_withoutSuffix.Add (ReplaceSuffix (filePath, previousVersionStringFind, ""));
				}
			} else {
				foreach (string filePath in previousVersionFiles_oldSuffix) {
					previousVersionFiles_withoutSuffix.Add (filePath);
				}
			}
		}
	
		public void Find_PreviousVersion ()
		{
			foreach (string currentPath in currentVersionFiles_oldSuffix) {
				string previousPath = ReplaceSuffix (currentPath, currentVersionStringFind, previousVersionStringFind);
				if (File.Exists (previousPath)) {
					previousVersionFiles_oldSuffix.Add (previousPath);
					if (printDebug)
						Debug.Log ("Found(Previous Version):" + previousPath);
				}
			}
		}

		void Save_PreviousVersion_NewSuffix ()
		{
			foreach (string filePath in previousVersionFiles_oldSuffix) {
				string newFilePath = ReplaceSuffix (filePath, previousVersionStringFind, previousVersionStringReplace);
				File.Copy (filePath, newFilePath);
				if (printDebug) {
					Debug.Log ("Saved (Previous Version):" + newFilePath);
				}
			}
		}

		void Delete_PreviousVersion_OldSuffix ()
		{
			foreach (string filePath in previousVersionFiles_oldSuffix) {
				File.Delete (filePath);
				if (printDebug) {
					Debug.Log ("Deleted (Previous Version):" + filePath);
				}
			}
		}
	
		void Save_CurrentVersion_NewSuffix ()
		{
			foreach (string filePath in currentVersionFiles_oldSuffix) {
				string newFilePath = ReplaceSuffix (filePath, currentVersionStringFind, currentVersionStringReplace);
				File.Copy (filePath, newFilePath);
				if (printDebug) {
					Debug.Log ("Saved (Previous Version):" + newFilePath);
				}
			}
		}

		void Delete_CurrentVersion_OldSuffix ()
		{
			foreach (string filePath in currentVersionFiles_oldSuffix) {
				File.Delete (filePath);
				if (printDebug) {
					Debug.Log ("Deleted (Previous Version):" + filePath);
				}
			}
		}

		void InitPackagePath ()
		{
			if (!packagePath.Contains (Application.dataPath)) {
				packagePath = Application.dataPath + "/" + GlassManager.default_packageName + "/";
			}
		}

		void InitFolderPaths ()
		{
			folders.Add (FolderPath ("Materials", true), true);
			folders.Add (FolderPath ("Textures", true), true);
			folders.Add (FolderPath ("Models", true), true);
			folders.Add (FolderPath ("Prefabs", true), true);
			folders.Add (FolderPath ("Scenes", true), true);
			folders.Add (FolderPath ("Animations", true), true);
			folders.Add (FolderPath ("Physics Materials", true), true);
		
			folders.Add (FolderPath ("XML", false), true);
			folders.Add (FolderPath ("Documentation", false), true);
		}

		public string FolderPath (string folderName, bool projectRelative)
		{
			string folderPath = packagePath + "/" + folderName + "/";
		if(projectRelative)
			folderPath =  FileUtil.GetProjectRelativePath(folderPath);
			return folderPath;
		}

		public static string ReplaceSuffix (string filename, string currentSuffix, string newSuffix)
		{
			string result = filename;

			string currentSuffixFormatted = currentSuffix;
			string newSuffixFormatted = newSuffix;

			if (filename.Contains (metaString))
				result.Remove(result.IndexOf(metaString), metaString.Length);

			if (!currentSuffixFormatted.Contains ("."))
				currentSuffixFormatted += ".";
			
			if (!newSuffixFormatted.Contains ("."))
				newSuffixFormatted += ".";

			int index = filename.LastIndexOf (currentSuffixFormatted);
			
			if (index == -1)
				return filename;
			
			result = result.Remove (index, currentSuffixFormatted.Length).Insert (index, newSuffixFormatted);

			if (filename.Contains (metaString))
				result += metaString;

			return result;
		}
		
	}

	#endregion
	
}

#endif
