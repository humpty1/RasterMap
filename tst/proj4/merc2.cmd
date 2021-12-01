rem тест для пересчета широты и долготы в проецию меркатора

echo off
set LNGLTT=+proj=latlong +ellps=WGS84
set MERC2=+proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +a=6378137 +b=6378137 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs


rem echo on
rem echo 37.617778 55.751667 |  cs2cs +proj=latlong +ellps=WGS84 +to +proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +a=6378137 +b=6378137 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs
rem echo 37.617778 55.751667      | cs2cs %LNGLTT%  +to %MERC2%    -f %%.08f

rem exit
echo wgs84    37.617778       55.751667
echo mercator 4187591.89      7509137.58
rem  55.751667N  37.617778 E
echo 37.617778 55.751667      | cs2cs %LNGLTT%  +to %MERC2%    -f %%.08f
echo 4187591.89   7509137.58  | cs2cs %MERC2% +to  %LNGLTT%   -f %%.08f
            