rem SET PATH=g:\bin\texLive\bin\win32;G:\bin\Graphviz\bin\;%PATH%
rem SET PATH=C:\bin\texLive\bin\win32;G:\bin\Graphviz2008\bin\;%PATH%


rem SET R=/r:Logger.cs.dll
rem SET R=/r:ClassLibrary1.dll 
rem     public enum IMPORTANCELEVEL { Spam, Debug, Warning, Stats, Error, FatalError, Info,  Ignore };

SET NAMEZIP=sasdw
echo %NAMEZIP%
rm -rf  _bld
rm   ./cs/*.exe


rem 
SET EXZIP=-x *.exe -x *.eRr   -x *.log -x *.db
