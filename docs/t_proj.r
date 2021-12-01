

.^
PROJ.4 -  библиотека для выполнения преобразований между картографическими проектциями
(Смотри

.;\cite{PROJ.4}).
Собственно, преобразование координат выполняется утилитой cs2cs.exe, запускаемой из командной строки.
Дистрибутив находится по адресу

\url{http://download.osgeo.org/proj/proj-4.9.2.tar.gz}
или по адресу
\url{http://www.agp1.hx0.ru/tools/proj-4.9.2.tar.gz}.



.^
Можно скачать уже откомпилированную версию 4.4.6 Рroj.4
по адресу
\url{http://download.osgeo.org/proj/proj446_win32_bin.zip}.

.^




Построение библиотеки   под Win32 можно выполнить при помощи Microsoft Visual Studio 2010 Expession
в окне  Visual Studio Command Prompt:
.b
.;-
\includegraphics[width=0.90\textwidth]{./pics/vscp_2010.png}
\begin {center}
\caption{Инсталляция Proj.4}
\end{center}
\begin {figure}
\label{1vscp:01}
\end{figure}
.;-

.^
Для этого
распакуйте архив в каталог 
с:\\proj.
И выполните компиляцию проекта следующими командами:
.n

C:\> cd proj
C:\PROJ> nmake /f makefile.vc
C:\PROJ> nmake /f makefile.vc install-all

.f
.^
Не удалось заметить влияние отсутствие файлов *.gsb на работоспособность
библиотеки.
.^
Для тестирования выполнялся пересчет координат 55.751667 северной широты, 37.617778 восточной долготы.

 из теста

\url{http://wiki.gis-lab.info/w/Пересчет_координат_из_Lat/Long_в_проекцию_Меркатора_и_обратно},
c тестами на 
\url{http://gis-lab.info/qa/dd2mercator.html}, результаты были отрицательные.




.3 ПЕРЕСЧЕТ WGS84 -  Mercator



Тестирование выполняется следующими командами:
.n
.<./../tst/proj4/merc.cmd
.f
Тесты закончились предсказанным результатом на обеих доступных версиях proj4


.3 ПЕРЕСЧЕТ WGS84 - Web Mercator

.n
.<./../tst/proj4/merc2.cmd
.f
.3 Исходный код CPP- версии для обеих Меркаторов
.n
.<./../tst/c/1.cpp
.f


Результат работы программы  должен быть такой:
.n
Mercator X: 4187591.89 Y: 7473789.46
etalon   X: 4187591.89 Y: 7473789.46
SpherMercator X: 4187591.89 Y: 7509137.58
etalon        X: 4187591.89 Y: 7509137.58
.f








.^
geod.exe  - вычисление расстояний,
cs2cs.exe - пеоесчет систем координат.


.2 ИСПОЛЬЗОВАНИЕ PROJ.4

.^
.= список проекций
в списке приведены только две.
cs2cs.exe -lu


.n

lonlat : Lat/long (Geodetic)

latlon : Lat/long (Geodetic alias)

merc : Mercator

.f

.=

.= список единиц измерения

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
.= вычисление расстояний

geod.exe  - вычисление расстояний,

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
Проекция Меркатора описана в Серапинасе стр.152,
Бугаевский, стр.122.






.2 ПЕРЕСЧЕТ WGS84 - СИСТЕМА КООРДИНАТ 42 ГОДА (6Я ЗОНА)

.^
При  тестировании использованы координаты в обеих системах
предоставленные Аэрокосмическим центром Национального Авиационного Университета

.n
WGS84:    29.99576	  50.46615	  972.409
sk42:     5597125.518	6286838.264	954.466            
.f
Тестирование выполнялось следующими командами:
.n
.<./../tst/proj4/wgs842sk42.cmd 
.f

Результаты тестирования:
.n
.<./../tst/proj4/.t2.txt
.f










