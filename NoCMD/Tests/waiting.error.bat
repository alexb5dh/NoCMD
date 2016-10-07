@start ..\bin\debug\out.txt
@start ..\bin\debug\error.txt
@..\bin\Debug\NoCMD.exe /w "for /L %%i in (0, 1, 5) do @>&2 echo %%i & @timeout /t 5 > nul" /out "..\bin\debug\out.txt" /error "..\bin\debug\error.txt"