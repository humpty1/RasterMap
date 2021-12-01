using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

using Logger;
using wnd;
using geo;

namespace MapWnd
{
    public class wStatistic : Form
    {
        Button btnOK;
        TabControl tbControl;
        
        TabPage tbPage1;
        TabPage tbPage2;
        TabPage tbPage3;
        TabPage tbPage4;

        Chart chartAltitude;
        Chart chartSpeed;
        //Chart chartStops;

        Label gnrlTimeLb;
        Label gnrlLenghtLb;
        Label gnrlSpeedLb;
        Label gnrlTimeStopsLb;
        Label gnrlCountStopsLb;
        Label gnrlExcessSpeedLb;

        TextBox gnrlTimeTb;
        TextBox gnrlSpeedTb;
        TextBox gnrlLenghtTb;
        TextBox gnrlTimeStopsTb;
        TextBox gnrlCountStopsTb;
        TextBox gnrlExcessSpeedTb;

        DataGridView dataGridStops;
        DataGridView dataGridSpeed;

        Line _ln;

        Ellipsoid g = new Ellipsoid(ellipsoid.wgs84);

        double Distance = 0;
        double gnrlTime = 0;
        double MediumSpeed = 0;
        double gnrlTimeStops = 0;
        int CountStops = 0;
        int ExcessSpeed = 0;

        Line exSpeed;
        Line stops;
        Loger _l;
        rec r;

        public wStatistic(Line ln, Line exSpeed, Line stops, rec r, Loger l = null)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Size = new System.Drawing.Size(700, 430);
            this.Text = Names.Text(wnd.inscr.STAT);

            this.r = r;
            _l = null;
            _l = l;
            tuple a = r.make();
            
            this.exSpeed = exSpeed;
            this.stops = stops;
            this._ln = ln;

            Names.Add(inscr.STOPS, "Остановки(ок)", "Зупинки(ок)");
            Names.Add(inscr.EXCESS, "Превышение", "Перевищення");
            Names.Add(inscr.GNRL, "Основное(ая)", "Загальне(а)");
            Names.Add(inscr.AVERAGE, "Средняя", "Середня");
            Names.Add(inscr.LNGHT, "Длина", "Довжина");
            Names.Add(inscr.COUNT, "К-тво", "К-ть");
            Names.Add(inscr.TIME, "Время", "Час");
            Names.Add(inscr.DATE, "Дата", "Дата");

            ///tabControl
            this.tbControl = new TabControl();
            this.tbControl.Size = new Size(685, 350);

            this.tbPage1 = new TabPage(Names.Text(wnd.inscr.GNRL));     //Общая
            this.tbPage2 = new TabPage(Names.Text(wnd.inscr.ALT));      //Профиль высоты
            this.tbPage3 = new TabPage(Names.Text(wnd.inscr.SPEED));    //Профиль скорости
            this.tbPage4 = new TabPage(Names.Text(wnd.inscr.STOPS) 
                + " / " + Names.Text(wnd.inscr.EXCESS)+ " " +Names.Text(wnd.inscr.SPEED));    //Координаты стоянок и превышения скорости

            this.tbControl.Controls.Add(this.tbPage1);
            this.tbControl.Controls.Add(this.tbPage2);
            this.tbControl.Controls.Add(this.tbPage3);
            this.tbControl.Controls.Add(this.tbPage4);

            //Button
            btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.AutoSize = true;
            btnOK.Click += btnOK_Click;
            btnOK.Location = new Point(
                this.Width - btnOK.Width - 25, 
                tbControl.Height + 5);

            ///Label
            gnrlTimeLb        = new _Label("");     //общее время
            gnrlLenghtLb      = new _Label("");     //длинна пути
            gnrlSpeedLb       = new _Label("");     //средняя скорость
            gnrlTimeStopsLb   = new _Label("");     //время остановок
            gnrlCountStopsLb  = new _Label("");     //кол-во остановок
            gnrlExcessSpeedLb = new _Label("");     //превышение скорости

