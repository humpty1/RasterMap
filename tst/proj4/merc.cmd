rem тест для пересчета широты и долготы в проецию меркатора

echo off
set LNGLTT=+proj=latlong +ellps=WGS84
set MERC=+proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +ellps=WGS84 +datum=WGS84 +units=m +no_defs



echo wgs84    37.617778 55.751667
echo mercator 4187591.89   7473789.46
rem  55.751667N  37.617778 E
echo 37.617778 55.751667      | cs2cs %LNGLTT%  +to %MERC%
echo 4187591.89   7473789.46  | cs2cs %MERC% +to  %LNGLTT%   -f %%.08f
            