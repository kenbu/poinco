using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace kenbu.Poinco{

    [CustomEditor(typeof(ExtendedImage))]  
    public class ImageDataRuntimeApply : RuntimeApply<ImageData, ExtendedImage> {

    }

    public class ImageData : ScriptableObject {
        public Color color;
    }

}