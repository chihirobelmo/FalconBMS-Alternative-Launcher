# BitSwarm: Bittorrent library (.net standard) for clients & streaming purposes

## [Introduction]
BitSwarm implements __Bittorrent Protocol v2__ from scratch to achieve best performance and to expose low-level parameters for customization. It uses a custom thread pool and part files (<a href="https://github.com/SuRGeoNix/APF">APF</a>) for fast resume from previous incomplete session.

## [Supports]
* Inputs (torrent file, magnet link, SHA1/Base32 hash & session file)
* Automatic Save & Load from a previous incomplete session
* XML Import/Export Options for Timing/Feeding & Logging Configuration
* Feeders (DHT, PEX, Trackers & External Trackers File)
* Peers Communication (autonomous life-cycle with minimal dropped bytes)
* Piece SHA1 validation & phony packets / broken clients protection
* Start/End Game modes (fast metadata retrieval, initial boosting & closure)
* Sleep mode for minimal resources usage (disables feeders & uses half threads based on specify download rate)
* Focus Areas to bypass normal pieces selection (for streaming)
* Dynamic change of options (eg) for streaming - useful for changing timeouts/retries during seeking/buffering for fast resets on requested pieces)

## [Todo]
* uTP
* IPv6 DHT/Trackers
* Download / Upload Rate Limits
* Uploading / Seeding
* VPN / Proxy / Encryption
* NAT Traversal / PnP / Hole-punching

## [Examples]

### 1. Library Overview
``` c#
// Step 0: Prepare BitSwarm's Options (Timeouts/Feeders/Logging etc.)

Options opt = new Options();
	//opt.X = Y; check Options.cs for all available options

// Step 1: Create BitSwarm Instance

	// With Default Options
BitSwarm bitSwarm = new BitSwarm();
	// With Custom Options
BitSwarm bitSwarm = new BitSwarm(opt);

// Step 2: Subscribe events

	// Receives torrent data (on torrent file/session will fire directly, on magnetlink/hash will fire on metadata received - notify user with torrent detail and optionally choose which files to include)
bitSwarm.MetadataReceived   += BitSwarm_MetadataReceived; 	// e.Torrent
	// Receives statistics (refresh every 2 seconds - notify user with the current connections/bytes/speed of downloading)
bitSwarm.StatsUpdated       += BitSwarm_StatsUpdated;		// e.Stats
	// Notifies with the new status (notify user with 0: Finished, 1: Stopped, 2: Error)
bitSwarm.StatusChanged      += BitSwarm_StatusChanged;		// e.Status
	// Notifies that is going to stop (user can prevent it from finishing, by including other previously excluded files)
bitSwarm.OnFinishing        += BitSwarm_OnFinishing;		// e.Cancel

// Step 3: Open input (Current BitSwarm's valid inputs Torrent File/Magnet Link/SHA1 Hash/Base32 Hash/Session File)

	// Open Torrent File
bitSwarm.Open("/home/surgeonix/ubuntu.torrent");
	// Open Magnet Link
bitSwarm.Open("magnet:?xt=urn:btih:D1101A2B9D202811A05E8C57C557A20BF974DC8A");
	// Open SHA1 Hash
bitSwarm.Open("D1101A2B9D202811A05E8C57C557A20BF974DC8A");
	// Open Base32 (SHA1) Hash
bitSwarm.Open("RX46NCATYQRS3MCQNSEXVZGCCDNKTASQ");
	// Open BitSwarm's .bsf Session File (BitSwarm will search automatically when you provide other inputs for an existing session file based on SHA1 hash)
bitSwarm.Open("/home/surgeonix/.bitswarm/.sessions/D1101A2B9D202811A05E8C57C557A20BF974DC8A.bsf");

// Step 4: Start downloading
bitSwarm.Start();

// Step 5: Dispose when you are done
bitSwarm.Dispose();
```

### 2. <a href="https://github.com/SuRGeoNix/BitSwarm/blob/master/BitSwarm%20(Console%20Core%20Demo)/Program.cs">BitSwarm</a>: Console Client (.NET Core)
<p align="center"><img src="Images/bitswarm.gif"/></p>

### 3. <a href="https://github.com/SuRGeoNix/BitSwarm/blob/master/BitSwarm%20(WinForms%20Demo)/frmMain.cs">BitSwarm</a>: GUI Client (.NET Framework Winforms)
<p align="center"><img src="Images/bitswarm_gui.png"/></p>

### 4. <a href="https://github.com/SuRGeoNix/Flyleaf">Flyleaf</a>: Video Player & Torrent Streamer (.NET Framework Winforms)
<p align="center"><img src="Images/flyleaf.png"/></p>

## Remarks

This project has been created for fun and educational reasons. Hopefully, it will help other developers to understand deeper the bittorrent protocol as the documentations and standards are very complicated and messy. I tried to add detailed comments within the code. Don't use it as is, it does not currently implement upload and sharing, which means that it is an arrogant and selfish beggar!

| Logs Sample | Stats Sample 1 |
| :-------------:       |:-------------:            |
| <a href="Images/logs1.png"><img src="Images/logs1.png" width="50%" heigt="50%"/></a> | <a href="Images/stats1.png"><img src="Images/stats1.png" width="50%" heigt="50%"/></a> |

| Stats Sample 2 | Stats Sample 3 |
| :-------------:       |:-------------:            |
| <a href="Images/stats2.png"><img src="Images/stats2.png" width="50%" heigt="50%"/></a> | <a href="Images/stats3.png"><img src="Images/stats3.png" width="50%" heigt="50%"/></a> |