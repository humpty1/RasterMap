#pragma warning disable 642

using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Net;
using System.IO;
using System;
using Args;
using Logger;
using MBTile;
using MapWnd;
using geo;
using ut;
using wnd;


namespace MapWnd
{
    class Program
    {
        static public ArgFlg vF;     ///расшириная справка
        static public ArgStr logNm;  ///<чтобы задать уровень журналирования именем (в данном приложении не используется).
        static public ArgFlg hlp;    ///отображения справки
        static public ArgFlg open;   ///использовать Open Street Map тайлы
        static public ArgFlg visi;   ///использовать Visicom тайлы
        static public ArgFlg yand;   ///использовать Yandex тайлы
        static public ArgFlg dixiF;
        static public ArgFlg ASCF;   //  aero spase flag
        static public ArgFlg miniF;
        static public ArgFlg gpx2csvF;
        //static public ArgFlg wayF;

        static public ArgFlg tile4trck; ///загрузить тайлы на 19 зуме для покрытие всего трека

        static public ArgFlg create; ///создание новой базы даных
        static public ArgFlg select; ///просмотр содержимого базы даных
        static public ArgFlg clear;  ///очистка базы даных 

        static public ArgIntMM lng;  ///язык программы

        static public ArgIntMM zoom; ///зум карты
        static public ArgInt count;  ///размерность (количество сторк/столбцов) карты
        static public ArgInt countCache;//размера кэша для скачки тайлов

        static public ArgInt tmStep; ///интервал движеня точки 

        static public ArgStr db;     ///строка подлючения к базе данных
        static public ArgStr flNM;   ///имя файла с треком
        static public ArgStr decPnt; ///десятичная точка

        static public ArgFloatMM B;  ///<   широта
        static public ArgFloatMM L;  ///<   долгота

        static public ArgStr trckCol; ///цвет трека
        static public ArgStr startVW; ///цвет начальной области карты на мини карте
        static public ArgStr curVW;   ///цвет текущей области карты на мини карте

        static Loger l;
        static Program()
        {
            trckCol = new ArgStr("DarkBlue", "track", "track"
                    , "color to draw track", null, WCNST.colors);
            startVW = new ArgStr("Red", "start", "start"
                     , "color to draw start view", null, WCNST.colors);
            curVW = new ArgStr("Gray", "current", "current"
                     , "color to draw current view", null, WCNST.colors);

            string lLvl = "log level names:{" + Loger.ILList() + "}";
            logNm = new ArgStr("Error", "log", "logName", lLvl, "NNN");
            vF = new ArgFlg(false, "v", "verbose", "additional info");
            hlp = new ArgFlg(false, "?", "help", "help info");

            open = new ArgFlg(false, "osm", "openstreet", "to download tiles from openstreem.org");
            visi = new ArgFlg(false, "vi", "visicom", "to download tiles from visicom.ua");
            yand = new ArgFlg(false, "ya", "yandex", "to download tiles from yandex.ru");

            open.required = true;
            visi.required = true;
            yand.required = true;

            open.show = false;
            visi.show = false;
            yand.show = false;

            flNM = new ArgStr(".noSuchFile.csv", "f", "file", "file with way point", "fileName");
            flNM.show = false;

            miniF   = new ArgFlg(false, "mini", "mini", "just  latitude + longitude  format");
            miniF.show = false;

            dixiF   = new ArgFlg(false, "dixi", "digsee", "the www.DigSee.com file format");
            dixiF.show = false;

            ASCF   = new ArgFlg(false, "ASC", "ASC", "the AeroSpaceCenter file format");
            ASCF.show = false;
            gpx2csvF   = new ArgFlg(false, "g2c", "gpx2csv", "gpx2csv convertor format");
            gpx2csvF.show = false;

            //wayF = new ArgFlg(false, "w", "way", "show trajectory");

            tmStep = new ArgInt(100, "ts", "timeStep", "seconds between two dots");
            lng = new ArgIntMM(0, "l", "language", "language of application (0-en,1-ru,2-ua)", "LNG");
            lng.setMax(2);
            lng.setMin(0);
            zoom = new ArgIntMM(10, "z", "zoom", "zoom level for map", "ZZZ");
            zoom.setMax(19);
            zoom.setMin(2);

            int ccc = 3;
            count = new ArgInt(ccc, "cou", "count"
                         , "the number of tiles of map (" + ccc + "x" + ccc + ")", "NNN");
            countCache
                   = new ArgInt(-1, "cache", "cache", "count(KB) of max size db to cache tile", "NNN");

            decPnt = new ArgStr(".", "dp", "decimalPoint", "decimal point", "P");

            db = new ArgStr("Data Source=MBTiles.db", "db"
                              , "dbCon", "set the connection string to database", "stringConection");
            clear = new ArgFlg(false, "c", "clear", "clear database");
            select = new ArgFlg(false, "s", "select", "to select list of all the tiles");
            create = new ArgFlg(false, "crt", "create", "create new database");

            tile4trck = new ArgFlg(false, "t4t", "tile4trck", "download in internet tiles need to track");

            ///50.441074, 30.431393   -- нау
            B = new ArgFloatMM(50.441074, "ltt", "latitude", "degrees of latitude", "B");
            B.setMax(80.0);
            B.setMin(0.0);
            L = new ArgFloatMM(30.431393, "lng", "longitude", "degrees of longitude", "L");
            L.setMax(180.0);
            L.setMin(0.0);
            
            Names.Add(inscr.TMSTP,   "Интервал",     "Інтервал");
            Names.Add(inscr.CACHE,   "Кэш",          "Кеш");
            Names.Add(inscr.LANG,    "Язык",         "Мова");
            Names.Add(inscr.FILE,    "Файл",         "Файл");
            Names.Add(inscr.HLP,     "Справка",      "Справка"); 
            Names.Add(inscr.DPOINT,  "Десят. точка", "Десят. крапка");
            Names.Add(inscr.VERB,    "Расшириная справка", "Розширина справка");
            Names.Add(inscr.DBCON,   "Подключение",  "Підключення");
            Names.Add(inscr.LOGLVL,  "Уровень логирования", "Рівень логування");
        }

