This is a work in progress.  It's also a bit difficult and time intensive to do.

The goal of this folder is to provide a way for you to build a compatible video from the 720i sources and the CDROM sources.

Acquiring the 720i sources is out of scope for this project. I purchased a physical copy of the Japanese edition of the game from a seller in Japan and imported it to where I live.  Once you have the physical sources, it isn't obvious how to get at the mpg video without problems or re-encoding.
A few things to note: 
 * I picked only the English track audio because it makes things a little easier to deal with, however, the application has a channel picker so you can pick the audio channel once in the application.  In the 720i sources, The English audio is the second audio track;The Japanese audio is the first audio track.
 * You might have to try a variety of tools to read/repair the timecode of the original sources. There is software that results in a proper mpeg without re-encoding the video track.  Many result in weird timecodes that causes problems with reading the video

Once you have extracted the video from the sources, you must deinterlace it.  The original sources are in, essentially, mpg format.
To get the best quality, remember to deinterlace the original video track /prior/ to any re-encoding of the video track. If you re-encode it prior to deinterlacing, you introduce errors that make deinterlacing less successful.

How you deinterlace it is unimportant, however, you must make sure not to alter the time in the video. Video editing programs are smart enough to be able to understand how FPS relates to the frames in the video and what that means for the timing of each scene in the video.  
The easiest to use for interlacing is YADIF.  This gives reasonable quality, but also results in some line artifacts in dark areas (such as people's eyes) that will get magnified if you try to use an AI Upscaler.  

I used QTGMC and AviSynth+.  The documentation, files and setup to use QTGMC is very sketchy.  It involves copying DLLs to the windows system folders and wow64 folders.  It's sketchy.  Once it is complete, the following script was able to deinterlace it.

My .avs script was:
SetFilterMTMode ("QTGMC",2)
FFmpegSource2("E:\rawVid\Borg\DVD\mpg\VTS_02_1.mpg",atrack=1)
ConvertToYV12(interlaced=true)
AssumeTFF()
#DoubleWeave().SelectOdd()
QTGMC(preset="Slower", EdiThreads=1)
BilinearResize(720,540)
#Prefetch(18)

Using one thread was important in that script.  More than one thread is faster, but caused the interlace line order to swap after scene cuts.  You can tell if this has happened by viewing the resulting video and noticing it moving forward and then jumping back, then jumping forward and then back again, kind of like a rubberband.  Two steps forward, one step back.

You must also have the CDROM video. The 720i sources are missing some scenes and the CDROM video is used to fill the gap. 

------------------

I'm working on a C# application that will pick up the files from this point on and cut the 720p sources.   While I'm working on this, you may be able to cut them yourself with your preferred editor with these directions.

----------------------------
For this, you need to split the CDROM's Main_1's video and audio track into a .mp4 file and a .wav file. This will re-encode the video track but it's already abysmal quality so you're not losing anything.  These will be used in the EDL file.
 
Once all of that is in order, you must adjust the speed of the 720p source.  This is because the 420p CDROM video runs at a different speed than the 720i video.

This is easy to do with ffmpeg.
ffmpeg -i VTS_02_1.mp4  -vf "setpts=(PTS-STARTPTS)/1.000158982" -af atempo=1.000158982 VTS_02_1_CDROM_Speed.mp4

From the resulting sources, you can use the EDL cuts to compose the video in your editor. 
Note: 
 * I have only produced an EDL cut file for Main_1 so far (6/11/2023). I will produce Main_2 and Main_3 soon and update this file. I'll also provide a few LUT that will recolor the source like TNG
 * The EDL cuts are in a Semi-colon separated format with a direct path to the audio files. 
 * * You must change the location of the video files in the EDL cut file to where you have them stored.
 * To produce the scenes that are missing in the 720i source, you will need the Main_1 video track as an mp4 and the audio track as a wav from the original CD sources. (this can be done with ffmpeg)

Eventually, an application will be made to do this last bit.