            gnrlTimeLb.Text = Names.Text(inscr.GNRL) + " " + Names.Text(inscr.TIME);
            gnrlSpeedLb.Text = Names.Text(inscr.AVERAGE) + " " + Names.Text(inscr.SPEED);
            gnrlLenghtLb.Text = Names.Text(inscr.GNRL) + " " + Names.Text(inscr.LNGHT);
            gnrlTimeStopsLb.Text = Names.Text(inscr.TIME) + " " + Names.Text(inscr.STOPS);
            gnrlCountStopsLb.Text = Names.Text(inscr.COUNT) + " " + Names.Text(inscr.STOPS);
            gnrlExcessSpeedLb.Text = Names.Text(inscr.EXCESS) + " " + Names.Text(inscr.SPEED);

            ///Text Boxes
            gnrlTimeTb = new TextBox();
            gnrlLenghtTb = new TextBox();
            gnrlSpeedTb = new TextBox();
            gnrlTimeStopsTb = new TextBox();
            gnrlCountStopsTb = new TextBox();
            gnrlExcessSpeedTb = new TextBox();

            ///Table Layout Panel
            TableLayoutPanel tlP = new TableLayoutPanel();
            tlP.RowCount = 6;
            tlP.ColumnCount = 2;
            tlP.Dock = DockStyle.Fill;

            tlP.Controls.Add(gnrlTimeLb,          0, 0);
            tlP.Controls.Add(gnrlLenghtLb,        0, 1);
            tlP.Controls.Add(gnrlSpeedLb,         0, 2);
            tlP.Controls.Add(gnrlTimeStopsLb,     0, 3);
            tlP.Controls.Add(gnrlCountStopsLb,    0, 4);
            tlP.Controls.Add(gnrlExcessSpeedLb,   0, 5);

            tlP.Controls.Add(gnrlTimeTb,          1, 0);
            tlP.Controls.Add(gnrlLenghtTb,        1, 1);
            tlP.Controls.Add(gnrlSpeedTb,         1, 2);
            tlP.Controls.Add(gnrlTimeStopsTb,     1, 3);
            tlP.Controls.Add(gnrlCountStopsTb,    1, 4);
            tlP.Controls.Add(gnrlExcessSpeedTb,   1, 5);

            ///Group Box
            GroupBox gpStop = new GroupBox();
            gpStop.Text = wnd.inscr.STOPS;
            gpStop.Dock = DockStyle.Left;
            gpStop.Size = new Size(335, 200);

            GroupBox gpSpeed = new GroupBox();
            gpSpeed.Text = wnd.inscr.EXCESS + " " + wnd.inscr.SPEED;
            gpSpeed.Dock = DockStyle.Right;
            gpSpeed.Size = new Size(335, 200);

            ///DataGridView
            ///Stops
            dataGridStops = new DataGridView();
            dataGridStops.Dock = DockStyle.Fill;
            //dataGridStops.Size = new System.Drawing.Size(300, 200);
            dataGridStops.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridStops.RowHeadersVisible = false;

            if(a.ContainsKey("DateTime"))
            {
                dataGridStops.ColumnCount = 3;
                dataGridStops.Columns[2].HeaderCell.Value = Names.Text(inscr.DATE);
            }
            else
                dataGridStops.ColumnCount = 2;

            dataGridStops.Columns[0].Width = 70;
            dataGridStops.Columns[0].HeaderCell.Value = Names.Text(inscr.LNG);
            dataGridStops.Columns[1].Width = 70;
            dataGridStops.Columns[1].HeaderCell.Value = Names.Text(inscr.LTT);

            ///ExcessSpeed
            dataGridSpeed = new DataGridView();
            dataGridSpeed.Dock = DockStyle.Fill;
            //dataGridSpeed.Size = new System.Drawing.Size(300, 200);
            dataGridSpeed.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridSpeed.RowHeadersVisible = false;

