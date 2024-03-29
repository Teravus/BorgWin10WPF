# BorgWin10WPF
 Unofficial, remake of the Star Trek: Borg game engine in a Windows 10 Compatible executable.  This is a remake, not reimplementation. (Contents needed from the original media to 'reimplement' the game are missing, therefore, I can't really call it a reimplementation because I can't really deduce how it was made from the contents of the original media.)  Semantics aside;

 The goal is to allow you to play the game on newer computers with contents from the original media.
 
 You still need the original media to play.  This will not work without the content from the original disks.   No, I will not provide a copy of the disks or the disk content…  
 but you can get them on eBay for pretty cheap now if you don't already have them.

 So..   this is playable now with the original media.

![Game Engine Screen Shot 0](https://raw.githubusercontent.com/Teravus/BorgWin10WPF/main/BorgWin10WPF/Assets/Game_Engine_Screen_shot.png)
![Game Engine Screen Shot 1](https://raw.githubusercontent.com/Teravus/BorgWin10WPF/main/BorgWin10WPF/Assets/Game_Engine_Screen_shot2.png)

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
Open the 'sln' file.   Make sure the nuget packages get restored.  Pick your platform (either x86 or x64, but don't leave it on 'Any').  Build.  

The program will be built and put in the bin/x64/Debug folder.  If you build it for x86, it will be in the bin/x86/Debug folder.

---

## Downloads of the already built application
Not everyone can or is willing to build the game from source.  In that case youc an download the Main application (but not the original game content) from the released tags. We're currently on Alpha-2 [Download Link](https://github.com/Teravus/BorgWin10WPF/releases/tag/Alpha-2). Again, I want to reiterate that this download contains no game files. You must have or obtain the original game media and follow the import procedure in the ReadMe file in the ZIP or source to play the game. I am unable to provide the original media.

---

## Media Setup
Once you have built this...  
This game doesn't work without some of the original media from the disks.
Inside the built folder, there is a folder called CDAssets.

From the original disks, you need to copy some of the AVI files to the CDAssets Folder.

From Disk 1:

- LOGO.AVI
- IP.AVI 
- MAIN_1.AVI

From Disk 2: 
- MAIN_2.AVI

From Disk 3: 
- MAIN_3.AVI

From the BorgWin10WPF\CDAssets Folder:
- ffmpeg folder 
- prepareVideos.bat 

---
Once you have all of those files in the CDAssets folder.  
Run prepareVideos.bat.   
It should make X versions of 5 of the videos MAIN_1X.AVI, IPX.AVI, MAIN_2X.AVI, MAIN_3X.AVI, and LOGOX.AVI.  
The X versions have the audio re-synchronized so LibVLC can play them in sync with the video.  (The Duck TrueMotion codec is a very early version of webm and is barely supported)

At that point you should be able to run the game test.  The game will check for the videos and let you know if you're missing one when you attempt to start a new game.

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

Also, the Simon & Schuster Interactive division was shuttered in 2003.  The parent company went through several hands, Paramount, Viacom, CBS Corporation. Paramount Global tried to sell it to Penguin Random House, however, the sale was blocked with an antitrust lawsuit by USDOJ.  In Aug 7, 2023, Simon & Schuster was sold to Kohlberg Kravis Roberts & Co (KKR) Private Equity.

From: https://en.wikipedia.org/wiki/Simon_%26_Schuster   
From: https://www.hollywoodreporter.com/business/business-news/simon-schuster-sold-paramount-1235526542/

It is highly unlikely that any of the parties involved will remake the original game.

--

## Additional Legal Notice from https://www.startrek.com/fan-films
While this isn't a fan film, CBS/Paramount would like fan made projects to have an explicit disclaimer regarding trademarks, logos and other proprties held by CBS/Paramount Pictures and other Star Trek Franchises.

Star Trek and all related marks, logos and characters are solely owned by CBS Studios Inc. This fan project is not endorsed by, sponsored by, nor affiliated with CBS, Paramount Pictures, or any other Star Trek franchise, and is a non-commercial fan-made recreation of a software application from scratch intended for recreational use. No commercial exhibition or distribution is permitted. No alleged independent rights will be asserted against CBS or Paramount Pictures.

---

The code for the engine is released under the MIT license. It makes use of libvlc and libvlcsharp as a video player which is released under the GNU Lesser GPL license, version 2.1. https://github.com/videolan/libvlcsharp/ 

If you need to contact me about this software or a legal issue regarding this software, please do so at Teravus at gmail dot com.  Or Discord: RebootTech#6247. The intent is to make this engine free to use and distribute, however, it only works as long as you have the original licensed media.

I also tend to stream on Twitch on Thursdays at around 4:30 PM Pacific time under the screen name 'RebootTech'.  You can also poke me there about this project.
