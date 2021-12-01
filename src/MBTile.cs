using System.Collections.Generic;
using System.Drawing.Drawing2D;
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
    public enum Service
    {
        OpenStreetMap,
        Visicom,
        Yandex
    };

    public class Space
    {
        public struct PointF
        {
            public double latitude;
            public double longitude;
        }

        public PointF leftTop;
        public PointF rightBot;

        public Space()
        {
            leftTop.latitude = 0;
            leftTop.longitude = 0;

            rightBot.latitude = 0;
            rightBot.longitude = 0;
        }
        public Space(double latLeftTop, double lonLeftTop, double latRightBottom, double lonRightBottom)
        {
            leftTop.latitude = latLeftTop;
            leftTop.longitude = lonLeftTop;

            rightBot.latitude = latRightBottom;
            rightBot.longitude = lonRightBottom;
        }
        public static implicit operator string (Space p) {
           return  string.Format(

              "{0}:{1}"
//              "{0:0.000000}N,{1:0.000000}E:{2:0.000000}N,{3:0.000000}E"
                  , mk.txt(p.leftTop.latitude,  p.leftTop.longitude),
                                 mk.txt(p.rightBot.latitude,  p.rightBot.longitude));
        }

    }

    public partial class MBTiles : usingLogger,  IDisposable
    {
        string strConnection;

        double width = 0;
        double height = 0;

        int countX;
        int countY;

        public Service service;
        public int zoom { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }

        //public Image map;
        public List<List<Tile>> listTile;

        TileController controler;

        public MBTiles(string strConnection, Service service
                 , int countX, int countY, Space space
                   , Loger log) : base (log)
        {
            this.strConnection = strConnection;
            this.service = service;

            this.zoom = findZoom(space.leftTop.latitude, space.leftTop.longitude, space.rightBot.latitude, space.rightBot.longitude, countX, countY);


            this.countX = countX;
            this.countY = countY;

            controler = new TileController(strConnection, log);

            listTile = GetTile(space.leftTop.latitude, space.leftTop.longitude, space.rightBot.latitude, space.rightBot.longitude, countX, countY);
            findSize(out width, out height);

            WriteLine(IMPORTANCELEVEL.Stats
                    , "ctor (space): Service '{0}' was started. Width/Height/Space: {1}/{2} dd /{3}"
                        , service, width, height, space.ToString());
        }

        public MBTiles(string strConnection, Service service
                 , int countX, int countY
                   , double b, double l, int zoom
                    , Loger log) : base (log)
        {
            this.strConnection = strConnection;
            this.service = service;

            this.zoom = zoom;
            this.lat = b;
            this.lon = l;

            this.countX = countX;
            this.countY = countY;

            controler = new TileController(strConnection, log);

            listTile = GetTile(lat, lon, countX, countY);
            findSize(out width, out height);

            WriteLine(IMPORTANCELEVEL.Stats
                   , "ctor(b,l,zoom) Service '{0}' was started. Width/Height: {1}/{2}dd"
                        , service, width, height);
        }

        //B - lat, L - lon
        public void BL2XY(double b, double l, out int X, out int Y)
        {
            BL2XY(b, l, zoom, out X, out Y);
        }
        public void BL2XY(double b, double l, int Zoom, out int X, out int Y)
        {
            BL2XY(b, l, Zoom, listTile, out X, out Y);
        }
        public void BL2XY(double b, double l, int Zoom, List<List<Tile>> list, out int X, out int Y)
        {
            int tileColumn = 0, tileRow = 0;
            int picX = 0, picY = 0;
            double flatX = 0, flatY = 0;

            if (service == Service.OpenStreetMap)
                openLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, Zoom);
            else if (service == Service.Visicom)
                visiLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, Zoom);
            else if (service == Service.Yandex)
                yandexLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, Zoom);

            int x = 0, y = 0;
            BL2xy(b, l, Zoom, out x, out y);

            int tilesColumnMin = list[0][0].column;
            int tilesRowMin = list[0][0].row;

            int coefficientX = tileColumn - tilesColumnMin;
            int coefficientY = tileRow - tilesRowMin;

            if (service == Service.Visicom)
                coefficientY = tilesRowMin - tileRow;

            //X, Y 0...256*countTile
            X = x + 256 * coefficientX;
            Y = y + 256 * coefficientY;
        }

        public Space XY2BL(int X, int Y)
        {
            Space space = new Space();

            //X, Y 0...256*countTile
            int coefficientX = (X < 256) ? 0 : (X > (listTile.Count - 1) * 256) ? (listTile.Count - 1) : X / 256;
            int coefficientY = (Y < 256) ? 0 : (Y > (listTile.Count - 1) * 256) ? (listTile.Count - 1) : Y / 256;

            int x = X - 256 * coefficientX;
            int y = Y - 256 * coefficientY;

            if (service == Service.OpenStreetMap)
                openXY2LonLat(listTile[coefficientX][coefficientY].column, listTile[coefficientX][coefficientY].row, x, y, zoom,
                    out space.leftTop.latitude, out space.leftTop.longitude);
            else if (service == Service.Visicom)
                visiXY2LonLat(listTile[coefficientX][coefficientY].column, listTile[coefficientX][coefficientY].row, x, y, zoom,
                    out space.leftTop.latitude, out space.leftTop.longitude);
            else if (service == Service.Yandex)
                yandexXY2LonLat(listTile[coefficientX][coefficientY].column, listTile[coefficientX][coefficientY].row, x, y, zoom,
                    out space.leftTop.latitude, out space.leftTop.longitude);

            space.rightBot.latitude = space.leftTop.latitude + width;
            space.rightBot.longitude = space.leftTop.longitude - height;

            return space;
        }

        public void BL2xy(double b, double l, out int x, out int y)
        {
            BL2xy(b, l, zoom, out x, out y);
        }
        public void BL2xy(double b, double l, int Zoom, out int x, out int y)
        {
            int tileColumn = 0, tileRow = 0;
            int picX = 0, picY = 0;
            double flatX = 0, flatY = 0;

            if (service == Service.OpenStreetMap)
                openLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, Zoom);
            else if (service == Service.Visicom)
                visiLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, Zoom);
            else if (service == Service.Yandex)
                yandexLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, Zoom);

            //x, y 0...256
            x = picX;
            y = picY;
        }
        public void BL2xy(double b, double l, Tile tile, out int x, out int y)
        {
            int tileColumn = 0, tileRow = 0;
            int picX = 0, picY = 0;
            double flatX = 0, flatY = 0;

            if (service == Service.OpenStreetMap)
                openLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, tile.zoom);
            else if (service == Service.Visicom)
                visiLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, tile.zoom);
            else if (service == Service.Yandex)
                yandexLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, tile.zoom);

            //x, y 0...256
            x = picX;
            y = picY;
        }

        public Space xy2BL(Tile tile, int x, int y)
        {
            //x, y 0...256
            Space space = new Space();

            if (service == Service.OpenStreetMap)
                openXY2LonLat(tile.column, tile.row, x, y, tile.zoom, out space.leftTop.latitude, out space.leftTop.longitude);
            else if (service == Service.Visicom)
                visiXY2LonLat(tile.column, tile.row, x, y, tile.zoom, out space.leftTop.latitude, out space.leftTop.longitude);
            else if (service == Service.Yandex)
                yandexXY2LonLat(tile.column, tile.row, x, y, tile.zoom, out space.leftTop.latitude, out space.leftTop.longitude);

            double wdMini, hgMini;
            findSize(tile, out wdMini, out hgMini);

            space.rightBot.latitude = space.leftTop.latitude + wdMini;
            space.rightBot.longitude = space.leftTop.longitude - hgMini;

            return space;
        }

        public Tile BL2tile(double b, double l)
        {
            //tileCol,Row 0...countList
            int tileColumn = 0, tileRow = 0;
            int picX = 0, picY = 0;
            double flatX = 0, flatY = 0;

            Tile resultTile = new Tile();

            if (service == Service.OpenStreetMap)
                openLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, zoom);
            else if (service == Service.Visicom)
                visiLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, zoom);
            else if (service == Service.Yandex)
                yandexLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, zoom);

            for (int i = 0; i < listTile.Count; i++)
            {
                for (int j = 0; j < listTile[i].Count; j++)
                    if (listTile[i][j].column == tileColumn && listTile[i][j].row == tileRow)
                        resultTile = listTile[i][j];
            }

            if (resultTile.tile == null)
                throw new Exception("You entered is not a point of its domain!");

            return resultTile;
        }
        public Tile BL2tile(double b, double l, int zoom)
        {
            //tileCol,Row 0...countList
            int tileColumn = 0, tileRow = 0;
            int picX = 0, picY = 0;
            double flatX = 0, flatY = 0;

            Tile resultTile = null;

            if (service == Service.OpenStreetMap)
                openLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, zoom);
            else if (service == Service.Visicom)
                visiLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, zoom);
            else if (service == Service.Yandex)
                yandexLonLat2XY(out tileColumn, out tileRow, out picX, out picY, out flatX, out flatY, b, l, zoom);

            if (resultTile == null)
            {
                //throw new Exception("You entered is not a point of its domain!");
                string link = "";
                int serv = -1;
                if (service == Service.OpenStreetMap)
                {
                    serv = 0;
                    link = String.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", zoom, tileColumn, tileRow);
                }
                else if (service == Service.Visicom)
                {
                    serv = 1;
                    link = String.Format("http://tms0.visicom.ua/2.0.0/planet3/base/{0}/{1}/{2}.png", zoom, tileColumn, tileRow);
                }
                else if (service == Service.Yandex)
                {
                    serv = 2;
                    link = String.Format("http://vec01.maps.yandex.net/tiles?l=map&v=2.28.0&y={2}&x={1}&z={0}&lang=ru-RU", zoom, tileColumn, tileRow);
                }

                System.Net.WebRequest webReq = System.Net.WebRequest.Create(link);
                System.Net.WebResponse webRes = webReq.GetResponse();
                Stream stream = webRes.GetResponseStream();

                resultTile = new Tile(serv, zoom, tileColumn, tileRow, ReadAllBytes(stream));
            }

            return resultTile;
        }
        public Space tile2BL(Tile tile)
        {
            Space space = new Space();

            if (service == Service.OpenStreetMap)
            {
                openXY2LonLat(tile.column, tile.row, 0, 0, zoom, out space.leftTop.latitude, out space.leftTop.longitude);
                openXY2LonLat(tile.column, tile.row, 256, 256, zoom, out space.rightBot.latitude, out space.rightBot.longitude);
            }
            else if (service == Service.Visicom)
            {
                visiXY2LonLat(tile.column, tile.row, 0, 0, zoom, out space.leftTop.latitude, out space.leftTop.longitude);
                visiXY2LonLat(tile.column, tile.row, 256, 256, zoom, out space.rightBot.latitude, out space.rightBot.longitude);
            }
            else if (service == Service.Yandex)
            {
                yandexXY2LonLat(tile.column, tile.row, 0, 0, zoom, out space.leftTop.latitude, out space.leftTop.longitude);
                yandexXY2LonLat(tile.column, tile.row, 256, 256, zoom, out space.rightBot.latitude, out space.rightBot.longitude);
            }

            space.rightBot.latitude += width;
            space.rightBot.longitude -= height;

            return space;
        }

        public bool dotOnCenteraltile(double B, double L)
        {
            Space spaceTile = tile2BL(listTile[(int)(countX / 2)][(int)(countY / 2)]);

            if (B > spaceTile.leftTop.latitude  ||
                B < spaceTile.rightBot.latitude ||
                L < spaceTile.leftTop.longitude ||
                L > spaceTile.rightBot.longitude)
                return false;

            return true;
        }
	    public bool dotOnTile(double B, double L, Tile tile)
        {
            Space spaceTile = tile2BL(tile);

            if (B > spaceTile.leftTop.latitude ||
                B < spaceTile.rightBot.latitude ||
                L < spaceTile.leftTop.longitude ||
                L > spaceTile.rightBot.longitude)
                return false;

            return true;
        }

        int findZoom(double latLeftTop, double lonLeftTop, double latRightBottom, double lonRightBottom, int countX, int countY)
        {
            int tilesColumnMax = 0, tilesRowMax = 0;
            int tilesColumnMin = 0, tilesRowMin = 0;

            int picXMax = 0, picYMax = 0;
            int picXMin = 0, picYMin = 0;

            double flatXMax = 0, flatYMax = 0;
            double flatXMin = 0, flatYMin = 0;
            int zoom = 19;

            int countXReal = 0;
            int countYReal = 0;

            if (service == Service.OpenStreetMap)
            {
                openLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                openLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }
            else if (service == Service.Visicom)
            {
                visiLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                visiLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }
            else if (service == Service.Yandex)
            {
                yandexLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                yandexLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }

            countXReal = Math.Abs(tilesColumnMax - tilesColumnMin);
            countYReal = Math.Abs(tilesRowMax - tilesRowMin);
            while (countXReal > countX && countYReal > countY)
            {
                zoom--;
                if (service == Service.OpenStreetMap)
                {
                    openLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                    openLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
                }
                else if (service == Service.Visicom)
                {
                    visiLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                    visiLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
                }
                else if (service == Service.Yandex)
                {
                    yandexLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                    yandexLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
                }

                countXReal = Math.Abs(tilesColumnMax - tilesColumnMin);
                countYReal = Math.Abs(tilesRowMax - tilesRowMin);
            }
            return zoom;
        }
        
        /// <summary>
        /// Возвращает матрицу тайлов для покрытия заданого квадрата, с заданным зумом
        /// </summary>
        /// <param name="latLeftTop">Широта левого верхнего угла</param>
        /// <param name="lonLeftTop">Долгота левого верхнего угла</param>
        /// <param name="latRightBottom">Широта правого нижнего угла</param>
        /// <param name="lonRightBottom">Долгота правого нижнего угла</param>
        /// <param name="zoom">Зум карты</param>
        /// <param name="service">Индекм сервиса тайлов (0 - OpenStreetMap, 1 - Visicom, 2 - Yandex)</param>
        /// <returns></returns>
        public List<List<Tile>> GetTile(double latLeftTop, double lonLeftTop
                                , double latRightBottom, double lonRightBottom, int zoom)
        {
            var listTile = new List<List<Tile>> { };

            int tilesColumnMin = 0, tilesRowMin = 0;
            int tilesColumnMax = 0, tilesRowMax = 0;

            int picXMax = 0, picYMax = 0;
            int picXMin = 0, picYMin = 0;

            double flatXMax = 0, flatYMax = 0;
            double flatXMin = 0, flatYMin = 0;

            string serv = "";
            if (service == Service.OpenStreetMap)
            {
                serv = "osm";
                openLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                openLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }
            else if (service == Service.Visicom)
            {
                serv = "visicom";
                visiLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                visiLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }
            else if (service == Service.Yandex)
            {
                serv = "yandex";
                yandexLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                yandexLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }

            int exitI = Math.Abs(tilesColumnMax - tilesColumnMin);
            int exitJ = Math.Abs(tilesRowMax - tilesRowMin);
            int columnTile = 0;
            int rowTile = 0;
            string link = "";
            DateTime St = DateTime.Now;

            WriteLine(IMPORTANCELEVEL.Info, "max Rectangle for your space: {0}x{1}"
                                , exitI, exitJ);

            for (int i = 0; i <= exitI; i++)
            {
                listTile.Add(new List<Tile> { });
                for (int j = 0; j <= exitJ; j++)
                {
                    
                    columnTile = tilesColumnMin + i;
                    rowTile = tilesRowMin + j;

                    if (service == Service.OpenStreetMap)
                    {
                        System.Threading.Thread.Sleep(5);
                        link = String.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", zoom, columnTile, rowTile);
                    }
                    else if (service == Service.Visicom)
                    {
                        rowTile = tilesRowMin - j;
                        link = String.Format("http://tms0.visicom.ua/2.0.0/planet3/base/{0}/{1}/{2}.png", zoom, columnTile, rowTile);
                    }
                    else if (service == Service.Yandex)
                    {
                        link = String.Format("http://vec01.maps.yandex.net/tiles?l=map&v=2.28.0&y={2}&x={1}&z={0}&lang=ru-RU", zoom, columnTile, rowTile);
                    }

                    DateTime st = DateTime.Now;
                    Tile tile = controler.Select(controler.SelectService(serv), zoom, columnTile, rowTile);
                    DateTime fn = DateTime.Now;

                    if (tile == null)
                    {
                        try
                        {
                            System.Net.WebRequest webReq = System.Net.WebRequest.Create(link);
                            System.Net.WebResponse webRes = webReq.GetResponse();
                            Stream stream = webRes.GetResponseStream();

                            tile = new Tile(controler.SelectService(serv), zoom, columnTile, rowTile, ReadAllBytes(stream));
                            controler.Insert(tile);
                            fn = DateTime.Now;

                            WriteLine(IMPORTANCELEVEL.Stats, "{0}: zoom/column/row/link: {1}/{2}/{3}/'{4}')"
                                , serv, zoom, columnTile, rowTile, link);

                            WriteLine("{0}: zoom/column/row: {1}/{2}/{3} was downloaded from tile server ({4} secs)"
                                , serv, zoom, columnTile, rowTile, (fn - st).TotalSeconds);
                        }
                        catch (Exception e)
                        {
                            WriteLine(IMPORTANCELEVEL.Error
                              , "{0}: zoom/column/row: {1}/{2}/{3} : {4}/{5}"
                              , serv, zoom, columnTile, rowTile, "No internet connection! Use cap for tile. ", e.Message);
                            //throw new Exception("No internet connection! " + e.Message);

                            tile = controler.Select(controler.SelectService(serv), 0, 0, 0);
                        }
                    }
                    else
                    {
                        WriteLine("{0}: zoom/column/row: {1}/{2}/{3} was selected from db ({4} secs)"
                           , serv, zoom, columnTile, rowTile, (fn - st).TotalSeconds);
                        controler.UpdateUsages(tile);
                        /*try {  }
                        catch (Exception e)
                        {
                            WriteLine(IMPORTANCELEVEL.Error
                                , "{0}: zoom/column/row: {1}/{2}/{3}: {4}/{5}"
                                , serv, zoom, columnTile, rowTile, "The tile is absent in database! ", e.Message);
                            throw new Exception("The tile is absent in database! " + e.Message);
                        }*/
                    }
                    listTile[i].Add(tile);
                }
            }
            
            DateTime Fn = DateTime.Now;
            WriteLine(IMPORTANCELEVEL.Stats, "{0} secs of full query"
               , (Fn - St).TotalSeconds);

            return listTile;
        }
        /// <summary>
        /// Возвращает матрицу тайлов для покрытия заданого квадрата, с заданным зумом
        /// </summary>
        /// <param name="latLeftTop">Широта левого верхнего угла</param>
        /// <param name="lonLeftTop">Долгота левого верхнего угла</param>
        /// <param name="latRightBottom">Широта правого нижнего угла</param>
        /// <param name="lonRightBottom">Долгота правого нижнего угла</param>
        /// <param name="zoom">Зум карты</param>
        /// <param name="service">Индекм сервиса тайлов (0 - OpenStreetMap, 1 - Visicom, 2 - Yandex)</param>
        /// <returns></returns>
        /// <summary>
        List<List<Tile>> GetTile(double latLeftTop, double lonLeftTop
             , double latRightBottom, double lonRightBottom, int countX, int countY)
        {
            var listTile = new List<List<Tile>> { };

            int tilesColumnMax = 0, tilesRowMax = 0;
            int tilesColumnMin = 0, tilesRowMin = 0;

            int picXMax = 0, picYMax = 0;
            int picXMin = 0, picYMin = 0;

            double flatXMax = 0, flatYMax = 0;
            double flatXMin = 0, flatYMin = 0;
            string serv = "";
            if (service == Service.OpenStreetMap)
            {
                serv = "osm";
                openLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                openLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }
            else if (service == Service.Visicom)
            {
                serv = "visicom";
                visiLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                visiLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }
            else if (service == Service.Yandex)
            {
                serv = "yandex";
                yandexLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                yandexLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }

            int exitI = countX;
            int exitJ = countY;
            int columnTile = 0;
            int rowTile = 0;
            string link = "";
            DateTime St = DateTime.Now;

            for (int i = 0; i < exitI; i++)
            {
                listTile.Add(new List<Tile> { });
                for (int j = 0; j < exitJ; j++)
                {
                    columnTile = tilesColumnMin + i;
                    rowTile = tilesRowMin + j;

                    if (service == Service.OpenStreetMap)
                    {
                        System.Threading.Thread.Sleep(5);
                        link = String.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", zoom, columnTile, rowTile);
                    }
                    else if (service == Service.Visicom)
                    {
                        rowTile = tilesRowMin - j;
                        link = String.Format("http://tms0.visicom.ua/2.0.0/planet3/base/{0}/{1}/{2}.png", zoom, columnTile, rowTile);
                    }
                    else if (service == Service.Yandex)
                    {
                        link = String.Format("http://vec01.maps.yandex.net/tiles?l=map&v=2.28.0&y={2}&x={1}&z={0}&lang=ru-RU", zoom, columnTile, rowTile);
                    }

                    DateTime st = DateTime.Now;
                    Tile tile = controler.Select(controler.SelectService(serv), zoom, columnTile, rowTile);
                    DateTime fn = DateTime.Now;

                    if (tile == null)
                    {
                        try
                        {
                            System.Net.WebRequest webReq = System.Net.WebRequest.Create(link);
                            System.Net.WebResponse webRes = webReq.GetResponse();
                            Stream stream = webRes.GetResponseStream();

                            tile = new Tile(controler.SelectService(serv), zoom, columnTile, rowTile, ReadAllBytes(stream));
                            controler.Insert(tile);
                            fn = DateTime.Now;

                            WriteLine(IMPORTANCELEVEL.Stats, "{0}: zoom/column/row/link: {1}/{2}/{3}/'{4}')"
                                , serv, zoom, columnTile, rowTile, link);

                            WriteLine("{0}: zoom/column/row: {1}/{2}/{3} was downloaded from tile server ({4} secs)"
                                , serv, zoom, columnTile, rowTile, (fn - st).TotalSeconds);
                        }
                        catch (Exception e)
                        {
                            WriteLine(IMPORTANCELEVEL.Error
                               , "{0}: zoom/column/row: {1}/{2}/{3} : {4}/{5}"
                               , serv, zoom, columnTile, rowTile, "No internet connection! Use cap for tile. ", e.Message);
                            //throw new Exception("No internet connection! " + e.Message);

                            tile = controler.Select(controler.SelectService(serv), 0, 0, 0);
                        }
                    }
                    else
                    {
                        WriteLine("{0}: zoom/column/row: {1}/{2}/{3} was selected from db ({4} secs)"
                           , serv, zoom, columnTile, rowTile, (fn - st).TotalSeconds);
                        controler.UpdateUsages(tile);
                        /*try {  }
                        catch (Exception e)
                        {
                            WriteLine(IMPORTANCELEVEL.Error
                                , "{0}: zoom/column/row: {1}/{2}/{3}: {4}/{5}"
                                , serv, zoom, columnTile, rowTile, "The tile is absent in database! ", e.Message);
                            throw new Exception("The tile is absent in database! " + e.Message);
                        }*/
                    }
                    listTile[i].Add(tile);
                }
            }

            DateTime Fn = DateTime.Now;
            WriteLine(IMPORTANCELEVEL.Stats, "{0} secs of full query"
               , (Fn - St).TotalSeconds);

            return listTile;
        }
        /// <summary>
        /// Возвращает матрицу тайлов для покрытия заданой точки, заданым количеством тайлов
        /// </summary>
        /// <param name="lat">Широта точки относительно которой строиться карта</param>
        /// <param name="lon">Долгота точки относительно которой строиться карта</param>
        /// <param name="countX">Количество тайлов по X (кол-во столбцов матрицы)</param>
        /// <param name="countY">Количество тайлов по Y (кол-во строк матрицы)</param>
        /// <returns></returns>
        List<List<Tile>> GetTile(double lat, double lon, int countX, int countY)
        {
            var listTile = new List<List<Tile>> { };

            int tilesColumn = 0, tilesRow = 0;
            int picX = 0, picY = 0;
            double flatX = 0, flatY = 0;

            string serv = "";
            if (service == Service.OpenStreetMap)
            {
                serv = "osm";
                openLonLat2XY(out tilesColumn, out tilesRow, out picX, out picY, out flatX, out flatY, lat, lon, zoom);
            }
            else if (service == Service.Visicom)
            {
                serv = "visicom";
                visiLonLat2XY(out tilesColumn, out tilesRow, out picX, out picY, out flatX, out flatY, lat, lon, zoom);
            }
            else if (service == Service.Yandex)
            {
                serv = "yandex";
                yandexLonLat2XY(out tilesColumn, out tilesRow, out picX, out picY, out flatX, out flatY, lat, lon, zoom);
            }

            int tilesColumnMin = tilesColumn - (int)(countX / 2);
            int tilesColumnMax = tilesColumn + (int)(countX / 2);

            int tilesRowMin = service == Service.Visicom ? tilesRow + (int)(countY / 2) : tilesRow - (int)(countY / 2);
            int tilesRowMax = service == Service.Visicom ? tilesRow - (int)(countY / 2) : tilesRow + (int)(countY / 2);

            int exitI = countX;
            int exitJ = countY;

            int columnTile = 0;
            int rowTile = 0;

            string link = "";
            DateTime St = DateTime.Now;

            for (int i = 0; i < exitI; i++)
            {
                listTile.Add(new List<Tile> { });
                for (int j = 0; j < exitJ; j++)
                {
                    columnTile = tilesColumnMin + i;
                    rowTile = tilesRowMin + j;

                    if (service == Service.OpenStreetMap)
                    {
                        System.Threading.Thread.Sleep(5);
                        link = String.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", zoom, columnTile, rowTile);
                    }
                    else if (service == Service.Visicom)
                    {
                        rowTile = tilesRowMin - j;
                        link = String.Format("http://tms0.visicom.ua/2.0.0/planet3/base/{0}/{1}/{2}.png", zoom, columnTile, rowTile);
                    }
                    else if (service == Service.Yandex)
                    {
                        link = String.Format("http://vec01.maps.yandex.net/tiles?l=map&v=2.28.0&y={2}&x={1}&z={0}&lang=ru-RU", zoom, columnTile, rowTile);
                    }

                    DateTime st = DateTime.Now;
                    Tile tile = controler.Select(controler.SelectService(serv), zoom, columnTile, rowTile);
                    DateTime fn = DateTime.Now;

                    if (tile == null)
                    {
                        try
                        {
                            System.Net.WebRequest webReq = System.Net.WebRequest.Create(link);
                            System.Net.WebResponse webRes = webReq.GetResponse();
                            Stream stream = webRes.GetResponseStream();

                            tile = new Tile(controler.SelectService(serv), zoom, columnTile, rowTile, ReadAllBytes(stream));
                            controler.Insert(tile);
                            fn = DateTime.Now;

                            WriteLine(IMPORTANCELEVEL.Stats, "{0}: zoom/column/row/link: {1}/{2}/{3}/'{4}')"
                                , serv, zoom, columnTile, rowTile, link);

                            WriteLine("{0}: zoom/column/row: {1}/{2}/{3} was downloaded from tile server ({4} secs)"
                                , serv, zoom, columnTile, rowTile, (fn - st).TotalSeconds);
                        }
                        catch (Exception e)
                        {
                            WriteLine(IMPORTANCELEVEL.Error
                              , "{0}: zoom/column/row: {1}/{2}/{3} : {4}/{5}"
                              , serv, zoom, columnTile, rowTile, "No internet connection! Use cap for tile. ", e.Message);
                            //throw new Exception("No internet connection! " + e.Message);

                            tile = controler.Select(controler.SelectService(serv), 0, 0, 0);
                        }
                    }
                    else
                    {
                        WriteLine("{0}: zoom/column/row: {1}/{2}/{3} was selected from db ({4} secs)"
                           , serv, zoom, columnTile, rowTile, (fn - st).TotalSeconds);
                        controler.UpdateUsages(tile);
                        /*try {  }
                        catch (Exception e)
                        {
                            WriteLine(IMPORTANCELEVEL.Error
                                , "{0}: zoom/column/row: {1}/{2}/{3}: {4}/{5}"
                                , serv, zoom, columnTile, rowTile, "The tile is absent in database! ", e.Message);
                            throw new Exception("The tile is absent in database! " + e.Message);
                        }*/
                    }
                    listTile[i].Add(tile);
                }
            }

            DateTime Fn = DateTime.Now;
            WriteLine(IMPORTANCELEVEL.Stats, "{0} secs of full query"
               , (Fn - St).TotalSeconds);

            return listTile;
        }

        public List<List<Tile>> GetTileInternet(double latLeftTop, double lonLeftTop
                                , double latRightBottom, double lonRightBottom, int zoom)
        {
            var listTile = new List<List<Tile>> { };

            int serv = -1;
            int tilesColumnMax = 0, tilesRowMax = 0;
            int tilesColumnMin = 0, tilesRowMin = 0;

            int picXMax = 0, picYMax = 0;
            int picXMin = 0, picYMin = 0;

            double flatXMax = 0, flatYMax = 0;
            double flatXMin = 0, flatYMin = 0;

            if (service == Service.OpenStreetMap)
            {
                serv = 0;
                openLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                openLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }
            else if (service == Service.Visicom)
            {
                serv = 1;
                visiLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                visiLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }
            else if (service == Service.Yandex)
            {
                serv = 2;
                yandexLonLat2XY(out tilesColumnMin, out tilesRowMin, out picXMin, out picYMin, out flatXMin, out flatYMin, latLeftTop, lonLeftTop, zoom);
                yandexLonLat2XY(out tilesColumnMax, out tilesRowMax, out picXMax, out picYMax, out flatXMax, out flatYMax, latRightBottom, lonRightBottom, zoom);
            }

            int exitI = Math.Abs(tilesColumnMax - tilesColumnMin);
            int exitJ = Math.Abs(tilesRowMax - tilesRowMin);
            int columnTile = 0;
            int rowTile = 0;
            string link = "";

            for (int i = 0; i <= exitI; i++)
            {
                listTile.Add(new List<Tile> { });
                for (int j = 0; j <= exitJ; j++)
                {
                    columnTile = tilesColumnMin + i;
                    rowTile = tilesRowMin + j;

                    if (service == Service.OpenStreetMap)
                    {
                        System.Threading.Thread.Sleep(5);
                        link = String.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", zoom, columnTile, rowTile);
                    }
                    else if (service == Service.Visicom)
                    {
                        rowTile = tilesRowMin - j;
                        System.Threading.Thread.Sleep(5);
                        link = String.Format("http://tms0.visicom.ua/2.0.0/planet3/base/{0}/{1}/{2}.png", zoom, columnTile, rowTile);
                    }
                    else if (service == Service.Yandex)
                    {
                        link = String.Format("http://vec01.maps.yandex.net/tiles?l=map&v=2.28.0&y={2}&x={1}&z={0}&lang=ru-RU", zoom, columnTile, rowTile);
                    }

                    try
                    {
                        System.Net.WebRequest webReq = System.Net.WebRequest.Create(link);
                        System.Net.WebResponse webRes = webReq.GetResponse();
                        Stream stream = webRes.GetResponseStream();

                        listTile[i].Add(
                            new Tile(serv, zoom, columnTile, rowTile, ReadAllBytes(stream))
                        );

                        /*WriteLine(IMPORTANCELEVEL.Stats, "{0}: zoom/column/row/link: {1}/{2}/{3}/'{4}')"
                            , service, zoom, columnTile, rowTile, link);*/
                    }
                    catch (Exception e)
                    {
                        /*WriteLine(IMPORTANCELEVEL.Error
                           , "{0}: zoom/column/row: {1}/{2}/{3} : {4}/{5}"
                           , service, zoom, columnTile, rowTile, "No internet connection! ", e.Message);*/
                    }
                }
            }
            return listTile;
        }

        public Tile Space2tile(Space space)
        {
            string link = "";
            string serv = "";
            int tilesColumnMin_MiniMap = 0;
            int tilesColumnMax_MiniMap = 1;

            int tilesRowMin_MiniMap = 0;
            int tilesRowMax_MiniMap = 1;
            int z = zoom;

            int picX_, picY_;
            double flatX_, flatY_;
            double 
                firstPointX = space.leftTop.latitude,
                firstPointY = space.leftTop.longitude,
                lastPointX = space.rightBot.latitude,
                lastPointY = space.rightBot.longitude;

            for (; z >= 0 && (tilesColumnMin_MiniMap != tilesColumnMax_MiniMap
                          || tilesRowMin_MiniMap != tilesRowMax_MiniMap); z--)
            {
                if (service == Service.OpenStreetMap)
                {
                    openLonLat2XY(out tilesColumnMax_MiniMap, out tilesRowMax_MiniMap, out picX_, out picY_, out flatX_, out flatY_, lastPointX, lastPointY, z);
                    openLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                }
                else if (service == Service.Visicom)
                {
                    WriteLine(IMPORTANCELEVEL.Debug, "{0}: zoom {1}"
                        , "Space2tile", z);
                    visiLonLat2XY(out tilesColumnMax_MiniMap, out tilesRowMax_MiniMap, out picX_, out picY_, out flatX_, out flatY_, lastPointX, lastPointY, z);
                    visiLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                }
                else if (service == Service.Yandex)
                {
                    yandexLonLat2XY(out tilesColumnMax_MiniMap, out tilesRowMax_MiniMap, out picX_, out picY_, out flatX_, out flatY_, lastPointX, lastPointY, z);
                    yandexLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                }

            }
            if (z < 0)
                z = 0;

            if (service == Service.OpenStreetMap)
            {
                openLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                link = String.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap);
                serv = "osm";
            }
            else if (service == Service.Visicom)
            {
                visiLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                link = String.Format("http://tms0.visicom.ua/2.0.0/planet3/base/{0}/{1}/{2}.png", z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap);
                serv = "visicom";
            }
            else if (service == Service.Yandex)
            {
                yandexLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                link = String.Format("http://vec01.maps.yandex.net/tiles?l=map&v=2.28.0&y={2}&x={1}&z={0}&lang=ru-RU", z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap);
                serv = "yandex";
            }

            //column - tilesX, row - tilesY
            Tile tile = controler.Select(controler.SelectService(serv), z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap);
            if (tile == null)
            {
                try
                {
                    System.Net.WebRequest webReq = System.Net.WebRequest.Create(link);
                    System.Net.WebResponse webRes = webReq.GetResponse();
                    Stream stream = webRes.GetResponseStream();

                    tile = new Tile(controler.SelectService(serv), z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap, ReadAllBytes(stream));
                    controler.Insert(tile);
                }
                catch (Exception e)
                {
                    //throw new Exception("No internet connection! " + e.Message);
                    tile = controler.Select(controler.SelectService(serv), 0, 0, 0);
                }
            }
            else
            {
                controler.UpdateUsages(tile);
            }

            return tile;
        }
        public Tile Space2tile(double firstPointX, double firstPointY,
           double lastPointX, double lastPointY)
        {
            int tilesColumnMin_MiniMap = 0;
            int tilesColumnMax_MiniMap = 1;

            int tilesRowMin_MiniMap = 0;
            int tilesRowMax_MiniMap = 1;

            int picX_, picY_;
            double flatX_, flatY_;

            int z = zoom;
            string serv = "";
            string link = "";
            for (; z >= 0 && (tilesColumnMin_MiniMap != tilesColumnMax_MiniMap
                          || tilesRowMin_MiniMap != tilesRowMax_MiniMap); z--)
            {
                if (service == Service.OpenStreetMap)
                {
                    openLonLat2XY(out tilesColumnMax_MiniMap, out tilesRowMax_MiniMap, out picX_, out picY_, out flatX_, out flatY_, lastPointX, lastPointY, z);
                    openLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                }
                else if (service == Service.Visicom)
                {
                    WriteLine(IMPORTANCELEVEL.Debug, "{0}: zoom {1}"
                        , "Space2tile", z);
                    visiLonLat2XY(out tilesColumnMax_MiniMap, out tilesRowMax_MiniMap, out picX_, out picY_, out flatX_, out flatY_, lastPointX, lastPointY, z);
                    visiLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                }
                else if (service == Service.Yandex)
                {
                    yandexLonLat2XY(out tilesColumnMax_MiniMap, out tilesRowMax_MiniMap, out picX_, out picY_, out flatX_, out flatY_, lastPointX, lastPointY, z);
                    yandexLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                }

            }
            if (z < 0)
                z = 0;
            WriteLine(IMPORTANCELEVEL.Info, "Space2tile: {0}: zoom: {1}"
               , serv, z);

            if (service == Service.OpenStreetMap)
            {
                openLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                link = String.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", z, tilesColumnMax_MiniMap, tilesRowMax_MiniMap);
                serv = "osm";
            }
            else if (service == Service.Visicom)
            {
                visiLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                link = String.Format("http://tms0.visicom.ua/2.0.0/planet3/base/{0}/{1}/{2}.png", z, tilesColumnMax_MiniMap, tilesRowMax_MiniMap);
                serv = "visicom";
            }
            else if (service == Service.Yandex)
            {
                yandexLonLat2XY(out tilesColumnMin_MiniMap, out tilesRowMin_MiniMap, out picX_, out picY_, out flatX_, out flatY_, firstPointX, firstPointY, z);
                link = String.Format("http://vec01.maps.yandex.net/tiles?l=map&v=2.28.0&y={2}&x={1}&z={0}&lang=ru-RU", z, tilesColumnMax_MiniMap, tilesRowMax_MiniMap);
                serv = "yandex";
            }

//            System.Windows.Forms.MessageBox.Show("Test " + z + " " + tilesColumnMin_MiniMap + " " + tilesRowMin_MiniMap);

            DateTime st = DateTime.Now;
            Tile tile = controler.Select(
                      controler.SelectService(serv), z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap
            );
            DateTime fn = DateTime.Now;

            if (tile == null)
            {
                try
                {
                    System.Net.WebRequest webReq = System.Net.WebRequest.Create(link);
                    System.Net.WebResponse webRes = webReq.GetResponse();
                    Stream stream = webRes.GetResponseStream();

                    tile = new Tile(controler.SelectService(serv), z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap, ReadAllBytes(stream));
                    controler.Insert(tile);
                    fn = DateTime.Now;

                    WriteLine(IMPORTANCELEVEL.Stats, "Space2tile: {0}: zoom/column/row/link: {1}/{2}/{3})"
                        , serv, z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap);

                    WriteLine("{0}: zoom/column/row: {1}/{2}/{3} was downloaded from tile server ({4} secs)"
                        , serv, z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap, (fn - st).TotalSeconds);
                }
                catch (Exception e)
                {
                    WriteLine(IMPORTANCELEVEL.Error
                       , "{0}: zoom/column/row: {1}/{2}/{3} : {4}/{5}"
                       , serv, z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap, "No internet connection! ", e.Message);
                    //throw new Exception("No internet connection! " + e.Message);
                    tile = controler.Select(controler.SelectService(serv), 0, 0, 0);
                }
            }
            else
            {
                WriteLine(IMPORTANCELEVEL.Stats, "Space2tile: {0}: zoom/column/row: {1}/{2}/{3} was selected from db ({4} secs)"
                   , serv, z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap, (fn - st).TotalSeconds);
                try { controler.UpdateUsages(tile); }
                catch (Exception e)
                {
                    WriteLine(IMPORTANCELEVEL.Error
                        , "{0}: zoom/column/row: {1}/{2}/{3}: {4}/{5}"
                        , serv, z, tilesColumnMin_MiniMap, tilesRowMin_MiniMap, "The tile is absent in database! ", e.Message);
                    throw new Exception("The tile is absent in database! " + e.Message);
                }
            }
            return tile;
        }

        byte[] ReadAllBytes(Stream reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }

        public void Dispose()
        {
            listTile = null;

            controler.Dispose();
        }
    }
    public abstract class DOWNLOAD
    {
        public static void Download(string path, Service service, int zoom)
        {
            string link = "";
            int countTile = (int)Math.Pow(2, zoom);

            for (int i = 0; i < countTile; i++)
            {
                for (int j = 0; j < countTile; j++)
                {
                    if (service == Service.OpenStreetMap)
                    {
                        System.Threading.Thread.Sleep(5);
                        link = String.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", zoom, i, j);
                    }
                    else if (service == Service.Visicom)
                        link = String.Format("http://tms0.visicom.ua/2.0.0/planet3/base/{0}/{1}/{2}.png", zoom, i, j);
                    else if (service == Service.Yandex)
                        link = String.Format("http://vec01.maps.yandex.net/tiles?l=map&v=2.28.0&y={2}&x={1}&z={0}&lang=ru-RU", zoom, i, j);
                    
                    try
                    {
                        System.Net.WebRequest webReq = System.Net.WebRequest.Create(link);
                        System.Net.WebResponse webRes = webReq.GetResponse();
                        Stream stream = webRes.GetResponseStream();

                        string filename = String.Format("{0}{1}.{2}.{3}.{4}.png", path, service.ToString(), zoom, i, j);

                        Image.FromStream(stream).Save(filename);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("No internet connection! " + e.Message);
                    }
                }
            }
        }
    }
}
