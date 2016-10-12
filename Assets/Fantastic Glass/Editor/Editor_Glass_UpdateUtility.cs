#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml.Serialization;

namespace FantasticGlass
{
    #region Preset Merger

    public class Glass_PresetMerger
    {
        static string packagePath = "Fantastic Glass";
        static string xmlPath = "XML";
        static string presetListName = "glass_preset_list";

        static Glass_PresetMerger()
        {
        }

        public static void MergePresets(bool showDebug)
        {
			if(showDebug)
            Debug.Log("Importing old presets into new preset list..");

            string xmlFullPath = Application.dataPath + "/" + packagePath + "/" + xmlPath + "/";

            string presetListPath = xmlFullPath + presetListName + ".xml";

            List<string> existingPresetNames = LoadList(presetListPath, showDebug);

            List<string> existingPresets = new List<string>();

            foreach (string existingPresetName in existingPresetNames)
            {
                existingPresets.Add(xmlFullPath + existingPresetName + ".xml");
            }

            List<string> foundFiles = FilesInFolder(xmlFullPath);

            for (int i = 0; i < foundFiles.Count; i++)
            {
                string foundFile = foundFiles[i];
                foundFiles[i] = foundFile.Replace("\\", "/");
            }

            for (int i = existingPresets.Count - 1; i >= 0; i--)
            {
                string existingPresetFile = existingPresets[i];
                if (!File.Exists(existingPresetFile))
                {
                    existingPresets.RemoveAt(i);
                    continue;
                }
                GlassPreset existingPreset = GlassPreset.LoadFromXML(existingPresetFile, showDebug);
                if (existingPreset == null)
                {
                    existingPresets.RemoveAt(i);
                    continue;
                }
                foundFiles.Remove(existingPresetFile);
            }

            for (int i = foundFiles.Count - 1; i >= 0; i--)
            {
                if (foundFiles[i].Contains("_old.xml"))
                    foundFiles.RemoveAt(i);
            }

            int parsedPresetCount = 0;

            foreach (string foundFile in foundFiles)
            {
                GlassPreset foundPreset = GlassPreset.LoadFromXML(foundFile, showDebug);
                if (foundPreset != null)
                {
                    if (foundPreset.name != null)
                    {
                        string newPresetName = foundPreset.name;
                        string newPresetFilename = foundFile;

                        if (existingPresetNames.Contains(newPresetName))
                        {
                            NewPresetName(newPresetName);
                            NewPresetFilename(newPresetFilename);
							
							if(showDebug)
                            Debug.Log("Saved old preset '" + foundPreset.name + "' to '" + newPresetName + "'.");

                            foundPreset.name = newPresetName;
                            foundPreset.Save(newPresetFilename);
                        }
						
						if(showDebug)
                        Debug.Log("Importing preset '" + newPresetName + "' to Preset List.");

                        parsedPresetCount++;

                        existingPresetNames.Add(newPresetName);
                    }
                }
            }
			
			if(showDebug)
            Debug.Log("Sorting presets alphabetically.");

            existingPresetNames.Sort();
			
			if(showDebug)
            Debug.Log("Saving merged list.");

            SaveList(existingPresetNames, presetListPath);

            EditorTools.Message_static("Merging Presets Finished!", "Finished importing " + parsedPresetCount + " presets.");
        }

        public static string NewPresetName(string currentName)
        {
            return currentName + "_old";
        }

        public static string NewPresetFilename(string currentFilename)
        {
            return currentFilename.Replace(".xml", "_old.xml");
        }

        public static void CreateWindow()
        {
            Editor_Glass_UpdateUtility window = ScriptableObject.CreateInstance(typeof(Editor_Glass_UpdateUtility)) as Editor_Glass_UpdateUtility;
            window.ShowUtility();
        }

        public static List<string> LoadList(string path, bool showDebug)
        {
            if (!File.Exists(path))
			{
				if(showDebug)
                	Debug.Log("(Preset List) File does not exists:" + path);
                return null;
            }

            List<string> loadedList = null;

            XmlSerializer xmlserialiser = new XmlSerializer(typeof(List<string>));
            FileStream fileStream = new FileStream(path, FileMode.Open);

            try
            {
                loadedList = xmlserialiser.Deserialize(fileStream) as List<string>;
            }
            catch (Exception e)
			{
				if(showDebug)
                	Debug.LogError("Glass,MergePresets: Error loading preset list XML: '" + path + "'" + e.Message);
            }

            fileStream.Close();

            return loadedList;
        }

        public static void SaveList(List<string> _list, string _path)
        {
            XmlSerializer xmlserialiser = new XmlSerializer(typeof(List<string>));
            FileStream fileStream = new FileStream(_path, FileMode.Create);
            xmlserialiser.Serialize(fileStream, _list);
            fileStream.Close();
        }

        public static List<string> FilesInFolder(string path)
        {
            DirectoryInfo pathInfo = new DirectoryInfo(path);
            List<string> filePaths = new List<string>();
            foreach (FileInfo fileInfo in pathInfo.GetFiles())
            {
                string filePath = fileInfo.FullName;
                if (filePath.Contains(".meta"))
                    continue;
                filePaths.Add(filePath);
            }
            return filePaths;
        }
    }

    #endregion

    #region AutoStart

