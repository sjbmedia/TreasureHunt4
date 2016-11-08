#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Xml.Serialization;
using System.IO;

namespace FantasticGlass
{

    /// <summary>
    /// Glass editor.
    /// NOTE: Do not place this file in an 'Editor' folder as it will break.
    /// </summary>
    [CustomEditor(typeof(Glass))]
    [CanEditMultipleObjects]
    public class Glass_Editor : Editor
    {
        #region Member Variables

        EditorTools tools;

        Glass_GlassManager_Settings managerSettings = null;

        Glass glass;
        List<Glass> glassList = new List<Glass>();

        bool showSection_Presets;
        bool showSection_Materials;
        bool showSection_SharedGlass;
        bool showSection_Shader;
        bool showSection_Depth;
        bool showSection_ImportantNotes;
        bool showSection_Albedo;
        bool showSection_AboutPresetName;
        bool showSection_Distortion;
        bool showBump;
        bool showSection_Fog;
        bool showSection_Extinction;
        bool showSection_Aberration;
        bool showSection_AboutSharedGlass;
        bool showSection_Physics;
        bool showSection_Physics_AboutZFighting;
        bool showSection_Surface;
        bool showSection_Textures;

        bool showList_SharedGlass;
        List<Glass> glassOthers = null;

        Material tempMatFront;
        Material tempMatBack;
        //bool materialChangeDetected;

        string[] textureAALabels = new string[] { "None", "2 Samples", "4 Samples", "8 Samples" };
        int[] textureAAItems = new int[] { 1, 2, 4, 8 };
        int textureAAIndex = 0;

        Vector2 scrollSharedGlass = new Vector2();

        string presetName;
        GlassPreset currentPreset;
        string selectedPresetName;
        int selectedPresetIndex = -1;
        List<string> presetList;
        bool showPresetList = true;

        string packagePath = "Fantastic Glass";
        string xmlPath = "XML";
        string presetListPath = "glass_preset_list";
        string materialsPath = "Materials";

        bool editorWasDisabled = false;

        #endregion

        #region Callbacks (Enable, Disable)

        void OnEnable()
        {
            if (glass == null)
                glass = (Glass)target;

            if (tools == null)
                tools = new EditorTools("Glass_" + glass.presetName + glass.name + glass.gameObject.GetInstanceID().ToString());

            if (glass == null)
                return;

            if (glass.manager == null)
                glass.FindGlassManager();

            if (!glass.manager.LayersExist())
                GlassManager_Editor.Show_LayersWarning();

            if (!packagePath.Contains(Application.dataPath))
            {
                packagePath = Application.dataPath + "/" + packagePath + "/";
                xmlPath = packagePath + xmlPath + "/";
                presetListPath = xmlPath + presetListPath + ".xml";
                materialsPath = packagePath + materialsPath + "/";
                materialsPath = FileUtil.GetProjectRelativePath(materialsPath);
            }

            if (presetList == null)
                LoadPresetList();
            presetName = glass.presetName;
            selectedPresetIndex = presetList.IndexOf(presetName);

            glass.UpdateGlassManager();

            UpdateOtherGlassList();

            LoadGlassManagerSettings();
        }

        void LoadGlassManagerSettings()
        {
            managerSettings = Glass_GlassManager_Settings.Load(xmlPath, glass);
            if (glass.manager != null)
            {
                glass.manager.LoadManager();
            }
            UpdateTextureAAIndex();
        }

        void OnDisable()
        {
            editorWasDisabled = true;
        }

        #endregion

        #region GUI

        public override void OnInspectorGUI()
        {
            UpdateTarget();

            UpdateTargetList();

            UpdateEditorTools();

            FindGlassManager();

            if (editorWasDisabled)
            {
                LoadPresetList();
                editorWasDisabled = false;
            }

            Section_GlassName();
            Section_Presets();
            Section_SharedGlass();
            Section_ShaderOptions();
            Section_Textures();
            Section_PhysicsOptions();
            Section_Materials();
            Section_ManagerSettings();
            Section_ImportantNotes();
        }

        void UpdateTarget()
        {
            glass = (Glass)target;
        }

        void UpdateTargetList()
        {
            glassList.Clear();
            if (targets != null)
            {
                foreach (UnityEngine.Object targetIter in targets)
                {
                    Glass glassIter = targetIter as Glass;
                    if (glassIter != null)
                    {
                        if (!glassList.Contains(glassIter))
                            glassList.Add(glassIter);
                    }
                }
            }
        }

        void UpdateEditorTools()
        {
            if (tools == null)
                tools = new EditorTools("Glass_" + glass.name);
        }

        void FindGlassManager()
        {
            if (glass.manager == null)
                glass.FindGlassManager();
        }

        #endregion

        #region Section GlassName

        void Section_GlassName()
        {
            string glassName = glass.presetName;
            if (GlassNamesDiffer())
            {
                if (glassList.Count > 1)
                {
                    glassName = "";
                }
            }

            tools.StartChangeCheck();
            tools.LabelOption("Glass Name", ref glassName);

            if (tools.EndChangeCheck())
            {
                glass.presetName = glassName;
                glass.manager.FindGlass();
                foreach (Glass glassIter in glassList)
                {
                    glassIter.presetName = glassName;
                }
                UpdateOtherGlassList();
                tools.SetDirty(glass);
            }

            if (tools.ShowSection("(About Glass Name)", ref showSection_AboutPresetName))
            {
                tools.Label("The settings of Glass objects with matching Glass Names can be synchronised.\nSynchronisation settings are found in the GlassManager and Glass creator.", true);
                tools.EndSection();
            }
        }

        #endregion

        #region Section Presets

