//
#define AGPHOME

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using ut;
using wnd;
//using MapWnd;

namespace MapWnd
{
    class FormToShow : Form
    {
        const int add  = 70;
        TableLayoutPanel table_control;

        Panel map;
        Panel miniMap;
        ToolBar tb;

        public StatusStrip statStrip;
        public ToolStripStatusLabel statLabel;

        public PictureBox photoPicBox;

        public _Button pitchDownButton;
        public _Button pitchUpButton;

        public _Button zoomInButton;
        public _Button zoomOutButton;

        public _Button wayUpButton;
        public _Button wayRightButton;
        public _Button wayDownButton;
        public _Button wayLeftButton;

        public _Button rollRightButton;
        public _Button rollLeftButton;

        public _Button yawLeftButton;
        public _Button yawRightButton;

        public _Button resetButton;
        public _Button playPauseButton;

        public MapToShow mTS;

        bool   vF  ;

        public _ToolBarButton tlbParam; 
        public _ToolBarButton tlbParam2; 
        public _ToolBarButton tlbSave;
        public _ToolBarButton tlbStat;
        public _ToolBarButton tlbExport;

        static FormToShow(){
              Names.Add(inscr.STAT, "Статистика",   "Статистика"); 
              Names.Add(inscr.PITCH,"Тангаж",   "Тангаж"); 
              Names.Add(inscr.YAW,  "Рыскание", "Рискання"); 
              Names.Add(inscr.ZOOM, "Масштаб",  "Масштаб"); 
              Names.Add(inscr.WAY,  "Движение", "Рух"); 
              Names.Add(inscr.ROLL, "Крен",     "Крен"); 
              Names.Add(inscr.DOWN, "Вниз", "    Вниз"); 
              Names.Add(inscr.UP,   "Вверх",    "Вгору"); 
              Names.Add(inscr.IN,   "Увеличить","Збільшити");
              Names.Add(inscr.OUT,  "Уменьшить","Зменшити");
              Names.Add(inscr.LEFT, "Влево",    "Ліворуч"); 
              Names.Add(inscr.RIGHT,"Вправо",   "Праворуч"); 
              Names.Add(inscr.FRWD, "Вперёд",   "Вперед"); 
              Names.Add(inscr.WND,  "Окно для", "Вікно для");
              Names.Add(inscr.LTT,  "Широта",   "Широта");
              Names.Add(inscr.LNG,  "Долгота",  "Довгота");
              Names.Add(inscr.BACK, "Назад",    "Назад");
              Names.Add(inscr.ALT,  "Высота",   "Висота");
              Names.Add(inscr.RES,  "Сбросить",  "Скинути");
              Names.Add(inscr.PLAY, "Продолжить", "Продовжити");
              Names.Add(inscr.PAUSE,"Остановить", "Зупинити");
              Names.Add(inscr.SPEED,"Скорость(и)",  "Швидкість(ості)");
              Names.Add(inscr.STEP, "Шаг", "Крок");
        }

