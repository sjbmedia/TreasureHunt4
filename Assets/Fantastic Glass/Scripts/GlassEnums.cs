using UnityEngine;
using System.Collections;

namespace FantasticGlass
{

    public enum GlassPhysicalObjectType
    {
        BoxCollider,
        SphereCollider,
        MeshCollider,
        MeshCollider_Convex
    }

    public enum GlassPrimitiveType
    {
        None,
        Cube,
        Sphere,
        Capsule,
        Cylinder,
        Quad,
        Plane
    }

    public enum GlassFace
    {
        front,
        back
    }

    public enum GlassExtinctionAppearance
    {
        AsApplied,
        AsItAppears
    }

    public enum GlassMeshScaleFix
    {
        fbx,
        custom,
        None
    }


    /// <summary>
    /// Glass mesh scale fix lookup.
    /// </summary>
    public class GlassMeshScaleFixLookup
    {
        public static float scale_fbx = 100f;
        public static float scale_none = 1f;

        public GlassMeshScaleFixLookup()
        {
        }

        public static void GetScale(GlassMeshScaleFix fixType, ref float currentScale)
        {
            switch (fixType)
            {
                case GlassMeshScaleFix.fbx:
                    currentScale = scale_fbx;
                    break;
                case GlassMeshScaleFix.None:
                    currentScale = scale_none;
                    break;
                default:
                    break;
            }
        }

        public static void GetEnum(ref GlassMeshScaleFix fixType, float currentScale)
        {
            if (currentScale == scale_fbx)
                fixType = GlassMeshScaleFix.fbx;
            else if (currentScale == scale_none)
                fixType = GlassMeshScaleFix.None;
            else
                fixType = GlassMeshScaleFix.custom;
        }
    }

}