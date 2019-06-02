# What is Falcon BMS Alternative Launcher?

Falcon BMS Alternative Launcher is a replacement for stock BMS launcher including key/axis
mapping feature. It can configure and save BMS SETUP per Joysticks. When you launch BMS
through this app, it auto-generates proper setup files and overwrites them for current
device order before BMS find them changed and initialize your setup. You don't have to worry
about SETUP mixing up DX order nor resets axis setups even if device sort or numbers have
been changed.

For instance, even if you setup BMS while connecting Logicool Driving Force Pro you don't use
for BMS, and next day you launch BMS without Driving Force Pro(or maybe with some another
device), You will see key/axis setup still remains for BMS as the app has
overwritten key file / axismapping.dat / joystick.cal to corresponds current device setups

# What is the point using the app instead of IN-GAME UI SETUP page?

You can add/remove your device environment without setting up your axis/key from scratch. You
don't have to care is the connected devices are same to when you have played BMS last
time anymore.

Moreover, Falcon BMS Alternative Launcher has quicker and easier setup UI.

# Pay attention before use.

The app will overwrite following setup files and the Registry of Falcon BMS and auto-generate
backups to User/Config/Backup at its first launch.

- User/Config/axismapping.dat

- User/Config/BMS - Full.key

- User/Config/DeviceSorting.txt

- User/Config/Falcon bms.cfg

- User/Config/joystick.cal

- User/Config/<callsign>.pop


I recommend make backups of the registry.

- HKEY_LOCAL_MACHINE¥SOFTWARE¥BenchmarkSims¥Falcon BMS 4.33 U1

- HKEY_LOCAL_MACHINE¥SOFTWARE¥Wow6432Node¥BenchmarkSims¥Falcon BMS 4.33 U1

(Export .reg file via regedit.exe, For restoring just run reg file you have backed up.)

I have checked the app by myself and asked some of my friends for testing but it may still have
some glitches I am unaware of. Please report if something is not working fine. Also if any glitches
have happened in BMS while using my launcher, try if same things happen with fresh reinstalled BMS
before contacting Official Devs.

# How to install Falcon BMS Alternative Launcher?

Make a backup of the stock Falcon BMS 4.33 U1¥Bin¥x86¥Hub.exe before installing it.
Download Falcon BMS Alternative Launcher, unzip the file and extract all of the included files to
Falcon BMS 4.33 U1¥Bin¥x86, overwrite Hub.exe. Next time you launch BMS, Falcon BMS
Alternative Launcher will be launched instead of the stock launcher.

Installing the new launcher after the clean installation of BMS is the most secure way to use this
app.

