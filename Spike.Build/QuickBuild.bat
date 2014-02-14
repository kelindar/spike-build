rmdir "%TEMP%\Spike"
mkdir "%TEMP%\Spike"
Spike.Build.exe http://127.0.0.1:8002/spml/all -out:%TEMP%\Spike
explorer "%TEMP%\Spike"