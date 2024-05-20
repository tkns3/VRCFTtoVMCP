using System;
using System.Collections;
using System.Collections.Generic;

namespace VRCFTtoVMCP
{
    internal static class VRCFTParametersStore
    {
        private static object _LockObject = new();
        private static VRCFTParameterWeights _Weights = new();

        public static void SetWeight(VRCFTParametersV2 param, float value)
        {
            lock (_LockObject)
            {
                _Weights[param] = value;
            }
        }

        public static float GetWeight(VRCFTParametersV2 param)
        {
            float tmp;
            lock (_LockObject)
            {
                tmp = _Weights[param];
            }
            return tmp;
        }

        public static void CopyTo(VRCFTParameterWeights weights)
        {
            lock (_LockObject)
            {
                _Weights.CopyTo(weights);
            }
        }
    }

    internal class VRCFTParameterWeights: IEnumerable<float>
    {
        private float[] _Weights = new float[(int)VRCFTParametersV2.Max];

        public float this[VRCFTParametersV2 param]
        {
            get
            {
                return Math.Clamp(_Weights[(int)param], -1.0f, 1.0f);
            }
            set
            {
                _Weights[(int)param] = value;
            }
        }

        public float this[int param]
        {
            get
            {
                return Math.Clamp(_Weights[param], -1.0f, 1.0f);
            }
            set
            {
                _Weights[param] = value;
            }
        }

        public int Length
        {
            get
            {
                return _Weights.Length;
            }
        }

        public void CopyTo(VRCFTParameterWeights weights)
        {
            this._Weights.CopyTo(weights._Weights, 0);
        }

        public IEnumerator<float> GetEnumerator()
        {
            return ((IEnumerable<float>)_Weights).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Weights.GetEnumerator();
        }

        public float Original(VRCFTParametersV2 param)
        {
            return _Weights[(int)param];
        }

        public float Original(int param)
        {
            return _Weights[param];
        }

        public override string ToString()
        {
            string text = "";

            for (int i = 0; i < _Weights.Length; i++)
            {
                text += $"{(VRCFTParametersV2)i}={_Weights[i]:+0.000;-0.000;+0.000},";
            }

            return text;
        }
    }
}
