using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using System.Reflection;


namespace kenbu.Poinco{

    public class RuntimeApply<DataType, TargetType> : Editor 
        where DataType : ScriptableObject 
        where TargetType: UnityEngine.Object
    {

        protected DataType _saveData;

        protected  const string FILE_EXTENSION = ".asset";
        protected  const string FOLDER_PATH = "Assets/TransformCopy/Resources/";


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI ();

            EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
            EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;

            //保存したデータから変更あるか
            if (IsDirty ()) {
                EditorGUILayout.LabelField ("RuntimeApply*");
            } else {
                EditorGUILayout.LabelField ("RuntimeApply");
            }

            if (GUILayout.Button("Save")){
                Save ();
            }  
        }

        /// <summary>
        /// ファイルネーム
        /// </summary>
        /// <returns>The file name.</returns>
        protected string GetFileName(){
            var instance = target  as TargetType;
            return instance.GetInstanceID ().ToString ();
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
            _saveData = CreateInstance<DataType>();
            AssetDatabase.CreateAsset(_saveData, GetSaveDataPath());
            return _saveData;
        }

        /// <summary>
        /// 保存
        /// </summary>
        protected virtual void Save(){
            
            var targetType = target.GetType ();
            DataType d = GetOrCreateSaveData ();
            var dataType = d.GetType ();
            var fields = dataType.GetFields ();
            foreach (var filedInfo in fields) {
                var targetInfo = targetType.GetProperty (filedInfo.Name);

                var v = targetInfo.GetValue (target, null);
                filedInfo.SetValue (d, v);
            }

            AssetDatabase.Refresh();

        }



        /// <summary>
        /// 反映
        /// </summary>
        protected virtual void Apply(){
            var targetType = target.GetType ();
            DataType d = GetOrCreateSaveData ();
            var dataType = d.GetType ();
            var fields = dataType.GetFields ();
            foreach (var filedInfo in fields) {
                var targetInfo = targetType.GetProperty (filedInfo.Name);

                var v = filedInfo.GetValue (d);

                targetInfo.SetValue (target, v, null);

            }

        }

        /// <summary>
        /// 変更ありか
        /// </summary>
        /// <returns><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</returns>
        protected bool IsDirty(){
            /*
            var t = target as TargetType;
            var targetType = t.GetType ();

            DataType d = GetOrCreateSaveData ();
            var dataType = d.GetType ();
            var properties = dataType.GetProperties ();
            foreach (var propertyInfo in properties) {
                var targetInfo = targetType.GetProperty (propertyInfo.Name);
                if (propertyInfo.GetValue () != targetInfo.GetValue ()) {
                    return true;
                }
            }
            */
            return false;
        }


        protected void OnPlaymodeStateChanged(){
            //再生中に保存されてるのがあったら反映させる。
            if(EditorApplication.isPlaying == false && EditorApplication.isPlayingOrWillChangePlaymode == false){
                Apply ();
            }
        }

    }
}
