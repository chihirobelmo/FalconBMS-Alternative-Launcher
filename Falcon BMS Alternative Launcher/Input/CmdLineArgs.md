Documentation notes for BMS command-line arguments:

-mono
Opens a companion console window for the "monoprint" debug output -- this log output will also be saved in .\User\Logs subdirectory.

-ef
Short for "EyeFly" -- enables the ability for absolute control of external camera positioning.  Useful for debugging, and for capturing unique angles of action.  Consult sec. TODO for more information on EyeFly.

-acmi
Automatically begins ACMI recording, when entering 3D -- mainly useful for multiplayer hosts, especially when a dedicated, non-rendering server is used.  Warning: will result in very large, long acmi recording files.  Alternatively, use the TODO callback to toggle ACMI recording on/off.  Consult sec. TODO for more information on ACMI.

-nomovie
Skips the movie during the opening splash screen sequence.  Equivalent to `set g_bPlayIntroMovie 0` config setting.

-window
Launch the 2D menu screen in borderless-window mode -- can help workaround or avoid compatibility problems launching directly into fullscreen-exclusive mode.

-vr
Launch BMS in VR mode.  See also `g_nVRHMD` config parameter.

-novr
Launch BMS in normal flatscreen (non-VR) mode.  (Overrides the `g_nVRHMD` config parameter.)
