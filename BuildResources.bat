@echo off

call "F:\\UnityEditors\\2020.3.4f1\\Editor\\Unity.exe" -batchmode -nographics -quit -projectPath %~dp0 -executeMethod Build_Batch.RecompileAllScripts -logfile RecompileAllScripts.log

REM ERROR CHECK
if "%ERRORLEVEL%" NEQ "0" (
	pause
)