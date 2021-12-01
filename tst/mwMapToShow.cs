using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;

using MBTile;
using Args;
using Logger;
using geo;

using ut;
using wnd;

namespace MapWnd
{
    class MapToShow : usingLogger
    {
        public PictureBox _photoPicBox;
        public PictureBox _mapPicBox;
        public PictureBox _miniPicBox;

        public StatusStrip _stS;

        public _Label _yawLabel;
        public _Label _zoomLabel;
        public _Label _pitchLabel;
        public _Label _rollLabel;

        private System.Windows.Forms.SaveFileDialog _fileDialog1;
        private System.Windows.Forms.SaveFileDialog _mapExport1;
        System.Windows.Forms.Timer timer1;

        Ellipsoid g = new Ellipsoid(ellipsoid.wgs84);

        public double lat = 0.0;
        public double lon = 0.0;

        static MapToShow (){
              Names.Add(inscr.TRACK,   "Трек",         "Трек"); 
              Names.Add(inscr.START,   "Начальное",    "Початковий"); 
              Names.Add(inscr.CURRENT, "Текущее",      "Поточний"); 
        }
        public double Lat
        {
            get {
                if (_ln != null)
                    Lat = _ln.lst[indexPoint].Y;

                return lat;
            }
            set { lat = Math.Abs(value) % 80; }
        }
        public double Lon
        {
            get
            {
                if (_ln != null)
                    Lon = _ln.lst[indexPoint].X;

                return lon;
            }
            set { lon = Math.Abs(value) % 180; }
        }
        public int _zoom = 2;

        readonly int _countX;
        readonly int _countY;

        double step = 0.01;
        int stepPxl = 1;

        double yawDegree;
        double pitchDegree;
        double rollDegree;
        public double YawDegree
        {
            get
            {
                if (_ln != null)
                    if (_ln.lst[indexPoint].ContainsKey("yaw")) //проверка наличия ключа в данной заипси
                        YawDegree = (double)(_ln.lst[indexPoint]["yaw"]);
                    else
                    {
                        if (indexPoint == _ln.lst.Count - 1)
                            YawDegree = 0;
                        else
                        {
                            double distance, A_1, A_2;

                            g.inverseGeoDTask1(
                                _ln.lst[indexPoint].Y, _ln.lst[indexPoint].X,
                                _ln.lst[indexPoint + 1].Y, _ln.lst[indexPoint + 1].X,
                                out distance,
                                out A_1, out A_2);
                            if (A_1 <= 0.0 && A_2 <= 0.0)
                              ;
                            else
                              YawDegree = A_1;
                        }
                    }

                return yawDegree;
            }
            set { yawDegree = value % 360; }
        }
        public double PitchDegree
        {
            get
            {
                if (_ln != null)
                    if (_ln.lst[indexPoint].ContainsKey("pitch")) //проверка наличия ключа в данной заипси
                        PitchDegree = (double)(_ln.lst[indexPoint]["pitch"]);

                return pitchDegree;
            }
            set { pitchDegree = value % 360; }
        }
        public double RollDegree
        {
            get
            {
                if (_ln != null)
                    if (_ln.lst[indexPoint].ContainsKey("roll")) //проверка наличия ключа в данной заипси
                        RollDegree = (double)(_ln.lst[indexPoint]["roll"]);

                return rollDegree;
            }
            set { rollDegree = value % 360; }
        }

        string namePhoto;
        string NamePhoto
        {
            get
            {
                if (_ln != null)
                    if (_ln.lst[indexPoint].ContainsKey("photo")) //проверка наличия ключа в данной заипси
                        NamePhoto = _ln.lst[indexPoint]["photo"].ToString();

                return namePhoto;
            }
            set { namePhoto = value; }
        }
        public Bitmap photoLand
        {
            get
            {
                if (NamePhoto == "") return null;

                //убрать _ - так как это ужатые файлы
                string wayTrckFile = Program.flNM; //весь путь
                string path = 
                    Path.GetDirectoryName(wayTrckFile) == "" ?  //если папка с фотками лежит там где и файл
                        "./photos/_" + NamePhoto + ".jpg"       //вместо пути берем "./"
                            : 
                        Path.GetDirectoryName(wayTrckFile) + "/photos/_" + NamePhoto + ".jpg";//иначе берем полный путь 

                var masByteRgbQ = BitmapToByteRgbQ(LoadBitmap(path));

                return new Bitmap(RgbToBitmapQ(masByteRgbQ),
                    new Size(_photoPicBox.Width, _photoPicBox.Height)); //взят размер picBox-а на котором отображаеться картинка
            }
        }

        int altitude;
        int Altitude
        {
            get
            {
                if (_ln != null)
                    Altitude = _ln.lst[indexPoint].z;

                return altitude;
            }
            set { altitude = value; }
        }

        int _indexPoint;
        public int indexPoint
        {
            get { return _indexPoint; }
            set
            {
                //что б не выйти за границы списка
                if (_ln != null)
                    _indexPoint = Math.Abs(value) % _ln.lst.Count;
            }
        }

        Tile miniMap;

        int mainZoom = 0;

        int X, Y;

        int xShift = 0;
        int yShift = 0;

        public MBTiles _mbTile = null;
        public Line _ln;
        bool mkBSLr;
        bool vF;

        Font f;
        Pen blackPen;
        Pen trckPen;
        Pen mainSpacePen;
        Pen curentSpacePen;

        int lH;               // letter Height
        string cnn;
        Service _srv;

        Space sp00;
        Space sp11;

        Space mainSp00;
        Space mainSp11;

        wStatistic wStat;

