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

            if (rollAxis.GetDeviceNumber() == throttleAxis.GetDeviceNumber())
            {
                return;
            }
            cfg.Write("set g_nNumOfPOVs 2 " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV1DeviceID " + (rollAxis.GetDeviceNumber() + 2) + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV1ID 0 " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV2DeviceID " + (throttleAxis.GetDeviceNumber() + 2) + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV2ID 0 " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
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

                // Write pov-hat bindings, for the primary steering and/or throttle device(s).  Because BMS interprets these keyfile entries  
                // per DeviceSorting.txt file order, we must take care to emit the pov-hat entries for stick and throttle in that same order.
                // ie. pov-hat #0/2 for the earlier device in DeviceSorting-order; pov-hat #1/3 for the latter device in DeviceSorting-order.
                InGameAxAssgn rollAxis = (InGameAxAssgn)inGameAxis[AxisName.Roll.ToString()];
                InGameAxAssgn throttleAxis = (InGameAxAssgn)inGameAxis[AxisName.Throttle.ToString()];

                int povBase = 0;
                for (int i = 0; i < joyAssgns.Length; i++)
                {
                    JoyAssgn joy = joyAssgns[i];

                    if (i == rollAxis.GetDeviceNumber())
                    {
                        int hatId = 0;
                        if (povBase > 0 && throttleAxis.GetDeviceNumber() == rollAxis.GetDeviceNumber())
                            hatId = 1;
                        sw.Write(joy.GetKeyLinePOV(povBase++, hatId));
                    }

                    if (i == throttleAxis.GetDeviceNumber())
                    {
                        int hatId = 0;
                        if (povBase > 0 && throttleAxis.GetDeviceNumber() == rollAxis.GetDeviceNumber())
                            hatId = 1;
                        sw.Write(joy.GetKeyLinePOV(povBase++, hatId));
                    }

                    if (povBase >= 2) break;
                }
            }

        }
    }

}