![readme01](https://user-images.githubusercontent.com/32677587/53294990-b48b6580-3834-11e9-874d-f6f0443b96e0.png)

NOTE: Just after the fresh install of BMS it does not have registry information for current
selected theater or logbook, so if you first to install BMS and want to install the
app. You have to launch BMS via stock launcher once or directory run Bin/x86/Falcon
BMS.exe then install the app.

# How to use Falcon BMS Alternative Launcher?

# ** Axis Assign Page **

![readme02](https://user-images.githubusercontent.com/32677587/53295032-7773a300-3835-11e9-8695-f4b1516343fe.png)

In Axis Assign page, you can assign axis setups.
Clicking "Assign" button popups axis setup window for each control.

Axis Assign Page is divided into 2 pages, "FlightControl" and "Avionics & Radios".

# * Axis Setup Window *

![readme03](https://user-images.githubusercontent.com/32677587/53295043-a4c05100-3835-11e9-9e48-77897990f0e4.png)

The first time you launch the window for unsigned controls, the window will flash
"AWAITING INPUTS" label. Move your joystick physical axis you want to assign to
the specific control you have selected, for instance, if you have clicked "ASSIGN"
button just next to "ROLL" control label, lean your Joystick to left or right. When the
app has detected a joystick movement, it will assign that axis to the control
automatically.

If you have mistakenly assigned a different joy axis, click "RETRY" button and
move the joy axis you want to assign again. If you want to clear the assignment,
click "RETRY" or "CLEAR" then click "SAVE" before moving any joy axes, leaving
"AWAITING INPUTS" label flashing.

The window also have a drop down box for Deadzone and Saturation settings,
invert check box, and AB / IDLE detent setters for throttle control.

# ** Key Mapping Page **

![readme04](https://user-images.githubusercontent.com/32677587/53295052-db966700-3835-11e9-8d2c-a72f876c06ec.png)

In Key Mapping page, you can assign keyboard and joystick DX/POV setups.
Double clicking specific raw to open small Key Mapping Window, then press the key or DX/POV switches to assign them to the callback.

Key Mapping Page has a drop-down list to jump the Datagrid scroll to the specific
sections, especially useful to find essential HOTAS callbacks section.

You can also setup KEYCOMBO, SHIFTED DX, BUTTON RELEASE, and INVOKE which cannot be done from stock BMS UI

When you would like to setup key/buttons for KEYCOMBO, SHIFTED DX or RELEASE to
activate callbacks, click and enable "KEYCOMBO / PINKYSHIFT" button or "RELEASE"
button. When the button has lightened up it has been enabled. Then press buttons to assign
them.


# * TIPS:

![readme05](https://user-images.githubusercontent.com/32677587/53295216-113d4f00-383a-11e9-9ea3-ab58ed0450b6.png)

This is one example of "on else off" for toggle switches. REL means "release" and INV: DN
means "INVOKE KEY DOWN"

![readme06](https://user-images.githubusercontent.com/32677587/53295226-3b8f0c80-383a-11e9-98e6-28c760ffaa9c.png)

This works for 3-way ON-OFF-ON switches

# ** Launcher Page **

![readme07](https://user-images.githubusercontent.com/32677587/53295230-60837f80-383a-11e9-98d1-615d2d9e0753.png)

Launcher page has several shortcuts for BMS itself and other tools.

<b>* Platform:</b>

Here you can select which version (32bit or 64 bit) of BMS to launch.

<b>* Theater:</b>

![readme08](https://user-images.githubusercontent.com/32677587/53295239-91fc4b00-383a-11e9-9994-aa2cc4e5025e.png)

You can select from which theater to start BMS before launching BMS from the theater
combobox. You don't have to launch BMS only to change theater and relaunch it to
avoid CTD anymore.

When you selected a theater that has its own settings executable, clicking a "Theater own config" button that appears next to the theater combobox will launch those executable. Currently
the app supports this for Israel and Ikaros theater.

<b>* Command Line:</b>

These buttons will enable/disable each launch options for BMS. For further details read
BMS-Manual.pdf 3.2.5 Launching BMS 4.33

<b>* Documentation and Manuals:</b>

Don't you know where BMS Docs exists? Click the blue "open docs folder" button
now! For BMS beginner's, I recommend starting from the fantastic Docs/Falcon BMS
Manuals/BMS-Training.pdf. If you find anything unclear about BMS UI or Multiplayer
settings etc, read BMS-Manual.pdf. 

I developed this application to skip learning BMS's bit complicated control setups but
you still have to read and learn this sim from those manuals.

<b>* Launchers:</b>

"Launch without any setup override" checkbox ignores any setup you configured at Axis
Assign Page and Key Mapping Page. Use this in case of something not working
properly regarding this app. You can use this app just as same as stock launcher then.

Weapon Delivery Planner / Mission Commander / Weather Commander / F4WX /
F4AWACS are the 3rd party tools that will not come with BMS installation. Click the
icons and you will have to select install directory first time. After that these icons will be
work as shortcuts for those apps. When you updated those apps to a latest version,
delete or move older version from local storage and app will ask for updated install
folder. If you don't have them, just click cancel button of the folder browser and the app
will open download pages for each tools.
