using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms; 
using System.Collections; 
using System.Collections.Generic;
using System.Text.RegularExpressions; 
using System.Collections.Specialized;
using ut;
using Args;
using MapWnd;

namespace wnd
{
    class Names{
        static public int lang;
        static public StringDictionary en;  // 0
        static public StringDictionary ru;  // 1
        static public StringDictionary ua;  // 2

        static public  void Add(string key){
               Add(key, key, key);
        }
        static public void Add(string key, string ruT ){
            try{
               ua.Add(key, key);
               ru.Add(key, ruT);
               en.Add(key, key);
            }
            catch {
            }
        }
        static public void Add(string key, string ruT, string uaT ){
            try{
               ua.Add(key, uaT );
               ru.Add(key, ruT);
               en.Add(key, key);
            }
            catch {
            }
        }
        static Names()
        {
            lang = 0;
            ua                =                new StringDictionary();
            ru                =                new StringDictionary();
            en                =                new StringDictionary();

            Add(inscr.OK);
            Add(inscr.EXIT, "Выход",    "Вихід"); 
            Add(inscr.READ, "Прочитать"); 
            Add(inscr.SAVE, "Сохранить","Зберегти"); 
            Add(inscr.EXP,  "Экспорт",  "Експорт"); 
            Add(inscr.FLT,  "Поиск",    "Пошук"); 
            Add(inscr.PAR1, "Аргументы" ); 
            Add(inscr.PAR2, "Конфигурация", "Конфігурація"); 
            Add(inscr.ABOUT,"О себе",       "Про себе"); 
            Add(inscr.NTD, "Нечего делать!", "Нема чого робити!");
            Add(inscr.WRN, "Предупреждение", "Попередження");


        }
        static public string  Text (string key) {
           string rc =  CNST.NULL;
           if (lang == 2 )
             rc =  ua[key];
           else if (lang == 1 )
             rc =  ru[key];
           else 
             rc =  en[key];
           if (rc == null)
             return key;
           return rc;
        }
    }


    public class _Button : System.Windows.Forms.Button
    {
        public _Button()
            : base() {
        }
        public _Button(string nm)
        {
            Name = nm;
            Text = Names.Text(nm);
            Enabled = false;
        }

        public void handler(EventHandler click)
        {
            Enabled = true;
            Click += click;
        }
    }

    public class _Label : System.Windows.Forms.Label
    {
        public _Label
          (
            string nm,
            int p = 0
          )
        {
            Name = nm;
            Text = Names.Text(nm);
            AutoSize = true;
            Padding = new Padding(5, 7, 5, 0);
            
            float sizeFont = Font.Size;
            Font = new Font("Courier New", sizeFont);
        }
    }
    public class _LabelInfo : System.Windows.Forms.Label
    {
        public _LabelInfo()
        {
            AutoSize = true;
            Padding = new Padding(15, 7, 0, 0);
        }
    }

    public class _ToolBarButton : System.Windows.Forms.ToolBarButton
    {
        public _ToolBarButton
          (
            string nm
          )
        {
            Name = nm;
            Text = Names.Text(nm);
            Enabled = false;
        }
    }

    class WCNST
    {
       public static string[] colors;
       static  WCNST(){
           colors =    new string[] {" DarkBlue","Red", "Green", "Blue", "Yellow"};
       }

    }

    class inscr
    {
        public const string    OK = "OK";
                               
