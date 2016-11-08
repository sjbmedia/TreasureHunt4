#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_5_3_OR_NEWER	//	Unity 5.3
using UnityEditor.SceneManagement;
#endif
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace FantasticGlass
{
    public enum EditorToolsPreset_Option
    {
        NoOption,
        ItemChanged,
        ItemDeleted,
        ItemAdded,
        DeleteAll,
        DeleteItem,
        LoadItem,
        SaveItem
    }

    /// <summary>
    /// N.B. Do not put this in an 'Editor' folder as it will make it inaccessible.
    /// </summary>
    public class EditorTools
    {
        #region Member Variables

        public string filename = "";
        //
        public int scrollItemLength = 9;
        public float maxScrollHeight = 128f;
        public float minScrollHeight = 19f;
        public int pixelsPerScrollItem = 19;
        //
        public Color curveColour = new Color(0f, 1f, 0f);
        public float curveHeightMin = 19f;
        public float curveHeightMax = 38f;
        public bool enableDebugLogging = false;
        private GUIStyle style_wordwrap;

        #endregion

        #region Constructor

        public EditorTools(string name)
        {
            filename = name;
            style_wordwrap = new GUIStyle();
            style_wordwrap.wordWrap = true;
        }

        #endregion

        #region Dirty

        public void SetDirty(UnityEngine.Object obj, bool markSceneDirty = true)
        {
            if (Application.isPlaying)
                return;
#if UNITY_EDITOR
            if (obj != null)
                EditorUtility.SetDirty(obj);
#if UNITY_5_3_OR_NEWER
            if (markSceneDirty)
                EditorSceneManager.MarkAllScenesDirty();
#endif
#endif
        }

        public void StartEdit(UnityEngine.Object obj, string editLabel)
        {
            Undo.RecordObject(obj, editLabel);
        }

        public void FinishEdit(UnityEngine.Object obj, bool markSceneDirty = true)
        {
            SetDirty(obj, markSceneDirty);
        }

        public void EndEdit(UnityEngine.Object obj, bool markSceneDirty = true)
        {
            FinishEdit(obj, markSceneDirty);
        }

        #endregion

        #region Change Check

        public void BeginChangeCheck()
        {
            EditorGUI.BeginChangeCheck();
        }

        public void StartChangeCheck()
        {
            BeginChangeCheck();
        }

        public bool EndChangeCheck()
        {
            return EditorGUI.EndChangeCheck();
        }

        public bool StopChangeCheck()
        {
            return EndChangeCheck();
        }

        public bool FinishChangeCheck()
        {
            return EndChangeCheck();
        }

        #endregion

        #region Show List

        private bool ShowList(string label, ref List<GameObject> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(null);
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<int> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(0);
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<string> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add("");
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<Transform> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(null);
            }
            //
            return show;
        }

        public bool ShowList(string label, ref List<Vector3> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(new Vector3());
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<Vector2> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(new Vector2());
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<Material> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(null);
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<Renderer> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(null);
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<Glass> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            //if (listSize == 0)
            //    return show;
            //
            listSize = Mathf.Max(0, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(null);
            }
            //
            return show;
        }

        #endregion

        #region GUI List

        //  Vector2

        public bool GUI_List(string label, ref List<Vector2> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.Vector2Field("0", list[0]);
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem);
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.Vector2Field(i2.ToString(), list[i2]);
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  Vector3

        public bool GUI_List(string label, ref List<Vector3> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.Vector3Field("0", list[0]);
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem);
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.Vector3Field(i2.ToString(), list[i2]);
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  Material

        public bool GUI_List(string label, ref List<Material> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.ObjectField("0", list[0], typeof(Material), true) as Material;
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem);
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.ObjectField(i2.ToString(), list[i2], typeof(Material), true) as Material;
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  Renderer

        public bool GUI_List(string label, ref List<Renderer> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.ObjectField("0", list[0], typeof(Renderer), true) as Renderer;
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem);
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.ObjectField(i2.ToString(), list[i2], typeof(Renderer), true) as Renderer;
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }


        //  Glass
        public bool GUI_List(string label, ref List<Glass> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.ObjectField(list[0], typeof(Glass), true) as Glass;
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem);
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.ObjectField(i2.ToString(), list[i2], typeof(Glass), true) as Glass;
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  Transform

        public bool GUI_List(string label, ref List<Transform> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            ;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.ObjectField(list[0], typeof(Transform), true) as Transform;
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem);
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.ObjectField(i2.ToString(), list[i2], typeof(Transform), true) as Transform;
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  int

        public bool GUI_List(string label, ref List<int> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.IntField(list[0]);
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem);
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.IntField(i2.ToString(), list[i2]);
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  string

        public bool GUI_List(string label, ref List<string> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.TextField(list[0]);
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem);
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.TextField(i2.ToString(), list[i2]);
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        #endregion

        #region Button

        public bool Button(string buttonLabel)
        {
            return GUILayout.Button(buttonLabel, GUILayout.ExpandWidth(true));
        }

        #endregion

        #region Sections

        /// <summary>
        /// Displays a togglable indented section.
        /// REMEMBER to call EndSection() at section end.
        /// </summary>
        /// <param name="sectionTitle"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ShowSection(string sectionTitle, ref bool value)
        {
            value = EditorGUILayout.Foldout(value, sectionTitle);
            if (value)
                StartSection();
            return value;
        }

        /// <summary>
        /// Starts the indented section.
        /// REMEMBER to call EndSection() at section end.
        /// </summary>
        public void StartSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            EditorGUI.indentLevel++;
        }

        /// <summary>
        /// Ends the indented section.
        /// REMEMBER to call StartSection() first.
        /// </summary>
        /// <param name="showDivider">If set to <c>true</c> show divider.</param>
        /// <param name="_dividerHeight">Divider height.</param>
        public void EndSection(bool showDivider = false, int _dividerHeight = 1)
        {
            if (showDivider)
                Divider(_dividerHeight);
            //
            GUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        #endregion

        #region Labels

        public void Label(string label, bool wordWrap = false)
        {
            if (wordWrap)
                EditorGUILayout.LabelField(label, style_wordwrap);
            else
                EditorGUILayout.LabelField(label);
        }

        /// <summary>
        /// This displays a NON-editable string. Use the String or Text options for editable versions.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value"></param>
        public void Label(string label, string value)
        {
            EditorGUILayout.LabelField(label, value);
        }

        #endregion

        #region Decorations

        public void HorizontalLine(int _height = 1)
        {
            Divider(_height);
        }

        public void Divider(int _height = 1)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(_height) });
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

		public void Space()
		{
			EditorGUILayout.Space();
		}

        #endregion

        #region Inspector Settings

        public void GUI_InspectorSettings()
        {
            IntOption("Items Per Scroll View", ref scrollItemLength, 2, 128);
            //
            FloatOption("Scroll View height MIN", ref minScrollHeight, 0f, 512f);
            FloatOption("Scroll View height MAX", ref maxScrollHeight, 0f, 512f);
            //
            IntOption("Pixels per Scroll Item", ref pixelsPerScrollItem, 1, 128);
            //
            ColourOption("Curve Colour", ref curveColour);
            FloatOption("Curve Graph height MIN", ref curveHeightMin, 19f, 128);
            FloatOption("Curve Graph height MAX", ref curveHeightMax, 19f, 128);
        }

        #endregion

        #region Options (singular values)

        public bool BoolOption(string label, ref bool value)
        {
            value = EditorGUILayout.Toggle(label, value);
            return value;
        }

        public float FloatOption(string label, ref float value)
        {
            value = EditorGUILayout.FloatField(label, value);
            return value;
        }

        public float FloatOption(string label, ref float value, float min, float max)
        {
            value = EditorGUILayout.Slider(label, value, min, max);
            return value;
        }

        public int IntOption(string label, ref int value)
        {
            value = EditorGUILayout.IntField(label, value);
            return value;
        }

        public int IntOption(string label, ref int value, int min, int max)
        {
            value = EditorGUILayout.IntSlider(label, value, min, max);
            return value;
        }

        public Vector2 VectorOption(string label, ref Vector2 value)
        {
            value = EditorGUILayout.Vector2Field(label, value);
            return value;
        }

        public Vector3 VectorOption(string label, ref Vector3 value)
        {
            value = EditorGUILayout.Vector3Field(label, value);
            return value;
        }

        public Vector4 VectorOption(string label, ref Vector4 value)
        {
            value = EditorGUILayout.Vector4Field(label, value);
            return value;
        }

        public AnimationCurve CurveOption(string label, ref AnimationCurve value, Rect range)
        {
            value = EditorGUILayout.CurveField(label, value, curveColour, range, GUILayout.MinHeight(curveHeightMin), GUILayout.MaxHeight(curveHeightMax));
            return value;
        }

        public string StringOption(string label, ref string value, bool editable = true)
        {
            string initialValue = value;
            value = EditorGUILayout.TextField(label, value);
            if (!editable)
                value = initialValue;
            return value;
        }

        public string TextOption(string label, ref string value, bool editable = true)
        {
            string initialValue = value;
            value = StringOption(label, ref value);
            if (!editable)
                value = initialValue;
            return value;
        }

        public string LabelOption(string label, ref string value, bool editable = true)
        {
            string initialValue = value;
            value = StringOption(label, ref value);
            if (!editable)
                value = initialValue;
            return value;
        }

        public Shader ShaderOption(string label, ref Shader value)
        {
            value = EditorGUILayout.ObjectField(label, value, typeof(Shader), true) as Shader;
            return value;
        }

        public Color ColourOption(string label, ref Color colour)
        {
            colour = EditorGUILayout.ColorField(label, colour);
            return colour;
        }

        /// <summary>
        /// Pass in the current enum value and cast the returned value back to your enum type.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="currentEnum"></param>
        /// <returns></returns>
        public Enum EnumOption(string label, Enum currentEnum)
        {
            currentEnum = EditorGUILayout.EnumPopup(label, (Enum)currentEnum);
            return currentEnum;
        }

		/// <summary>
		/// Shows a selection of labels in a pop-up.
		///	Sets the referenced value and index to the latest selection.
		///	Returns the chosen value.
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="itemLabels">Item labels.</param>
		/// <param name="itemValues">Item values.</param>
		/// <param name="value">Value.</param>
		public int Popup(string label, string[] itemLabels, int[] itemValues, ref int currentIndex, ref int currentValue)
		{
			currentIndex = EditorGUILayout.Popup(label, currentIndex, itemLabels);
			currentValue = itemValues[currentIndex];
			return currentValue;
		}

        public GameObject GameObjectOption(string label, ref GameObject obj)
        {
            obj = EditorGUILayout.ObjectField(label, obj, typeof(GameObject), true) as GameObject;
            return obj;
        }

		public Camera CameraOption(string label, ref Camera cam)
		{
			cam = EditorGUILayout.ObjectField(label, cam, typeof(Camera), true) as Camera;
			return cam;
		}

        public GlassDepthCamera GlassDepthCamOption(string label, ref GlassDepthCamera glassDepthCam)
        {
            glassDepthCam = EditorGUILayout.ObjectField(label, glassDepthCam, typeof(GlassDepthCamera), true) as GlassDepthCamera;
            return glassDepthCam;
        }

        public Glass GlassOption(string label, ref Glass value)
        {
            value = EditorGUILayout.ObjectField(label, value, typeof(Glass), true) as Glass;
            return value;
        }

        public Mesh MeshOption(string label, ref Mesh mesh)
        {
            mesh = EditorGUILayout.ObjectField(label, mesh, typeof(Mesh), true) as Mesh;
            return mesh;
        }

        public Material MaterialOption(string label, ref Material material)
        {
            material = EditorGUILayout.ObjectField(label, material, typeof(Material), true) as Material;
            return material;
        }

        public Texture TextureOption(string label, ref Texture texture)
        {
            texture = EditorGUILayout.ObjectField(label, texture, typeof(Texture), true) as Texture;
            return texture;
        }

        public LayerMask LayerOption(string label, ref int layer)
        {
            string[] layerNames = new string[32];
            for (int i = 0; i < 32; i++)
            {
                layerNames[i] = LayerMask.LayerToName(i);
            }
            layer = EditorGUILayout.MaskField(label, layer, layerNames);
            return layer;
        }

        #endregion

        #region Messages

        /// <summary>
        /// A version of the Message function that can be called without an instance of EditorTools.
        /// </summary>
        /// <returns><c>true</c>, if message appeared and 'ok' was clicked, <c>false</c> otherwise returns false.</returns>
        /// <param name="title">Title.</param>
        /// <param name="message">Message.</param>
        /// <param name="ok">Ok.</param>
        /// <param name="cancel">Cancel.</param>
        public static bool Message_static(string title, string message, string ok = "OK", string cancel = "")
        {
            if (Application.isPlaying)
            {
                Debug.Log("[CANNOT show (static) message when Application is Playing: Message - Title:" + title + ". Message: " + message + ".");
                return false;
            }
            Debug.Log("(static) Message - Title:" + title + "; Message: " + message + ";");
            if (cancel.Length > 0)
            {
                return EditorUtility.DisplayDialog(title, message, ok, cancel);
            }
            else {
                return EditorUtility.DisplayDialog(title, message, ok);
            }
        }

        public bool Message(string title, string message, string ok = "OK", string cancel = "")
        {
            if (Application.isPlaying)
            {
                if (enableDebugLogging)
                    Debug.Log("[CANNOT show message when Application is Playing: Message - Title:" + title + ". Message: " + message + ".");
                return false;
            }
            if (enableDebugLogging)
                Debug.Log("Message - Title:" + title + "; Message: " + message + ";");
            if (cancel.Length > 0)
            {
                return EditorUtility.DisplayDialog(title, message, ok, cancel);
            }
            else {
                return EditorUtility.DisplayDialog(title, message, ok);
            }
        }

        public int MessageComplex(string title, string message, string yes = "Yes", string no = "No", string cancel = "Cancel")
        {
            if (enableDebugLogging)
                Debug.Log("Message (Complex) - Title: " + title + "; Message: " + message + ";");
            return EditorUtility.DisplayDialogComplex(title, message, yes, no, cancel);
        }

        #endregion

        #region Components (Objects saved as Binary files)

        public bool SaveComponent(object obj, string suffix)
        {
            BinaryFormatter bf = new BinaryFormatter();
            string componentPath = Application.persistentDataPath + "/" + filename + suffix + ".gd";
            FileStream file = File.Create(componentPath);
            bool success = false;
            try
            {
                bf.Serialize(file, obj);
                success = true;
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to serialize '" + componentPath + "'. Reason: " + e.Message);
            }
            finally
            {
                file.Close();
            }
            return success;
        }

        public void SaveComponent(object obj, string customFilename, string fileType)
        {
            BinaryFormatter bf = new BinaryFormatter();
            string componentPath = Application.persistentDataPath + "/" + customFilename + fileType;
            FileStream file = File.Create(componentPath);
            try
            {
                bf.Serialize(file, obj);
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to SAVE component: '" + componentPath + "' during Deserialize. Reason: " + e.Message);
            }
            finally
            {
                file.Close();
            }
        }

        public object LoadComponent(string suffix)
        {
            object obj = null;
            if (File.Exists(Application.persistentDataPath + "/" + filename + suffix + ".gd"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                string componentPath = Application.persistentDataPath + "/" + filename + suffix + ".gd";
                FileStream file = File.Open(componentPath, FileMode.Open);
                try
                {
                    obj = bf.Deserialize(file);
                }
                catch (SerializationException e)
                {
                    Debug.LogError("Failed to LOAD component: '" + componentPath + "' during Deserialize. Reason: " + e.Message);
                }
                finally
                {
                    file.Close();
                }
            }
            return obj;
        }

        public bool DeleteComponent(string suffix)
        {
            string componentPath = Application.persistentDataPath + "/" + filename + suffix + ".gd";
            if (File.Exists(componentPath))
            {
                try
                {
                    File.Delete(componentPath);
                }
                catch
                {
                    Debug.LogError("Failed to DELETE component: '" + componentPath + "' during Deserialize.");
                }
                finally
                {
                }
                return true;
            }
            else {
                Debug.LogError("Tried to delete file that does not exist at path: " + Application.persistentDataPath + "/" + filename + suffix + ".gd");
            }
            return false;
        }

        #endregion

        #region Prefabs

        public static GameObject LoadDefaultPrefab(string path)
        {
            if (!path.Contains(".Prefab"))
                if (!path.Contains(".prefab"))
                    path += ".Prefab";
            return EditorGUIUtility.Load(path) as GameObject;
        }

        #endregion

        #region Defaults

        public static UnityEngine.Object LoadDefault(string path)
        {
            return EditorGUIUtility.Load(path);
        }

        #endregion

        #region Presets

        public EditorToolsPreset_Option PresetList(string label, string listFilePath, ref string currentItem, ref int currentItemIndex, ref List<string> presetList, ref bool showPreset, bool showLoadButton = true, bool showSaveButton = true, bool showDeleteSingleButton = true, bool showDeleteAllButton = false)
        {
            if (currentItemIndex == -1)
            {
                currentItemIndex = 0;
                if (presetList.Count > currentItemIndex)
                {
                    currentItem = presetList[currentItemIndex];
                }
            }

            string currentItemString = "empty";
            if (currentItem != null)
            {
                if (currentItem.Length > 0)
                {
                    currentItemString = currentItem;
                }
            }
            else
            {
                if (currentItemIndex > -1)
                {
                    if (presetList.Count > currentItemIndex)
                    {
                        currentItem = presetList[currentItemIndex];
                        currentItemString = currentItem;
                    }
                }
            }

            if (!ShowSection(label + " (" + currentItemString + ")", ref showPreset))
            {
                return EditorToolsPreset_Option.NoOption;
            }

            /*
            if (presetList == null)
                presetList = LoadComponent (listFilePath) as List<string>;
            else if (presetList.Count == 0)
                presetList = LoadComponent (listFilePath) as List<string>;
            */

            if (presetList == null)
            {
                presetList = new List<string>();
                //SaveComponent(presetList, listFilename);
            }
            //
            currentItem = EditorGUILayout.TextField("Preset Name", currentItem);
            //
            EditorGUI.BeginChangeCheck();
            currentItemIndex = EditorGUILayout.Popup("Preset List", currentItemIndex, presetList.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                if (presetList.Count > 0)
                {
                    currentItem = presetList[currentItemIndex];
                }
                EndSection();
                return EditorToolsPreset_Option.ItemChanged;
            }
            //  SAVE
            if (showSaveButton)
            {
                if (GUILayout.Button("Save"))
                {
                    if (currentItem == null)
                    {
                        EditorUtility.DisplayDialog("Saving Preset FAILED", "Please give the current preset a name.", "OK");
                        EndSection();
                        return EditorToolsPreset_Option.NoOption;
                    }
                    if (presetList.Count == 0)
                    {
                        return EditorToolsPreset_Option.SaveItem;
                    }
                    if (presetList.Contains(currentItem))
                    {
                        if (currentItemIndex != presetList.LastIndexOf(currentItem))
                        {
                            EditorUtility.DisplayDialog("Unable To Save Preset", "A preset already exists with that name.", "OK");
                            EndSection();
                            return EditorToolsPreset_Option.NoOption;
                        }
                    }
                    //
                    EndSection();
                    return EditorToolsPreset_Option.SaveItem;
                }
            }
            //  LOAD
            if (showLoadButton)
            {
                if (GUILayout.Button("Load"))
                {
                    EndSection();
                    return EditorToolsPreset_Option.LoadItem;
                }
            }
            //  DELETE (Single)
            if (showDeleteSingleButton)
            {
                if (GUILayout.Button("Delete (PERMANENT)"))
                {
                    if (EditorUtility.DisplayDialog("Delete Delected Preset?", "Are you sure you wish to PERMANENTLY delete this prefab?", "Yes", "No"))
                    {
                        EndSection();
                        return EditorToolsPreset_Option.DeleteItem;
                    }
                    else {
                        EndSection();
                        return EditorToolsPreset_Option.NoOption;
                    }
                }
            }
            //  DELETE (All)
            if (showDeleteAllButton)
            {
                if (GUILayout.Button("Delete ALL (PERMANENT)"))
                {
                    if (EditorUtility.DisplayDialog("Delete ALL Presets?", "Are you sure you wish to PEMANENTLY delete ALL prefabs?", "Yes", "No"))
                    {
                        EndSection();
                        return EditorToolsPreset_Option.DeleteAll;
                    }
                    else {
                        EndSection();
                        return EditorToolsPreset_Option.NoOption;
                    }
                }
            }
            //
            EndSection();
            return EditorToolsPreset_Option.NoOption;
        }

        //  SAVE / DELETE
        //  use the public functions to save presets, as it handles the whole process
        private bool SavePreset(object presetObject, string presetName, string listFilename)
        {
            return SavePreset(presetObject, PresetFilename(presetName, listFilename));
        }

        private bool SavePreset(object presetObject, string presetFilename)
        {
            return SaveComponent(presetObject, presetFilename);
        }

        private bool DeletePreset(string presetName, string listFilename)
        {
            return DeletePreset(PresetFilename(presetName, listFilename));
        }

        private bool DeletePreset(string presetFilename)
        {
            return DeleteComponent(presetFilename);
        }

        //  SAVE/DELETE W/POST
        //	These versions of functions handle the whole process, including managing the Editor GUI aspect.

        public bool SavePreset(object presetObject, string presetName, int presetIndex, ref List<string> presetList, string listFilename)
        {
            //if (SavePreset(presetObject, presetName, listFilename))
            //{
            return SavedPreset(presetName, presetIndex, ref presetList, listFilename);
            //}
            //return false;
        }

        public bool DeletePreset(string presetName, string listFilename, int presetIndex, ref List<string> presetlist)
        {
            //if (DeletePreset(presetName, listFilename))
            //{
            return DeletedPreset(presetName, presetIndex, ref presetlist, listFilename);
            //}
            //return false;
        }

        //  POST - SAVE/DELETE

        public bool SavedPreset(string currentPresetName, int currentPresetIndex, ref List<string> presetList, string listFilename)
        {
            int indexOfPreset = presetList.IndexOf(currentPresetName);
            if (indexOfPreset < 0 || indexOfPreset >= presetList.Count || presetList.Count == 0)
            {
                presetList.Add(currentPresetName);
                //SaveComponent(presetList, listFilename);
            }
            return true;
        }

        public bool DeletedPreset(string presetName, int presetIndex, ref List<string> presetList, string listFilename)
        {
            if (presetList.IndexOf(presetName) == presetIndex)
            {
                presetList.Remove(presetName);
                //SaveComponent(presetList, listFilename);
                return true;
            }
            else if (presetList.Contains(presetName))
            {
                if (enableDebugLogging)
                    Debug.Log("Attempting to delete preset:" + presetName + ". However, the index does not match. Deleting name match...");
                presetList.Remove(presetName);
                //SaveComponent(presetList, listFilename);
                return true;
            }
            else {
                return false;
            }
        }

        public string PresetFilename(string presetName, string listFilename)
        {
            return filename + listFilename + presetName;
        }

        #endregion
    }
}

#endif
