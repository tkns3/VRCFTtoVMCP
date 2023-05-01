using System;
using System.Net;
using System.Timers;
using VRCFTtoVMCP.Osc;

namespace VRCFTtoVMCP
{
    internal class VMCPSender
    {
        Timer? _timer;
        string _address = "127.0.0.1";
        int _port = 39540;
        int _span = 30;
        VRCFTParameterWeights _weights = new();

        public void Start(string address, int port, int fps)
        {
            Stop();
            _ = new IPEndPoint(IPAddress.Parse(address), port);
            _address = address;
            _port = port;
            _span = (int)(1000d / fps);
            _timer = new Timer(_span);
            _timer.AutoReset = true;
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            SendVMCP();
        }

        void SendVMCP()
        {
            var count = MessageCount.CountUpThisApp2VMC();
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff}   {count}");
            VRCFTParametersStore.CopyTo(_weights);
            using var oscClient = new OscClient(_address, _port);
            var bundle = new Bundle();
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "BrowDownLeft", _weights[VRCFTParametersV2.BrowDownLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "BrowDownRight", _weights[VRCFTParametersV2.BrowDownRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "BrowInnerUp", _weights[VRCFTParametersV2.BrowInnerUp]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "BrowOuterUpLeft", _weights[VRCFTParametersV2.BrowOuterUpLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "BrowOuterUpRight", _weights[VRCFTParametersV2.BrowOuterUpRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "CheekPuff", _weights[VRCFTParametersV2.CheekPuffSuck] > 0 ? _weights[VRCFTParametersV2.CheekPuffSuck] : 0f));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "CheekSquintLeft", _weights[VRCFTParametersV2.CheekSquintLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "CheekSquintRight", _weights[VRCFTParametersV2.CheekSquintRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeBlinkLeft", EyeBlink(_weights[VRCFTParametersV2.EyeLidLeft])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeBlinkRight", EyeBlink(_weights[VRCFTParametersV2.EyeLidRight])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeLookDownLeft", MoveDown(_weights[VRCFTParametersV2.EyeLeftY])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeLookDownRight", MoveDown(_weights[VRCFTParametersV2.EyeRightY])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeLookInLeft", MoveRight(_weights[VRCFTParametersV2.EyeLeftX]))); // LeftEye LookRight
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeLookInRight", MoveLeft(_weights[VRCFTParametersV2.EyeRightX]))); // RightEye LookLeft
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeLookOutLeft", MoveLeft(_weights[VRCFTParametersV2.EyeLeftX]))); // LeftEye LookLeft
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeLookOutRight", MoveRight(_weights[VRCFTParametersV2.EyeRightX]))); // RightEye LookRight
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeLookUpLeft", MoveUp(_weights[VRCFTParametersV2.EyeLeftY])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeLookUpRight", MoveUp(_weights[VRCFTParametersV2.EyeRightY])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeSquintLeft", _weights[VRCFTParametersV2.EyeSquintLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeSquintRight", _weights[VRCFTParametersV2.EyeSquintRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeWideLeft", EyeWide(_weights[VRCFTParametersV2.EyeLidLeft])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "EyeWideRight", EyeWide(_weights[VRCFTParametersV2.EyeLidRight])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "JawForward", MoveForward(_weights[VRCFTParametersV2.JawZ])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "JawLeft", MoveLeft(_weights[VRCFTParametersV2.JawX])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "JawOpen", _weights[VRCFTParametersV2.JawOpen]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "JawRight", MoveRight(_weights[VRCFTParametersV2.JawX])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthClose", _weights[VRCFTParametersV2.MouthClosed]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthDimpleLeft", _weights[VRCFTParametersV2.MouthDimpleLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthDimpleRight", _weights[VRCFTParametersV2.MouthDimpleRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthFrownLeft", _weights[VRCFTParametersV2.MouthFrownLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthFrownRight", _weights[VRCFTParametersV2.MouthFrownRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthFunnel", _weights[VRCFTParametersV2.LipFunnel]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthLeft", MoveLeft(_weights[VRCFTParametersV2.MouthX])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthLowerDownLeft", _weights[VRCFTParametersV2.MouthLowerDownLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthLowerDownRight", _weights[VRCFTParametersV2.MouthLowerDownRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthPressLeft", _weights[VRCFTParametersV2.MouthPressLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthPressRight", _weights[VRCFTParametersV2.MouthPressRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthPucker", _weights[VRCFTParametersV2.LipPucker]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthRight", MoveRight(_weights[VRCFTParametersV2.MouthX])));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthRollUpper", _weights[VRCFTParametersV2.LipSuckUpper]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthRollLower", _weights[VRCFTParametersV2.LipSuckLower]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthShrugUpper", _weights[VRCFTParametersV2.MouthRaiserUpper]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthShrugLower", _weights[VRCFTParametersV2.MouthRaiserLower]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthSmileLeft", _weights[VRCFTParametersV2.MouthCornerPullLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthSmileRight", _weights[VRCFTParametersV2.MouthCornerPullRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthStretchLeft", _weights[VRCFTParametersV2.MouthStretchLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthStretchRight", _weights[VRCFTParametersV2.MouthStretchRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthUpperUpLeft", _weights[VRCFTParametersV2.MouthUpperUpLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "MouthUpperUpRight", _weights[VRCFTParametersV2.MouthUpperUpRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "NoseSneerLeft", _weights[VRCFTParametersV2.NoseSneerLeft]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "NoseSneerRight", _weights[VRCFTParametersV2.NoseSneerRight]));
            bundle.Add(new Message("/VMC/Ext/Blend/Val", "TongueOut", _weights[VRCFTParametersV2.TongueOut]));
            bundle.Add(new Message("/VMC/Ext/Blend/Apply"));
            oscClient.Send(bundle);
        }

        /// <summary>
        /// VRCFTのX系パラメータの値をPerfectSyncのMoveRightの値に変換する。
        /// X系パラメータは0.0が移動なし、+1.0が右側への移動最大、-1.0が左側への移動最大を表す。
        /// PerfectSyncのMoveRightは0.0が移動なし、+1.0が右側への移動最大を表す。
        /// </summary>
        /// <param name="vrcftX">0.0が移動なし、+1.0が右側への移動最大、-1.0が左側への移動最大。</param>
        /// <returns>0.0が移動なし、+1.0が右側への移動最大</returns>
        static float MoveRight(float vrcftX)
        {
            if (vrcftX > 0)
            {
                return vrcftX;
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// VRCFTのX系パラメータの値をPerfectSyncのMoveLeftの値に変換する。
        /// X系パラメータは0.0が移動なし、+1.0が右側への移動最大、-1.0が左側への移動最大を表す。
        /// PerfectSyncのMoveLeftは0.0が移動なし、+1.0が左側への移動最大を表す。
        /// </summary>
        /// <param name="vrcftX">0.0が移動なし、+1.0が右側への移動最大、-1.0が左側への移動最大。</param>
        /// <returns>0.0が移動なし、+1.0が左側への移動最大</returns>
        static float MoveLeft(float vrcftX)
        {
            if (vrcftX > 0)
            {
                return 0;
            }
            else
            {
                return Math.Abs(vrcftX);
            }
        }

        /// <summary>
        /// VRCFTのY系パラメータの値をPerfectSyncのMoveUpの値に変換する。
        /// Y系パラメータは0.0が移動なし、+1.0が上側への移動最大、-1.0が下側への移動最大を表す。
        /// PerfectSyncのMoveUpは0.0が移動なし、+1.0が上側への移動最大を表す。
        /// </summary>
        /// <param name="vrcftY">0.0が移動なし、+1.0が上側への移動最大、-1.0が下側への移動最大。</param>
        /// <returns>0.0が移動なし、+1.0が上側への移動最大</returns>
        static float MoveUp(float vrcftY)
        {
            if (vrcftY > 0)
            {
                return vrcftY;
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// VRCFTのY系パラメータの値をPerfectSyncのMoveDownの値に変換する。
        /// Y系パラメータは0.0が移動なし、+1.0が上側への移動最大、-1.0が下側への移動最大を表す。
        /// PerfectSyncのMoveDownは0.0が移動なし、+1.0が下側への移動最大を表す。
        /// </summary>
        /// <param name="vrcftY">0.0が移動なし、+1.0が上側への移動最大、-1.0が下側への移動最大。</param>
        /// <returns>0.0が移動なし、+1.0が下側への移動最大</returns>
        static float MoveDown(float vrcftY)
        {
            if (vrcftY > 0)
            {
                return 0;
            }
            else
            {
                return Math.Abs(vrcftY);
            }
        }

        /// <summary>
        /// VRCFTのY系パラメータの値をPerfectSyncのMoveForwardの値に変換する。
        /// Z系パラメータは0.0が移動なし、+1.0が前側への移動最大、-1.0が後側への移動最大を表す。
        /// PerfectSyncのMoveForwardは0.0が移動なし、+1.0が前側への移動最大を表す。
        /// </summary>
        /// <param name="vrcftZ">0.0が移動なし、+1.0が前側への移動最大、-1.0が後側への移動最大。</param>
        /// <returns>0.0が移動なし、+1.0が前側への移動最大</returns>
        static float MoveForward(float vrcftZ)
        {
            if (vrcftZ > 0)
            {
                return vrcftZ;
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// VRCFTのEyeLid系パラメータの値をPerfectSyncのEyeWideの値に変換する。
        /// EyeLid系パラメータは0.0が目を閉じる、+0.8が目を開く(通常)、+1.0が目を大きく開くを表す。
        /// PerfectSyncのEyeWideは0.0が移動なし、+1.0が目を大きく開くの移動最大を表す。
        /// VRCFTの「EyeLid=0.0」がPerfectSyncの「EyeWide=0.0 + EyeBlink=1.0」。
        /// VRCFTの「EyeLid=0.8」がPerfectSyncの「EyeWide=0.0 + EyeBlink=0.0」。
        /// VRCFTの「EyeLid=1.0」がPerfectSyncの「EyeWide=1.0 + EyeBlink=0.0」。
        /// </summary>
        /// <param name="vrcftEyeLid">0.0が目を閉じる、+0.8が目を開く(通常)、+1.0が目を大きく開く</param>
        /// <returns>0.0が移動なし、+1.0が目を大きく開くの移動最大</returns>
        static float EyeWide(float vrcftEyeLid)
        {
            if (vrcftEyeLid < 0.8f)
            {
                return 0f;
            }
            else
            {
                return (vrcftEyeLid - 0.8f) * 5f;
            }
        }

        /// <summary>
        /// VRCFTのEyeLid系パラメータの値をPerfectSyncのEyeBlinkの値に変換する。
        /// EyeLid系パラメータは0.0が目を閉じる、+0.8が目を開く(通常)、+1.0が目を大きく開くを表す。
        /// PerfectSyncのEyeBlinkは0.0が移動なし、+1.0が目を閉じるの移動最大を表す。
        /// VRCFTの「EyeLid=0.0」がPerfectSyncの「EyeWide=0.0 + EyeBlink=1.0」。
        /// VRCFTの「EyeLid=0.8」がPerfectSyncの「EyeWide=0.0 + EyeBlink=0.0」。
        /// VRCFTの「EyeLid=1.0」がPerfectSyncの「EyeWide=1.0 + EyeBlink=0.0」。
        /// </summary>
        /// <param name="vrcftEyeLid">0.0が目を閉じる、+0.8が目を開く(通常)、+1.0が目を大きく開く</param>
        /// <returns>0.0が移動なし、+1.0が目を閉じるの移動最大</returns>
        static float EyeBlink(float vrcftEyeLid)
        {
            if (vrcftEyeLid < 0.8f)
            {
                return 1f - vrcftEyeLid * 10f / 8f;
            }
            else
            {
                return 0f;
            }
        }
    }
}