        void Section_Presets()
        {
            if (presetList == null)
            {
                LoadPresetList();
            }

            tools.StartChangeCheck();

            if (tools.ShowSection("Presets", ref glass.showSection_Presets))
            {
                if (tools.EndChangeCheck())
                {
                    presetName = glass.presetName;

                    selectedPresetIndex = presetList.IndexOf(presetName);
                }

                tools.Label("Save");

                tools.StringOption("Name", ref presetName, true);

                if (tools.Button("Save"))
                {
                    if (Application.isPlaying)
                    {
                        SavePreset(presetName);
                        Debug.Log("Saved settings as '" + presetName + ". Confirmation dialogue skipped as not displayable during Play. Load this preset after Play to retain these settings.");
                    }
                    else if (PresetExists(presetName))
                    {
                        if (tools.Message("Preset Exists", "A preset named '" + glass.presetName + "' aready exists.\n\nWould you like to replace it?", "Yes", "Cancel"))
                        {
                            SavePreset(presetName);
                            tools.Message("Preset Settings Saved", "Saved settings to preset '" + presetName + ".");
                        }
                    }
                    else {
                        SavePreset(presetName);
                        tools.Message("Preset Settings Saved", "Saved settings to preset '" + presetName + ".");
                    }
                }

                Section_Save_CopyList();

                tools.Label("Load");

                switch (tools.PresetList("Loadable Presets", presetListPath, ref selectedPresetName, ref selectedPresetIndex, ref presetList, ref showPresetList, true, false, false, false))
                {
                    case EditorToolsPreset_Option.LoadItem:
                        if (Application.isPlaying)
                        {
                            LoadPreset(selectedPresetName);
                            Debug.Log("Loaded settings from preset '" + selectedPresetName + ". Confirmation dialogue skipped as not displayable during Play. Load again after Play to retain these settings.");
                        }
                        else
                        {
                            LoadPreset(selectedPresetName);
                            tools.Message("Preset Settings Loaded", "Loaded settings from preset '" + selectedPresetName + ".");
                            presetName = glass.presetName;
                        }
                        break;
                    default:
                        break;
                }

                Section_Load_CopyList();

                tools.EndSection();
            }
        }

        void Section_Save_CopyList()
        {
            if (!tools.BoolOption("Save" + " Everything", ref glass.saveCopyList.everything))
            {
                tools.StartSection();
                tools.BoolOption("Colour", ref glass.saveCopyList.albedo);
                tools.BoolOption("Distortion", ref glass.saveCopyList.distortion);
                tools.BoolOption("Bump", ref glass.saveCopyList.bump);
                tools.BoolOption("Extinction", ref glass.saveCopyList.extinction);
                tools.BoolOption("Aberration", ref glass.saveCopyList.aberration);
                tools.BoolOption("Fog", ref glass.saveCopyList.fog);
                tools.BoolOption("Surface", ref glass.saveCopyList.surface);
                tools.BoolOption("Mesh", ref glass.saveCopyList.model);
                tools.BoolOption("Depth", ref glass.saveCopyList.depth);
                tools.BoolOption("Z Fight Radius", ref glass.saveCopyList.zFightRadius);
                tools.EndSection();
            }
        }

        void Section_Load_CopyList()
        {
            if (!tools.BoolOption("Load" + " Everything", ref glass.loadCopyList.everything))
            {
                tools.StartSection();
                tools.BoolOption("Colour", ref glass.loadCopyList.albedo);
                tools.BoolOption("Distortion", ref glass.loadCopyList.distortion);
                tools.BoolOption("Bump", ref glass.loadCopyList.bump);
                tools.BoolOption("Extinction", ref glass.loadCopyList.extinction);
                tools.BoolOption("Aberration", ref glass.loadCopyList.aberration);
                tools.BoolOption("Fog", ref glass.loadCopyList.fog);
                tools.BoolOption("Surface", ref glass.loadCopyList.surface);
                //tools.BoolOption("Mesh", ref glass.loadCopyList.model);
                tools.BoolOption("Depth", ref glass.loadCopyList.depth);
                tools.BoolOption("Z Fight Radius", ref glass.loadCopyList.zFightRadius);
                tools.EndSection();
            }
        }

        bool PresetExists(string presetName)
        {
            return presetList.Contains(presetName);
        }

        string SelectedPresetFilename()
        {
            return xmlPath + selectedPresetName + ".xml";
        }

        string NewPresetFilename()
        {
            return xmlPath + presetName + ".xml";
        }

        void SavePreset(string presetName)
        {
            currentPreset = glass.GeneratePreset(glass.saveCopyList);
            currentPreset.name = presetName;
            currentPreset.Save(NewPresetFilename());
            if (!presetList.Contains(presetName))
            {
                presetList.Add(presetName);
                SavePresetList();
                LoadPresetList();
            }
            selectedPresetIndex = presetList.IndexOf(presetName);
            selectedPresetName = presetName;
            LoadPreset(presetName);
        }

        void LoadPreset(string presetName)
        {
            currentPreset = GlassPreset.LoadFromXML(SelectedPresetFilename()) as GlassPreset;

            if (currentPreset != null)
            {
                glass.LoadFromPreset(currentPreset, glass.loadCopyList);
                glass.manager.GlassModified(glass);
                tools.SetDirty(glass);
            }
            else {
                Debug.LogError("Preset does not exist: " + presetName);
            }
        }

        void SavePresetList()
        {
            SaveList(presetList, presetListPath);
        }

        public void SaveList(List<string> _list, string _path)
        {
            XmlSerializer xmlserialiser = new XmlSerializer(typeof(List<string>));
            FileStream fileStream = new FileStream(_path, FileMode.Create);
            xmlserialiser.Serialize(fileStream, _list);
            fileStream.Close();
        }

        void LoadPresetList()
        {
            presetList = LoadList(presetListPath);
        }

        public List<string> LoadList(string _path)
        {
            if (!File.Exists(_path))
            {
                Debug.Log("(Preset List) File does not exists:" + _path);
                return null;
            }
            XmlSerializer xmlserialiser = new XmlSerializer(typeof(List<string>));
            FileStream fileStream = new FileStream(_path, FileMode.Open);
            List<string> loadedList = xmlserialiser.Deserialize(fileStream) as List<string>;
            fileStream.Close();
            return loadedList;
        }

        #endregion

        #region Section SharedGlass

        private void Section_SharedGlass()
        {
            //if (glass.manager.GlassIsShared(glass))
            if (glassOthers == null)
                UpdateOtherGlassList();
            if (glassOthers.Count > 1)
            {
                tools.StartChangeCheck();
                if (tools.ShowSection("Shared Glass (" + glassOthers.Count + ")", ref glass.showSection_SharedGlass))
                {
                    if (tools.ShowSection("About Shared Glass", ref showSection_AboutSharedGlass))
                    {
                        tools.Label("The settings for Glass objects that match are synchronised.\nThe way in which Glass is matched is defined in the GlassManager and the 'GameObject/Create Other/Glass...' function.", true);
                        tools.EndSection();
                    }
                    tools.GUI_List("Objects sharing these Glass settings (" + glassOthers.Count + ")", ref glassOthers, ref showList_SharedGlass, ref scrollSharedGlass);
                    tools.EndSection();
                }
                if (tools.EndChangeCheck())
                {
                    UpdateOtherGlassList();
                }
            }
        }