        static public  Arg[] mkAr( )
        {
          return Arg.mkArgs(
                  hlp
                , flNM
                , dixiF
                , ASCF
                , miniF
                , gpx2csvF
                , db
                , open
                , visi
                , yand
                , B
                , L
                , zoom
//                , count      только 3 на 3
//                , clear
//                , select
//                , create
                , countCache
//                , tmStep
                , decPnt
                , lng
                , vF
                , logNm
          );
        }
        static public void usage()
        {

            System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom(".\\MBTile.dll");
            int ma = 0, mi = 0, bld = 0;
            Arg.version(out ma, out mi, out bld);
            Arg.mkVHelp(
                  String.Format("test unit to work with raster map")

                // System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
                //  +"/"+
                , asm.GetName().Version.ToString()
                , "-osm | -vi | -ya  [-f fileName  { -dixi | -ASC | -mini | -g2c } ]"
                , vF
                , mkAr()
                );
            Environment.Exit(1);
        }
        static public string mkArgs()
        {
            return Arg.mkArgs(  mkAr(), 1);
        }
        static protected string me {
            get {
                return (new Program()).GetType().ToString();
            }
        }
        static public void WriteLine(String Format       ///< строка с форматом сообщения
                             , params object[] Segments   ///< параметры сообщения
                    ) {
            if (l != null)
                l.WriteLine(String.Format("{0}->{1}", me, Format)
                    , Segments);
        }

        static public void WriteLine(IMPORTANCELEVEL Importance  ///< важность данного сообщения
                             , String Format              ///< строка с форматом сообщения
                             , params object[] Segments   ///< параметры сообщения
                    ) {
            if (l != null)
                l.WriteLine(Importance
                  , String.Format("{0}->{1}", me, Format)
                    , Segments);
        }
        [STAThread]
        static void Main(string[] args)
        {
            Service service = Service.Visicom;
            IMPORTANCELEVEL iLvl = IMPORTANCELEVEL.Error;

            FormToShow form = null;
            MapToShow mTS = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (flNM.check(ref i, args)) ;
                else if (dixiF.check(ref i, args))    { ASCF.restore();  miniF.restore(); gpx2csvF.restore(); }
                else if (ASCF.check(ref i, args))     { dixiF.restore(); miniF.restore(); gpx2csvF.restore(); }
                else if (miniF.check(ref i, args))    { ASCF.restore();  dixiF.restore(); gpx2csvF.restore(); }
                else if (gpx2csvF.check(ref i, args)) { dixiF.restore(); ASCF.restore();  miniF.restore();   }
                else if (db.check(ref i, args)) ;
                else if (clear.check(ref i, args)) ;
                else if (select.check(ref i, args)) ;
                else if (create.check(ref i, args)) ;
                else if (open.check(ref i, args)) service = Service.OpenStreetMap;
                else if (visi.check(ref i, args)) service = Service.Visicom;
                else if (yand.check(ref i, args)) service = Service.Yandex;
                //else if (wayF.check(ref i, args)) ;
                else if (zoom.check(ref i, args)) ;
                else if (lng.check(ref i, args)) ;
                else if (B.check(ref i, args)) ;
                else if (L.check(ref i, args)) ;
                else if (tmStep.check(ref i, args)) ;
                else if (count.check(ref i, args)) ;
                else if (countCache.check(ref i, args)) ;
                else if (vF.check(ref i, args)) ;
                else if (decPnt.check(ref i, args)) ;
                else if (logNm.check(ref i, args))
                    iLvl = Loger.strtoLvl(logNm);
                else if (hlp.check(ref i, args))
                    usage();
            }

            //Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU", false);
            char NumberDecimalSeparator
              = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

            bool createWay = false;
            var listWayPoint = new List<PointF> { };    //Список точек из файла
            var listWayPointPaint = new List<Point> { };    //Список точек для рисования траектории

