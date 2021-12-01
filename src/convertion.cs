using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Linq;
using System.IO;
using System;

using Logger;
using ut;          	
using System.Data.SQLite;

namespace MBTile
{

    public partial  class MBTiles : usingLogger,  IDisposable {
        //  находим размер одного пикселя в градусах
        void findSize(out double width, out double height)  {
            findPxlSize( out  width, out height);
        }
        void findSize(Tile tile, out double width, out double height)
        {
            findPxlSize(tile, out width, out height);
        }

        public void findPxlSize(out double width, out double height)
        {
            double lat00 = 0, lon00 = 0;
            double lat11 = 0, lon11 = 0;
            if (service == Service.OpenStreetMap)
            {
                openXY2LonLat(listTile[0][0].column, listTile[0][0].row, 0, 0, zoom, out lat00, out lon00);
                openXY2LonLat(listTile[0][0].column, listTile[0][0].row, 1, 1, zoom, out lat11, out lon11);
            }
            else if (service == Service.Visicom)
            {
                visiXY2LonLat(listTile[0][0].column, listTile[0][0].row, 0, 0, zoom, out lat00, out lon00);
                visiXY2LonLat(listTile[0][0].column, listTile[0][0].row, 1, 1, zoom, out lat11, out lon11);
            }
            else if (service == Service.Yandex)
            {
                yandexXY2LonLat(listTile[0][0].column, listTile[0][0].row, 0, 0, zoom, out lat00, out lon00);
                yandexXY2LonLat(listTile[0][0].column, listTile[0][0].row, 1, 1, zoom, out lat11, out lon11);
            }

            height = lat11 - lat00;               ///
            width  = lon11 - lon00;              ///
        }
        public void findPxlSize(Tile tile, out double width, out double height)
        {
            double lat00 = 0, lon00 = 0;
            double lat11 = 0, lon11 = 0;
            if (service == Service.OpenStreetMap)
            {
                openXY2LonLat(tile.column, tile.row, 0, 0, tile.zoom, out lat00, out lon00);
                openXY2LonLat(tile.column, tile.row, 1, 1, tile.zoom, out lat11, out lon11);
            }
            else if (service == Service.Visicom)
            {
                visiXY2LonLat(tile.column, tile.row, 0, 0, tile.zoom, out lat00, out lon00);
                visiXY2LonLat(tile.column, tile.row, 1, 1, tile.zoom, out lat11, out lon11);
            }
            else if (service == Service.Yandex)
            {
                yandexXY2LonLat(tile.column, tile.row, 0, 0, tile.zoom, out lat00, out lon00);
                yandexXY2LonLat(tile.column, tile.row, 1, 1, tile.zoom, out lat11, out lon11);
            }

            height = lat11 - lat00;               ///
            width = lon11 - lon00;              ///
        }

        public void findTlSize(out double width, out double height) {
            findPxlSize( out  width, out height);
            width*=256;
            height*=256;
        }
                     // найти координаты размер верхнего левого тайла
        public void findTlCoorSize(out double B,  out double height, out double L, out double width) {
            double lat00 = 0, lon00 = 0;
            double lat11 = 0, lon11 = 0;
            if (service == Service.OpenStreetMap)
            {
                openXY2LonLat(listTile[0][0].column, listTile[0][0].row, 0, 0, zoom, out lat00, out lon00);
                openXY2LonLat(listTile[0][0].column, listTile[0][0].row, 1, 1, zoom, out lat11, out lon11);
            }
            else if (service == Service.Visicom)
            {
                visiXY2LonLat(listTile[0][0].column, listTile[0][0].row, 0, 0, zoom, out lat00, out lon00);
                visiXY2LonLat(listTile[0][0].column, listTile[0][0].row, 1, 1, zoom, out lat11, out lon11);
            }
            else if (service == Service.Yandex)
            {
                yandexXY2LonLat(listTile[0][0].column, listTile[0][0].row, 0, 0, zoom, out lat00, out lon00);
                yandexXY2LonLat(listTile[0][0].column, listTile[0][0].row, 1, 1, zoom, out lat11, out lon11);
            }

            B = lat00;
            L = lon00;
            height = (lat11 - lat00)*256;               ///
            width  = (lon11 - lon00)*256;              ///
        }