        #endregion

        #region Section Other Glass List

        void UpdateOtherGlassList()
        {
            glassOthers = glass.manager.SharedGlassOthers(glass);
        }

        #endregion

        #region Settings

        void Section_ShaderOptions()
        {
            if (tools.ShowSection("Settings", ref glass.showSection_Shader))
            {
                Section_Albedo();
                Section_Distortion();
                Section_Extinction();
                Section_Aberration();
                Section_Fog();
                Section_Depth();
                Section_Surface();
                tools.EndSection();
            }
        }

        #endregion

        #region Settings - Albedo

        void Section_Albedo()
        {
            tools.StartChangeCheck();
            if (tools.ShowSection("Albedo", ref glass.showSection_Albedo))
            {
                tools.ColourOption("Base Colour", ref glass.colour_albedo);
                tools.TextureOption("Texture", ref glass.texture_albedo);
                if (glass.texture_albedo != null)
                {
                    tools.ColourOption("Texture Colour", ref glass.colour_albedoTexture);
                }
                tools.FloatOption("Opacity", ref glass.opacity);
                tools.EndSection();
            }
            if (tools.EndChangeCheck())
            {
                glass.UpdateTexturesAndColours();
                tools.SetDirty(glass);
            }
        }

        #endregion

        #region Settings - Distortion

        void Section_Distortion()
        {
            if (tools.ShowSection("Distortion", ref glass.showSection_Distortion))
            {
                tools.BeginChangeCheck();
                tools.TextureOption("Texture", ref glass.texture_distortion);

                if (tools.BoolOption("Front", ref glass.enableDistortion_front))
                {
                    tools.StartSection();
                    if (glass.texture_distortion != null)
                    {
                        tools.FloatOption("Bump", ref glass.distortion_front.x);
                        /*
                        if (glass.distortion_front.x != 0f)
                        {
                            tools.BoolOption("Double Depth Test", ref glass.enableDoubleDepth_front);
                        }
                        */
                    }
                    tools.BoolOption("Detailed Depth", ref glass.enableDoubleDepth_front);
                    tools.FloatOption("Mesh", ref glass.distortion_front.y);
                    tools.FloatOption("Overall", ref glass.distortion_front.z);
                    tools.FloatOption("Max", ref glass.distortion_front.w);
                    tools.FloatOption("Edge Bend", ref glass.distortionEdgeBend_front);
                    tools.FloatOption("Depth Fade", ref glass.distortionDepthFade_front);
                    tools.EndSection();
                }

                if (tools.BoolOption("Back", ref glass.enableDistortion_back))
                {
                    tools.StartSection();
                    if (glass.texture_distortion != null)
                    {
                        tools.FloatOption("Bump", ref glass.distortion_back.x);
                        /*
                        if (glass.distortion_back.x != 0f)
                        {
                            tools.BoolOption("Double Depth Test", ref glass.enableDoubleDepth_back);
                        }
                        */
                    }
                    tools.BoolOption("Detailed Depth", ref glass.enableDoubleDepth_back);
                    tools.FloatOption("Mesh", ref glass.distortion_back.y);
                    tools.FloatOption("Overall", ref glass.distortion_back.z);
                    tools.FloatOption("Max", ref glass.distortion_back.w);
                    tools.FloatOption("Edge Bend", ref glass.distortionEdgeBend_back);
                    tools.FloatOption("Depth Fade", ref glass.distortionDepthFade_back);
                    tools.EndSection();
                }

                EditorGUILayout.Space();

                /*
                if (glass.texture_distortion != null)
                {
                    if ((glass.distortion_back.x != 0f) || (glass.distortion_front.x != 0f))
                    {
                        if (tools.ShowSection("Detailed Depth?", ref showSection_AboutDoubleDepth))
                        {
                            tools.Label("In short, when enabled, the effects will look more accurate. The second depth test allows the use of the distorted volume for extinction, aberration, and fog. However, when combined with high bump distortion, it can lead to edges bleeding/ghosting; try disabling when this occurs.", true);
                            tools.EndSection();
                        }
                    }
                }
                */
                if (tools.ShowSection("Detailed Depth?", ref glass.showSection_AboutDoubleDepth))
                {
                    tools.Label("In short, when enabled, the effects will look more accurate. The second depth test allows the use of the distorted volume for extinction, aberration, and fog. However, when combined with high bump distortion, it can lead to edges bleeding/ghosting; try disabling when this occurs.", true);
                    tools.EndSection();
                }

                if (tools.EndChangeCheck())
                {
                    /*
                    if (glass.texture_distortion == null)
                    {
                        glass.distortion_back.x = glass.distortion_front.x = 0f;
                    }
                    */
                    glass.UpdateDistortion();
                    tools.SetDirty(glass);
                }

                tools.EndSection();
            }

            if (tools.ShowSection("Bump", ref showBump))
            {
                tools.StartChangeCheck();

                tools.TextureOption("Texture", ref glass.texture_distortion);
                if (glass.texture_distortion != null)
                {
                    tools.FloatOption("Front", ref glass.bumpFront);
                    tools.FloatOption("Back", ref glass.bumpBack);
                }

                if (tools.EndChangeCheck())
                {
                    glass.UpdateBump();
                    tools.SetDirty(glass);
                }

                tools.EndSection();
            }

        }

        #endregion

        #region settings - physics

        void Section_PhysicsOptions()
        {
            if (tools.ShowSection("Physics Options", ref glass.showSection_Physics))
            {

                tools.StartChangeCheck();

                tools.FloatOption("Z Fighting Fix Magnitude", ref glass.zFightRadius);

                if (tools.Button("Revert to Default"))
                {
                    glass.zFightRadius = Glass.default_zFightRadius;
                }

                if (tools.EndChangeCheck())
                {
                    if (tools.Message("Physics Options Changed", "Woud you like to apply these changes to all matching Glass objects?", "Yes", "No"))
                    {
                        glass.UpdatePhysics();
                        tools.SetDirty(glass);
                    }
                }

                if (tools.ShowSection("About Z Fighting Fix", ref showSection_Physics_AboutZFighting))
                {
                    tools.Label("Z-fighting may occur if physical objects intersect.\nThis fix involves expanding any collider attached to the Glass object by the small set magnitude.\nThe default value should be small enough to fix z-fighting without being noticable.");
                    tools.EndSection();
                }

                tools.EndSection();
            }
        }

