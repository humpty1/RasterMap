#pragma warning disable 219

//#define ATTEMPT
//#define DEBUG


using System;
using System.Timers; 

using System.Data;
using System.Drawing;
using System.Windows.Forms; 
using System.Collections; 
using System.Text.RegularExpressions; 
using System.IO;
using System.Threading;
using  System.Reflection;
using System.Globalization;
using Logger;

//        gVars.log.wrLn("ut.Tbl2_Ini.constructor: vv: {0} \n", vv);

namespace ut
{
  public class mk {
   static public string txt(double lat, double lon)
   {  if (0.0 <= lat && lat <= 90.0 && 0.0 <= lon && lon <= 180.0)
        return String.Format("{0:0.000000}N {1:0.000000}E", lat, lon);
      else 
        return String.Format("**.******N **.******E");
   }

   static public string Info(string dll){
//      Assembly  a = System.Reflection.Assembly.LoadFrom(".\\MBTile.dll");
      Assembly  a = System.Reflection.Assembly.LoadFrom(dll);


      string tit1 = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(a,
                        typeof(AssemblyTitleAttribute), false)
       ).Title;

      string descr1 = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(a,
                        typeof(AssemblyDescriptionAttribute), false)
       ).Description;
      return String.Format("{0}(ver:{1}) {2}",tit1, a.GetName().Version, descr1);
   }
  
  }

  public class warning {
   static public void 
   NRD(object sender, EventArgs e)
   {
       MessageBox.Show("this action is not ready!","warning");
   }

   static public void 
   NTD(object sender, EventArgs e)
   {
       MessageBox.Show("nothing to do!","warning");
   }
  }


  public struct CNST {
   
   public const      int  OK = 0;
   public const      int  NOTITEM = 1;
   public const      int  CANCEL  = 10;
   public const      int  WPAR  = 3;
   public const      int  ERR  = 5;
   public const      int  NRD  = 6;
   public const      string UNK_TXT = "";

//   string UNK_TXT = "not set";

   public const
   string YES = "yes";
   public const
   string NO =  "no";

   public const   
   int  WHEEL_DELTA= 120;

   public const string      _NRD    =  "is not ready!";
   public const string      _OK    =  "OK";
   public const string      _ESC   =  "Cancel";
   public const string      _EXIT  =  "Close";
   public const string      _ADD   =  "Add";
   public const string      _DEL   =  "Delete";
   public const string      _EDIT  =  "Edit";
   public const string      _REF   =  "Refresh";
   public const string      _FLT   =  "Filter";
   public const string      _FND   =  "Find";
   public const string      _ORD   =  "Order";
   public const string      _EXP   =  "Export";
   


   public const string      NULL   =   "<none>";

   public const string      NSF   =   "'<missing at server>'";

   public const string      stop0   =   "*stop*";
   public const string      stop1   =   "_STOP_";
   public const string      stop2   =   "Continue";
 }

  public struct SZ {
   public const
   int  TD_BUF = 500; // 45; //17; //32  17;//105; //

   public const   
   int  Y = 24;

   public const   
   int  X_BUTTON = 72;

   public const   
   int  X_LABEL1  = 64;

   public const   
   int  X_LABEL2  = 124;

   public const   
   int  X_TEXT   = 124;

   public const   
   int  X_FLD  = 1;
   public const   
   int  Y_FLD  = 2;

   public const   
   int  X_SPC  = 8;
   public const   
   int  Y_SPC  = 4;
 }
 
}
