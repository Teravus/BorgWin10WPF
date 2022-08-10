# BorgWin10WPF
 Unofficial, remake of the Star Trek: Borg game engine in a Windows 10 Compatible executable.  This is a remake, not reimplementation. (Contents needed from the original media to 'reimplement' the game are missing, therefore, I can't really call it a reimplementation because I can't really deduce how it was made from the contents of the original media.)  Semantics aside;

 The goal is to allow you to play the game on newer computers with contents from the original media.
 
 You still need the original media to play.  This will not work without the content from the original disks.   No, I will not provide a copy of the disks or the disk content…  
 but you can get them on eBay for pretty cheap now if you don't already have them.

 This is still a work in progress.  Do not expect a working game.

 Based on original media content files, the windows 95 version of the Borg game engine is derived from the windows 95 Klingon engine. They have a common heritage, as a result, many of the ways things are done in the remake of Star Trek: Klingon also work with Borg.

 A few things to note that we learned from Klingon;
  - The engine uses frames of the original media video content when specifying time codes.
  - The original game video media had a variable frame rate that tried to achieve 15fps.  In reality, it came close at about 14.9fps.
  - There is a scene cut sheet that defines the boundaries of chapters within the original media.
  - The cut sheet names scenes with a specific name that it uses later as keys for actions and offsets for frames that define when a hotspot should be active.
  - Hotspots starting and ending frames for the Main media are assumed to be offset from the starting point of the chapter
  - To reimplement the game, you have enough information when you combine the scene cut-sheet, the hotspot definitions, and the original game video. (This one isn't entirely true with Borg. While many assumptions still apply; There is no scene cut-sheet file in the original media. The scene cut-sheet is actually a compiled-in const array. Also, multi-click seems to be custom handled in Borg instead of 'configured' in the hotspots like in Klingon. Idle scenes are undefined in the cut-sheet and hotspots. Info hotspots are globally timed, not based on chapter.)

---
 
## Build
This was built using Visual Studio 2019 Community Edition.  (freely downloadable)
Open the 'sln' file.   Make sure the nuget packages get restored.  Pick your platform.  Build.

The program will be built and put in the bin/x64/Debug folder.  If you build it for x86, it will be in the bin/x86/Debug folder.

---

## Media Setup
Once you have built this...  
This game doesn't work without some of the original media from the disks.
Inside the built folder, there is a folder called CDAssets.

From the original disks, you need to copy some of the AVI files to the CDAssets Folder.

From Disk 1:

- HOLODECK.AVI
- IP.AVI 
- MAIN_1.AVI

From Disk 2: 
- MAIN_2.AVI

From Disk 3: 
- MAIN_3.AVI

From the KlingonWin10WPF\CDAssets Folder:
- ffmpeg folder 
- prepareVideos.bat 

---
Once you have all of those files in the CDAssets folder.  
Run prepareVideos.bat.   
It should make X versions of 4 of the videos MAIN_1X.AVI, IPX.AVI, MAIN_2X.AVI, and MAIN_3X.AVI.  
The X versions have the audio re-synchronized.

At that point you should be able to run the game test.

## Playing The Game

Just like the original game, it doesn't give you much information about how to play it.  

- Double click the video or press spacebar to pause the Holodeck program.  Double click or press space bar again to continue the Holodeck program.
- When you're in holodeck mode, you can click some things and the computer will tell you about them.
- Control the volume with the + and - buttons on the keyboard.
- Press 'S' on the keyboard to Save
- Press 'Q' on the keyboard to Quit (It won't ask you to save so watch out!)


## Debugging The Game

There is a debug overlay that you can load by pressing the Accent/Tilde button(`) on a US keyboard which gives you information about what hotspots are available and where your last click was in a mini square in the top left of the video and allows you to navigate around the game easily.
With the debug overlay open, and the video the last thing that you clicked on, 
Press 'C' to jump to the next challenge.
Press 'M' to jump 15 seconds forward in the video.
Press 'N' to jump 15 seconds back in the video.

## Legal Notice (Taken from game)

The original game:  Star Trek:(TM) Borg(tm) is published by Simon & Schuster Interactive, 
a division of Simon & Schuster, 
in the publishing operation of Viacom Inc. 
1230 Avenue of the Americas, New York, NY 10020

Star Trek(TM) & (C) 1996 Paramount Pictures.  
All Rights Reserved. 

STAR TREK and Related Properties are Trademarks of Paramount Pictures. 
(c) 1996 Simon & Schuster Interactive, a division of Simon & Schuster, Inc.

TrueMotion(R) is a registered trademark of The Duck Corporation.

Windows is a trademark and Microsoft is a registered trademark of Microsoft Corporation.

---

The remake of the FMV engine is entirely new and includes none of the game assets or content.  Simon & Schuster never released code for the original edition. 

Also, some corporate history for the license holders;
After this was released, The Duck Corporation changed their name to On2 Technologies and Google acquired On2 and used the technology to develop webm.

Also, the Simon & Schuster Interactive division was shuttered in 2003.  The parent company went through several hands, Paramount, Viacom, CBS Corporation. Currently it is held by Paramount Global with a pending sale to Penguin Random House.  The sale is blocked with an antitrust lawsuit by USDOJ.
From: https://en.wikipedia.org/wiki/Simon_%26_Schuster .   In theory, they /could/ remake the original game, but it is highly unlikely.

---

The code for the engine is released under the MIT license. It makes use of libvlc and libvlcsharp as a video player which is released under the GNU Lesser GPL license, version 2.1. https://github.com/videolan/libvlcsharp/ 

If you need to contact me about this software or a legal issue regarding this software, please do so at Teravus at gmail dot com.  Or Discord: RebootTech#6247. The intent is to make this engine free to use and distribute, however, it only works long as you have the original licensed media.

I also tend to stream on Twitch on Thursdays at around 4:30 PM Pacific time under the screen name 'RebootTech'.
