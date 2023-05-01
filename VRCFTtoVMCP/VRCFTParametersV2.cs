using System.Collections.Generic;

namespace VRCFTtoVMCP
{
    public enum VRCFTParametersV2
    {
        // Eye Tracking Parameters
        EyeX,
        EyeY,
        EyeLeftX,
        EyeLeftY,
        EyeRightX,
        EyeRightY,
        EyeLidRight,
        EyeLidLeft,
        EyeLid,
        EyeSquintRight,
        EyeSquintLeft,
        EyeSquint,
        PupilDilation,
        PupilDiameterRight,
        PupilDiameterLeft,

        // Expression Tracking Parameters
        BrowPinchRight,
        BrowPinchLeft,
        BrowLowererRight,
        BrowLowererLeft,
        BrowInnerUpRight,
        BrowInnerUpLeft,
        BrowOuterUpRight,
        BrowOuterUpLeft,
        NasalDilationRight,
        NasalDilationLeft,
        NasalConstrictRight,
        NasalConstrictLeft,
        CheekSquintRight,
        CheekSquintLeft,
        CheekPuffSuckRight,
        CheekPuffSuckLeft,
        JawOpen,
        MouthClosed,
        JawX,
        JawZ,
        JawClench,
        JawMandibleRaise,
        LipSuckUpperRight,
        LipSuckUpperLeft,
        LipSuckLowerRight,
        LipSuckLowerLeft,
        LipSuckCornerRight,
        LipSuckCornerLeft,
        LipFunnelUpperRight,
        LipFunnelUpperLeft,
        LipFunnelLowerRight,
        LipFunnelLowerLeft,
        LipPuckerUpperRight,
        LipPuckerUpperLeft,
        LipPuckerLowerRight,
        LipPuckerLowerLeft,
        MouthUpperUpRight,
        MouthUpperUpLeft,
        MouthLowerDownRight,
        MouthLowerDownLeft,
        MouthUpperDeepenRight,
        MouthUpperDeepenLeft,
        NoseSneerRight,
        NoseSneerLeft,
        MouthUpperX,
        MouthLowerX,
        MouthCornerPullRight,
        MouthCornerPullLeft,
        MouthCornerSlantRight,
        MouthCornerSlantLeft,
        MouthDimpleRight,
        MouthDimpleLeft,
        MouthFrownRight,
        MouthFrownLeft,
        MouthStretchRight,
        MouthStretchLeft,
        MouthRaiserUpper,
        MouthRaiserLower,
        MouthPressRight,
        MouthPressLeft,
        MouthTightenerRight,
        MouthTightenerLeft,
        TongueOut,
        TongueX,
        TongueY,
        TongueRoll,
        TongueArchY,
        TongueShape,
        TongueTwistRight,
        TongueTwistLeft,
        SoftPalateClose,
        ThroatSwallow,
        NeckFlexRight,
        NeckFlexLeft,

        // Addtional Simplified Tracking Parameters
        BrowDownRight,
        BrowDownLeft,
        BrowOuterUp,
        BrowInnerUp,
        BrowUp,
        BrowExpressionRight,
        BrowExpressionLeft,
        BrowExpression,
        MouthSmileRight,
        MouthSmileLeft,
        MouthSadRight,
        MouthSadLeft,
        JawY,
        CheekSquint,
        CheekPuffSuck,
        MouthX,
        LipSuckUpper,
        LipSuckLower,
        LipSuck,
        LipFunnelUpper,
        LipFunnelLower,
        LipFunnel,
        LipPuckerRight,
        LipPuckerLeft,
        LipPucker,
        MouthUpperUp,
        MouthLowerDown,
        MouthOpen,
        NoseSneer,
        SmileFrownRight,
        SmileFrownLeft,
        SmileFrown,
        SmileSadRight,
        SmileSadLeft,
        SmileSad,

        Max
    }

    public static class VRCFTParametersV2Parser
    {
        static readonly Dictionary<string, VRCFTParametersV2> _dic = new();

        public static VRCFTParametersV2 Parse(string value)
        {
            if (_dic.Count == 0)
            {
                for (int i = 0; i < (int)VRCFTParametersV2.Max; i++)
                {
                    _dic.Add($"/avatar/parameters/v2/{(VRCFTParametersV2)i}", (VRCFTParametersV2)i);
                }
            }
            if (_dic.TryGetValue(value, out var param))
            {
                return param;
            }
            else
            {
                return VRCFTParametersV2.Max;
            }
        }

        public static bool TryParse(string value, out VRCFTParametersV2 param)
        {
            param = Parse(value);
            return param != VRCFTParametersV2.Max;
        }
    }
}
