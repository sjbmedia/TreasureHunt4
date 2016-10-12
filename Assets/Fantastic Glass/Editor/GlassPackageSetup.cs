#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace FantasticGlass
{
    [ExecuteInEditMode]
    [InitializeOnLoad]
    public class GlassPackageSetup
    {
        public static string glassDefineSymbol = "FD_GLASS";
        public static bool debugGlassPackageSetup = false;

        static GlassPackageSetup()
        {
            foreach (BuildTargetGroup buildTargetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (buildTargetGroup == BuildTargetGroup.Unknown)
                {
                    if (debugGlassPackageSetup)
                        Debug.Log("Glass Package Setup: skipping unknown target group '" + buildTargetGroup.ToString() + "'");
                    continue;
                }
#if UNITY_5_4_OR_NEWER
                if ((buildTargetGroup == (BuildTargetGroup)15)
                    || (buildTargetGroup == (BuildTargetGroup)16))
                {
                    if (debugGlassPackageSetup)
                        Debug.Log("Glass Package Setup: skipping built target group '" + buildTargetGroup + "' as group is obsolote.");
                    continue;
                }
#endif
                string buildTargetGroupDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                if (!buildTargetGroupDefineSymbols.Contains(glassDefineSymbol))
                {
                    Debug.Log("Added Define Symbols for Glass package.");
                    if (buildTargetGroupDefineSymbols.Length == 0)
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, glassDefineSymbol);
                    else
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, "; " + glassDefineSymbol);
                }
            }
        }
    }

}

#endif