        public const string    ALT = "Altitude";
        public const string    ADD = "Add";
        public const string    ABOUT = "About";
        public const string    BACK = "Back";
        public const string    DEL = "Delete";
        public const string    DOWN = "Down";
        public const string    ESC = "Cancel";
        public const string    EXIT = "Exit";
        public const string    READ = "Read";
        public const string    EXP = "Export";
        public const string    EDIT = "Edit";
        public const string    FLT = "Search";
        public const string    FRWD = "Forward";
        public const string    IN = "In";
        public const string    LEFT = "Left";
        public const string    LTT = "Latitude";
        public const string    LNG = "Longitude";
        public const string    NTD = "Nothing to do!";
        public const string    ORD = "Order";
        public const string    PAR1 = "Arguments";
        public const string    PAR2 = "Parameters";
        public const string    PITCH = "Pitch";
        public const string    REF = "Refresh";
        public const string    RIGHT = "Right";
        public const string    ROLL = "Roll";
        public const string    SAVE = "Save";
        public const string    STAT = "Statistics";
        public const string    OUT = "Out";
        public const string    UP = "Up";
        public const string    RES = "Reset";
        public const string    PLAY = "Play";
        public const string    PAUSE = "Pause";
        public const string    SPEED = "Speed";
        public const string    STEP = "Step";
        public const string    WAY = "Movement";
        public const string    WRN = "Warning";
        public const string    WND = "Window for";
        public const string    YAW = "Yaw";
        public const string    ZOOM = "Zoom";
        public const string    TRACK = "track";
        public const string    START = "start";
        public const string    CURRENT = "current";
        public const string    TMSTP = "timeStep";
        public const string    GNRL = "General";
        public const string    STOPS = "Stops";
        public const string    TIME = "Time";
        public const string    DATE = "Date";
        public const string    LNGHT = "Lenght";
        public const string    AVERAGE = "Average";
        public const string    COUNT = "Count";
        public const string    EXCESS = "Excess";
        public const string    CACHE = "cache";
        public const string    LANG = "language";
        public const string    FILE = "file";
        public const string    HLP = "help";
        public const string    DPOINT = "decimalPoint";
        public const string    VERB = "verbose";
        public const string    DBCON = "dbCon";
        public const string    LOGLVL = "logName";

        static Dictionary<int, int> zmTbl;
        static inscr(){
           zmTbl.Add(0 ,1000000000   );
           zmTbl.Add(1 , 500000000   );
           zmTbl.Add(2 , 200000000   );
           zmTbl.Add(3 , 100000000   );
           zmTbl.Add(4 ,  50000000   );
           zmTbl.Add(5 ,  25000000   );
           zmTbl.Add(6 ,  10000000   );
           zmTbl.Add(7 ,   5000000   );
           zmTbl.Add(8 ,   2000000   );
           zmTbl.Add(9 ,   1000000   );
           zmTbl.Add(10 ,   500000  );
           zmTbl.Add(11 ,   200000  );
           zmTbl.Add(12 ,   100000  );
           zmTbl.Add(13 ,   100000  );
           zmTbl.Add(14 ,    50000 );
           zmTbl.Add(15 ,    25000 );
           zmTbl.Add(16 ,    10000 );
           zmTbl.Add(17 ,     5000);
           zmTbl.Add(18 ,     2000);
           zmTbl.Add(19,      1000);
           zmTbl.Add(20,       500);
        }
        static string txt (int zm) {
           if (zm < 0)  zm = 0;
           if (zm > 20) zm = 20;
           return string.Format("~1:{0}", zmTbl[zm]);
        }
    }

    public class _TextBox : TextBox {
        public Arg retArg;           ///< перемення для возврата введенного значения
        public string defVal;         ///< умолчательное значение

       // public InputTypes inputType;        // NEW Тип поля ввода
        public handler txtChanged;

        public _TextBox(Arg a)
            : base() {
            retArg = a;
            defVal = string.Copy(a.val()); /// взяли значение по умолчанию.
        }
        /// возвращает порядоковый номер поля ввода 
        public int ordNo() {
            int oNo = 0;
            if (Name != null && Name.Length > 1) {
                string no = Name.Substring(1);
                int.TryParse(no, out oNo);
            }
            return oNo;
        }

        ///  восстановить значения по умолчанию
        public virtual void restore() {
            Text = string.Copy(defVal);
        }
        ///  вернуть введенный текст и восстановить значение по умолчанию.
        public virtual void set() {
            retArg.set(Text);
            Text = string.Copy(defVal);
        }

    }

}