        /// <summary>
        /// Переводит из координат точки на тайле Yandex-а в реальные координаты
        /// </summary>
        /// <param name="tilesX">Колонка тайла</param>
        /// <param name="tilesY">Строка тайла</param>
        /// <param name="picX">X координата точки на тайле</param>
        /// <param name="picY">Y координата точки на тайле</param>
        /// <param name="z">Зум карты</param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        void yandexXY2LonLat(double tilesX, double tilesY 
                              , double picX, double picY, int z,
                    out double latitude,
                    out double longitude)
        {
            double e = 2.7182818284590452;
            double pi = 3.1415926535897932;

            double a = 6378137.0;
            double c1 = 0.00335655146887969;
            double c2 = 0.00000657187271079536;
            double c3 = 0.00000001764564338702;
            double c4 = 0.00000000005328478445;

            double flatX = tilesX * 256 + picX;
            double flatY = tilesY * 256 + picY;

            double mercX = flatX * Math.Pow(2, (23 - z)) / 53.5865938 - 20037508.342789;
            double mercY = 20037508.342789 - (flatY * Math.Pow(2, (23 - z))) / 53.5865938;
            double g = pi / 2 - 2 * Math.Atan(1.0 / Math.Exp(mercY / a));
            double f = g + c1 * Math.Sin(2 * g) + c2 * Math.Sin(4 * g) + c3 * Math.Sin(6 * g) + c4 * Math.Sin(8 * g);

            latitude = f * 180.0 / pi;
            longitude = mercX / a * 180.0 / pi;
        }
        /// <summary>
        /// Переводит из координат точки на тайле OpenStreetMap-а в реальные координаты
        /// </summary>
        /// <param name="tilesX">Колонка тайла</param>
        /// <param name="tilesY">Строка тайла</param>
        /// <param name="picX">X координата точки на тайле</param>
        /// <param name="picY">Y координата точки на тайле</param>
        /// <param name="z">Зум карты</param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <summary>
        void openXY2LonLat(double tilesX, double tilesY, double picX, double picY, int z,
                    out double latitude,
                    out double longitude)
        {
            double e = 2.7182818284590452;
            double pi = 3.1415926535897932;
            double bm0 = 256 * Math.Pow(2, z) / 2.0;

            double flatX = tilesX * 256 + picX;
            double flatY = tilesY * 256 + picY;
            double lonrad = pi * (flatX - bm0) / bm0;
            double c1 = 2 * pi * (bm0 - flatY) / bm0;
            double latrad = Math.Asin((Math.Pow(e, c1) - 1) / (Math.Pow(e, c1) + 1));

            latitude = 180 * latrad / pi;
            longitude = 180 * lonrad / pi;
        }
        /// <summary>
        /// Переводит из координат точки на тайле Visicom-а в реальные координаты
        /// </summary>
        /// <param name="tilesX">Колонка тайла</param>
        /// <param name="tilesY">Строка тайла</param>
        /// <param name="picX">X координата точки на тайле</param>
        /// <param name="picY">Y координата точки на тайле</param>
        /// <param name="z">Зум карты</param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        void visiXY2LonLat(double tilesX, double tilesY, double picX, double picY, int z,
                    out double latitude,
                    out double longitude)
        {
            int[] Xtmp = { 0, 1, 3, 7, 15, 31, 63, 127, 255, 511, 1023, 2047, 2095, 8191, 16383, 32767, 65535, 131071, 262143, 524287 };

            double e = 2.7182818284590452;
            double pi = 3.1415926535897932;
            double bm0 = 256 * Math.Pow(2, z) / 2.0;

            double flatX = tilesX * 256 + picX;
            double flatY = (Xtmp[z] - tilesY) * 256 + picY;
            double lonrad = pi * (flatX - bm0) / bm0;
            double c1 = 2 * pi * (bm0 - flatY) / bm0;
            double latrad = Math.Asin((Math.Pow(e, c1) - 1) / (Math.Pow(e, c1) + 1));

            latitude = 180 * latrad / pi;
            longitude = 180 * lonrad / pi;
        }

        /// <summary>
        /// Преобразование из реальных координат в координаты тайла для Yandex-a
        /// </summary>
        /// <param name="tilesX">Колонка тайла</param>
        /// <param name="tilesY">Строка тайла</param>
        /// <param name="picX">X координата точки на тайле</param>
        /// <param name="picY">Y координата точки на тайле</param>
        /// <param name="flatX">X координата точки в плоскости проэкции Земли</param>
        /// <param name="flatY">Y координата точки в плоскости проэкции Земли</param>
        /// <param name="lat">Широта</param>
        /// <param name="lon">Долгота</param>
        /// <param name="z">Зум карты</param>
        void yandexLonLat2XY(out int tilesX
                              , out int tilesY
                              , out int picX
                              , out int picY
                              , out double flatX
                              , out double flatY
                              , double lat
                              , double lon
                              , int z
                              )
        {
            double pi = 3.1415926535897932;
            double latrad = (lat * pi) / 180.0;
            double lonrad = (lon * pi) / 180.0;

            double a = 6378137.0;
            double k = 0.0818191908426;
            double f = Math.Tan(pi / 4.0 + latrad / 2.0) / Math.Pow(Math.Tan(pi / 4.0 + Math.Asin(k * Math.Sin(latrad)) / 2.0), k);

            flatX = (20037508.342789 + a * lonrad) * 53.5865938 / Math.Pow(2, 23 - z);
            flatY = (20037508.342789 - a * Math.Log(f)) * 53.5865938 / Math.Pow(2, 23 - z);

            tilesX = (int)(flatX / 256);
            tilesY = (int)(flatY / 256);
            picX = ((int)flatX) % 256;
            picY = ((int)flatY) % 256;
        }
        /// <summary>
        /// Преобразование из реальных координат в координаты тайла для OpenStreetMAp-a
        /// </summary>
        /// <param name="tilesX">Колонка тайла</param>
        /// <param name="tilesY">Строка тайла</param>
        /// <param name="picX">X координата точки на тайле</param>
        /// <param name="picY">Y координата точки на тайле</param>
        /// <param name="flatX">X координата точки в плоскости проэкции Земли</param>
        /// <param name="flatY">Y координата точки в плоскости проэкции Земли</param>
        /// <param name="lat">Широта</param>
        /// <param name="lon">Долгота</param>
        /// <param name="z">Зум карты</param>
        /// <summary>
        void openLonLat2XY(out int tilesX
                        , out int tilesY
                        , out int picX
                        , out int picY
                        , out double flatX
                        , out double flatY
                        , double lat
                        , double lon
                        , int zoom
                        )
        {
            double pi = 3.1415926535897932;
            double latrad = (lat * pi) / 180.0;
            double lonrad = (lon * pi) / 180.0;
            double bm0 = 256 * Math.Pow(2.0, (double)zoom) / 2.0;

            flatX = (bm0 * (1 + lonrad / pi));
            flatY = (bm0 * (1 - 0.5 * Math.Log((1 + Math.Sin(latrad)) / (1 - Math.Sin(latrad))) / pi));

            tilesX = (int)(flatX) / 256;
            tilesY = (int)(flatY) / 256;
            picX = (int)(flatX) % 256;
            picY = (int)(flatY) % 256;
        }
         /// <summary>
        /// Преобразование из реальных координат в координаты тайла для Visicom-a
        /// </summary>
        /// <param name="tilesX">Колонка тайла</param>
        /// <param name="tilesY">Строка тайла</param>
        /// <param name="picX">X координата точки на тайле</param>
        /// <param name="picY">Y координата точки на тайле</param>
        /// <param name="flatX">X координата точки в плоскости проэкции Земли</param>
        /// <param name="flatY">Y координата точки в плоскости проэкции Земли</param>
        /// <param name="lat">Широта</param>
        /// <param name="lon">Долгота</param>
        /// <param name="z">Зум карты</param>
       /// <summary>
        public void visiLonLat2XY(out int tilesX
                        , out int tilesY
                        , out int picX
                        , out int picY
                        , out double flatX
                        , out double flatY
                        , double lat
                        , double lon
                        , int zoom
                        )
        {
            int[] Xtmp = { 0, 1, 3, 7, 15, 31, 63, 127, 255, 511, 1023, 2047, 2095, 8191, 16383, 32767, 65535, 131071, 262143, 524287 };

            double pi = 3.1415926535897932;
            double latrad = (lat * pi) / 180.0;
            double lonrad = (lon * pi) / 180.0;
            double bm0 = 256 * Math.Pow(2.0, (double)zoom) / 2.0;

            flatX = (bm0 * (1 + lonrad / pi));
            flatY = (bm0 * (1 - 0.5 * Math.Log((1 + Math.Sin(latrad)) / (1 - Math.Sin(latrad))) / pi));

            tilesX = (int)(flatX) / 256;
            tilesY = Xtmp[zoom] - (int)(flatY) / 256;
            picX = (int)(flatX) % 256;
            picY = (int)(flatY) % 256;
        }
   }
}
