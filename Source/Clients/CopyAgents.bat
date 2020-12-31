mkdir "%~1Agents"

mkdir "%~1Agents\Output"
for /r "%~2Agents\Output" %%f in (*.pdb) do @copy "%%f" "%~1Agents\Output"
for /r "%~2Agents\Output" %%f in (*.dll) do (
  (echo "%%f" | find /I "\obj\" 1>NUL) || (copy "%%f" "%~1Agents\Output")
)

mkdir "%~1Agents\Players"
for /r "%~2Agents\Players" %%f in (*.pdb) do @copy "%%f" "%~1Agents\Players"
for /r "%~2Agents\Players" %%f in (*.dll) do (
  (echo "%%f" | find /I "\obj\" 1>NUL) || (copy "%%f" "%~1Agents\Players")
)
