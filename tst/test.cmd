rem �� �������
a.exe  -f _myTracks\2016-10-29_17-16-33.Track_002.csv      -osm   -v -g2c

a.exe  -f          C:\agp\128\prj\map\afs\CK_WGS_84.csv   -v -ASC  -ya

a.exe   -f _dixi\exportgps_maxim.csv  -vi   -v  -dixi
rem �� �������


exit 

rem mini format
a.exe  -f _mini.csv     -v -mini -ya


a.exe -z 2


a.exe  -f _dixi\exportgps.csv    -v -dixi -z 8

rem ���� ������������ ������
a.exe  -f G:\agp\128\prj\map\ASF\CK_WGS_84.csv    -v -ASC
a.exe  -f          C:\agp\128\prj\map\afs\CK_WGS_84.csv   -v -ASC  -ya
a.exe  -f D:\_agp\prj\map\AFS\CK_WGS_84.csv    -v -ASC


_asc 

rem ��� ����� ����
a.exe  -f _asc\CK_WGS_84.short.csv     -v -ASC -z 16
a.exe  -f _asc\CK_WGS_84.short.csv    -v -ASC -z 16
exit
a.exe -log Debug -f _dixi\exportgps_vit.short.csv   -vi   -v -dixi

exit

rem ������ ����������� � �������� ����� ����������� ���������
a.exe -log Debug -f _dixi\exportgps_vit.short.csv   -vi   -v -dixi
exit
a.exe -log Debug -f _dixi\exportgps_maxim.csv  -vi   -v  -dixi

rem �������� ��������������� ����� ��� �������� ����� � ������������ �� ��� �����
a.exe -f _dixi\exportgps_vit.csv -vi -w

rem ����� ������ ������ ��� ����� � ��(usages)
a.exe -f _dixi\exportgps_vit.csv -vi -s

exit 
rem ��� �������
a.exe  -f _myTracks\2016-10-29_17-16-33.Track_002.csv      -osm   -v -g2c

a.exe  -f _myTracks\2015-09-20_19-34-36.Track_000.csv     -osm   -v -g2c
a.exe  -f _myTracks\2015-09-20_21-08-35.Track_000.csv     -vi   -v -g2c
a.exe  -f _myTracks\2015-09-20_21-08-35.Track_001.csv     -ya   -v -g2c
 