        public FormToShow(MapToShow _mTS, bool verbose, int lang = 0)
        {
            mTS = _mTS;
            Names.lang = lang;

            tb = new ToolBar();
            tb.Padding = new Padding(1) ;
          
            tb.ButtonSize = new System.Drawing.Size(
                          (int)(SZ.X_BUTTON * 0.8),
                          (int)(SZ.Y * 1.0)
                      );

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Controls.Add(tb);

            _ToolBarButton tlbExit   = new _ToolBarButton (inscr.EXIT);
                           tlbExport = new _ToolBarButton (inscr.EXP);
                           tlbSave   = new _ToolBarButton (inscr.SAVE);
                           tlbStat   = new _ToolBarButton (inscr.STAT);
            _ToolBarButton tlbFlt    = new _ToolBarButton (inscr.FLT);
                           tlbParam  = new _ToolBarButton (inscr.PAR1);
                           tlbParam2 = new _ToolBarButton (inscr.PAR2);
            ToolBarButton tlbAbout  = new _ToolBarButton (inscr.ABOUT);

            tb.ButtonClick += new ToolBarButtonClickEventHandler(bttnClick);
            tlbExit.Enabled  = true;  
            tlbAbout.Enabled = true;
            
            tb.Buttons.AddRange( new ToolBarButton[] { tlbExit  
                                                      , tlbSave
                                                      , tlbExport
                                                      , tlbStat  
                                                      , tlbFlt
                                                      , tlbParam 
                                                      , tlbParam2
                                                      , tlbAbout 
            });
            int hTSz = tb.Size.Height;
            //this.AutoSize = true;
            this.Size =
                    lang == 1
                ? new Size(1075 + add, 780 + add + hTSz)
                : new Size(1045 + add, 780 + add + hTSz);

#if AGPHOME

            this.Size  = new Size(Size.Width+64, Size.Height );   //agp1
#else
            this.Size  = new Size(Size.Width, Size.Height );   //agp1
#endif



            ///
            ///Panel            
            ///
            map = new Panel();
            map.AutoScroll = true;
            map.Size = new Size(700  + add, 700+ add);
            map.Location = new Point(10, 12 + hTSz);
            map.BackColor = Color.LightGray;
            map.BorderStyle = BorderStyle.FixedSingle;

            miniMap = new Panel();
            miniMap.Size = new Size(256, 256);
            miniMap.BackColor = Color.LightGray;
            miniMap.Location = 
                    lang == 1
                ? new Point(725 + add + 30, 554)
                : new Point(725 + add + 15, 554);
            miniMap.BorderStyle = BorderStyle.FixedSingle;

            photoPicBox = new PictureBox();
            photoPicBox.Size = new Size(256, 256);
            photoPicBox.BackColor = Color.LightGray;
            photoPicBox.Location = 
                    lang == 1 
                ? new Point(725 + add + 30, 286)
                : new Point(725 + add + 15, 286);
            photoPicBox.BorderStyle = BorderStyle.FixedSingle;
            photoPicBox.Image = new Bitmap(photoPicBox.Width, photoPicBox.Height);

            Initialize_Table_Control();

            ///
            ///StatusStrip
            ///
            statStrip = new StatusStrip();
            statLabel = new ToolStripStatusLabel();
            statLabel.AutoSize = true;
            statStrip.Items.Add(statLabel);

            mTS._stS = statStrip;

            Controls.Add(statStrip);

            this.Controls.Add(table_control);
            this.Controls.AddRange(new Control[] { map, miniMap, photoPicBox });

            Add_newMap(mTS._mapPicBox);
            Add_newMiniMap(mTS._miniPicBox);
            Add_newPhoto(mTS._photoPicBox);

            vF = verbose;
        }

