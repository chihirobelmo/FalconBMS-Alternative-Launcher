using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using FalconBMS.Launcher.Input;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Override
{
    public class OverrideSettingFor434U1 : OverrideSettingFor434
    {
        public OverrideSettingFor434U1(MainWindow mainWindow, AppRegInfo appReg) : base(mainWindow, appReg)
        {
        }

        protected override void OverridePovDeviceIDs(StreamWriter cfg, Hashtable inGameAxis)
        {
            InGameAxAssgn rollAxis = (InGameAxAssgn)inGameAxis[AxisName.Roll.ToString()];
            InGameAxAssgn throttleAxis = (InGameAxAssgn)inGameAxis[AxisName.Throttle.ToString()];

            if (rollAxis.IsAssigned() == false)
                return;

            bool hasThrottleAxis = throttleAxis.IsAssigned();

            bool singleDevice = ( 
                throttleAxis.IsAssigned() == false || 
                throttleAxis.GetDeviceNumber() == rollAxis.GetDeviceNumber()
                );

            // Because BMS only supports a maximum of 2 pov-hats (same device or different devices), we are imposing a
            // simplifying constraint -- that pov1 will always map to the pitch/roll device, and pov2 will always map
            // to throttle device (if throttle axis is mapped).
            // Note 1: these may be 2 hats on the same device
            // Note 2: it's somewhat common for throttle axis to be unassigned (ie. buttons or the new gamepad triggers)
            // Note 3: for some unexplained historical quirk of BMS, the device-numbering here starts at '2'.
            cfg.Write("set g_nNumOfPOVs 2 " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV1DeviceID " + (rollAxis.GetDeviceNumber() + 2) + " " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV1ID 0 " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV2DeviceID " + ((hasThrottleAxis ? throttleAxis : rollAxis).GetDeviceNumber() + 2) + " " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV2ID " + (singleDevice ? "1" : "0") + " " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            return;
        }

        protected override void WriteKeyLines(string filename, Hashtable inGameAxis, KeyFile keyFile, JoyAssgn[] joyAssgns)
        {
            using (StreamWriter sw = Utils.CreateUtf8TextWihoutBom(filename))
            {
                sw.NewLine = "\n"; // probably not necessary, but for consistency with existing keyfile serialization code that hardcodes "\n" everywhere

                // Write keybd bindings.
                for (int i = 0; i < keyFile.keyAssign.Length; i++)
                    sw.Write(keyFile.keyAssign[i].GetKeyLine());

                // Write button bindings.
                for (int i = 0; i < joyAssgns.Length; i++)
                {
                    JoyAssgn joy = joyAssgns[i];
                    sw.Write(joy.GetKeyLineDX(i, joyAssgns.Length));
                }

                //TEST plan:
                // [pass] 2 devices 2 hats - stick first in DeviceSorting.txt order
                // [pass] 2 devices 2 hats - throttle first in DeviceSorting.txt order
                // [pass] 2 devices w hats, but roll+throttle on primary (only 1 hat should work)
                // [?] 1 device with roll+throttle, but 2 hats (is there such a product?)
                // [pass] 1 device with roll+throttle, single hat
                // [pass] 1 device with roll, throttle=xInput
                // [pass] 0 analog axes (eg. new startup)

                // Write pov-hat bindings, for the primary steering and/or throttle device(s).  See notes above, in OverridePovDeviceIDs().
                InGameAxAssgn rollAxis = (InGameAxAssgn)inGameAxis[AxisName.Roll.ToString()];
                InGameAxAssgn throttleAxis = (InGameAxAssgn)inGameAxis[AxisName.Throttle.ToString()];

                if (rollAxis.IsAssigned() == false)
                    return;

                bool singleDevice = (
                    throttleAxis.IsAssigned() == false ||
                    throttleAxis.GetDeviceNumber() == rollAxis.GetDeviceNumber()
                    );

                JoyAssgn joyStick = rollAxis.GetJoy();
                JoyAssgn joyThrottle = throttleAxis.GetJoy();

                // Map first hat on physical flightstick device => BMS "first pov id".
                sw.Write(joyStick.GetKeyLinePOV(povBase:0, hatId:0));

                // Either 2nd hat on flightstick device, or first hat on physical throttle device => BMS "second pov id".
                if (singleDevice)
                    sw.Write(joyStick.GetKeyLinePOV(povBase: 1, hatId: 1));
                else
                    sw.Write(joyThrottle.GetKeyLinePOV(povBase: 1, hatId: 0));
            }
            return;
        }

    }
}
