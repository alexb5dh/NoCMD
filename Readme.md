# NoCMD
Lightweight utility for hiding long-running cmd commands to notification area.

Standard and error outputs can be redirected to files. 
Any notempty stderr ouput and exit code will be shown via balloon tooltip.

![image](https://cloud.githubusercontent.com/assets/4650832/19205724/d847a934-8cec-11e6-99c8-a544f2da2897.png)

## Usage
Basis usage:
```cmd
nocmd script.bat
```

Command with spaces need to be wrapped with quotes:
```cmd
nocmd "timeout /t 10"
```

Output stream can be redirected:
```cmd
nocmd script.bat /out out.txt
nocmd script.bat /o out.txt
```

As well as error stream:
```cmd
nocmd script.bat /error error.txt
nocmd script.bat /e error.txt
```

## Requirements
- .NET 4.5
