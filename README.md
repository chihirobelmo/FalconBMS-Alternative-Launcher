# What is Falcon BMS Alternative Launcher?

Falcon BMS Alternative Launcher is a replacement for stock BMS launcher including key/axis
mapping feature. It can configure and save BMS SETUP per Joysticks. When you launch BMS
through this app, it auto-generates proper setup files and overwrites them for current
device order before BMS find them changed and initialize your setup. You don't have to worry
about SETUP mixing up DX order nor resets axis setups even if device sort or numbers have
been changed.

# Pay attention before use.

The app will overwrite following setup files and the Registry of Falcon BMS.
It will auto-generate backups to User/Config/Backup at its first launch.

- User/Config/axismapping.dat
- User/Config/DeviceSorting.txt
- User/Config/Falcon bms.cfg
- User/Config/joystick.cal
- User/Config/<callsign>.pop
  
# Disclaimer
  
The appearance of U.S. Department of Defense (DoD) visual information does not imply or constitute DoD endorsement

# For Developers
  
  Required SDK/Projects etc
  
  - DirectX Software Development Kit: https://www.microsoft.com/en-us/download/details.aspx?id=6812
  - Microsoft Visual Studio Installer Projects: https://marketplace.visualstudio.com/items?itemName=visualstudioclient.MicrosoftVisualStudio2017InstallerProjects
  
  - BitSwarm.exe from https://github.com/SuRGeoNix/BitSwarm
  
Put bitswarm.exe and BMSUpdate.xml at bin/Debug or bin/Release
  
Restore NuGet packages and update "references" for first launch .sln after git clone.
Also uncheck Managed Debugging Assistants -> LoaderLock.
