mkdir "%~1Agents"

mkdir "%~1Agents\SampleConverters"
for /r "%~2Agents\SampleConverters" %%f in (*.pdb) do (
  (echo "%%f" | find /I "\NostalgicPlayerGuiKit.pdb" 1>NUL) || (echo "%%f" | find /I /V "\%~3\" 1>NUL) || (copy "%%f" "%~1Agents\SampleConverters")
)
for /r "%~2Agents\SampleConverters" %%f in (*.dll) do (
  (echo "%%f" | find /I "\obj\" 1>NUL) || (echo "%%f" | find /I "\ref\" 1>NUL) || (echo "%%f" | find /I /V "\%~3\" 1>NUL) || (echo "%%f" | find /I "\NostalgicPlayerGuiKit.dll" 1>NUL) || (copy "%%f" "%~1Agents\SampleConverters")
)

mkdir "%~1Agents\ModuleConverters"
for /r "%~2Agents\ModuleConverters" %%f in (*.pdb) do (
  (echo "%%f" | find /I "\NostalgicPlayerGuiKit.pdb" 1>NUL) || (echo "%%f" | find /I /V "\%~3\" 1>NUL) || (copy "%%f" "%~1Agents\ModuleConverters")
)
for /r "%~2Agents\ModuleConverters" %%f in (*.dll) do (
  (echo "%%f" | find /I "\obj\" 1>NUL) || (echo "%%f" | find /I "\ref\" 1>NUL) || (echo "%%f" | find /I /V "\%~3\" 1>NUL) || (echo "%%f" | find /I "\NostalgicPlayerGuiKit.dll" 1>NUL) || (copy "%%f" "%~1Agents\ModuleConverters")
)

mkdir "%~1Agents\Output"
for /r "%~2Agents\Output" %%f in (*.pdb) do (
  (echo "%%f" | find /I "\NostalgicPlayerGuiKit.pdb" 1>NUL) || (echo "%%f" | find /I /V "\%~3\" 1>NUL) || (copy "%%f" "%~1Agents\Output")
)
for /r "%~2Agents\Output" %%f in (*.dll) do (
  (echo "%%f" | find /I "\obj\" 1>NUL) || (echo "%%f" | find /I "\ref\" 1>NUL) || (echo "%%f" | find /I /V "\%~3\" 1>NUL) || (echo "%%f" | find /I "\NostalgicPlayerGuiKit.dll" 1>NUL) || (copy "%%f" "%~1Agents\Output")
)

mkdir "%~1Agents\Players"
for /r "%~2Agents\Players" %%f in (*.pdb) do (
  (echo "%%f" | find /I "\NostalgicPlayerGuiKit.pdb" 1>NUL) || (echo "%%f" | find /I /V "\%~3\" 1>NUL) || (copy "%%f" "%~1Agents\Players")
)
for /r "%~2Agents\Players" %%f in (*.dll) do (
  (echo "%%f" | find /I "\obj\" 1>NUL) || (echo "%%f" | find /I "\ref\" 1>NUL) || (echo "%%f" | find /I /V "\%~3\" 1>NUL) || (echo "%%f" | find /I "\NostalgicPlayerGuiKit.dll" 1>NUL) || (copy "%%f" "%~1Agents\Players")
)

mkdir "%~1Agents\Visuals"
for /r "%~2Agents\Visuals" %%f in (*.pdb) do (
  (echo "%%f" | find /I "\NostalgicPlayerGuiKit.pdb" 1>NUL) || (echo "%%f" | find /I /V "\%~3\" 1>NUL) || (copy "%%f" "%~1Agents\Visuals")
)
for /r "%~2Agents\Visuals" %%f in (*.dll) do (
  (echo "%%f" | find /I "\obj\" 1>NUL) || (echo "%%f" | find /I "\ref\" 1>NUL) || (echo "%%f" | find /I /V "\%~3\" 1>NUL) || (echo "%%f" | find /I "\NostalgicPlayerGuiKit.dll" 1>NUL) || (copy "%%f" "%~1Agents\Visuals")
)
