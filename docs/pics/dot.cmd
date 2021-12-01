rem F:/bin/graphviz/bin/circo.exe -Tpng %1 -o %1.png
rem "C:\Program Files\ATT\Graphviz\bin\dot.exe" -Tpng %1 -o %1.png
rem "G:\bin\Graphviz\bin\dot.exe" -Tpng %1 -o .%1.png
"G:\bin\graphviz\bin\dot.exe" -Tpng %1 -o .%1.png
echo %errorlevel%
