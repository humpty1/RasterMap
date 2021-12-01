#include <math.h>
#include <stdio.h>

#define M_PI 3.141592654
#define M_PI_4 M_PI/4.0
// Бугаевский стр 388 
#define D_R (M_PI / 180.0)
#define R_D (180.0 / M_PI)
#define R_MAJOR 6378137.0
#define R_MINOR 6356752.3142
#define RATIO (R_MINOR/R_MAJOR)
#define ECCENT (sqrt(1.0 - (RATIO * RATIO)))
#define COM (0.5 * ECCENT)
 
double  fmin ( double a, double b){
  if (a < b)
    return a;
  return b;
}
double  fmax ( double a, double b){
  if (a > b)
    return a;
  return b;
}



static double deg_rad (double ang) {
        return ang * D_R;
}
 
void LatLong2Merc(double lon, double lat, double* x, double* y) {
	*x =  R_MAJOR * deg_rad (lon);
	lat = fmin (89.5, fmax (lat, -89.5));
        double phi = deg_rad(lat);
        double sinphi = sin(phi);
        double con = ECCENT * sinphi;
        con = pow((1.0 - con) / (1.0 + con), COM);
        double ts = tan(0.5 * (M_PI * 0.5 - phi)) / con;
        *y = 0 - R_MAJOR * log(ts);
}

void LatLong2SpherMerc(double lon, double lat, double* x, double* y) {
	lat = fmin (89.5, fmax (lat, -89.5));
	*x = R_MAJOR * deg_rad (lon);
	*y = R_MAJOR * log(tan(M_PI_4 + deg_rad(lat)/2 ));
}
 
void main(int argc, char **argv){

  double x=0.0;
  double  y=0.0;
  double x1=0.0;
  double  y1=0.0;
  double x2=0.0;
  double  y2=0.0;
  LatLong2Merc(37.617778,55.751667,&x,&y);
  LatLong2Merc(0.0, 55.751667,&x1,&y1);
  LatLong2Merc(37.617778, 0.0,&x2,&y2);
  printf("Mercator 37.617778E,55.751667N) X: %10.2f Y: %10.2f\n",x,y);
  printf("etalon   X: %10.2f Y: %10.2f\n",4187591.89,7473789.46);
  printf("Mercator (0.0E, 55.751667N) X: %10.2f Y: %10.2f\n",x1,y1);
  printf("Mercator (37.617778E, 0.0N) X: %10.2f Y: %10.2f\n",x2,y2);

 

  LatLong2SpherMerc(37.617778,55.751667,&x,&y);
  printf("SpherMercator X: %10.2f Y: %10.2f\n",x,y);
  printf("etalon        X: %10.2f Y: %10.2f\n",4187591.89,7509137.58);

       
}

