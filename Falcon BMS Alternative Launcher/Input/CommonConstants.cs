﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FalconBMS.Launcher.Input
{
    public static class CommonConstants
    {
        public const int DX_MAX_HATS = 4;//number of pov-hats supported by DirectInput, per device
        public const int DX_MAX_AXES = 8;//number of analog axes supported by DirectInput, per device
        public const int DX_MAX_BUTTONS_LEGACY = 32;
        public const int DX_MAX_BUTTONS = 128;//number of buttons supported by DirectInput, per device

        public static readonly int PRS0     = 0;
        public static readonly int PRS128   = 128;

        public static readonly int DX_PRESS         = 0;
        public static readonly int DX_PRESS_SHIFT   = 1;
        public static readonly int DX_RELEASE       = 2;
        public static readonly int DX_RELEASE_SHIFT = 3;

        public static readonly int KEYBOARD_KEYLENGTH = 238;

        public static readonly int POV360   = 36000;
        public static readonly int POV45    = 36000 / 8;

        public static readonly int AXISMIN = 0;
        public static readonly int AXISMAX = 65536;

        public static readonly int FLUSHTIME1 = 900;
        public static readonly int FLUSHTIME2 = 1500;

        public static readonly int BINAXISMIN = 0;
        public static readonly int BINAXISMAX = 15000;

        public static readonly string BMS_FULL = "BMS - Full";
        public static readonly string BMS_AUTO = "BMS - Auto";

        public static readonly int JOYNUMOFFSET = 2;
        public static readonly int JOYNUMUNASSIGNED = -1;
//        public static readonly int JOYNUMMOUSEWHEEL = -2;

        public static readonly string CFGOVERRIDECOMMENT = "// SETUP OVERRIDE";

        public static readonly string DEFAULTPILOTNAME = "Joe Pilot";
        public static readonly string DEFAULTCALLSIGN = "Viper";

        public static readonly string SETUPV100 = "Setup.v100.";
        public static readonly string STOCKXML = " {Stock}.xml";
//REVIEW: dead code? //public static readonly string MOUSEXML = " Mousewheel.xml";
        public static readonly string LOGCAT = "bms-logcat.exe";
        public static readonly string CFGFILE = "Falcon BMS.cfg";
        public static readonly string USERCFGFILE = "Falcon BMS User.cfg";

        public static readonly string STOCKFOLDER = "/Stock/";
        public static readonly string CONFIGFOLDER = "/User/Config/";
        public static readonly string BACKUPFOLDER = "/User/Config/Backup/";
        public static readonly string LAUNCHERFOLDER = "/Launcher";
        public static readonly string CONFIGFOLDERBACKSLASH = "\\User\\Config\\";

        public const string F15_TAG = "F15ABCD";

        public static readonly string SIMDONOTHING = "SimDoNothing";

        public static readonly SolidColorBrush LIGHTBLUE  = new SolidColorBrush(Color.FromArgb(0x80, 0x38, 0x78, 0xA8));
        public static readonly SolidColorBrush LIGHTRED   = new SolidColorBrush(Color.FromArgb(0x80, 240, 0, 0));
        public static readonly SolidColorBrush LIGHTGREEN = new SolidColorBrush(Color.FromArgb(0x80, 0, 240, 0));
        public static readonly SolidColorBrush GREYBLUE   = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0xD9, 0xEA));
        public static readonly SolidColorBrush WHITEILUM  = new SolidColorBrush(Color.FromArgb(0xFF, 0xF7, 0xF7, 0xF7));
        public static readonly SolidColorBrush BLUEILUM   = new SolidColorBrush(Color.FromArgb(255, 128, 255, 255));
    }
}