        Line exSpeed = null;
        Line stops = null;
        Line dotClick = null;
        rec r = null;

        public MapToShow(
                  string strConnection, Service service
                    , int countX, int countY
                      , double lat, double lon, int zoom
                       ,rec r
                        , bool verbose
                         , Loger l) : base(l)
        {
            _countX = countX;
            _countY = countY;
            _zoom = zoom;
            Lat = lat;
            Lon = lon;

            _mapPicBox = new PictureBox();
            _mapPicBox.Size = new Size(256 * countX, 256 * countY);

            _mapPicBox.Paint += paintBL1;       //рисование карты
            _mapPicBox.Paint += paintInscr;     //рисование надписей
            _mapPicBox.Paint += paintTrck;      //рисование трека
            _mapPicBox.Paint += paintDotClick;  //рисование наклацаных точек

            _mapPicBox.MouseClick += _mapPicBox_MouseClick;

            _miniPicBox = new PictureBox();
            _miniPicBox.Size = new Size(256, 256);

            _miniPicBox.Paint += paintMiniMap;  //рисование мини карты
            _miniPicBox.Paint += paintAddition; //рисование доп. инфор.

            _photoPicBox = new PictureBox();
            _photoPicBox.Size = new Size(256, 256);

            mkBSLr = true;
            cnn = strConnection;
            _srv = service;

            _mbTile = new MBTiles(cnn, _srv, _countX, countY, Lat, Lon, _zoom, l);
            if (_ln != null)
                _ln.commit(_mbTile.BL2XY);
            vF = verbose;

            mainZoom = _mbTile.zoom;

            double w;
            double h;

            _mbTile.findPxlSize(out w, out h);
            step = (w + h);//2.0;

            //версия мини карты основана на большой карте
            mainSp00 = _mbTile.XY2BL(0, 0);
            mainSp11 = _mbTile.XY2BL(256 * _countX-1, 256 * _countY-1);

            miniMap = setMini();

            WriteLine(IMPORTANCELEVEL.Warning, "Ctr: zoom main/mini: service main/mini {0}/{1}:  {2}/{3}"
                  , _zoom, miniMap.zoom
                   , _srv, miniMap.service );

            f = new Font("Time New Roman", SZ.FONT, FontStyle.Bold);
            blackPen = new Pen(Brushes.Black, 1);
            trckPen = new Pen(Color.FromName(Program.trckCol), 1);
            mainSpacePen = new Pen(Color.FromName(Program.startVW), 2); //красная кисть для рисования области начальной карты
            curentSpacePen = new Pen(Color.FromName(Program.curVW), 2); //серая кисть для рисования области текущей карты

            _fileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this._fileDialog1.FileName = "data.txt"; //
            this._fileDialog1.InitialDirectory = ".";            // 
            this._fileDialog1.Filter = "txt files (*.txt)|*.txt| csv files (*.csv)|*.csv| All files (*.*)|*.*";
            this._fileDialog1.FilterIndex = 1;
            this._fileDialog1.RestoreDirectory = true;

            _mapExport1 = new System.Windows.Forms.SaveFileDialog();
            this._mapExport1.FileName = "data.png"; //
            this._mapExport1.InitialDirectory = ".";            // 
            this._mapExport1.Filter = "map files (*.png)|*.png| All files (*.*)|*.*";
            this._mapExport1.FilterIndex = 1;
            this._mapExport1.RestoreDirectory = true;

            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new System.EventHandler(timer1_Tick);
            timer1.Interval = (int)Program.tmStep;

            this.r = r;

            exSpeed = new Line("excessSpeed", line.dots
                    , new Pen(Color.Red)
                    , "latitude"
                    , "longitude"
                    , "altitude"
                    , l);
            stops = new Line("stops", line.dots
                    , new Pen(Color.Green)
                    , "latitude"
                    , "longitude"
                    , "altitude"
                    , l);

            exSpeed.commit(_mbTile.BL2XY);
            stops.commit(_mbTile.BL2XY);
        }

        void _mapPicBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (dotClick == null)
            {
                dotClick = new Line("dotsClick", line.dots
                    , new Pen(Color.DarkGray)
                    , "longitude"
                    , "latitude"
                    , "altitude"
                    , l);
            }

            dotClick.add(
                    _mbTile.XY2BL(e.X, e.Y).leftTop.longitude
                  , _mbTile.XY2BL(e.X, e.Y).leftTop.latitude
                  , 0
                  , r.make()
                );

            Invalidate();
        }

        public void Invalidate()
        {
            _mapPicBox.Invalidate();
            _photoPicBox.Invalidate();
            _miniPicBox.Invalidate();
        }

        public void setTrck(Line ln)
        {
            WriteLine(IMPORTANCELEVEL.Error, "setTrck ");
            _ln = ln;
            _ln.commit(_mbTile.BL2XY);
            _photoPicBox.Image = (Image)photoLand;
        }

        ///paint Base Map --start
        void paintBL1(object s, PaintEventArgs e)
        {
            WriteLine(IMPORTANCELEVEL.Warning, "paint base layer. Zoom/focus: {0}/{1}"
                  , _zoom
                    , mk.txt(Lat, Lon));
            drawBaseLayer(e.Graphics);
        }
        void paintInscr(object s, PaintEventArgs e)
        {
            drawString(e.Graphics); //рисование широты-долготы
        }
        void paintTrck(object s, PaintEventArgs e)
        {
            WriteLine(IMPORTANCELEVEL.Warning, "paint track");
            
            //Graphics.FromImage(_mapPicBox.Image)
            drawTrck(e.Graphics);
            drawFocus(e.Graphics, YawDegree);

            if (wStat != null)
            {
                wStat.drawStops(e.Graphics, _mbTile); //рисование остановок
                wStat.drawExSpeed(e.Graphics, _mbTile); //рисование точек привешение скорости
            }   
        }

