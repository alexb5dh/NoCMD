@start ..\bin\debug\error.txt
@start ..\bin\debug\out.txt
@..\bin\Debug\NoCMD.exe "for /L %%i in (0, 1, 15) do @echo %%i & @timeout /t 1 > nul" /out "..\bin\debug\out.txt" /error "..\bin\debug\error.txt"