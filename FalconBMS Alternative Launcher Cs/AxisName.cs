using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconBMS_Alternative_Launcher_Cs
{
    public enum AxisName
    {
        Roll,
        Pitch,
        Yaw,
        Throttle,
        Throttle_Right,
        Toe_Brake,
        Toe_Brake_Right,
        Trim_Roll,
        Trim_Pitch,
        Trim_Yaw,
        Radar_Antenna_Elevation,
        Cursor_X,
        Cursor_Y,
        Range_Knob,
        HUD_Brightness,
        Reticle_Depression,
        HMS_Brightness,
        FLIR_Brightness,
        Intercom,
        COMM_Channel_1,
        COMM_Channel_2,
        MSL_Volume,
        Threat_Volume,
        AI_vs_IVC,
        FOV,
        Camera_Distance
    }

    public enum AxisNumName
    {
        Axis_X = 0,
        Axis_Y = 1,
        Axis_Z = 2,
        Rotation_X = 3,
        Rotation_Y = 4,
        Rotation_Z = 5,
        Slider_0 = 6,
        Slider_1 = 7
    }
}
