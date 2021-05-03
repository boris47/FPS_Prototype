
call "F:\\UnityEditors\\2019.4.23f1\\Editor\\Unity.exe" -batchmode -nographics -quit -projectPath %~dp0 -executeMethod Build_Batch.Build_Development 1> BuildDev.log 2> ErrorsDev.log

pause