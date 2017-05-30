using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine.UI;


namespace kenbu.Poinco{


    public class Poinco : EditorWindow
    {
        [MenuItem ("Window/Poinco")]
        public static void ShowWindow () {
            EditorWindow.GetWindow(typeof(Poinco));
        }

        private void OnGUI(){
            GUILayout.Label ("再生中の編集を反映させる機能です。");

            if (EditorApplication.isPlaying == false) {
                GUILayout.Label ("※再生中のみ有効");

                return;
            }
                
            //ヒエラルキーで選択されてものを探す。
            GameObject go = Selection.activeGameObject;



            if (go == null) {
                GUILayout.Label ("ヒエラルキーから選択してね。");
                return;
            }


            GUILayout.Label (GetPath(go.transform));

            if (go.GetComponent <RectTransform> () != null) {
                AddButton <RectTransformData, RectTransform>("RectTransform");
            }else {
                AddButton <TransformData, Transform>("Transform");
            }
            AddButton <GraphicData, Graphic>("Graphic.Color");
            AddButton <TextData, Text>("Text");
            AddButton <TextFormatData, Text>("TextFormat");

        }

        private void AddButton<DataType, TargetType> (string buttonName)
            where DataType : ScriptableObject 
            where TargetType: Component
        {
            var go = Selection.activeGameObject;
            if (go.GetComponent <TargetType> () != null) {
                if (GUILayout.Button (buttonName)) {
                    new RuntimeApplyExec<DataType, TargetType> (go.GetComponent <TargetType> ()).Save ();
                }
            }
        }

        private static string GetPath(Transform t){
            string path = "";
            while (t != null) {
                path =  "/" + t.name + path;
                t = t.parent;
            }
            return path;
        }

    }


    //データがた

    public class GraphicData : ScriptableObject {
        public Color color;
    }

    public class TransformData : ScriptableObject {
        public Vector3 localPosition;
        public Vector3 localScale;
        public Quaternion localRotation;
    }

    public class RectTransformData : ScriptableObject {
        public Vector2 anchoredPosition;
        public Vector3 anchoredPosition3D;
        public Vector2 anchorMax;
        public Vector2 anchorMin;
        public Vector2 offsetMax;
        public Vector2 offsetMin;
        public Vector2 pivot;
        public Vector2 sizeDelta;
        public Vector3 localScale;
        public Quaternion localRotation;
    }


    public class TextData : ScriptableObject {
        public string text;
    }
    public class TextFormatData : ScriptableObject {
        //
        // Properties
        //
        public bool alignByGeometry;

        public TextAnchor alignment;


        public Font font;

        public int fontSize;

        public FontStyle fontStyle;

        public HorizontalWrapMode horizontalOverflow;

        public float lineSpacing;


        public bool resizeTextForBestFit;

        public int resizeTextMaxSize;

        public int resizeTextMinSize;

        public bool supportRichText;

        public VerticalWrapMode verticalOverflow;

        //
    }




    public class RuntimeApplyExec<DataType, TargetType> 
        where DataType : ScriptableObject 
        where TargetType: Component
    {

        protected DataType _saveData;

        protected  const string FILE_EXTENSION = ".asset";
        protected  const string FOLDER_PATH = "Assets/Poinco/Resources/";
        protected int _instanceId;

        public RuntimeApplyExec(TargetType _instance){
            _instanceId = _instance.GetInstanceID ();
        }

        /// <summary>
        /// ファイルネーム
        /// </summary>
        /// <returns>The file name.</returns>
        protected string GetFileName(){
            return _instanceId.ToString () + ScriptableObject.CreateInstance<DataType>().ToString () + GetTarget ().name;
        }

        /// <summary>
        /// ファイルパス
        /// </summary>
        /// <returns>The save data path.</returns>
        protected string GetSaveDataPath(){
            return FOLDER_PATH + GetFileName () + FILE_EXTENSION;
        }

        /// <summary>
        /// セーブしたデータを取得します。
        /// </summary>
        /// <returns>The save data.</returns>
        protected DataType GetOrCreateSaveData(){
            //メモリ上にある。
            if (_saveData != null) {
                return _saveData;
            }
            //リソースにある
            _saveData = Resources.Load (GetFileName()) as DataType;
            if (_saveData != null) {
                return _saveData;
            }
            //生成する
            _saveData = ScriptableObject.CreateInstance<DataType>();
            AssetDatabase.CreateAsset(_saveData, GetSaveDataPath());
            return _saveData;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save(){
            var ins = GetTarget ();
            EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
            EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;

            var targetType = ins.GetType ();
            DataType d = GetOrCreateSaveData ();
            var dataType = d.GetType ();
            var fields = dataType.GetFields ();
            foreach (var filedInfo in fields) {
                var targetInfo = targetType.GetProperty (filedInfo.Name);

                var v = targetInfo.GetValue (ins, null);
                filedInfo.SetValue (d, v);
            }

            AssetDatabase.Refresh();

        }



        /// <summary>
        /// 反映
        /// </summary>
        protected virtual void Apply(){
            var ins = GetTarget ();

            if (ins == null) {
                return;
            }

            var targetType = ins.GetType ();
            DataType d = GetOrCreateSaveData ();
            var dataType = d.GetType ();
            var fields = dataType.GetFields ();
            foreach (var filedInfo in fields) {
                var targetInfo = targetType.GetProperty (filedInfo.Name);

                var v = filedInfo.GetValue (d);

                targetInfo.SetValue (ins, v, null);

            }

        }


        protected void OnPlaymodeStateChanged(){
            //再生中に保存されてるのがあったら反映させる。
            if(IsPlayComplete()){
                EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
                Apply ();
            }
        }

        protected bool IsPlayComplete(){
            return EditorApplication.isPlaying == false && EditorApplication.isPlayingOrWillChangePlaymode == false;
        }

        protected TargetType GetTarget(){
            var id = _instanceId;
            TargetType[] list = GameObject.FindObjectsOfType<TargetType> ();

            foreach (var t in list) {
                if (t.GetInstanceID () == _instanceId) {
                    return t;
                }
            }

            return null;
        }

    }

}