        #endregion

        #region settings - Extinction

        void Section_Extinction()
        {
            if (tools.ShowSection("Extinction", ref glass.showSection_Extinction))
            {
                tools.StartChangeCheck();

                glass.extinctionAppearance = (GlassExtinctionAppearance)tools.EnumOption("Extinction Appearance", glass.extinctionAppearance);

                Section_Extinction_Front();
                Section_Extinction_Back();
                Section_Extinction_Both();

                tools.EndSection();

                if (tools.EndChangeCheck())
                {
                    glass.UpdateExtinction();
                    tools.SetDirty(glass);
                }
            }
        }

        void Section_Extinction_Front()
        {
            if (glass.enableExtinction_both)
                return;

            tools.StartChangeCheck();

            if (tools.BoolOption("Front (Default)", ref glass.enableExtinction_front))
            {

                tools.StartSection();
                Section_Extinction_Front_Options();
                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Extinction = GlassFace.front;
            }
        }

        void Section_Extinction_Back()
        {
            if (glass.enableExtinction_both)
                return;

            tools.StartChangeCheck();

            if (tools.BoolOption("Back", ref glass.enableExtinction_back))
            {
                tools.StartSection();
                Section_Extinction_Back_Options();
                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Extinction = GlassFace.back;
            }
        }

        void Section_Extinction_Front_Options()
        {
            switch (glass.extinctionAppearance)
            {
                case GlassExtinctionAppearance.AsItAppears:
                    tools.ColourOption("Colour (as it appears)", ref glass.extinctionFlipped_front);
                    glass.extinction_front = GetExtinctionColour(GlassFace.front);
                    break;
                case GlassExtinctionAppearance.AsApplied:
                    tools.ColourOption("Colour (extinction)", ref glass.extinction_front);
                    glass.extinctionFlipped_front = GetExtinctionColour_Flipped(GlassFace.front);
                    break;
            }

            tools.FloatOption("Intensity", ref glass.extinctionMagnitude_front.x, -10f, 10f);
            tools.FloatOption("Minimum", ref glass.extinctionMagnitude_front.y, -10f, 10f);
            tools.FloatOption("Maximum", ref glass.extinctionMagnitude_front.z, -10f, 10f);

            tools.BoolOption("Cap (min,max)", ref glass.capExtinction_front);

            tools.TextureOption("Texture", ref glass.texture_extinction_front);
        }

        void Section_Extinction_Back_Options()
        {
            switch (glass.extinctionAppearance)
            {
                case GlassExtinctionAppearance.AsItAppears:
                    tools.ColourOption("Colour (as it appears)", ref glass.extinctionFlipped_back);
                    glass.extinction_back = GetExtinctionColour(GlassFace.back);
                    break;
                case GlassExtinctionAppearance.AsApplied:
                    tools.ColourOption("Colour (extinction)", ref glass.extinction_back);
                    glass.extinctionFlipped_back = GetExtinctionColour_Flipped(GlassFace.back);
                    break;
            }

            tools.FloatOption("Intensity", ref glass.extinctionMagnitude_back.x, -10f, 10f);
            tools.FloatOption("Minimum", ref glass.extinctionMagnitude_back.y, -10f, 10f);
            tools.FloatOption("Maximum", ref glass.extinctionMagnitude_back.z, -10f, 10f);

            tools.BoolOption("Cap (min,max)", ref glass.capExtinction_back);

            tools.TextureOption("Texture", ref glass.texture_extinction_back);
        }

        void Section_Extinction_Both()
        {
            tools.StartChangeCheck();

            if (tools.BoolOption("Both (matching)", ref glass.enableExtinction_both))
            {
                tools.StartSection();

                switch (glass.lastFaceEdited_Extinction)
                {
                    case GlassFace.front:
                        Section_Extinction_Front_Options();
                        break;
                    case GlassFace.back:
                        Section_Extinction_Back_Options();
                        break;
                }

                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                if (glass.enableExtinction_both)
                    SynchroniseExtinctionFaces();
            }
        }

        /// <summary>
        /// Copies values from the last edited face to its opposite. Called only when both faces' values are linked.
        /// </summary>
        void SynchroniseExtinctionFaces()
        {
            switch (glass.lastFaceEdited_Extinction)
            {
                case GlassFace.front:
                    glass.extinction_back = glass.extinction_front;
                    glass.extinctionMagnitude_back = glass.extinctionMagnitude_front;
                    glass.extinctionFlipped_back = glass.extinctionFlipped_front;
                    glass.texture_extinction_back = glass.texture_extinction_front;
                    glass.capExtinction_back = glass.capExtinction_front;
                    break;
                case GlassFace.back:
                    glass.extinction_front = glass.extinction_back;
                    glass.extinctionMagnitude_front = glass.extinctionMagnitude_back;
                    glass.extinctionFlipped_front = glass.extinctionFlipped_back;
                    glass.texture_extinction_front = glass.texture_extinction_back;
                    glass.capExtinction_front = glass.capExtinction_back;
                    break;
            }
        }

        #endregion

        #region Settings - Aberration

        void Section_Aberration()
        {
            if (tools.ShowSection("Aberration", ref glass.showSection_Aberration))
            {
                tools.StartChangeCheck();

                Section_Aberration_Front();
                Section_Aberration_Back();
                Section_Aberration_Both();

                tools.EndSection();

                if (tools.EndChangeCheck())
                {
                    glass.UpdateAberration();
                    tools.SetDirty(glass);
                }
            }
        }

        void Section_Aberration_Front()
        {
            if (glass.enableAberration_both)
                return;

            if (tools.BoolOption("Front (Default)", ref glass.enableAberration_front))
            {
                tools.StartSection();
                Section_Aberration_Front_Options();
                tools.EndSection();
            }
        }