        void paintDotClick(object s, PaintEventArgs e)
        {
            WriteLine(IMPORTANCELEVEL.Warning, "paint dotClick");

            drawDotClick(e.Graphics);
        }
        /*                        L         L+w
                                  B
                                  B-h

                  dPrt  == от 1 до 1000000 -- 1000000 это 1 градус*/

        public void drawBaseLayer(Graphics g)
        {
            var list = _mbTile.listTile;
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list[i].Count; j++)
                {
                    WriteLine(IMPORTANCELEVEL.Stats,
                           "{0}: Image: i/j: {1}/{2} "
                                , "drawBaseLayer"
                                  , i, j
                                  );
                    Tile tile = list[i][j];
                    g.DrawImage
                        (
                            new Bitmap(new MemoryStream(tile.tile)),
                            new Point(256 * i, 256 * j)
                        );
                    if (vF)
                        g.DrawRectangle(Pens.Black, 256 * i, 256 * j, 256, 256); //рисование границ тайла
                }
            }

            _zoomLabel.Text = string.Format("{0}:{1,2}", Names.Text(inscr.ZOOM), _zoom);
            _yawLabel.Text = string.Format("{0}:{1,5:0.0}°", Names.Text(inscr.YAW), YawDegree);
            _rollLabel.Text = string.Format("{0}:{1,5:0.0}°", Names.Text(inscr.ROLL), RollDegree);
            _pitchLabel.Text = string.Format("{0}:{1,5:0.0}°", Names.Text(inscr.PITCH), PitchDegree);
        }
        public void drawString(Graphics g)
        {
            int padding = 5;
            double Ltl; //  left
            double Ltr; //  right

            double B;  //  top
            double Bl; //  top
            double Bt; //  top
            double Bb; //  bottom

            //  считаем с 0
            int BSz;  //  размер 1го тайла по меридиану
            int LSzT; //  размер 1го по верхней параллели
            int LSzB; //  размер 1го по нижней параллели
            Point a;
            Point b;

            string units = "m";

            Ellipsoid el = new Ellipsoid(ellipsoid.wgs84);

            B = _mbTile.XY2BL(256 * 1, 0).leftTop.latitude;
            Bl = _mbTile.XY2BL(256 * 1, 256 * 3).rightBot.latitude;

            Ltl = _mbTile.XY2BL(256 * 1, 0).leftTop.longitude;
            Ltr = _mbTile.XY2BL(256 * 2, 0).leftTop.longitude;

            LSzT = (int)el.prlD2m(B, Ltr - Ltl);
            LSzB = (int)el.prlD2m(Bl, Ltr - Ltl);
            units = "m";
            if (Ltr - Ltl > 1) {
                units = "km";
                LSzT /= 1000;
                LSzB /= 1000;
                LSzT = (int)LSzT; //оставил только целые км. можно округлить +0.5
                LSzB = (int)LSzB; //оставил только целые км. можно округлить +0.5
            }
            g.DrawString( // top of map
                LSzT.ToString() + units
                    , f
                    , Brushes.Black
                    , 256 * 1 + padding + 128
                    , padding);

            g.DrawString(        // bottom of map
                LSzB.ToString() + units
                    , f
                    , Brushes.Black
                    , 256 * 1 - padding + 128
                    , 256 * (_mbTile.listTile.Count) - padding - SZ.FONT - 5);

            Bt = _mbTile.XY2BL(0, 256 * 1).leftTop.latitude;
            Bb = _mbTile.XY2BL(0, 256 * 2).rightBot.latitude;

            BSz = (int)el.mrdD2m(Bb, Bt - Bb);

            units = "m";
            if (Bt - Bb > 1) {
                units = "km";
                BSz /= 1000;
                BSz = (int)BSz;
            }
            g.DrawString(        // bottom of map // left of map
                BSz.ToString() + units
                    , f
                    , Brushes.Black
                    , padding
                    , 256 * 1 + 128 - padding - SZ.FONT - 2);


            int countRoundB = Bt - Bb > 1 ? 3 : 6;
            int countRoundL = Ltr - Ltl > 1 ? 3 : 6;
            //вывод долгот-широт
            for (int i = 1; i < _mbTile.listTile.Count; i++)
            {
                //подпись долгот
                g.DrawString(
                    Math.Round(_mbTile.XY2BL(256 * i, 0).leftTop.longitude, countRoundL).ToString() + "E"
                        , f
                        , Brushes.Black
                        , 256 * i + padding, padding);
                a = new Point(256 * i, 0);
                b = new Point(256 * i, 2 * padding);
                g.DrawLine(blackPen, a, b);

                a = new Point(256 * i, 256 * _mbTile.listTile.Count);
                b = new Point(256 * i, 256 * _mbTile.listTile.Count - 2 * padding);
                g.DrawLine(blackPen, a, b);

                for (int j = 1; j < _mbTile.listTile[i].Count; j++)
                {
                    //подпись широт
                    g.DrawString(
                        Math.Round(_mbTile.XY2BL(0, 256 * j).leftTop.latitude, countRoundB).ToString() + "N"
                        , f
                        , Brushes.Black
                        , padding
                        , 256 * j - padding - SZ.FONT - 2);

                    a = new Point(0, 256 * j);
                    b = new Point(2 * padding, 256 * j);
                    g.DrawLine(blackPen, a, b);
                }
            }

            //копирайт
            if (_srv == Service.OpenStreetMap)
                g.DrawString(
                    (char)169 + "Учасники OpenStreetMap", f, Brushes.Black,
                    _countX * 256 - 170, _countY * 256 - SZ.FONT - padding - 5
                    );
            else if (_srv == Service.Visicom)
                g.DrawString(
                    (char)169 + " 2016 ПрАТ \"Візіком\"", f, Brushes.Black,
                    _countX * 256 - 150, _countY * 256 - SZ.FONT - padding - 5
                    );
            else if (_srv == Service.Yandex)
                g.DrawString(
                    (char)169 + " Яндекс", f, Brushes.Black, 
                    _countX * 256 - 65, _countY * 256 - SZ.FONT - padding - 5
                    );

            WriteLine(IMPORTANCELEVEL.Warning
                , "paint string (longitutes/latitudes/meters): Bt/Bl: {0:0.000000}/{1:0.000000} ",
                                 Bt, Bb);
            WriteLine(IMPORTANCELEVEL.Debug
                , "paint string (top B/sz / bottom B/sz: {0:0.000000}/{1} / {2:0.000000}/{3}",
                                 B, LSzT, Bl, LSzB);
        }
        public void drawFocus(Graphics graphics, double angle)
        {
            _mbTile.BL2XY(Lat, Lon, out X, out Y);

            X += xShift;
            Y += yShift;

            WriteLine(IMPORTANCELEVEL.Warning
                    , "{0}: new shifts(x/y)/coor: ({1}/{2})/{3}", "drawFocus"
                      , xShift, yShift, mk.txt(Lat, Lon));

            //отметка на карте заданой точки
            graphics.DrawPie(
                new Pen(Color.Black, 3) //кисть для рисования
                , X - 10, Y - 10        //точка левого верхнего угла прямоугольника
                , 20, 20                //размеры описывающего круг прямоугольника
                , 60 + (int)angle            //угол от Ох и до первой стороны сектора
                , 60);                  //угол сектора

            if (_stS != null)
                _stS.Items[0].Text = mk.txt(Lat, Lon) + ", " + inscr.ALT + ": " + Altitude + ", NN: " + NamePhoto;
        }
        public void drawTrck(Graphics g)
        {
            if (_ln != null)
                if (_ln.ps != null)
                {
                    WriteLine(IMPORTANCELEVEL.Warning
                            , "Ready to draw track:length {0}  ", _ln.ps.Length);

                    if (_ln.ps.Length == 1)
                        g.DrawEllipse(trckPen, _ln.ps[0].X, _ln.ps[0].Y, 2, 2); //2px - размер квадрата в который вписан елипс
                    else
                        g.DrawLines(trckPen, _ln.ps);

                    for (int i = 0; i < _ln.ps.Length; i++)
                    {
                        WriteLine(IMPORTANCELEVEL.Debug
                          , " coors :x/y {0}/{1}", _ln.ps[i].X, _ln.ps[i].Y
                        );
                    }
                }
                else
                    WriteLine(IMPORTANCELEVEL.Error, "No any array ps ");
        }

        public void drawDotClick(Graphics g)
        {
            if (dotClick == null)
                return;

            dotClick.commit(_mbTile.BL2XY);

            if (dotClick.ps.Length == 0)
            {
                WriteLine(IMPORTANCELEVEL.Error, "No any array ps ");
                return;
            }

            WriteLine(IMPORTANCELEVEL.Warning
                        , "Ready to draw dotClick:length {0}  ", dotClick.ps.Length);

            if (dotClick.ps.Length == 1)
                g.DrawEllipse(trckPen,
                    dotClick.ps[0].X - 1, dotClick.ps[0].Y - 1,
                    2, 2); //2px - размер квадрата в который вписан елипс
            else
                g.DrawLines(trckPen, dotClick.ps);
        }
        ///paint Base Map --end

        ///paint Mini Map --start
        void paintMiniMap(object s, PaintEventArgs e)
        {
            WriteLine(IMPORTANCELEVEL.Warning, "paint base layer for miniMap. Zoom: {0}"
                 , _zoom);

            drawBaseMini(e.Graphics);
        }
        void paintAddition(object s, PaintEventArgs e)
        {
            WriteLine(IMPORTANCELEVEL.Warning, "paint addition information on mini map.");
            
            //drawTrckInMini(e.Graphics);
            drawAdditionInfo(e.Graphics);
            drawFocusInMini(e.Graphics, YawDegree);
        }

        public void drawBaseMini(Graphics g) //рисование мини карты
        {
            WriteLine(IMPORTANCELEVEL.Warning, "drawBaseMini: zoom main/mini: service main/mini {0}/{1}: {2}/{3}"
            , _zoom, miniMap.zoom
             , _srv, miniMap.service );

            g.DrawImage(
                new Bitmap(new MemoryStream(miniMap.tile)),
                new Point(0, 0)
                );
        }
        public void drawFocusInMini(Graphics g, double angle) //рисование точки фокуса на мини карте
        {
            Space dotFocus = _mbTile.XY2BL(X, Y); //область точки фокуса
            _mbTile.BL2xy(dotFocus.leftTop.latitude, dotFocus.leftTop.longitude,
                miniMap, out X, out Y);  //точка фокуса в пикселях на мини карте
            WriteLine(IMPORTANCELEVEL.Warning
           , "drawFocusInMini: zoom main/mini: service main/mini {0}/{1}: {2}/{3}; row/column {4}/{5}"
            , _zoom, miniMap.zoom
             , _srv, miniMap.service, miniMap.row, miniMap.column);

            WriteLine(IMPORTANCELEVEL.Warning
               , "drawFocusInMini: coor/X/Y: {0}/{1}/{2}"
            , mk.txt(dotFocus.leftTop.latitude, dotFocus.leftTop.longitude)
             , X, Y);

            g.DrawPie(
                new Pen(Color.Black, 3)  //кисть для рисования
                , X - 10, Y - 10         //точка левого верхнего угла прямоугольника
                , 10, 10                 //размеры описывающего круг прямоугольника
                , 60 + (int)angle        //угол от Ох и до первой стороны сектора
                , 60);                   //угол сектора
        }
        public void drawAdditionInfo(Graphics g) //рисование квадратов на миникарте
        {
            int X00, Y00, 
                X11, Y11;

              WriteLine(IMPORTANCELEVEL.Info
               , "drawAdditionInfo: zoom main/mini: service main/mini {0}/{1}: {2}/{3} count {4}; row/column {5}/{6}"
              , _zoom, miniMap.zoom
               , _srv, miniMap.service, _countX, miniMap.row, miniMap.column  );

            sp00 = _mbTile.XY2BL(0, 0);
            sp11 = _mbTile.XY2BL(256 * _countX, 256 * _countY);


            _mbTile.BL2xy(50.513427, 29.838867, miniMap, out X00, out Y00);
            WriteLine(IMPORTANCELEVEL.Info
               , "drawAdditionInfo0: coor/X/Y: {0}/{1}/{2}"
            ,  mk.txt(50.513427, 29.838867)
             , X00, Y00 );


            //версия квадрата на мини карте, который показывал область большой карты
            _mbTile.BL2xy(sp00.leftTop.latitude, sp00.leftTop.longitude, miniMap, out X00, out Y00);
            WriteLine(IMPORTANCELEVEL.Info
               , "drawAdditionInfo: coor/X/Y: {0}/{1}/{2}"
            ,  mk.txt(sp00.leftTop.latitude, sp00.leftTop.longitude)
             , X00, Y00 );
            _mbTile.BL2xy(sp11.rightBot.latitude, sp11.rightBot.longitude, miniMap, out X11, out Y11);
            g.DrawRectangle(curentSpacePen, X00, Y00, Math.Abs(X11 - X00), Math.Abs(Y11 - Y00)); //область текущей карты

            _mbTile.BL2xy(mainSp00.leftTop.latitude, mainSp00.leftTop.longitude, miniMap.zoom, out X00, out Y00);
            _mbTile.BL2xy(mainSp11.rightBot.latitude, mainSp11.rightBot.longitude, miniMap.zoom, out X11, out Y11);
            g.DrawRectangle(mainSpacePen, X00, Y00, Math.Abs(X11 - X00), Math.Abs(Y11 - Y00)); //область начальной карты
        }
        //нет надобности V
        public void drawTrckInMini(Graphics g) //рисование трека на мини карте
        {
            if (_ln != null)
                if (_ln.lst != null)
                {
                    WriteLine(IMPORTANCELEVEL.Info
                            , "Ready to draw track:length {0}  ", _ln.lst.Count);
                    int X, Y;

                    //List<Point> trck = new List<Point> { };
                    Point[] trck = new Point[_ln.lst.Count];
                    for (int i = 0; i < _ln.lst.Count; i++)
                    {
                        _mbTile.BL2xy(_ln.lst[i].Y, _ln.lst[i].X, miniMap, out X, out Y);
                        //trck.Add(new Point(X, Y));
                        trck[i] = new Point(X, Y);

                        WriteLine(IMPORTANCELEVEL.Debug
                              , " coors :x/y {0}/{1}", X, Y
                            );
                    }

                    if (_ln.lst.Count == 1)
                        g.DrawEllipse(trckPen, trck[0].X, trck[0].Y, 2, 2); //2px - размер квадрата в который вписан елипс
                    else
                        g.DrawLines(trckPen, trck);
                }
                else
                    WriteLine(IMPORTANCELEVEL.Error, "No any array ln ");
        }

        Tile setMini()
        {
            WriteLine(IMPORTANCELEVEL.Debug, "setMini is here ");
            
            sp00 = _mbTile.XY2BL(0, 0);
            sp11 = _mbTile.XY2BL(256 * _countX-1, 256 * _countY-1);
            Tile rc = 
            _mbTile.Space2tile(new Space(
                    Math.Max(sp00.leftTop.latitude, mainSp00.leftTop.latitude),
                    Math.Min(sp00.leftTop.longitude, mainSp00.leftTop.longitude),
                    Math.Min(sp11.rightBot.latitude, mainSp11.rightBot.latitude),
                    Math.Max(sp11.rightBot.longitude, mainSp11.rightBot.longitude))
                );
            
            WriteLine(IMPORTANCELEVEL.Debug, "--current {0}/{1}"
                            ,mk.txt(sp00.leftTop.latitude, sp00.leftTop.longitude)
                             ,mk.txt(sp11.rightBot.latitude, sp11.rightBot.longitude)      );
            WriteLine(IMPORTANCELEVEL.Debug, "----start {0}/{1}"
                            ,mk.txt(mainSp00.leftTop.latitude, mainSp00.leftTop.longitude)
                             ,mk.txt(mainSp11.rightBot.latitude, mainSp11.rightBot.longitude )     );
            WriteLine(IMPORTANCELEVEL.Debug, "------max {0}/{1}"
                            ,mk.txt(
                                      Math.Max(sp00.leftTop.latitude, mainSp00.leftTop.latitude)
                                 , 
                                      Math.Min(sp00.leftTop.longitude, mainSp00.leftTop.longitude)
                             )
                             ,mk.txt(
                                     Math.Min(sp11.rightBot.latitude, mainSp11.rightBot.latitude)
                                 , 
                                     Math.Max(sp11.rightBot.longitude, mainSp11.rightBot.longitude)
                              )     );

            WriteLine(IMPORTANCELEVEL.Info, "--zoom/row/column: {0}/{1}/{2}"
                           ,  rc.zoom
                           ,  rc.row
                           ,   rc.column
            );

       

            WriteLine(IMPORTANCELEVEL.Info, "setMini is finished, zoom/service: {0}/{1}", rc.zoom, rc.service);
            
           return rc;
        }
        ///paint Mini Map --end 

        //получение новой карты, когда точка фокуса вылазит за центральный тайл
        void newMap()
        {
            if (!_mbTile.dotOnCenteraltile(Lat, Lon))
            {
                _mbTile = new MBTiles(cnn, _srv, _countX, _countY, Lat, Lon, _zoom, l);
                if (_ln != null)
                    _ln.commit(_mbTile.BL2XY);

                //версия мини карты основана на большой карте
                miniMap = setMini();
            }
        }

        /// buttons table control --start
        public void zoomUp(object sender, EventArgs e)
        {
            if (_zoom > 2)
                _zoom--;
            double w;
            double h;

            _mbTile.findPxlSize(out w, out h);
            step = (w + h);///2.0;

            _mbTile = new MBTiles(cnn, _srv, _countX, _countY, Lat, Lon, _zoom, l);
            if (_ln != null)
                _ln.commit(_mbTile.BL2XY);

            if (dotClick != null)
            {
                dotClick.begin();
                dotClick.lst.Clear();
                dotClick.commit(_mbTile.BL2XY);
            }

            WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: is here: zoom/step: {1}/{2:0.0000}", "zoomUp", _zoom, step);
            //            warning.NRD(sender, e);

            //mini map
            miniMap = setMini();
            //
            Invalidate();

            _zoomLabel.Text = Names.Text(inscr.ZOOM) + ": " + _zoom;
        }
        public void zoomDown(object sender, EventArgs e)
        {
            if (_zoom < 19)
                _zoom++;

            double w;
            double h;

            _mbTile.findPxlSize(out w, out h);
            step = (w + h);///2.0;

            WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: is here: zoom/step: {1}/{2:0.0000}", "zoomDown", _zoom, step);
            _mbTile = new MBTiles(cnn, _srv, _countX, _countY, Lat, Lon, _zoom, l);

            if (_ln != null)
                _ln.commit(_mbTile.BL2XY);

            if (dotClick != null)
            {
                dotClick.begin();
                dotClick.lst.Clear();
                dotClick.commit(_mbTile.BL2XY);
            }

            //mini map
            miniMap = setMini();

            Invalidate();

            _zoomLabel.Text = Names.Text(inscr.ZOOM) + ": " + _zoom;
        }

        public void forward(object sender, EventArgs e)
        {
            if (_ln == null)
            {
                Lat += step * math.cos(YawDegree);// xShift; 
                Lon += step * math.sin(YawDegree);// yShift;
            }
            else  {
                indexPoint++;
                _photoPicBox.Image = (Image)photoLand;
            }

            newMap();

            WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: new shifts(x/y)/coor: ({1}/{2})/{3}", "forward"
                      , xShift, yShift, mk.txt(Lat, Lon));
            Invalidate();
        }
        public void back(object sender, EventArgs e)
        {
            if (_ln == null)
            {
                Lat += step * math.cos(180 + YawDegree);// xShift;
                Lon += step * math.sin(180 + YawDegree);// yShift;
            }
            else  {
                indexPoint--;
                _photoPicBox.Image = (Image)photoLand;
            }

            newMap();

            WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: new shifts(x/y)/coor: ({1}/{2})/{3}", "back"
                      , xShift, yShift, mk.txt(Lat, Lon));
            Invalidate();
        }
        //лево-право пока что не изменял
        public void left(object sender, EventArgs e)
        {
            if (_ln == null)
            {
                Lat += step * math.cos(270 + YawDegree);// xShift;
                Lon += step * math.sin(270 + YawDegree);// yShift;
            }
            newMap();

            WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: new shifts(x/y)/coor: ({1}/{2})/{3}", "left"
                      , xShift, yShift, mk.txt(Lat, Lon));
            Invalidate();
        }
        public void right(object sender, EventArgs e)
        {
            if (_ln == null)
            {
                Lat += step * math.cos(90 + YawDegree);// xShift;
                Lon += step * math.sin(90 + YawDegree);// yShift;
            }
            newMap();

            WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: new shifts(x/y)/coor: ({1}/{2})/{3}", "right"
                      , xShift, yShift, mk.txt(Lat, Lon));
            Invalidate();
        }

        public void yawRight(object sender, EventArgs e)
        {
            YawDegree += 5;

            WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: degree is : {1}"
                    , "yaw right", YawDegree);

            Invalidate();

            _yawLabel.Text = inscr.YAW + ": " + YawDegree + "°";
        }
        public void yawLeft(object sender, EventArgs e)
        {
            YawDegree -= 5;

            WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: degree is : {1}"
                    , "yaw right", YawDegree);

            Invalidate();

            _yawLabel.Text = inscr.YAW + ": " + YawDegree + "°";
        }

        public void reset(object sender, EventArgs e)
        {
            indexPoint = 0;
            _photoPicBox.Image = (Image)photoLand;
            newMap();

            WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: new shifts(x/y)/coor: ({1}/{2})/{3}", "reset"
                      , xShift, yShift, mk.txt(Lat, Lon));

            if(dotClick != null)
                if(dotClick.lst != null)
                    dotClick.lst.Clear();

            if (stops != null)
                if (stops.lst != null)
                    stops.lst.Clear();

            if (exSpeed != null)
                if (exSpeed.lst != null)
                    exSpeed.lst.Clear();

            Invalidate();
        }
        public void play_pause(object sender, EventArgs e)
        {
            _Button btn = (_Button)sender;
            if (timer1.Enabled) {
                timer1.Enabled = false;
                btn.Text = Names.Text(inscr.PLAY);
            }
            else {
                timer1.Enabled = true;
                btn.Text = Names.Text(inscr.PAUSE);
            }

            /*WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: new shifts(x/y)/coor: ({1}/{2})/{3}", "reset"
                      , xShift, yShift, mk.txt(_lat, _lon));
            Invalidate();*/
        }
        ///buttons table control --end        DarkBlue Red Gray Yellow Black  Green Blue
        //сюда добавлять   нажатия на тулбар
        public bool tBClick(object sender, ToolBarButtonClickEventArgs e)
        {
            #region PAR2
            if (e.Button.Name == inscr.PAR2)
            {
                OkCancelDlg it = new OkCancelDlg(String.Format("{0} {1}"
                                              , Names.Text(inscr.WND)
                                                     , Names.Text(inscr.PAR2))
                                       , this.l
                                       , Program.trckCol, Program.startVW
                                       , Program.curVW, Program.tmStep
                                        , Program.B
                                         , Program.L);
                DialogResult rc = it.ShowDialog();
                if (rc == DialogResult.OK)
                {   // установить новые параметры
                    //при изменении B\L создаем новый _mbTile
                    Lat = Program.B;
                    Lon = Program.L;

                    _mbTile = new MBTiles(cnn, _srv, _countX, _countY, Lat, Lon, _zoom, l);
                    mainZoom = _mbTile.zoom;

                    double w, h;
                    _mbTile.findPxlSize(out w, out h);
                    step = (w + h);//2.0;

                    //версия мини карты основана на большой карте
                    mainSp00 = _mbTile.XY2BL(0, 0);
                    mainSp11 = _mbTile.XY2BL(256 * _countX - 1, 256 * _countY - 1);
                    miniMap = setMini();

                    trckPen = new Pen(Color.FromName(Program.trckCol), 1);
                    mainSpacePen = new Pen(Color.FromName(Program.startVW), 2); 
                    curentSpacePen = new Pen(Color.FromName(Program.curVW), 2);

                    timer1.Interval = (int)Program.tmStep;

                    Invalidate();
                }

                return true;
            }
            #endregion
            #region PAR1
            if (e.Button.Name == inscr.PAR1)
            {
                OkDlg it = new OkDlg(String.Format("{0} {1}"
                                              , Names.Text(inscr.WND)
                                                     , Names.Text(inscr.PAR1))
                                       , this.l
                                       , Program.mkAr());
                it.ShowDialog();
                /*
                       MessageBox.Show(CNST._NRD, String.Format("{0} {1}"
                                                     , Names.Text(inscr.WND)
                                                           , Names.Text(inscr.PAR1)));
                */
                return true;
            }
            #endregion
            #region SAVE
            else if (e.Button.Name == inscr.SAVE)
            {
                if (_ln != null)
                {
                    if (_fileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter sw = new StreamWriter(_fileDialog1.FileName))
                        {
                            _ln.save(sw);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Names.Text(inscr.NTD), Names.Text(inscr.WRN));
                }
                return true;
            }
            #endregion
            #region STAT
            else if (e.Button.Name == inscr.STAT)
            {
                if (_ln != null)
                {
                    wStat = new wStatistic(_ln, exSpeed, stops, r, this.l);
                    wStat.ShowDialog();
                }

                Invalidate(); //перерисовка карты после статистики 
                              //для отображения остановок  и 
                              //превышение скорости на карте

                return true;
            }
            #endregion
            #region EXP
            else if (e.Button.Name == inscr.EXP)
            {
                if (_mapExport1.ShowDialog() != DialogResult.OK)
                    return true;

                DateTime s = DateTime.Now;
                if (dotClick != null)
                {
                    dotClick.add(
                              dotClick.lst[0].X
                            , dotClick.lst[0].Y
                            , dotClick.lst[0].z
                            , r.make()
                        );
                    #region comentary
                    //download tile in 19 zoom to one picture
                    /*var list = Program.tile4trck
                        ? //если задали ключ берем тайлы с интернета
                         _mbTile.GetTileInternet(
                              dotClick.lst.Max(x => x.X)
                            , dotClick.lst.Min(x => x.Y)
                            , dotClick.lst.Min(x => x.X)
                            , dotClick.lst.Max(x => x.Y)
                            , 19)
                        :  //иначе "общаемся" с бд
                           _ln != null
                            ?//если есть трек берем все тайлы для данного трека
                                _mbTile.GetTile(
                                  _ln.lst.Max(x => x.X)
                                , _ln.lst.Min(x => x.Y)
                                , _ln.lst.Min(x => x.X)
                                , _ln.lst.Max(x => x.Y)
                                , 19)
                            :   //иначе берем тайлы для натыканой линии
                                 _mbTile.GetTile(
                                  dotClick.lst.Max(x => x.X)
                                , dotClick.lst.Min(x => x.Y)
                                , dotClick.lst.Min(x => x.X)
                                , dotClick.lst.Max(x => x.Y)
                                , 19);*/
                    #endregion

                    /*MessageBox.Show(
                                 dotClick.lst.Min(x => x.X) + "-minX"
                        + "\n" + dotClick.lst.Max(x => x.Y) + "-maxY"
                        + "\n" + dotClick.lst.Max(x => x.X) + "-maxX"
                        + "\n" + dotClick.lst.Min(x => x.Y) + "-minY"
                        );*/

                    var list = _mbTile.GetTile(
                                  dotClick.lst.Max(lat => lat.Y)
                                , dotClick.lst.Min(lon => lon.X)
                                , dotClick.lst.Min(lat => lat.Y)
                                , dotClick.lst.Max(lon => lon.X)
                                , 19);

                    Bitmap result = new Bitmap(256 * list.Count, 256 * list[0].Count);
                    Graphics gr = Graphics.FromImage(result);

                    ///tmp solution
                    PointF[] drawDot = new PointF[dotClick.lst.Count];
                    for (int i = 0; i < dotClick.lst.Count; i++)
                    {
                        int X, Y;
                        _mbTile.BL2XY(
                            dotClick.lst[i].Y, dotClick.lst[i].X
                            , 19, list
                            , out X, out Y);

                        drawDot[i] = new PointF(X, Y);
                    }
                    ///

                    GraphicsPath gPath = new GraphicsPath();
                    gPath.AddPolygon(drawDot);

                    Region region = new Region(gPath);

                    for (int i = 0; i < list.Count; i++)
                    {
                        for (int j = 0; j < list[i].Count; j++)
                        {
                            int x = i * 256;    //Х коор. тайла на карте
                            int y = j * 256;    //Y коор. тайла на карте
                            int size = 256;     //размер 1 тайла

                            //если тайл не накрывает регион затираем его
                            if (!region.IsVisible(new Rectangle(x, y, size, size)))
                                list[i][j] = null;
                        }
                    }

                    ///рисование карты на картинке
                    for (int i = 0; i < list.Count; i++)
                        for (int j = 0; j < list[i].Count; j++)
                            if(list[i][j] != null)
                                gr.DrawImage(
                                    new Bitmap(new MemoryStream(list[i][j].tile)),
                                    new Point(i * 256, j * 256));

                    ///tmp Solution
                    if (drawDot.Length == 1)
                        gr.DrawEllipse(trckPen,
                            drawDot[0].X, drawDot[0].Y,
                            2, 2); //2px - размер прямоугольника в который вписан елипс
                    else
                        gr.DrawLines(trckPen, drawDot);
                    ///

                    /*if (dotClick.ps.Length == 1)
                        gr.DrawEllipse(trckPen,
                            dotClick.ps[0].X - 2, dotClick.ps[0].Y - 2,
                            4, 4); //размер прямоугольника в который вписан елипс
                    else
                        gr.DrawLines(trckPen, dotClick.ps);*/

                    result.Save(_mapExport1.FileName);

                    DateTime f = DateTime.Now;
                    WriteLine(IMPORTANCELEVEL.Info, "{0} secs of full download"
                        , (f - s).TotalSeconds);

                    dotClick.begin();
                    dotClick.lst.Clear(); //после сохранения картинки очищаем натыканые точки

                    Invalidate();
                }
                return true;
            }
            #endregion
            return false;
        }

        public void timer1_Tick(object sender, EventArgs e) 
        {
            WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: new ", "timer"
                      );
            if (_ln == null)
            {
                Lat += step * math.cos(YawDegree);// xShift; 
                Lon += step * math.sin(YawDegree);// yShift;
            }
            else {
                indexPoint++;
                if (indexPoint > _ln.lst.Count)
                    indexPoint = 0;
            }

            _photoPicBox.Image = (Image)photoLand;
            newMap();

            WriteLine(IMPORTANCELEVEL.Debug
                    , "{0}: new shifts(x/y)/coor: ({1}/{2})/{3}", "forward"
                      , xShift, yShift, mk.txt(Lat, Lon));

            Invalidate();
        }

        ///work with bitmap
        public Bitmap LoadBitmap(string fileName)
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    return new Bitmap(fs);
            }
            catch
            {
                return new Bitmap(
                    _photoPicBox.Width, _photoPicBox.Height);
            }
        }
        public unsafe byte[,,] BitmapToByteRgbQ(Bitmap bmp)
        {
            int width = bmp.Width,
                height = bmp.Height;
            byte[,,] res = new byte[3, height, width];

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            try
            {
                byte* curpos;
                fixed (byte* _res = res)
                {
                    byte* _r = _res, _g = _res + width * height, _b = _res + 2 * width * height;
                    for (int h = 0; h < height; h++)
                    {
                        curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                        for (int w = 0; w < width; w++)
                        {
                            *_b = *(curpos++); ++_b;
                            *_g = *(curpos++); ++_g;
                            *_r = *(curpos++); ++_r;
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return res;
        }
        public unsafe Bitmap RgbToBitmapQ(byte[,,] rgb)
        {
            if ((rgb.GetLength(0) != 3))
            {
                throw new ArrayTypeMismatchException("Size of first dimension for passed array must be 3 (RGB components)");
            }

            int width = rgb.GetLength(2),
                height = rgb.GetLength(1);

            Bitmap result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            BitmapData bd = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                byte* curpos;
                fixed (byte* _rgb = rgb)
                {
                    byte* _r = _rgb, _g = _rgb + width * height, _b = _rgb + 2 * width * height;
                    for (int h = 0; h < height; h++)
                    {
                        curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                        for (int w = 0; w < width; w++)
                        {
                            *(curpos++) = *_b; ++_b;
                            *(curpos++) = *_g; ++_g;
                            *(curpos++) = *_r; ++_r;
                        }
                    }
                }
            }
            finally
            {
                result.UnlockBits(bd);
            }

            return result;
        }
    }
}