    [ExecuteInEditMode]
    [InitializeOnLoad]
    public class Glass_AutoRenamer_AutoStart
    {
        public static string Glass_AutoRenamer_AutoStart_DisableString = "Glass_AutoRenamer_AutoStart_DisableString";

        static Glass_AutoRenamer_AutoStart()
        {
            if (!Application.isPlaying)
            {
                if (PlayerPrefs.GetInt(Glass_AutoRenamer_AutoStart_DisableString) != 1)
                {
                    CreateWindow();
                }
            }
        }

        public static void CreateWindow()
        {
            Editor_Glass_UpdateUtility window = ScriptableObject.CreateInstance(typeof(Editor_Glass_UpdateUtility)) as Editor_Glass_UpdateUtility;
            window.ShowUtility();
        }
    }

    #endregion

    #region Main Update Utility

    [ExecuteInEditMode]
    public class Editor_Glass_UpdateUtility : EditorWindow
    {
        #region Member Variables
        Glass_FileAutoRenamer renamer;
        EditorTools tools;
        bool showSection_AutoRenamer = false;
        bool showSection_advancedOptions = false;
        bool showSection_RetainPresets = false;
        Vector2 mainScroll = new Vector2();
        bool disableAutostart = false;
		bool presetDebugOutput = false;
        #endregion

        #region Init

        [MenuItem("Window/Glass Update Utility", false, 0)]
        static void Init()
        {
            Editor_Glass_UpdateUtility window = ScriptableObject.CreateInstance(typeof(Editor_Glass_UpdateUtility)) as Editor_Glass_UpdateUtility;
            window.ShowUtility();
        }

        #endregion

        #region Callbacks (OnEnable)

        void OnEnable()
        {
            disableAutostart = (PlayerPrefs.GetInt(Glass_AutoRenamer_AutoStart.Glass_AutoRenamer_AutoStart_DisableString) == 1);
        }

        #endregion

        #region GUI

        void OnGUI()
        {
            if (tools == null)
            {
                tools = new EditorTools("Glass Update Utility");
            }

            if (renamer == null)
            {
                renamer = new Glass_FileAutoRenamer();
            }

            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);

            tools.Label("Hi, Thanks for buying this version of Fantastic Glass!", true);

            tools.Label("\nIf this is your first time installing Fantastic Glass, you can skip using this utility:\n", true);

            tools.StartChangeCheck();
            tools.BoolOption("Stop showing this window", ref disableAutostart);
            if (tools.EndChangeCheck())
            {
                PlayerPrefs.SetInt(Glass_AutoRenamer_AutoStart.Glass_AutoRenamer_AutoStart_DisableString, disableAutostart ? 1 : 0);
            }
            if (disableAutostart)
            {
                tools.Label("(You can open this utility from 'Window' > 'Glass Update Utility')", true);
            }

            tools.Label("\nIf you are upgrading from an older version, these tools will help retain as much of the older assets and any presets you made as possible.\n", true);

            ShowSection_RetainPresets();

            ShowSection_AutoRenamer();

            EditorGUILayout.EndScrollView();
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                this.Close();
            }
        }

        void ShowSection_AutoRenamer()
        {
            if (tools.ShowSection("Asset Files", ref showSection_AutoRenamer))
            {
                tools.StartSection();

                tools.Label("This tool can help organise and rename new and old versions of Fantastic Glass assets.", true);

                tools.Label("\nOld assets will be given a suffix e.g. '_1_0_0' for assets from version 1.", true);

                tools.Label("\nWithout running this tool, the latest assets will instead retain a suffix.\n", true);

                tools.EndSection();

                if (tools.ShowSection("Advanced Options", ref showSection_advancedOptions))
                {
                    tools.BoolOption("Find New", ref renamer.findCurrent);
                    tools.BoolOption("Find Previous", ref renamer.findPrevious);

                    tools.Space();

                    tools.BoolOption("Save New", ref renamer.saveCurrent);
                    tools.BoolOption("Save Previous", ref renamer.savePrevious);

                    tools.Space();

                    tools.BoolOption("Delete (old) New", ref renamer.deleteCurrent);
                    tools.BoolOption("Delete (old) Previous", ref renamer.deletePrevious);

                    tools.Space();

                    tools.BoolOption("Ignore Meta Files", ref renamer.ignoreMetaFiles);

                    tools.Space();

                    tools.BoolOption("Print Debug Info", ref renamer.printDebug);

                    tools.EndSection();
                }

                tools.StartSection();

                tools.Label("\nTo replace existing assets with the latest (without copies/suffixes), untick 'Save Previous' in the Advanced Options.", true);

                tools.EndSection();

                if (tools.Button("Execute AutoRename"))
                {
                    //renamer.Init();
                    renamer.FindAndRenameAllFiles();
                    tools.Message("Renaming Complete!", "Renaming Complete!");
                }

                tools.EndSection();
            }

        }

        void ShowSection_RetainPresets()
        {
            if (tools.ShowSection("Presets", ref showSection_RetainPresets))
            {
                if (tools.Button("Import Old / Exisiting Presets"))
                {
					Glass_PresetMerger.MergePresets(presetDebugOutput);
                }

				tools.BoolOption("Print Debug Data", ref presetDebugOutput);

                tools.EndSection();
            }
        }

        #endregion

    }

    #endregion
}

#endif
