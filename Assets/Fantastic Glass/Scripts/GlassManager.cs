using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using System.Xml.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FantasticGlass
{
    [Serializable]
    public class GlassManager : MonoBehaviour
    {
        #region Member Variables

        public GlassSystemSettings settings;
        public GlassManagerSettingsType managerSettingsType = default_managerSettingsType;
        public static GlassManagerSettingsType default_managerSettingsType = GlassManagerSettingsType.LoadFromGlass;
        public Glass chosenGlassSettingsSource = null;
        public bool showSection_SettingsType = false;
        public List<Glass> glass = new List<Glass>();
        public List<Material> activeMaterials = new List<Material>();
        public Dictionary<Material, List<Glass>> activeMaterialsAndObjects = new Dictionary<Material, List<Glass>>();
        //	DEPTH TECHNIQUE 1.1
        public GlassDepthCamera camFront = null;
        public GlassDepthCamera camBack = null;
        public GlassDepthCamera camOther = null;
        //
        public string frontLayerName = default_frontLayerName;
        public string backLayerName = default_backLayerName;
        public static string default_frontLayerName = "GlassFront";
        public static string default_backLayerName = "GlassBack";
        public int backLayerMask = -1;
        public int frontLayerMask = -1;
        public int otherLayerMask = -1;
        //
        public List<string> frontLayerNames = new List<string>();
        public List<string> backLayerNames = new List<string>();
        public List<string> otherLayerNames = new List<string>();
        //
        public GlassDepthTechnique depthTechnique = GlassDepthCamera.default_depthTechnique;
        public GlassNormalTechnique normalTechnique = GlassDepthCamera.default_normalTechnique;
        public GlassFrontDepthTechnique frontDepthTechnique = GlassDepthCamera.default_frontDepthTechnique;
        //#if UNITY_5_4_OR_NEWER
        public bool enable54Workaround = false;
        //#endif
        //
        public string depthTextureFront = "_DepthFront";
        public string depthTextureBack = "_DepthBack";
        public string depthTextureOther = "_DepthOther";
        public Shader depthShaderBack;
        public Shader depthShaderFront;
        public int depthTextureAA = default_depthTextureAA;
        public int depthTextureAniso = default_depthTextureAniso;
        public FilterMode depthTextureFilterMode = default_depthTextureFilterMode;
        public CameraClearFlags depthTextureClearMode = default_depthTextureClearMode;
        public Camera mainCamera = null;
        //
        public static int default_depthTextureAA = 1;
        public static int default_depthTextureAniso = 16;
        public static FilterMode default_depthTextureFilterMode = FilterMode.Trilinear;
        public static CameraClearFlags default_depthTextureClearMode = CameraClearFlags.Skybox;
        //
        public bool renderDepthsSeperately = default_renderDepthSeperately;
        public float depthUpdateRate = default_depthUpdateRate;
        public float frontUpdateRate = default_frontUpdateRate;
        public float backUpdateRate = default_backUpdateRate;
        public float otherUpdateRate = default_otherUpdateRate;
        public float camHighUpdateRate = default_camHighUpdateRate;
        public float camLowUpdateRate = default_camLowUpdateRate;
        public int depthWait = 0;
        int depthWaitFront = 0;
        int depthWaitBack = 0;
        int depthWaitOther = 0;
        int depthWaitDelta = 0;
        //
        public static bool default_renderDepthSeperately = true;
        public static float default_depthUpdateRate = 144f;
        public static float default_frontUpdateRate = 144f;
        public static float default_backUpdateRate = 144f;
        public static float default_otherUpdateRate = 144f;
        public static float default_camHighUpdateRate = 30f;
        public static float default_camLowUpdateRate = 1f;
        public static int default_depthWait = 0;
        //	DEPTH TECHNIQUE 1.1
        public bool initialised = false;
        public string shaderBackName = "Depth/WorldNormals_BackFace";//"Depth/BackFace";
        public string shaderFrontName = "Depth/WorldNormals_FrontFace";//"Depth/FrontFace";
        public static GlassManager _instance = null;
        public static string managerObjectPath = "Assets/Fantastic Glass/Prefabs/GlassManager";
        public bool showDebugInfo = false;
        public bool matchByMaterial = false;
        public bool matchByGlassName = true;
        public bool synchroniseGlass = true;
        public bool disableLayerWarnings = false;
        public static float min_updateRates = 1f;
        public static float max_updateRates = 1000f;
        //  Files & Paths
        public static string default_packageName = "Fantastic Glass";
        public static string default_xml_Pathname = "XML";
        public static string default_materials_Pathname = "Materials";
        public static string default_settings_Filename = "glass_system_settings";
        public static string default_presetList_Filename = "glass_preset_list";
        public string packagePath = "";
        public string xmlPath = "";
        public string materialsPath = "";
        public string settingsPath = "";
        public string presetListPath = "";
        //  GUI Variables
        public bool showCameras = false;
        public bool showMainCamera = false;
        public bool showDepth = false;
        public bool showDepthTextures = false;
        public bool showDepthNormalTechniques = false;
        public bool showGlass = false;
        public bool showGlassList = false;
        public bool showLayers = false;
        public bool showLayersAdvanced = false;
        public bool showLayers_Depth = false;
        public bool showLayersAdvanced_Depth = false;
        public bool showPerformance = false;
        public bool showQuality = false;
        public bool show_materials = false;
        public bool showShaders = false;
        public bool showGlassUsingMaterial = false;
        public bool showSynchroniseSettings = false;
        public bool showAdvancedOptions = false;
        public bool showAdvancedInfo = false;
        public bool show_info_namesAndPaths = false;
        public bool showDebugging = false;
        public bool show_list_activeMaterials = false;
        public bool show_list_frontLayers = false;
        public bool show_list_backLayers = false;
        public bool show_list_otherLayers = false;
        public Vector2 scrollGlassList = new Vector2();
        public Vector2 scrollActiveMaterialList = new Vector2();
        public Vector2 scroll_frontLayers = new Vector2();
        public Vector2 scroll_backLayers = new Vector2();
        public Vector2 scroll_otherLayers = new Vector2();
        //  DEBUGGING DEPTH IN RELEASE MODE:
        //public bool debugDepthtexture = false;

        #endregion

        #region SINGLETON

        public static GlassManager Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }
                _instance = FindObjectOfType<GlassManager>();
                if (_instance != null)
                {
                    return _instance;
                }
                //  TODO: remove entirely if deemed useless in future versions
                //_instance = CreateFromDefault ();
                if (_instance != null)
                {
                    return _instance;
                }
                _instance = new GameObject("Glass Manager").AddComponent<GlassManager>();
                _instance.Initialise();
                return _instance;
            }
        }

        #endregion

        #region Start

        void Start()
        {
            Initialise();
            //
            StartCoroutine(Update_DepthBack_CO());
            StartCoroutine(Update_DepthFront_CO());
            StartCoroutine(Update_DepthOther_CO());
            StartCoroutine(Update_DepthAll_CO());
            //
            //	DEPTH TECHNIQUE 1
            StartCoroutine(Update_CamerasHighPriority_CO());
            StartCoroutine(Update_CamerasLowPriority_CO());
        }

        #endregion

        #region Create

        /*
        public static GlassManager CreateFromDefault()
        {
            GameObject glassManagerPrefab = EditorTools.LoadDefaultPrefab(GlassManager.managerObjectPath);
            GlassManager gman = null;
            GameObject mobj = null;
            if (glassManagerPrefab != null)
            {
                mobj = Instantiate(glassManagerPrefab);
                mobj.name = "Glass Manager";
                gman = mobj.GetComponent<GlassManager>();
            }
            if (gman != null)
            {
                return gman;
            }
#if UNITY_EDITOR
            DestroyImmediate(mobj);
#else
                Destroy(mobj);
#endif
            return null;
        }
        */

        #endregion

        #region Init

        public void Initialise()
        {
            if (!Application.isPlaying)
                return;

            if (initialised)
                return;

            InitPaths();

            UpdateLayerNames();
            UpdateLayerMasks();

            FindGlass();
            UpdateActiveMaterials();
            UpdateShaders();
            FindMainCamera();

            //  Should be loaded before using any settings e.g. Adding depth cameras
            LoadManager();

            AddDepthCam_Back();
            //	DEPTH TECHNIQUE 1.1
            AddDepthCam_Front();
            AddDepthCam_Other();

            UpdateDepthTechnique(true);
            UpdateNormalTechnique(true);
            UpdateFrontDepthTechnique(true);

            foreach (Glass g in glass)
            {
                camBack.AddRecipient(g.material_back);
                camBack.AddRecipient(g.material_front);

                //	DEPTH TECHNIQUE 1.1
                camFront.AddRecipient(g.material_back);
                camFront.AddRecipient(g.material_front);

                camOther.AddRecipient(g.material_back);
                camOther.AddRecipient(g.material_front);
            }

            if (Application.isPlaying)
                initialised = true;
        }

        public void InitPaths()
        {
            if (!packagePath.Contains(Application.dataPath))
            {
                packagePath = Application.dataPath + "/" + GlassManager.default_packageName + "/";
                xmlPath = packagePath + GlassManager.default_xml_Pathname + "/";
                presetListPath = xmlPath + GlassManager.default_presetList_Filename + ".xml";
                settingsPath = xmlPath + GlassManager.default_settings_Filename + ".xml";
                materialsPath = packagePath + GlassManager.default_materials_Pathname + "/";
#if UNITY_EDITOR
                materialsPath = FileUtil.GetProjectRelativePath(materialsPath);
#endif
            }
        }

        public GlassSystemSettings LoadSystemSettings()
        {
            settings = GlassSystemSettings.LoadFromXML(settingsPath);
            return settings;
        }

        public void FindGlass()
        {
            glass.Clear();

            foreach (Glass foundGlass in FindObjectsOfType<Glass>())
            {
                foundGlass.Initialise();
                glass.Add(foundGlass);
            }

            while (glass.Contains(null))
                glass.Remove(null);
        }

        public void UpdateShaders()
        {
            if ((depthShaderBack == null) || (depthShaderFront == null))
                LoadDefaultShaders();
        }

        public void LoadDefaultShaders()
        {
            depthShaderBack = Shader.Find(shaderBackName);
            depthShaderFront = Shader.Find(shaderFrontName);
        }

        #endregion

        #region Save / Load Manager

        public void LoadManager()
        {
            switch (managerSettingsType)
            {
                case GlassManagerSettingsType.LoadFromManager:
                    {
                        //  no need to do anything here, settings should be stored within the object instance
                        break;
                    }
                case GlassManagerSettingsType.LoadFromGlass:
                    {
                        //  1.  iterate glass objects
                        FindGlass();
                        Glass_GlassManager_Settings chosenLoadedSettings = null;

                        //  2.  If there is a chosen glass object to load from, try to load it first
                        //  2.1.Provide the Glass objects in the scene as a selection
                        chosenLoadedSettings = Glass_GlassManager_Settings.Load(xmlPath, chosenGlassSettingsSource);
                        if (chosenLoadedSettings != null)
                        {
                            ParseSettings(chosenLoadedSettings);
                            return;
                        }

                        List<Glass_GlassManager_Settings> loadedSettings = new List<Glass_GlassManager_Settings>();
                        foreach (Glass glassObject in glass)
                        {
                            //  3.  load settings from each glass object's associated glass manager setting
                            Glass_GlassManager_Settings loadedManagerSettings = Glass_GlassManager_Settings.Load(xmlPath, glassObject);

                            if (loadedManagerSettings != null)
                            {
                                if (!loadedSettings.Contains(loadedManagerSettings))
                                {
                                    loadedSettings.Add(loadedManagerSettings);
                                }
                            }
                        }

                        if (loadedSettings.Count == 0)
                        {
                            return;
                        }

                        if (loadedSettings.Count == 1)
                        {
                            ParseSettings(loadedSettings[0]);
                        }

                        //  4.  if there is a conflict, pick the chosen object to load from (most popular if none exists)
                        ParseSettings(loadedSettings);

                        break;
                    }
                    //  TODO:
                    /*
                case GlassManagerSettingsType.LoadFromGlass_Performance:
                {
                //  use the same code as GlassManagerSettingsType.LoadFromGlass
                break;
                }
                case GlassManagerSettingsType.LoadFromGlass_Quality:
                    {
                        //  1.  iterate glass objects
                        FindGlass();
                        foreach (Glass glassObject in glass)
                        {
                            //  2.  load settings from each glass object's associated glass manager setting

                            //  3.  if there is a conflict, load differences and create all requred depths etc.
                        }
                        break;
                    }
                    */
            }
        }

        public void ParseSettings(Glass_GlassManager_Settings _settings)
        {
            depthTechnique = _settings.depthTechnique;
            normalTechnique = _settings.normalTechnique;
            frontDepthTechnique = _settings.frontDepthTechnique;

            depthTextureAA = _settings.depthTextureAA;
            depthTextureAniso = _settings.depthTextureAniso;
            depthTextureFilterMode = _settings.depthTextureFilterMode;
            depthTextureClearMode = _settings.depthTextureClearMode;

            enable54Workaround = _settings.enable54Workaround;

            FinishedParsingSettings();
        }

        public void ParseSettings(List<Glass_GlassManager_Settings> settingsList)
        {
            Dictionary<GlassDepthTechnique, float> depthTechnique_counter = new Dictionary<GlassDepthTechnique, float>();
            Dictionary<GlassNormalTechnique, float> normalTechnique_counter = new Dictionary<GlassNormalTechnique, float>();
            Dictionary<GlassFrontDepthTechnique, float> frontDepthTechnique_counter = new Dictionary<GlassFrontDepthTechnique, float>();
            Dictionary<int, float> aa_counter = new Dictionary<int, float>();
            Dictionary<int, float> aniso_counter = new Dictionary<int, float>();
            Dictionary<FilterMode, float> filterMode_counter = new Dictionary<FilterMode, float>();
            Dictionary<CameraClearFlags, float> clearMode_counter = new Dictionary<CameraClearFlags, float>();

            int latestEditTime = 0;
            Glass_GlassManager_Settings lastEditedSetting = null;
            foreach (Glass_GlassManager_Settings _settings in settingsList)
            {
                if(_settings.lastEdited>=latestEditTime)
                {
                    lastEditedSetting = _settings;
                    latestEditTime = _settings.lastEdited;
                }
            }

            foreach (Glass_GlassManager_Settings _settings in settingsList)
            {
                float settingValue = (_settings == lastEditedSetting) ? 1.5f : 1f;  //  last edited gets a 0.5f boost so as not to infringe on genuinely more popular choices
                depthTechnique_counter[_settings.depthTechnique] = depthTechnique_counter.ContainsKey(_settings.depthTechnique) ? depthTechnique_counter[_settings.depthTechnique] + settingValue : settingValue;
                normalTechnique_counter[_settings.normalTechnique] = normalTechnique_counter.ContainsKey(_settings.normalTechnique) ? normalTechnique_counter[_settings.normalTechnique] + settingValue : settingValue;
                frontDepthTechnique_counter[_settings.frontDepthTechnique] = frontDepthTechnique_counter.ContainsKey(_settings.frontDepthTechnique) ? frontDepthTechnique_counter[_settings.frontDepthTechnique] + settingValue : settingValue;
                aa_counter[_settings.depthTextureAA] = aa_counter.ContainsKey(_settings.depthTextureAA) ? aa_counter[_settings.depthTextureAA] + settingValue : settingValue;
                aniso_counter[_settings.depthTextureAniso] = aniso_counter.ContainsKey(_settings.depthTextureAniso) ? aniso_counter[_settings.depthTextureAniso] + settingValue : settingValue;
                filterMode_counter[_settings.depthTextureFilterMode] = filterMode_counter.ContainsKey(_settings.depthTextureFilterMode) ? filterMode_counter[_settings.depthTextureFilterMode] + settingValue : settingValue;
                clearMode_counter[_settings.depthTextureClearMode] = clearMode_counter.ContainsKey(_settings.depthTextureClearMode) ? clearMode_counter[_settings.depthTextureClearMode] + settingValue : settingValue;
            }

            //  Depth
            float counterCheck = 0f;
            foreach (GlassDepthTechnique key in depthTechnique_counter.Keys)
            {
                if (depthTechnique_counter[key] > counterCheck)
                {
                    depthTechnique = key;
                    counterCheck = depthTechnique_counter[key];
                }
            }
            //  Normal
            counterCheck = 0f;
            foreach (GlassNormalTechnique key in normalTechnique_counter.Keys)
            {
                if (normalTechnique_counter[key] > counterCheck)
                {
                    normalTechnique = key;
                    counterCheck = normalTechnique_counter[key];
                }
            }
            //  Front Depth
            counterCheck = 0f;
            foreach (GlassFrontDepthTechnique key in frontDepthTechnique_counter.Keys)
            {
                if (frontDepthTechnique_counter[key] > counterCheck)
                {
                    frontDepthTechnique = key;
                    counterCheck = frontDepthTechnique_counter[key];
                }
            }
            //  AA
            counterCheck = 0f;
            foreach (int key in aa_counter.Keys)
            {
                if (aa_counter[key] > counterCheck)
                {
                    depthTextureAA = key;
                    counterCheck = aa_counter[key];
                }
            }
            //  Aniso
            counterCheck = 0f;
            foreach (int key in aniso_counter.Keys)
            {
                if (aniso_counter[key] > counterCheck)
                {
                    depthTextureAniso = key;
                    counterCheck = aniso_counter[key];
                }
            }
            //  Filter Mode
            counterCheck = 0f;
            foreach (FilterMode key in filterMode_counter.Keys)
            {
                if (filterMode_counter[key] > counterCheck)
                {
                    depthTextureFilterMode = key;
                    counterCheck = filterMode_counter[key];
                }
            }
            //  Clear Mode
            counterCheck = 0f;
            foreach (CameraClearFlags key in clearMode_counter.Keys)
            {
                if (clearMode_counter[key] > counterCheck)
                {
                    depthTextureClearMode = key;
                    counterCheck = clearMode_counter[key];
                }
            }

            FinishedParsingSettings();
        }

        /// <summary>
        /// Call this once you have finished loading GlassManager settings from Glass objects
        /// </summary>
        void FinishedParsingSettings()
        {
            UpdateDepthTechnique();
            UpdateNormalTechnique();
            UpdateFrontDepthTechnique();

            UpdateCameraSettings();
        }

        #endregion

        #region Layers

        public bool LayersExist()
        {
            if (disableLayerWarnings)
                return true;

            if (!AllLayersDefined())
            {
                if (!UpdateLayerMasks())
                    return false;
            }

            if (!AllLayersDefined())
            {
                UpdateLayerNames();
                UpdateLayerMasks();
            }

            if (!AllLayersDefined())
            {
                return false;
            }

            return true;
        }

        bool AllLayersDefined()
        {
            if ((frontLayerMask == -1) || (backLayerMask == -1) || (otherLayerMask == -1))
            {
                if (showDebugInfo)
                    Debug.Log("All Layers Not Defined: some masks are not yet defined.");
                return false;
            }

            if ((frontLayerMask == backLayerMask) || (frontLayerMask == otherLayerMask) || (backLayerMask == otherLayerMask))
            {
                if (showDebugInfo)
                    Debug.Log("All Layers Not Defined: some masks have the same value.");
                return false;
            }

            if (LayerMask.NameToLayer(frontLayerName) == -1)
            {
                Debug.Log("Front Layer Not Found: '" + frontLayerName + "'.");
                return false;
            }
            if (LayerMask.NameToLayer(backLayerName) == -1)
            {
                Debug.Log("Back Layer Not Found: '" + backLayerName + "'.");
                return false;
            }

            return true;
        }

        bool AllLayersMatchNames()
        {
            if (LayerMask.GetMask(frontLayerNames.ToArray()) != frontLayerMask)
                return false;
            if (LayerMask.GetMask(backLayerNames.ToArray()) != backLayerMask)
                return false;
            if (LayerMask.GetMask(otherLayerNames.ToArray()) != otherLayerMask)
                return false;
            return true;
        }

        public void UpdateLayerNames()
        {
            if (showDebugInfo)
                Debug.Log("Updating Layer Names...");

            UpdateFrontLayerNames();
            UpdateBackLayerNames();
            UpdateOtherLayerNames();

            if (showDebugInfo)
                Debug.Log("Finished Updating Layer Names.");
        }

        public bool UpdateLayerMasks()
        {
            if (showDebugInfo)
                Debug.Log("Updating Layer Masks...");
            //	DEPTH TECHNIQUE 1.1
            if (!UpdateFrontLayerMask())
            {
                if (showDebugInfo)
                    Debug.Log("Error updating Front Layer Mask. Check Glass layers exists.");
                return false;
            }
            if (!UpdateBackLayerMask())
            {
                if (showDebugInfo)
                    Debug.Log("Error updating Back Layer Mask. Check Glass layers exists.");
                return false;
            }
            if (!UpdateOtherLayerMask())
            {
                if (showDebugInfo)
                    Debug.Log("Error updating Other Layer Mask. Check Glass layers exists.");
                return false;
            }
            if (showDebugInfo)
                Debug.Log("Finished Updating Layer Masks.");
            return true;
        }

        //	Layer Names

        //	DEPTH TECHNIQUE 1.1
        public void UpdateFrontLayerNames()
        {
            if (frontLayerNames.Count == 0)
                frontLayerNames.Add(frontLayerName);
        }

        public void UpdateBackLayerNames()
        {
            if (backLayerNames.Count == 0)
                backLayerNames.Add(backLayerName);
        }

        public void UpdateOtherLayerNames()
        {
            if (otherLayerNames.Count == 0)
            {
                for (int i = 0; i <= 31; i++)
                {
                    string foundLayerName = LayerMask.LayerToName(i);
                    if (foundLayerName.Length > 0)
                    {
                        //	DEPTH TECHNIQUE 1.1
                        if (!frontLayerNames.Contains(foundLayerName))
                        {
                            if (!backLayerNames.Contains(foundLayerName))
                            {
                                otherLayerNames.Add(foundLayerName);
                            }
                        }
                    }
                }
            }
        }

        //	Layer Masks

        //	DEPTH TECHNIQUE 1.1
        public bool UpdateFrontLayerMask()
        {
            CleanLayerNames(ref frontLayerNames);
            frontLayerMask = LayerMask.GetMask(frontLayerNames.ToArray());
            return (frontLayerMask != -1);
        }

        public bool UpdateBackLayerMask()
        {
            CleanLayerNames(ref backLayerNames);
            backLayerMask = LayerMask.GetMask(backLayerNames.ToArray());
            return (backLayerMask != -1);
        }

        public bool UpdateOtherLayerMask()
        {
            CleanLayerNames(ref otherLayerNames);
            otherLayerMask = LayerMask.GetMask(otherLayerNames.ToArray());

            //  fix for Unity 5.2 wherein the default layer isn't included in result of GetMask:
            if (otherLayerNames.Contains("Default"))
            {
                int defaultLayer = 1 << 0;
                if ((otherLayerMask & defaultLayer) != defaultLayer)
                {
                    otherLayerMask = otherLayerMask | defaultLayer;
                }
            }

            return (otherLayerMask != -1);
        }

        public void CleanLayerNames(ref List<string> namesList)
        {
            for (int i = namesList.Count - 1; i >= 0; i--)
            {
                string layerNameTest = namesList[i];
                if (LayerMask.NameToLayer(layerNameTest) == -1)
                    namesList.RemoveAt(i);
            }
        }

        #endregion

        #region Shared / Matching Glass

        public List<Glass> SharedGlassOthers(Glass _glass)
        {
            List<Glass> others = new List<Glass>();
            foreach (Glass otherGlass in glass)
            {
                if (GlassMatch(_glass, otherGlass))
                {
                    if (!others.Contains(otherGlass))
                        others.Add(otherGlass);
                }
            }
            return others;
        }

        public bool GlassIsShared(Glass _glass)
        {
            foreach (Glass otherGlass in glass)
            {
                if (otherGlass == _glass)
                    continue;

                if (GlassMatch(_glass, otherGlass))
                    return true;
            }
            return false;
        }

        public bool GlassMatch(Glass glass1, Glass glass2)
        {
            return glass1.Matches(glass2);
        }

        #endregion

        #region Material Values

        /// <summary>
        /// Tells the Glass Manager that a Glass object has changed.
        /// The Glass Manager will then likely synchronise the changed Glass's settings with others that match.
        /// </summary>
        /// <param name="glasModified"></param>
        public void GlassModified(Glass glasModified)
        {
            if (!synchroniseGlass)
                return;
            //
            if (glasModified == null)
                return;
            //
            FindGlass();
            //
            foreach (Glass otherGlass in glass)
            {
                if (otherGlass == null)
                    continue;
                if (otherGlass == glasModified)
                    continue;
                otherGlass.MaterialValuesChanged(glasModified);
            }
        }

        #endregion

        #region Physics Values

        public void PhysicsValueChanged(Glass glassChanged)
        {
            if (!synchroniseGlass)
                return;
            //
            if (glassChanged == null)
                return;
            //
            foreach (Glass otherGlass in glass)
            {
                if (otherGlass == null)
                    continue;
                if (otherGlass == glassChanged)
                    continue;
                otherGlass.PhysicsValuesChanged(glassChanged);
            }
        }

        #endregion

        #region Materials

        public void UpdateActiveMaterials()
        {
            if (showDebugInfo)
                Debug.Log("Updating Active Materials.");
            lock (activeMaterials)
            {
                activeMaterials.Clear();
                activeMaterialsAndObjects.Clear();
                foreach (Glass activeGlass in glass)
                {
                    if (activeGlass == null)
                        continue;
                    if (!activeMaterials.Contains(activeGlass.material_back))
                        activeMaterials.Add(activeGlass.material_back);
                    //	DEPTH TECHNIQUE 1.1
                    if (!activeMaterials.Contains(activeGlass.material_front))
                        activeMaterials.Add(activeGlass.material_front);
                }
                //
                foreach (Material activeMaterial in activeMaterials)
                {
                    if (activeMaterial == null)
                        continue;
                    if (activeMaterialsAndObjects.ContainsKey(activeMaterial))
                        continue;
                    activeMaterialsAndObjects.Add(activeMaterial, new List<Glass>());
                    foreach (Glass activeGlass in glass)
                    {
                        if (activeGlass == null)
                            continue;
                        if (activeGlass.material_back == activeMaterial)
                            if (!activeMaterialsAndObjects[activeMaterial].Contains(activeGlass))
                                activeMaterialsAndObjects[activeMaterial].Add(activeGlass);

                        //	DEPTH TECHNIQUE 1.1
                        if (activeGlass.material_front == activeMaterial)
                            if (!activeMaterialsAndObjects[activeMaterial].Contains(activeGlass))
                                activeMaterialsAndObjects[activeMaterial].Add(activeGlass);
                    }
                }
            }
        }

        internal void AssignedNewMaterial(int v)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Depth / Cameras

        public void UpdateDepthTechnique(bool forceUpdate = false)
        {
            if (camFront != null)
                camFront.SetDepthTechnique(depthTechnique, forceUpdate);
            if (camBack != null)
                camBack.SetDepthTechnique(depthTechnique, forceUpdate);
            if (camOther != null)
                camOther.SetDepthTechnique(depthTechnique, forceUpdate);

            //  TODO: deprecate with further testing
            /*
            if (camBack != null)
            {
                normalTechnique = camBack.normalTechnique;
                frontDepthTechnique = camBack.frontDepthTechnique;
            }
            else if (camOther != null)
            {
                normalTechnique = camOther.normalTechnique;
                frontDepthTechnique = camOther.frontDepthTechnique;
            }
            else if (camFront != null)
            {
                normalTechnique = camFront.normalTechnique;
                frontDepthTechnique = camFront.frontDepthTechnique;
            }
            else
            {
                normalTechnique = GlassDepthCamera.NormalTechFromDepth(normalTechnique, depthTechnique);
                frontDepthTechnique = GlassDepthCamera.FrontDepthTechFromDepth(frontDepthTechnique, depthTechnique, enable54Workaround);
            }
            */
            normalTechnique = GlassDepthCamera.NormalTechFromDepth(normalTechnique, depthTechnique);
            frontDepthTechnique = GlassDepthCamera.FrontDepthTechFromDepth(frontDepthTechnique, depthTechnique, enable54Workaround);
        }

        public void UpdateNormalTechnique(bool forceUpdate = false)
        {
            if (camFront != null)
                camFront.SetNormalTechnique(normalTechnique, forceUpdate);
            if (camBack != null)
                camBack.SetNormalTechnique(normalTechnique, forceUpdate);
            if (camOther != null)
                camOther.SetNormalTechnique(normalTechnique, forceUpdate);

            //  TODO: deprecate with further testing
            /*
            if (camBack != null)
            {
                depthTechnique = camBack.depthTechnique;
                frontDepthTechnique = camBack.frontDepthTechnique;
            }
            else if (camOther != null)
            {
                depthTechnique = camOther.depthTechnique;
                frontDepthTechnique = camOther.frontDepthTechnique;
            }
            else if (camFront != null)
            {
                depthTechnique = camFront.depthTechnique;
                frontDepthTechnique = camFront.frontDepthTechnique;
            }
            else
            {
                depthTechnique = GlassDepthCamera.DepthTechFromNormal(depthTechnique, normalTechnique);
                frontDepthTechnique = GlassDepthCamera.FrontDepthTechFromNormal(frontDepthTechnique, normalTechnique, enable54Workaround);
            }
            */
            depthTechnique = GlassDepthCamera.DepthTechFromNormal(depthTechnique, normalTechnique);
            frontDepthTechnique = GlassDepthCamera.FrontDepthTechFromNormal(frontDepthTechnique, normalTechnique, enable54Workaround);
        }

        public void UpdateFrontDepthTechnique(bool forceUpdate = false)
        {
            if (camFront != null)
                camFront.SetFrontDepthTechnique(frontDepthTechnique, forceUpdate);
            if (camBack != null)
                camBack.SetFrontDepthTechnique(frontDepthTechnique, forceUpdate);
            if (camOther != null)
                camOther.SetFrontDepthTechnique(frontDepthTechnique, forceUpdate);

            //  TODO: deprecate with further testing
            /*
            if (camBack != null)
            {
                depthTechnique = camBack.depthTechnique;
                normalTechnique = camBack.normalTechnique;
            }
            else if (camOther != null)
            {
                depthTechnique = camOther.depthTechnique;
                normalTechnique = camOther.normalTechnique;
            }
            else if (camFront != null)
            {
                depthTechnique = camFront.depthTechnique;
                normalTechnique = camFront.normalTechnique;
            }
            else
            {
                GlassDepthCamera.DepthNormalTechFromFrontDepth(frontDepthTechnique, ref depthTechnique, ref normalTechnique, enable54Workaround);
            }
            */
            GlassDepthCamera.DepthNormalTechFromFrontDepth(frontDepthTechnique, ref depthTechnique, ref normalTechnique, enable54Workaround);
        }

        //	DEPTH TECHNIQUE 1.1
        public void AddDepthCam_Front()
        {
            AddDepthCam(ref camFront, "glass_depthCam_Front", frontLayerMask, LayerMask.NameToLayer(frontLayerName), depthTextureFront, depthShaderFront);
            //  DEBUGGING DEPTH IN RELEASE BUILDS
            //camFront.debugDepthObject.transform.localPosition = camFront.debugDepthObject.transform.localPosition + new Vector3(-0.5f, 0f, 0f);
        }

        public void AddDepthCam_Back()
        {
            AddDepthCam(ref camBack, "glass_depthCam_Back", backLayerMask, LayerMask.NameToLayer(backLayerName), depthTextureBack, depthShaderBack);
            //  DEBUGGING DEPTH IN RELEASE BUILDS
            //camBack.debugDepthObject.transform.localPosition = camBack.debugDepthObject.transform.localPosition + new Vector3(0f, 0f, 0f);
        }

        public void AddDepthCam_Other()
        {
            AddDepthCam(ref camOther, "glass_depthCam_Other", otherLayerMask, LayerMask.NameToLayer(frontLayerName), depthTextureOther, depthShaderFront);
            //  DEBUGGING DEPTH IN RELEASE BUILDS
            //camOther.debugDepthObject.transform.localPosition = camOther.debugDepthObject.transform.localPosition + new Vector3(0.5f, 0f, 0f);
        }

        public void AddDepthCam(ref GlassDepthCamera cam, string _name, LayerMask _cameralayer, int _glasslayer, string _DepthTexture, Shader _depthShader)
        {
            if (cam == null)
            {
                FindMainCamera();

                GameObject camObj = GameObject.Find(_name);
                if (camObj == null)
                {
                    camObj = new GameObject(_name);
                    camObj.transform.parent = mainCamera.transform;
                    camObj.transform.localPosition = Vector3.zero;
                    camObj.transform.localRotation = Quaternion.identity;
                }

                cam = camObj.GetComponentInChildren<GlassDepthCamera>();
                if (cam == null)
                {
                    cam = camObj.AddComponent<GlassDepthCamera>();
                }
            }
            cam.Initialise(_cameralayer, _glasslayer, _DepthTexture, _depthShader, this);
        }

        public Camera FindMainCamera()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera != null)
            {
                mainCamera.renderingPath = RenderingPath.DeferredShading;
            }

            if (settings != null)
            {
                if (settings.enableAlwaysSetOptimumCamera)
                {
                    mainCamera.renderingPath = RenderingPath.DeferredShading;
                }
            }

            if (mainCamera == null)
            {
                if (Camera.allCamerasCount > 0)
                {
                    mainCamera = Camera.allCameras[0];
                    mainCamera.renderingPath = RenderingPath.DeferredShading;
                }
                else {
                    GameObject mainCameraObject = new GameObject("MainCamera");
                    mainCamera = mainCameraObject.AddComponent<Camera>();
                    mainCamera.renderingPath = RenderingPath.DeferredShading;
                    Debug.LogWarning("GlassManager: No main camera was found so one was created.");
                }
            }

            return mainCamera;
        }

        IEnumerator Update_DepthAll_CO()
        {
            while (true)
            {
                if (initialised && (!renderDepthsSeperately))
                {
                    depthWaitDelta++;
                    if (depthWaitDelta >= depthWait)
                    {
                        if (frontDepthTechnique == GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF)
                        {
                            camFront.RenderDepth();
                        }
                        camBack.RenderDepth();
                        camOther.RenderDepth();
                        depthWaitDelta = 0;
                    }
                }
                //
                yield return new WaitForSeconds(1f / depthUpdateRate);
            }
        }

        //	DEPTH TECHNIQUE 1.1
        private IEnumerator Update_DepthFront_CO()
        {
            bool renderFrontDepth = true;

            while (true)
            {
                if (initialised && renderDepthsSeperately)
                {
                    switch (frontDepthTechnique)
                    {
                        case FantasticGlass.GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_ON:
                            {
                                //	we do NOT require the depth from this camera
                                renderFrontDepth = false;
                                break;
                            }
                        default:
                            {
                                //	we DO require the depth from this camera
                                renderFrontDepth = true;
                                break;
                            }
                    }
                    if (renderFrontDepth)
                    {
                        depthWaitFront++;
                        if (depthWaitFront >= depthWait)
                        {
                            camFront.RenderDepth();
                            depthWaitFront = 0;
                        }
                    }
                }
                //
                yield return new WaitForSeconds(1f / frontUpdateRate);
            }
        }

        private IEnumerator Update_DepthBack_CO()
        {
            while (true)
            {
                if (initialised && renderDepthsSeperately)
                {
                    depthWaitBack++;
                    if (depthWaitBack >= depthWait)
                    {
                        camBack.RenderDepth();
                        depthWaitBack = 0;
                    }
                }
                //
                yield return new WaitForSeconds(1f / backUpdateRate);
            }
        }

        private IEnumerator Update_DepthOther_CO()
        {
            while (true)
            {
                if (initialised && renderDepthsSeperately)
                {
                    depthWaitOther++;
                    if (depthWaitOther >= depthWait)
                    {
                        camOther.RenderDepth();
                        depthWaitOther = 0;
                    }
                }
                yield return new WaitForSeconds(1f / otherUpdateRate);
            }
        }

        private IEnumerator Update_CamerasHighPriority_CO()
        {
            while (true)
            {
                if (initialised)
                {
                    if (normalTechnique == GlassNormalTechnique.NORMAL_WORLD_CAM_SHADER)
                    {
                        SetNormalFromCamera();
                    }
                }
                yield return new WaitForSeconds(1f / camHighUpdateRate);
            }
        }

        private IEnumerator Update_CamerasLowPriority_CO()
        {
            while (true)
            {
                if (initialised)
                {
                    UpdateCameraDepths();
                }
                yield return new WaitForSeconds(1f / camLowUpdateRate);
            }
        }

        //

        void UpdateCameraDepths()
        {
            float depthFrontLength = camFront.DepthLength();
            float depthBackLength = camBack.DepthLength();
            float depthOtherLength = camOther.DepthLength();

            switch (frontDepthTechnique)
            {
                case GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_ON:
                    {
                        foreach (Glass aGlass in glass)
                        {
                            aGlass.SetDepthBack(depthBackLength);
                            aGlass.SetDepthOther(depthOtherLength);
                        }
                        break;
                    }
                default:
                    foreach (Glass aGlass in glass)
                    {
                        aGlass.SetDepthFront(depthFrontLength);
                        aGlass.SetDepthBack(depthBackLength);
                        aGlass.SetDepthOther(depthOtherLength);
                    }
                    break;
            }
        }

        public void UpdateCameraSettings()
        {
            if (camFront != null)
                camFront.SetDepthTextureClearMode(depthTextureClearMode);
            if (camBack != null)
                camBack.SetDepthTextureClearMode(depthTextureClearMode);
            if (camOther != null)
                camOther.SetDepthTextureClearMode(depthTextureClearMode);
        }

        //

        private void SetFrontFace()
        {
            foreach (Glass aGlass in glass)
            {
                aGlass.SetFrontFace();
            }
        }

        private void SetBackFace()
        {
            foreach (Glass aGlass in glass)
            {
                aGlass.SetBackFace();
            }
        }

        private void SetAllFace()
        {
            foreach (Glass aGlass in glass)
            {
                aGlass.SetAllFace();
            }
        }

        //	NORMAL TECHNIQUE 1
        void SetNormalFromCamera()
        {
            foreach (Glass aGlass in glass)
            {
                if (aGlass == null)
                    continue;
                aGlass.SetCameraNormal(mainCamera.transform.forward);
            }
        }

        public void SetDepthTexture(RenderTexture renderTexture, string textureName)
        {
            foreach (Glass aGlass in glass)
            {
                if (aGlass == null)
                    continue;
                aGlass.SetDepthTexture(renderTexture, textureName);
            }
        }

        public void SetLayer(int _layer)
        {
            if (_layer < 0 || _layer > 31)
            {
                Debug.LogError("An error has occured trying to set a Glass object's layer. Make sure 'GlassFront' and 'GlassBack' exist in Tags and Layers.");
                frontLayerMask = -1;
                backLayerMask = -1;
                otherLayerMask = -1;
                return;
            }
            foreach (Glass aGlass in glass)
            {
                aGlass.gameObject.layer = _layer;
            }
        }

        #endregion
    }

}