        public void bttnClick(object sender, ToolBarButtonClickEventArgs e)
        {
           if (mTS.tBClick( sender,  e)){    /// тут вызывается метод из переданного объекта
                                             /// mapToShow  карта для рисования, если вызвался
              ;                              /// то делать ничего не надо
           }
           else if (e.Button.Name == inscr.EXIT) {
              Close();
           }
           else if (e.Button.Name == inscr.ABOUT) {
               string tit = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(),
                                 typeof(AssemblyTitleAttribute), false)
                ).Title;
               string descr = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(),
                                 typeof(AssemblyDescriptionAttribute), false)
                ).Description;

               String FormTitle = String.Format("{0}", 
                                        descr 
                 + String.Format(" for testing \n{0}", mk.Info("MBTile.dll"))
                 + String.Format(" \n also there are\n{0}", mk.Info("Logger.cs.dll"))
                 + String.Format("  \nand {0}", mk.Info("elGeo.dll"))
                 + String.Format("  \nand {0}", mk.Info("System.Data.SQLite.dll"))
                                  );

              MessageBox.Show(FormTitle, String.Format("{0}(ver:{1})",tit, Application.ProductVersion));
           }
           else
              warning.NRD(sender,  e);
        }

        void Initialize_Table_Control()
        {
            _Label pitchLabel = new _Label(inscr.PITCH);
            _Label yawLabel = new _Label(inscr.YAW);
            _Label zoomLabel = new _Label(inscr.ZOOM);
            _Label rollLabel = new _Label(inscr.ROLL);
            _Label wayLabel = new _Label(inscr.WAY);

            _Label speedLabel = new _Label(inscr.SPEED);
            _Label stepLabel = new _Label(inscr.STEP);

            mTS._pitchLabel = pitchLabel;
            mTS._zoomLabel = zoomLabel;
            mTS._rollLabel = rollLabel;
            mTS._yawLabel = yawLabel;
            ///

            ///
            ///Buttons
            ///
            resetButton = new _Button(inscr.RES);
            playPauseButton = new _Button(inscr.PLAY);

            pitchDownButton = new _Button(inscr.DOWN);
            pitchUpButton = new _Button(inscr.UP);

            yawLeftButton = new _Button(inscr.LEFT);
            yawRightButton = new _Button(inscr.RIGHT);

            zoomInButton = new _Button(inscr.IN);
            zoomOutButton = new _Button(inscr.OUT);

            wayUpButton = new _Button(inscr.FRWD);
            wayRightButton = new _Button(inscr.RIGHT);
            wayDownButton = new _Button(inscr.BACK);
            wayLeftButton = new _Button(inscr.LEFT);

            rollRightButton = new _Button(inscr.RIGHT);
            rollLeftButton = new _Button(inscr.LEFT);
            ///

            table_control = new TableLayoutPanel();
            //table_control.AutoSize = true;
            table_control.Size = new Size(350, 230);
            int hTSz = tb.Size.Height;

            table_control.Location = new Point(720 + add,   hTSz + 15);

            table_control.Controls.Add(resetButton,     1, 0);
            table_control.Controls.Add(playPauseButton, 1, 1);

            table_control.Controls.Add(pitchUpButton,   0, 0);
            table_control.Controls.Add(pitchDownButton, 0, 2);
            table_control.Controls.Add(pitchLabel,      0, 1);

            table_control.Controls.Add(zoomInButton,    2, 0);
            table_control.Controls.Add(zoomOutButton,   2, 2);
            table_control.Controls.Add(zoomLabel,       2, 1);

            table_control.Controls.Add(speedLabel, 0, 4);
            table_control.Controls.Add(stepLabel,  2, 4);

            table_control.Controls.Add(wayUpButton,    1, 4);
            table_control.Controls.Add(wayLeftButton,  0, 5);
            table_control.Controls.Add(wayRightButton, 2, 5);
            table_control.Controls.Add(wayDownButton,  1, 6);
            table_control.Controls.Add(wayLabel,       1, 5);
            
            table_control.Controls.Add(rollLeftButton, 0, 7);
            table_control.Controls.Add(rollLabel,      1, 7);
            table_control.Controls.Add(rollRightButton,2, 7);
            
            table_control.Controls.Add(yawLeftButton,  0, 8);
            table_control.Controls.Add(yawLabel,       1, 8);
            table_control.Controls.Add(yawRightButton, 2, 8);
        }
        
        public void Add_newMap(Control newMap)
        {
            newMap.Location = new Point(0, 0);
            map.Controls.Add(newMap);
        }
        public void Add_newMiniMap(Control newMiniMap)
        {
            newMiniMap.Location = new Point(0, 0);
            miniMap.Controls.Add(newMiniMap);
        }
        public void Add_newPhoto(Control newPhoto)
        {
            newPhoto.Location = new Point(0, 0);
            photoPicBox.Controls.Add(newPhoto);
        }
    }
}
