using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TemporalVR
{
    public class TMorphTest : TMorphObj
    {
        [Header("Test Configuration")]
        public bool autoGenerateKeyframes = true;
        public GameObject[] keyframeMeshes;  // 인스펙터에서 할당

        void Start()
        {
            if (autoGenerateKeyframes && keyframeMeshes.Length > 0)
            {
                GenerateKeyframesFromObjects();
                if (autoSave) SaveMorphData();
            }
            else if (ES3.KeyExists(saveKey + gameObject.name + "_keyframes"))
            {
                LoadMorphData();
            }
        }

        void GenerateKeyframesFromObjects()
        {
            keyframes.Clear();
            float timeStep = 10f / (keyframeMeshes.Length - 1);

            for (int i = 0; i < keyframeMeshes.Length; i++)
            {
                MeshFilter mf = keyframeMeshes[i].GetComponent<MeshFilter>();
                if (mf != null)
                {
                    TKeyframe kf = new TKeyframe();
                    kf.time = i * timeStep;
                    kf.CaptureFromMesh(mf.sharedMesh);
                    keyframes.Add(kf);
                }
            }
        }
    }
}