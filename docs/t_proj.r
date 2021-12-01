

.^
PROJ.4 -  ���������� ��� ���������� �������������� ����� ����������������� �����������
(������

.;\cite{PROJ.4}).
����������, �������������� ��������� ����������� �������� cs2cs.exe, ����������� �� ��������� ������.
����������� ��������� �� ������

\url{http://download.osgeo.org/proj/proj-4.9.2.tar.gz}
��� �� ������
\url{http://www.agp1.hx0.ru/tools/proj-4.9.2.tar.gz}.



.^
����� ������� ��� ����������������� ������ 4.4.6 �roj.4
�� ������
\url{http://download.osgeo.org/proj/proj446_win32_bin.zip}.

.^




���������� ����������   ��� Win32 ����� ��������� ��� ������ Microsoft Visual Studio 2010 Expession
� ����  Visual Studio Command Prompt:
.b
.;-
\includegraphics[width=0.90\textwidth]{./pics/vscp_2010.png}
\begin {center}
\caption{����������� Proj.4}
\end{center}
\begin {figure}
\label{1vscp:01}
\end{figure}
.;-

.^
��� �����
���������� ����� � ������� 
�:\\proj.
� ��������� ���������� ������� ���������� ���������:
.n

C:\> cd proj
C:\PROJ> nmake /f makefile.vc
C:\PROJ> nmake /f makefile.vc install-all

.f
.^
�� ������� �������� ������� ���������� ������ *.gsb �� �����������������
����������.
.^
��� ������������ ���������� �������� ��������� 55.751667 �������� ������, 37.617778 ��������� �������.

 �� �����

\url{http://wiki.gis-lab.info/w/��������_���������_��_Lat/Long_�_��������_���������_�_�������},
c ������� �� 
\url{http://gis-lab.info/qa/dd2mercator.html}, ���������� ���� �������������.




.3 �������� WGS84 -  Mercator



������������ ����������� ���������� ���������:
.n
.<./../tst/proj4/merc.cmd
.f
����� ����������� ������������� ����������� �� ����� ��������� ������� proj4


.3 �������� WGS84 - Web Mercator

.n
.<./../tst/proj4/merc2.cmd
.f
.3 �������� ��� CPP- ������ ��� ����� ����������
.n
.<./../tst/c/1.cpp
.f


��������� ������ ���������  ������ ���� �����:
.n
Mercator X: 4187591.89 Y: 7473789.46
etalon   X: 4187591.89 Y: 7473789.46
SpherMercator X: 4187591.89 Y: 7509137.58
etalon        X: 4187591.89 Y: 7509137.58
.f








.^
geod.exe  - ���������� ����������,
cs2cs.exe - �������� ������ ���������.


.2 ������������� PROJ.4

.^
.= ������ ��������
� ������ ��������� ������ ���.
cs2cs.exe -lu


.n

lonlat : Lat/long (Geodetic)

latlon : Lat/long (Geodetic alias)

merc : Mercator

.f

.=

.= ������ ������ ���������

cs2cs.exe -lu
.n
          km 1000.                Kilometer
           m 1.                   Meter
          dm 1/10                 Decimeter
          cm 1/100                Centimeter
          mm 1/1000               Millimeter
         kmi 1852.0               International Nautical Mile
          in 0.0254               International Inch
          ft 0.3048               International Foot
          yd 0.9144               International Yard
          mi 1609.344             International Statute Mile
        fath 1.8288               International Fathom
          ch 20.1168              International Chain
        link 0.201168             International Link
       us-in 1./39.37             U.S. Surveyor's Inch
       us-ft 0.304800609601219    U.S. Surveyor's Foot
       us-yd 0.914401828803658    U.S. Surveyor's Yard
       us-ch 20.11684023368047    U.S. Surveyor's Chain
       us-mi 1609.347218694437    U.S. Surveyor's Statute Mile
.f


.=
.= ���������� ����������

geod.exe  - ���������� ����������,

Geodesic Calculations

Geodesic calculations are calculations along lines (great circle) on the surface of the earth. They can answer questions like:
.(
.@
    What is the distance between these two points?
.@
    If I travel X meters from point A at bearing phi, where will I be. 

.)

\url{https://trac.osgeo.org/proj/wiki/GeodesicCalculations}

.=


.^
�������� ��������� ������� � ���������� ���.152,
����������, ���.122.






.2 �������� WGS84 - ������� ��������� 42 ���� (6� ����)

.^
���  ������������ ������������ ���������� � ����� ��������
��������������� ��������������� ������� ������������� ������������ ������������

.n
WGS84:    29.99576	  50.46615	  972.409
sk42:     5597125.518	6286838.264	954.466            
.f
������������ ����������� ���������� ���������:
.n
.<./../tst/proj4/wgs842sk42.cmd 
.f

���������� ������������:
.n
.<./../tst/proj4/.t2.txt
.f










