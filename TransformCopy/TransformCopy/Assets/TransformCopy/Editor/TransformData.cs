using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace kenbu.Poinco{

    [CustomEditor(typeof(Transform))]  
    public class TransformRuntimeApply : RuntimeApply<TransformData, Transform> {

    }

    public class TransformData : ScriptableObject {
        public Vector3 localPosition;
        public Vector3 localScale;
        public Quaternion localRotation;
    }

}