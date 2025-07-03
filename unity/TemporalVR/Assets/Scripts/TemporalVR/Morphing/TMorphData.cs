using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TemporalVR
{
    [System.Serializable]
    public class TMorphData
    {
        public string morphName;
        public List<TKeyframe> morphSequence;
        public AnimationCurve interpolationCurve;

        // ES3 File Save
        public void SaveToFile(string filename)
        {
            ES3.Save("morphData", this, filename);
        }

        public static TMorphData LoadFromFile(string filename)
        {
            if (ES3.FileExists(filename))
            {
                return ES3.Load<TMorphData>("morphData", filename);
            }
            return null;
        }
    }
}