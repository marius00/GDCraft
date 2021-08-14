copy inno\vcredist_x86.exe IAGrim\bin\release\
copy inno\ndp472-kb4054531-web.exe IAGrim\bin\release\
copy inno\2010sp1_vcredist_x86.exe IAGrim\bin\release\
copy inno\2015rc3_vc_redist.x86.exe IAGrim\bin\release\


copy /y IAGrim\bin\release\IAGrim.exe Installer\IAGrim.exe

Inno\iscc Inno\gdia.iss