            if (a.ContainsKey("DateTime"))
            {
                dataGridSpeed.ColumnCount = 3;
                dataGridSpeed.Columns[2].HeaderCell.Value = Names.Text(inscr.DATE);
            }
            else
                dataGridSpeed.ColumnCount = 2;

            dataGridSpeed.Columns[0].Width = 70;
            dataGridSpeed.Columns[0].HeaderCell.Value = Names.Text(inscr.LNG);
            dataGridSpeed.Columns[1].Width = 70;
            dataGridSpeed.Columns[1].HeaderCell.Value = Names.Text(inscr.LTT);

            ///Chart
            //Проверить в _ln наличие высоты
            chartAltitude = new Chart();
            chartAltitude.Dock = DockStyle.Fill;
            chartAltitude.ChartAreas.Add("Area1");
            chartAltitude.ChartAreas["Area1"].AxisY.TitleAlignment = StringAlignment.Far;
            chartAltitude.ChartAreas["Area1"].AxisY.ArrowStyle = AxisArrowStyle.Triangle;
            chartAltitude.ChartAreas["Area1"].AxisY.Title = "m";
            chartAltitude.ChartAreas["Area1"].AxisX.TitleAlignment = StringAlignment.Far;
            chartAltitude.ChartAreas["Area1"].AxisX.ArrowStyle = AxisArrowStyle.Triangle;
            chartAltitude.ChartAreas["Area1"].AxisX.Title = "index point";
            chartAltitude.Series.Add("Height");
            chartAltitude.Series["Height"].ChartType = SeriesChartType.FastLine;
            chartAltitude.Series["Height"].BorderWidth = 2;

            //Проверить в _ln наличие скорости
            chartSpeed = new Chart();
            chartSpeed.Dock = DockStyle.Fill;
            chartSpeed.ChartAreas.Add("Area1");
            chartSpeed.ChartAreas["Area1"].AxisY.Minimum = 0;
            chartSpeed.ChartAreas["Area1"].AxisY.TitleAlignment = StringAlignment.Far;
            chartSpeed.ChartAreas["Area1"].AxisY.ArrowStyle = AxisArrowStyle.Triangle;
            chartSpeed.ChartAreas["Area1"].AxisY.Title = "km/h";
            chartSpeed.ChartAreas["Area1"].AxisX.TitleAlignment = StringAlignment.Far;
            chartSpeed.ChartAreas["Area1"].AxisX.ArrowStyle = AxisArrowStyle.Triangle;
            chartSpeed.ChartAreas["Area1"].AxisX.Title = "index point";
            chartSpeed.Series.Add("Speed");
            chartSpeed.Series["Speed"].ChartType = SeriesChartType.FastLine;
            chartSpeed.Series["Speed"].BorderWidth = 2;

            /*chartSpeed.Series.Add("SpeedMin");
            chartSpeed.Series["SpeedMin"].ChartType = SeriesChartType.FastLine;
            chartSpeed.Series["SpeedMin"].Color = Color.Red;
            chartSpeed.Series["SpeedMin"].BorderWidth = 2;
            chartSpeed.Series.Add("SpeedMax");
            chartSpeed.Series["SpeedMax"].ChartType = SeriesChartType.FastLine;
            chartSpeed.Series["SpeedMax"].Color = Color.Red;
            chartSpeed.Series["SpeedMax"].BorderWidth = 2;*/

            /*chartStops = new Chart();
            chartStops.Dock = DockStyle.Right;
            chartStops.Size = new System.Drawing.Size(360, chartSpeed.Height);
            chartStops.ChartAreas.Add("Area1");
            chartStops.ChartAreas["Area1"].AxisY.Minimum = _ln.lst.Min(x => x.Y);
            chartStops.ChartAreas["Area1"].AxisY.Maximum = _ln.lst.Max(x => x.Y);
            chartStops.ChartAreas["Area1"].AxisX.Minimum = _ln.lst.Min(x => x.X);
            chartStops.ChartAreas["Area1"].AxisX.Maximum = _ln.lst.Max(x => x.X);

            chartStops.Series.Add("Stops");
            chartStops.Series["Stops"].ChartType = SeriesChartType.FastPoint;
            chartStops.Series["Stops"].BorderWidth = 2;*/