        void Section_Aberration_Back()
        {
            if (glass.enableAberration_both)
                return;

            if (tools.BoolOption("Back", ref glass.enableAberration_back))
            {
                tools.StartSection();
                Section_Aberration_Front_Options();
                tools.EndSection();
            }
        }

        void Section_Aberration_Both()
        {
            tools.StartChangeCheck();

            if (tools.BoolOption("Both (matching)", ref glass.enableAberration_both))
            {
                tools.StartSection();

                switch (glass.lastFaceEdited_Aberration)
                {
                    case GlassFace.front:
                        Section_Aberration_Front_Options();
                        break;
                    case GlassFace.back:
                        Section_Aberration_Back_Options();
                        break;
                }

                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                SynchroniseAberrationFaces();
            }
        }

        void Section_Aberration_Front_Options()
        {
            tools.StartChangeCheck();

            tools.ColourOption("Colour (spread)", ref glass.aberration_front);
            tools.FloatOption("Intensity", ref glass.aberrationMagnitude_front.x, -10f, 10f);
            tools.FloatOption("Minimum", ref glass.aberrationMagnitude_front.y, -10f, 10f);
            tools.FloatOption("Maximum", ref glass.aberrationMagnitude_front.z, -10f, 10f);
            tools.TextureOption("Texture", ref glass.texture_aberration_front);

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Aberration = GlassFace.front;
            }
        }

