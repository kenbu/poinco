using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Transform))]  
public class TransformCopy : Editor {

    private Savedata _saveData;

    private  const string FILE_EXTENSION = ".asset";
    private  const string FOLDER_PATH = "Assets/TransformCopy/Resources/";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI ();

        EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
        EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;

        //保存したデータから変更あるか
        if (IsDirty ()) {
            EditorGUILayout.LabelField ("TransformApply*");
        } else {
            EditorGUILayout.LabelField ("TransformApply");
        }

        if (GUILayout.Button("Apply")){
            Save ();
        }  
    }

    /// <summary>
    /// ファイルネーム
    /// </summary>
    /// <returns>The file name.</returns>
    private string GetFileName(){
        var instance = target  as Transform;
        return instance.GetInstanceID ().ToString ();
    }

    /// <summary>
    /// ファイルパス
    /// </summary>
    /// <returns>The save data path.</returns>
    private string GetSaveDataPath(){
        return FOLDER_PATH + GetFileName () + FILE_EXTENSION;
    }

    /// <summary>
    /// セーブしたデータを取得します。
    /// </summary>
    /// <returns>The save data.</returns>
    private Savedata GetOrCreateSaveData(){
        //メモリ上にある。
        if (_saveData != null) {
            return _saveData;
        }
        //リソースにある
        _saveData = Resources.Load (GetFileName()) as Savedata;
        if (_saveData != null) {
            return _saveData;
        }
        //生成する
        _saveData = CreateInstance<Savedata>();
        AssetDatabase.CreateAsset(_saveData, GetSaveDataPath());
        return _saveData;
    }

    /// <summary>
    /// 保存
    /// </summary>
    private void Save(){
        Transform i = target  as Transform;
        Savedata s = GetOrCreateSaveData ();
        s.localPosition = i.localPosition;
        s.localScale = i.localScale;
        s.localRotation = i.localRotation;

        AssetDatabase.Refresh();
    }



    /// <summary>
    /// 反映
    /// </summary>
    private void Apply(){
        var i = target  as Transform;
        Savedata s = GetOrCreateSaveData ();
        i.localPosition = s.localPosition;
        i.localScale = s.localScale;
        i.localRotation = s.localRotation;
    }
    private bool IsDirty(){

        var i = target  as Transform;
        Savedata s = GetOrCreateSaveData ();
        return i.localPosition != s.localPosition || i.localScale != s.localScale || i.localRotation != s.localRotation;
    }


    private void OnPlaymodeStateChanged(){
        //再生中に保存されてるのがあったら反映させる。
        if(EditorApplication.isPlaying == false && EditorApplication.isPlayingOrWillChangePlaymode == false){
            Apply ();
        }
    }

}