            ///Controls
            this.tbPage1.Controls.Add(tlP);
            this.tbPage2.Controls.Add(chartAltitude);
            this.tbPage3.Controls.Add(chartSpeed);
            //this.tbPage4.Controls.Add(chartStops);

            gpStop.Controls.Add(dataGridStops);
            gpSpeed.Controls.Add(dataGridSpeed);

            this.tbPage4.Controls.Add(gpStop);
            this.tbPage4.Controls.Add(gpSpeed);

            this.Controls.Add(btnOK);
            this.Controls.Add(tbControl);

            addAltitude();
            addSpeed(30);

            addGeneralInfo();
            addStopsExcess();
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        ///key for datetime in dictionary
        ///DateTime (DD.MM.YY HH:MM:SS)-> -dixi
        ///DateTime (DD.MM.YY HH:MM:SS)-> -g2c

        void addGeneralInfo()
        {
            gnrlTimeTb.Text = Math.Round(gnrlTime, 2) + " h";
            gnrlLenghtTb.Text = Math.Round(Distance, 3) + " km";
            gnrlSpeedTb.Text = Math.Round(MediumSpeed, 2) + " km/h";
            gnrlTimeStopsTb.Text = Math.Round(gnrlTimeStops, 2) + " h";
            gnrlExcessSpeedTb.Text = ExcessSpeed.ToString();
            gnrlCountStopsTb.Text = CountStops.ToString();
        }
        void addAltitude()
        {
            for (int i = 0; i < _ln.lst.Count; i++)
                chartAltitude.Series["Height"].Points.Add(_ln.lst[i].z);
        }
        
