
call "C:/2019.2.15f1/Editor/Unity.exe" -batchmode -nographics -quit -projectPath %~dp0 -executeMethod Build_Batch.Build_Release 1> BuildRel.log 2> ErrorsRel.log

pause