        void Section_Aberration_Back_Options()
        {
            tools.StartChangeCheck();

            tools.ColourOption("Colour (spread)", ref glass.aberration_back);
            tools.FloatOption("Intensity", ref glass.aberrationMagnitude_back.x, -10f, 10f);
            tools.FloatOption("Minimum", ref glass.aberrationMagnitude_back.y, -10f, 10f);
            tools.FloatOption("Maximum", ref glass.aberrationMagnitude_back.z, -10f, 10f);
            tools.TextureOption("Texture", ref glass.texture_aberration_back);

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Aberration = GlassFace.back;
            }
        }

        /// <summary>
        /// Copies values from the last edited face to its opposite. Called only when both faces' values are linked.
        /// </summary>
        void SynchroniseAberrationFaces()
        {
            switch (glass.lastFaceEdited_Aberration)
            {
                case GlassFace.front:
                    glass.aberration_back = glass.aberration_front;
                    glass.aberrationMagnitude_back = glass.aberrationMagnitude_front;
                    glass.texture_aberration_back = glass.texture_aberration_front;
                    break;
                case GlassFace.back:
                    glass.aberration_front = glass.aberration_back;
                    glass.aberrationMagnitude_front = glass.aberrationMagnitude_back;
                    glass.texture_aberration_front = glass.texture_aberration_back;
                    break;
            }
        }

        #endregion

        #region Settings - Fog

        void Section_Fog()
        {
            if (tools.ShowSection("Fog", ref glass.showSection_Fog))
            {
                tools.StartChangeCheck();

                Section_Fog_Front();
                Section_Fog_Back();
                Section_Fog_Both();

                tools.EndSection();

                if (tools.EndChangeCheck())
                {
                    glass.UpdateFog();
                    tools.SetDirty(glass);
                }
            }
        }

        void Section_Fog_Front()
        {
            if (glass.enableFog_both)
                return;

            if (tools.BoolOption("Front", ref glass.enableFog_front))
            {
                Section_Fog_Front_Options();
            }
        }

        void Section_Fog_Back()
        {
            if (glass.enableFog_both)
                return;

            if (tools.BoolOption("Back", ref glass.enableFog_back))
            {
                Section_Fog_Back_Options();
            }
        }

        void Section_Fog_Both()
        {
            tools.StartChangeCheck();

            if (tools.BoolOption("Both (matching)", ref glass.enableFog_both))
            {

                tools.StartSection();

                switch (glass.lastFaceEdited_Fog)
                {
                    case GlassFace.front:
                        Section_Fog_Front_Options();
                        break;
                    case GlassFace.back:
                        Section_Fog_Back_Options();
                        break;
                }

                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                SynchroniseFogFaces();
            }
        }

        void Section_Fog_Front_Options()
        {
            tools.StartChangeCheck();

            tools.StartSection();
            tools.ColourOption("Near", ref glass.fogNear_front);
            tools.ColourOption("Far", ref glass.fogFar_front);
            tools.FloatOption("Magnitude", ref glass.fogMagnitude_front);
            tools.EndSection();

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Fog = GlassFace.front;
            }
        }

        void Section_Fog_Back_Options()
        {
            tools.StartChangeCheck();

            tools.StartSection();
            tools.ColourOption("Near", ref glass.fogNear_back);
            tools.ColourOption("Far", ref glass.fogFar_back);
            tools.FloatOption("Magnitude", ref glass.fogMagnitude_back);
            tools.EndSection();

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Fog = GlassFace.front;
            }
        }

        /// <summary>
        /// Copies values from the last edited face to its opposite. Called only when both faces' values are linked.
        /// </summary>
        void SynchroniseFogFaces()
        {
            switch (glass.lastFaceEdited_Fog)
            {
                case GlassFace.front:
                    glass.fogNear_back = glass.fogNear_front;
                    glass.fogFar_back = glass.fogFar_front;
                    glass.fogMagnitude_back = glass.fogMagnitude_front;
                    break;
                case GlassFace.back:
                    glass.fogNear_front = glass.fogNear_back;
                    glass.fogFar_front = glass.fogFar_back;
                    glass.fogMagnitude_front = glass.fogMagnitude_back;
                    break;
            }
        }

        #endregion

        #region Settings - Depth

        private void Section_Depth()
        {
            if (tools.ShowSection("Depth", ref glass.showSection_Depth))
            {
                tools.StartChangeCheck();

                tools.FloatOption("Depth Multiplier", ref glass.depthMultiplier);
                tools.FloatOption("Depth Offset", ref glass.depthOffset);

                if (tools.EndChangeCheck())
                {
                    glass.UpdateDepth();
                    tools.SetDirty(glass);
                }

                tools.EndSection();
            }
        }

        #endregion

        #region settings - surface

        private void Section_Surface()
        {
            if (tools.ShowSection("Surface", ref glass.showSection_Surface))
            {
                tools.StartChangeCheck();

                tools.StartEdit(glass, "Changing Surface");

                tools.Label("Front");
                EditorGUILayout.Space();
                tools.StartSection();
                AmountTexture("Glossiness", ref glass.ShowSection_Gloss_Front, ref glass.glossiness_front, ref glass.texture_gloss_front, 0f, 1f);
                AmountTexture("Metallic", ref glass.ShowSection_Metal_Front, ref glass.metallic_front, ref glass.texture_metal_front, 0f, 1f);
                AmountTexture("Glow", ref glass.ShowSection_Glow_Front, ref glass.glow_front, ref glass.texture_glow_front, -100f, 100f);
                tools.EndSection();

                EditorGUILayout.Space();

                tools.Label("Back");
                EditorGUILayout.Space();
                tools.StartSection();
                AmountTexture("Glossiness", ref glass.ShowSection_Gloss_Back, ref glass.glossiness_back, ref glass.texture_gloss_back, 0f, 1f);
                AmountTexture("Metallic", ref glass.ShowSection_Metal_Back, ref glass.metallic_back, ref glass.texture_metal_back, 0f, 1f);
                AmountTexture("Glow", ref glass.ShowSection_Glow_Back, ref glass.glow_back, ref glass.texture_glow_back, -100f, 100f);
                tools.EndSection();

                if (tools.EndChangeCheck())
                {
                    glass.UpdateSurface();
                }

                tools.EndEdit(glass);

                tools.EndSection();
            }
        }

        void AmountTexture(string label, ref bool show, ref float amount, ref Texture texture, float min, float max)
        {
            if (tools.ShowSection(label, ref show))
            {
                tools.FloatOption("Amount", ref amount, min, max);
                tools.TextureOption("Texture", ref texture);
                tools.EndSection();
            }
        }

        #endregion

        #region Textures

        void Section_Textures()
        {
            if (tools.ShowSection("Textures", ref glass.showSection_Textures))
            {
                Section_Textures_Dimensions();

                tools.HorizontalLine();

                tools.StartEdit(glass, "Changing Textures");

                if (ShowTextures("Albedo", ref glass.showTextures_Albedo, ref glass.texture_albedo))
                    glass.UpdateTexturesAndColours();
                if (ShowTextures("Distortion/Bump", ref glass.showTextures_Distortion, ref glass.texture_distortion))
                    glass.UpdateDistortion();
                if (ShowTextures("Extinction", ref glass.showTextures_Extinction, ref glass.linkTextures_Extinction, ref glass.changedTexture_Extinction, ref glass.texture_extinction_front, ref glass.texture_extinction_back))
                    glass.UpdateExtinction();
                if (ShowTextures("Aberration", ref glass.showTextures_Aberration, ref glass.linkTextures_Aberration, ref glass.changedTexture_Aberration, ref glass.texture_aberration_front, ref glass.texture_aberration_back))
                    glass.UpdateAberration();
                if (ShowTextures("Glossiness", ref glass.showTextures_Glossiness, ref glass.linkTextures_Glossiness, ref glass.changedTexture_Glossiness, ref glass.texture_gloss_front, ref glass.texture_gloss_back))
                    glass.UpdateSurface();
                if (ShowTextures("Metallic", ref glass.showTextures_Metallic, ref glass.linkTextures_Metallic, ref glass.changedTexture_Metallic, ref glass.texture_metal_front, ref glass.texture_metal_back))
                    glass.UpdateSurface();
                if (ShowTextures("Glow", ref glass.showTextures_Glow, ref glass.linkTextures_Glow, ref glass.changedTexture_Glow, ref glass.texture_glow_front, ref glass.texture_glow_back))
                    glass.UpdateSurface();

                tools.FinishEdit(glass);

                tools.EndSection();
            }

        }

        bool ShowTextures(string label, ref bool show, ref Texture texture1)
        {
            bool changeOccurred = false;
            if (tools.ShowSection(label, ref show))
            {
                tools.StartChangeCheck();
                tools.TextureOption("Front & Back", ref texture1);
                if (tools.EndChangeCheck())
                {
                    changeOccurred = true;
                }
                tools.EndSection();
            }
            return changeOccurred;
        }

        private bool ShowTextures(string label, ref bool show, ref bool matchingTextures, ref int lastChanged, ref Texture frontTexture, ref Texture backTexture)
        {
            bool changeOccurred = false;
            if (tools.ShowSection(label, ref show))
            {
                if (matchingTextures)
                {
                    if (lastChanged == 0)
                    {
                        tools.StartChangeCheck();
                        tools.TextureOption("Front & Back", ref frontTexture);
                        if (tools.EndChangeCheck())
                        {
                            backTexture = frontTexture;
                            changeOccurred = true;
                        }
                    }
                    else
                    {
                        tools.StartChangeCheck();
                        tools.TextureOption("Front & Back", ref backTexture);
                        if (tools.EndChangeCheck())
                        {
                            frontTexture = backTexture;
                            changeOccurred = true;
                        }
                    }
                }
                else
                {
                    tools.StartChangeCheck();
                    tools.TextureOption("Front", ref frontTexture);
                    if (tools.EndChangeCheck())
                    {
                        lastChanged = 0;
                        changeOccurred = true;
                    }

                    tools.StartChangeCheck();
                    tools.TextureOption("Back", ref backTexture);
                    if (tools.EndChangeCheck())
                    {
                        lastChanged = 1;
                        changeOccurred = true;
                    }
                }

                tools.BoolOption("Matching Textures", ref matchingTextures);

                tools.EndSection();
            }

            return changeOccurred;
        }

        void UpdateTextureDimensions()
        {
            glass.UpdateTextureDimensions();
        }

        void Section_Textures_Dimensions()
        {
            if (tools.ShowSection("Dimensions", ref glass.showSection_Texture_Dimensions))
            {
                tools.Label("It is highly recommended that all textures have the same dimensions.", true);
                tools.Label("Use this section to find out the dimensions you are currently using.", true);

                if (glass.textureDimensions.Count == 0)
                {
                    tools.HorizontalLine();
                    tools.Label("No Textures Found");
                }

                foreach (Vector2 textureDimension in glass.textureDimensions)
                {
                    tools.Label(textureDimension.ToString());
                    tools.StartSection();
                    foreach (string textureName in glass.allTextures.Keys)
                    {
                        Texture texture = glass.allTextures[textureName];
                        if (texture != null)
                        {
                            Vector2 textureSize = new Vector2(texture.width, texture.height);
                            if (textureSize.Equals(textureDimension))
                            {
                                tools.TextureOption(textureName, ref texture);
                            }
                        }
                    }
                    tools.EndSection();
                }

                if (tools.Button("Update"))
                {
                    glass.UpdateTextureDimensions_Full();
                }

                tools.EndSection();
            }
        }

        #endregion

        #region settings - materials

        void Section_Materials()
        {
            if (tools.ShowSection("Materials", ref glass.showSection_Materials))
            {
                tempMatBack = glass.material_back;
                tempMatFront = glass.material_front;

                tools.StartChangeCheck();

                tools.MaterialOption("Backface", ref tempMatBack);

                if (tools.EndChangeCheck())
                {
                    glass.material_back = tempMatBack;
                    glass.Material_BackfaceChanged();
                    //materialChangeDetected = false;
                    UpdateOtherGlassList();
                    tools.SetDirty(glass);
                }

                tools.StartChangeCheck();

                tools.MaterialOption("Frontface", ref tempMatFront);

                if (tools.EndChangeCheck())
                {
                    glass.material_front = tempMatFront;
                    glass.Material_FrontfaceChanged();
                    //materialChangeDetected = false;
                    UpdateOtherGlassList();
                    tools.SetDirty(glass);
                }

                if (tools.Button("Create '" + glass.presetName + "' Materials"))
                {
                    if (ReplaceExistingMaterials(glass.presetName))
                    {
                        CreateNewMaterials(glass.presetName);

                        glass.UpdateRendererMaterials();

                        tools.SetDirty(glass);

                        glass.Material_Changed();
                    }
                }

                tools.EndSection();
            }
        }

        void DeselectAll()
        {
            Selection.objects = new UnityEngine.Object[0];
            Selection.activeGameObject = null;
        }

        /// <summary>
        /// Check for Material changes.
        /// This is seperate from creating new materials so as to avoid a conflict & potential crash.
        /// </summary>
        void CheckForMaterialChanges()
        {
            if (tempMatBack != glass.material_back)
            {
                //materialChangeDetected = true;
            }

            if (tempMatBack != glass.material_back)
            {
                //materialChangeDetected = true;
            }
        }

        void CreateNewMaterials(string name)
        {
            CreateMaterial(name, GlassFace.front);
            CreateMaterial(name, GlassFace.back);
        }

        void CreateMaterial(string name, GlassFace face)
        {
            string path = MaterialPath(name, face);

            Material material = null;

            if (File.Exists(path))
            {
                material = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
            }
            else
            {
                material = glass.GenerateMaterial(face);
                SaveMaterial(material, path);
            }

            switch (face)
            {
                case GlassFace.front:
                    if (material != glass.material_front)
                    {
                        if (glass.material_front != null)
                        {
                            material.CopyPropertiesFromMaterial(glass.material_front);
                            //  Workaround for Unity bug (821208) TODO: remove when fixed
                            material.shader = material.shader;
                        }
                    }
                    glass.material_front = material;
                    break;
                case GlassFace.back:
                    if (material != glass.material_back)
                    {
                        if (glass.material_back != null)
                        {
                            material.CopyPropertiesFromMaterial(glass.material_back);
                            //  Workaround for Unity bug (821208) TODO: remove when fixed
                            material.shader = material.shader;
                        }
                    }
                    glass.material_back = material;
                    break;
            }

            tools.SetDirty(material);
        }

        bool SaveMaterial(Material material, string path)
        {
            try
            {
                AssetDatabase.CreateAsset(material, path);
                return true;
            }
            catch (Exception e)
            {
                tools.Message("ERROR Creating Material", "Please check the directory exists for Glass Materials '" + path + "'.\n\n\nError: " + e.Message);
                return false;
            }
        }

        string MaterialPath(string name, GlassFace face)
        {
            switch (face)
            {
                case GlassFace.front:
                    return materialsPath + "Glass_" + name + "_front" + ".mat";
                case GlassFace.back:
                    return materialsPath + "Glass_" + name + "_back" + ".mat";
                default:
                    Debug.LogError("Unknown Glass Face in Flass Creator!");
                    return "";
            }
        }

        bool ReplaceExistingMaterials(string newMaterialName)
        {
            if (MaterialExists(newMaterialName, GlassFace.front))
            {
                return ReplaceMaterialsMessage();
            }

            if (MaterialExists(newMaterialName, GlassFace.back))
            {
                return ReplaceMaterialsMessage();
            }

            return true;
        }

        bool MaterialExists(string name, GlassFace face)
        {
            return File.Exists(MaterialPath(name, GlassFace.front));
        }

        bool ReplaceMaterialsMessage()
        {
            return tools.Message("Replace Existing Material?", "One or more materials already exist for this material.\n\nWould you like to replace them anyway?", "Yes", "Cancel");
        }

        #endregion

        #region Section Notes

        private void Section_ImportantNotes()
        {
            if (tools.ShowSection("IMPORTANT Notes", ref showSection_ImportantNotes))
            {
                tools.Label("NOTE 1: Changing options in materials will not be reflected here.", true);
                tools.Label("NOTE 2: Changes made during Play mode are not automatically saved.", true);
                tools.Label("     2.1: Workaround 1: Save to a Preset in Play mode then Load from that Preset in the Editor.", true);
                tools.Label("     2.2: Workaround 2: Copy and paste component values, using options cog on the Glass object.", true);
                //tools.Label("\t*feature in development for future release", true);
                tools.EndSection();
            }
        }

        #endregion

        #region Glass Manager Settings

        void Section_ManagerSettings()
        {
            tools.StartEdit(glass, "Changed Glass Manager Settings");

            tools.StartChangeCheck();

            if (tools.ShowSection("Manager Settings", ref glass.showSection_ManagerSettings))
            {
                if (tools.EndChangeCheck())
                {
                    LoadGlassManagerSettings();
                }

                if (managerSettings == null)
                {
                    managerSettings = new Glass_GlassManager_Settings();
                    managerSettings.glassPresetName = glass.presetName;
                    if (glass.manager != null)
                    {
                        glass.manager.LoadManager();
                    }
                }

                Section_ManagerSettings_Quality();

                Section_ManagerSettings_DepthNormalTechniques();

                tools.EndSection();
            }

            tools.FinishEdit(glass);
        }

        void Section_ManagerSettings_Quality()
        {
            if (tools.ShowSection("Quality", ref glass.showSection_Manager_Quality))
            {
                tools.StartChangeCheck();

                tools.Label("Depth Textures:");

                tools.Popup("Anti-Aliasing*", textureAALabels, textureAAItems, ref textureAAIndex, ref managerSettings.depthTextureAA);
                tools.IntOption("Aniso Level*", ref managerSettings.depthTextureAniso, 0, 16);
                managerSettings.depthTextureFilterMode = (FilterMode)tools.EnumOption("Filter Mode*", managerSettings.depthTextureFilterMode);

                //tools.StartChangeCheck();

                managerSettings.depthTextureClearMode = (CameraClearFlags)tools.EnumOption("Clear Mode", managerSettings.depthTextureClearMode);

                /*
                if (tools.EndChangeCheck())
                {
                    managerSettings.UpdateCameraSettings();
                }
                */

                if (tools.Button("Set Defaults"))
                {
                    managerSettings.depthTextureAA = GlassManager.default_depthTextureAA;
                    UpdateTextureAAIndex();
                    managerSettings.depthTextureAniso = GlassManager.default_depthTextureAniso;
                    managerSettings.depthTextureFilterMode = GlassManager.default_depthTextureFilterMode;
                    managerSettings.depthTextureClearMode = GlassManager.default_depthTextureClearMode;
                    // managerSettings.UpdateCameraSettings();
                }

                tools.Label("*Does not change during Play.");
                tools.Label("NB: Changes made during Play are not saved.");

                if (tools.FinishChangeCheck())
                {
                    managerSettings.Edited();
                    managerSettings.Save(xmlPath);
                    if (glass.manager == null)
                        glass.FindGlassManager();
                    glass.manager.LoadManager();
                }

                tools.EndSection(true);
            }
        }

        void Section_ManagerSettings_DepthNormalTechniques()
        {
            if (tools.ShowSection("Depth & Normal Techniques", ref glass.showSection_Manager_DepthNormalTech))
            {
                tools.StartChangeCheck();

                tools.StartChangeCheck();
                managerSettings.depthTechnique = (GlassDepthTechnique)tools.EnumOption("Depth", managerSettings.depthTechnique);
                if (tools.EndChangeCheck())
                {
                    managerSettings.UpdateDepthTechnique();
                }

                tools.StartChangeCheck();
                managerSettings.normalTechnique = (GlassNormalTechnique)tools.EnumOption("Normal", managerSettings.normalTechnique);
                if (tools.EndChangeCheck())
                {
                    managerSettings.UpdateNormalTechnique();
                }

                tools.StartChangeCheck();
                managerSettings.frontDepthTechnique = (GlassFrontDepthTechnique)tools.EnumOption("Front Depth", managerSettings.frontDepthTechnique);
                if (tools.EndChangeCheck())
                {
                    managerSettings.UpdateFrontDepthTechnique();
                }

#if UNITY_5_4_OR_NEWER
                    tools.StartChangeCheck();
                    tools.BoolOption("Unity 5.4 Fix (old cards)", ref managerSettings.enable54Workaround);
                    if (tools.EndChangeCheck())
                    {
                        managerSettings.UpdateFrontDepthTechnique();
                    }
#endif

                if (tools.EndChangeCheck())
                {
                    managerSettings.Edited();
                    managerSettings.Save(xmlPath);
                    if (glass.manager == null)
                        glass.FindGlassManager();
                    glass.manager.LoadManager();
                }

                tools.EndSection();
            }
        }

        void UpdateTextureAAIndex()
        {
            if (managerSettings == null)
                return;
            for (int i = 0; i < textureAAItems.Length; i++)
            {
                if (textureAAItems[i] == managerSettings.depthTextureAA)
                {
                    textureAAIndex = i;
                    break;
                }
            }
        }

        #endregion

        #region Extinction

        Color GetExtinctionColour(GlassFace face)
        {
            return GetExtinctionColour(face, glass.extinctionAppearance);
        }

        Color GetExtinctionColour_Flipped(GlassFace face)
        {
            return GetExtinctionColour_Flipped(face, glass.extinctionAppearance);
        }

        Color GetExtinctionColour(GlassFace face, GlassExtinctionAppearance appearance)
        {
            switch (appearance)
            {
                case GlassExtinctionAppearance.AsApplied:
                    {
                        switch (face)
                        {
                            case GlassFace.front:
                                return glass.extinction_front;
                            case GlassFace.back:
                                return glass.extinction_back;
                        }
                        break;
                    }
                case GlassExtinctionAppearance.AsItAppears:
                    {
                        switch (face)
                        {
                            case GlassFace.front:
                                return FlippedColour(glass.extinctionFlipped_front);
                            case GlassFace.back:
                                return FlippedColour(glass.extinctionFlipped_back);
                        }
                        break;
                    }
            }
            return Color.clear;
        }

        Color GetExtinctionColour_Flipped(GlassFace face, GlassExtinctionAppearance appearance)
        {
            switch (appearance)
            {
                case GlassExtinctionAppearance.AsApplied:
                    {
                        switch (face)
                        {
                            case GlassFace.front:
                                return glass.extinctionFlipped_front;
                            case GlassFace.back:
                                return glass.extinctionFlipped_back;
                        }
                        break;
                    }
                case GlassExtinctionAppearance.AsItAppears:
                    {
                        switch (face)
                        {
                            case GlassFace.front:
                                return FlippedColour(glass.extinction_front);
                            case GlassFace.back:
                                return FlippedColour(glass.extinction_back);
                        }
                        break;
                    }
            }
            return Color.clear;
        }

        Color FlippedColour(Color colour, bool flipAlpha = false)
        {
            return new Color(1f - colour.r,
                1f - colour.g,
                1f - colour.b,
                flipAlpha ? 1f - colour.a : colour.a);
        }

        #endregion

        #region Helper Functions

        private bool GlassNamesDiffer()
        {
            string glassName = glass.presetName;
            foreach (Glass glassIter in glassList)
                if (glassIter.presetName != glassName)
                    return true;
            return false;
        }

        #endregion
    }

}

#endif