        void addSpeed(double Time)
        {
            //полосы на 80 и 110 км/час
            /*chartSpeed.Series["SpeedMin"].Points.AddXY(0, 80);
            chartSpeed.Series["SpeedMin"].Points.AddXY(_ln.lst.Count, 80);
            chartSpeed.Series["SpeedMax"].Points.AddXY(0, 110);
            chartSpeed.Series["SpeedMax"].Points.AddXY(_ln.lst.Count, 110);*/

            chartSpeed.ChartAreas[0].AxisX.Minimum = 0;
            chartSpeed.ChartAreas[0].AxisX.Maximum = _ln.lst.Count;

            exSpeed.begin();
            exSpeed.lst.Clear();
            stops.begin();
            stops.lst.Clear();
            tuple tail = null;
            for (int i = 0; i < _ln.lst.Count - 1; i++)
            {
                double distance = 
                    g.distance(
                     _ln.lst[i].X, _ln.lst[i].Y,
                     _ln.lst[i + 1].X, _ln.lst[i + 1].Y);
                
                if(_ln.lst[i].ContainsKey("DateTime"))
                {
                    string dateTime;
                    dateTime = _ln.lst[i]["DateTime"].ToString();

                    string[] date = (dateTime.Split(' '))[0].Split('.');
                    string[] time = (dateTime.Split(' '))[1].Split(':');

                    DateTime t1 = new DateTime(
                        Convert.ToInt32(date[2]),
                        Convert.ToInt32(date[1]),
                        Convert.ToInt32(date[0]),
                        Convert.ToInt32(time[0]),
                        Convert.ToInt32(time[1]),
                        Convert.ToInt32(time[2]));

                    dateTime = _ln.lst[i + 1]["DateTime"].ToString();

                    date = (dateTime.Split(' '))[0].Split('.');
                    time = (dateTime.Split(' '))[1].Split(':');

                    DateTime t2 = new DateTime(
                        Convert.ToInt32(date[2]),
                        Convert.ToInt32(date[1]),
                        Convert.ToInt32(date[0]),
                        Convert.ToInt32(time[0]),
                        Convert.ToInt32(time[1]),
                        Convert.ToInt32(time[2]));

                    Time = (t2.Hour + (double)t2.Minute / 60 + (double)t2.Second / 60 / 60)
                         - (t1.Hour + (double)t1.Minute / 60 + (double)t1.Second / 60 / 60);

                    gnrlTime += Time;
                }

                double speed = (distance / 1000) / Time;

                chartSpeed.Series["Speed"].Points.Add(speed);

                Distance += distance / 1000;
                MediumSpeed += speed;

                /*tuple a = r.make();
                foreach (KeyValuePair<string, object> kvp in a)
                    MessageBox.Show(String.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value.ToString()));*/

                if (speed > 110)
                {
                    tail = new tuple();
                    if (tail.ContainsKey("DateTime")) 
                      tail.Add("DateTime", _ln.lst[i]["DateTime"]);
                    exSpeed.add(
                          _ln.lst[i].X
                         ,_ln.lst[i].Y
                         , 0
                         ,tail
                        );

                    ExcessSpeed++;
                }
                if ((int)speed == 0)
                {
                    tail = new tuple();
                    if (tail.ContainsKey("DateTime")) 
                       tail.Add("DateTime", _ln.lst[i]["DateTime"]);
                    CountStops++;

                    stops.add(
                        _ln.lst[i].X
                        , _ln.lst[i].Y
                        , 0
                        , tail
                        );

                    gnrlTimeStops += Time;
                }
            }

            MediumSpeed /= _ln.lst.Count - 1;
        }
        void addStopsExcess()
        {
            for (int i = 0; i < stops.lst.Count; i++)
            {
                dataGridStops.Rows.Add();

                dataGridStops.Rows[i].Cells[0].Value = stops.lst[i].Y;
                dataGridStops.Rows[i].Cells[1].Value = stops.lst[i].X;

                if (stops.lst[i].ContainsKey("DateTime"))  {
                    if (_l!=null)
                        _l.WriteLine(IMPORTANCELEVEL.Spam, "{0}: i/DateTime: {1}/'{2}'"
                               , "addStopsExcess", i, stops.lst[i]["DateTime"]);

                    dataGridStops.Rows[i].Cells[2].Value = stops.lst[i]["DateTime"];
                }
            }

            for (int i = 0; i < exSpeed.lst.Count; i++)
            {
                dataGridSpeed.Rows.Add();

                dataGridSpeed.Rows[i].Cells[0].Value = exSpeed.lst[i].Y;
                dataGridSpeed.Rows[i].Cells[1].Value = exSpeed.lst[i].X;

                if (exSpeed.lst[i].ContainsKey("DateTime"))
                    dataGridSpeed.Rows[i].Cells[2].Value = exSpeed.lst[i]["DateTime"];
            }
        }
        
        public void drawStops(Graphics g, MBTile.MBTiles _mb)
        {
            if (stops.lst == null)
                return;

            stops.commit(_mb.BL2XY);

            for (int i = 0; i < stops.ps.Length; i++)
            {
                int x = stops.ps[i].X;
                int y = stops.ps[i].Y;

                g.FillEllipse(
                    new SolidBrush(Color.Green), //кисть для рисования
                    x - 2, //сдвиг на половину длины описывающего 
                    y - 2, //прямоуголника, для попадания круга на трек
                    4, 4); //размер описывающего прямоугольника
            }
        }
        public void drawExSpeed(Graphics g, MBTile.MBTiles _mb)
        {
            if (exSpeed.lst == null)
                return;

            exSpeed.commit(_mb.BL2XY);

            for (int i = 0; i < exSpeed.ps.Length; i++)
            {
                int x = exSpeed.ps[i].X;
                int y = exSpeed.ps[i].Y;

                g.FillEllipse(
                    new SolidBrush(Color.Red), //кисть для рисования
                    x - 2, //сдвиг на половину длины описывающего 
                    y - 2, //прямоуголника, для попадания круга на трек
                    4, 4); //размер описывающего прямоугольника
            }
        }
    }
}