            double lat = B;
            double lon = L;
            Line ln = null;
            int Zoom = zoom;

            //границы города Киев
            // Space space = new Space(50.526113, 30.307416, 50.314883, 30.714473);

            DateTime st = DateTime.Now;
            using (l = new Loger(logNm, false))
            {
                if (vF) l.cnslLvl = IMPORTANCELEVEL.Debug;
                WriteLine(IMPORTANCELEVEL.Stats,
                   "Arguments of application are: \n<-----{0}\n<-----", mkArgs());


                //    WriteLine(IMPORTANCELEVEL.Debug            // to test space to string conversion 
                //        , "Kiev box: {0}", 
                //             (string)  space);
                string dP = decPnt;
                rec r = null;
                #region WAY_REGION
                if (File.Exists(flNM) && (gpx2csvF || dixiF || ASCF || miniF))  //если присуствует файл с траекторией отображаем ее на карте
                {
                    int lineno = count;
                    if (gpx2csvF) r = new gpxCsv(flNM, l, dP[0]);
                    if (miniF) r = new miniRec(flNM, l, dP[0]);
                    if (dixiF) r = new dixiRec(flNM, l, dP[0]);
                    if (ASCF) r = new ASCRec(flNM, l, dP[0]); // родили объект нужного типа

                    //   Pen p = new Pen(Color.DarkGray, 1);
                    Pen p = new Pen(Color.DarkBlue, 1);
                    tuple t;
                    ln = new Line("track test", line.track
                              , p
                              , "longitude"
                              , "latitude"
                              , "altitude"
                              , l
                              );
                    createWay = true;
                    //string[] sep = { " " };
                    WriteLine(IMPORTANCELEVEL.Debug, "decimal point is {0}/'{1}'"
                                , dP, decPnt.ToString());

                    WriteLine(IMPORTANCELEVEL.Warning, "Main: ready to read");

                    ln.begin();

                    while (r.ReadLine())
                    {
                        lineno++;
                        if (r.lr > 0.0 && r.bt > 0.0)
                            ln.add(r.lr, r.bt, r.du, r.make());
                    }
                    r.Dispose();

                    if (ln.lst.Count > 0)
                    {
                        //точка фокуса на первой точке из нашего файла
                        lon = ln.lst[0].X;
                        lat = ln.lst[0].Y;
                    }
                    else
                        WriteLine(IMPORTANCELEVEL.Warning, "Read / to few points of track: {0}", listWayPoint.Count);

                    B.edit = false;
                    L.edit = false;
                }
                else
                {
                    r = new miniRec(l);
                    WriteLine(IMPORTANCELEVEL.Error, "No such track: '{0}' or not set type of track"
                                              , (string)flNM);
                }
                #endregion
                
                #region WORK_DB_REGION
                WorkDb workDb = new WorkDb(db, l); //создание обьекта для работы с базе даных
                if (create) //при необходимости создание новых таблиц в базе даных{
                    workDb.NewTable();

                if(!(countCache < 0))
                    workDb.RemoveCache(countCache); //удаление старых тайлов
                #endregion

                mTS = new MapToShow(db, service, count, count, lat, lon, Zoom, r, vF, l);

                form = new FormToShow(mTS, vF, lng);
                form.tlbParam.Enabled = true;
                form.tlbParam2.Enabled = true;
                form.tlbSave.Enabled = true;
                form.tlbExport.Enabled = true;

                if (createWay)
                {
                    WriteLine(IMPORTANCELEVEL.Error, "setTrck is here ");

                    mTS.setTrck(ln);

                    form.tlbStat.Enabled = true;
                }

                form.wayUpButton.handler   (  mTS.forward);
                form.wayDownButton.handler (  mTS.back);
                form.wayLeftButton.handler (  mTS.left);
                form.wayRightButton.handler(  mTS.right);
                form.zoomInButton.handler  (  mTS.zoomDown);
                form.zoomOutButton.handler (  mTS.zoomUp);

                form.yawLeftButton.handler (  mTS.yawLeft);
                form.yawRightButton.handler(  mTS.yawRight);

                form.resetButton.handler   (  mTS.reset);
                form.playPauseButton.handler( mTS.play_pause);

                
                Application.EnableVisualStyles();

                if (clear || select)
                {
                    if (clear)  //очистка таблиц базы даных
                        workDb.RemoveDB();
                    else if (select)    //просмотр тайлов в базе даных
                    {
                        var listInfoTile = workDb.SelectAll();

                        for (int i = 0; i < listInfoTile.Count; i++, Console.WriteLine())
                            for (int j = 0; j < listInfoTile[i].Count; j++)
                                Console.Write(listInfoTile[i][j] + " ");
                    }
                }
                else
                    Application.Run(form);

                DateTime fn = DateTime.Now;

                WriteLine(IMPORTANCELEVEL.Stats, "time of work is {0} secs"
                     , (fn - st).TotalSeconds);
            }
        }
    }
}
