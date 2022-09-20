.\ffmpeg\ffmpeg.exe -y -itsoffset 1.0 -i MAIN_1.avi -i MAIN_1.avi -map 0:0 -map 1:1 -acodec copy -vcodec copy MAIN_1X.avi
.\ffmpeg\ffmpeg.exe -y -itsoffset 1.0 -i MAIN_2.avi -i MAIN_2.avi -map 0:0 -map 1:1 -acodec copy -vcodec copy MAIN_2X.avi
.\ffmpeg\ffmpeg.exe -y -itsoffset 1.0 -i MAIN_3.avi -i MAIN_3.avi -map 0:0 -map 1:1 -acodec copy -vcodec copy MAIN_3X.avi
.\ffmpeg\ffmpeg.exe -y -itsoffset 1.0 -i IP.avi -i IP.avi -map 0:0 -map 1:1 -acodec copy -vcodec copy IPX.avi
.\ffmpeg\ffmpeg.exe -y -itsoffset 1.0 -i LOGO.avi -i LOGO.avi -map 0:0 -map 1:1 -acodec copy -vcodec copy LOGOX.avi