@echo off
cd ../../../../game/bin/win64
resourcecompiler.exe -fshallow -maxtextureres 256 -dxlevel 110 -unbufferedio -i "%~1" -noassert -world -vis -nompi -nop4

pause