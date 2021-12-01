rem 
@echo off

set CS2CS=C:\proj\bin\cs2cs
set WGS84=+proj=latlong +ellps=WGS84 +datum=WGS84
set sk42_6=+proj=tmerc +lat_0=0 +lon_0=33 +k=1 +x_0=6500000 +y_0=0 +ellps=krass +units=m +no_defs


%CS2CS% %WGS84%  +to %sk42_6%         <test2.txt        >.test2.sk42.txt

echo -------- calculation  wgs48 to sk42 6th zone  >.t2.txt
echo -------- etalon first                         >>.t2.txt
echo  5597125.518	6286838.264	954.466              >>.t2.txt
type  .test2.sk42.txt                              >>.t2.txt
echo -------- calculation sk42 to wgs48            >>.t2.txt
%CS2CS% %sk42_6% +to %WGS84% -f %%.08f <.test2.sk42.txt  >.test2.txt
cat test2.txt .test2.txt >>.t2.txt
type  .t2.txt
