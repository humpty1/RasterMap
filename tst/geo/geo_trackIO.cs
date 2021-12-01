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
using Logger;
using ut;

namespace geo                                          //  x --> lr  left to right
{                                                      //  y --> bt  bottom to top
                                                       //  z --> du  down to up
    public class rec      : usingLogger, IDisposable
    {
        static public int dt2No;   //time
        static public int dtNo;   // datetime or date
        static public int lrNo;
        static public int btNo;
        static public int duNo;
        static public int p1No;
        static public int p2No;
        static public int p3No;
        static public int p4No;
        static public string dtNm;
        static public string lrNm;
        static public string btNm;
        static public string duNm;
        static public string p1Nm;
        static public string p2Nm;
        static public string p3Nm;
        static public string p4Nm;
        static object p1Type;
        static object p2Type;
        static object p3Type;
        static object p4Type;
        double    _lr;
        double    _bt;
        int       _du;
        DateTime  _tm;
        object    _p1;  
        object    _p2;  
        object    _p3;  
        object    _p4;  


        char          dP;             //  decimal point
        string        whereToGet;
        string[]      record;
        StreamReader file; 
        int          lineno;
        int          errors;
        static       string[] sep = { " " };     // separators
        int          lastErrLine = 0;
        string       lastError = "";
        bool         multySep = true;


        readonly   char NumberDecimalSeparator
              = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];



        public  DateTime  tm {                      //  координаты точки
           get{
                return _tm;
           }
        }

        public  double  lr {                      //  координаты точки
           get{
                return _lr;
           }
        }
        public  double  bt {
           get{
                return _bt;
           }
        }
        public  int    du {
           get{
                return _du;
           }
        }
        public void setDt(int oNo
                , string nm="time"
                , int oNo2=-1        // если дата и время разнесены по разным полям
        ){ //
					dtNo = oNo;
					dt2No = oNo2;
					dtNm = nm;
        }
        public void setLr(int oNo, string nm="longitude"){
					lrNo = oNo;
					lrNm = nm;
        }
        public void setBt(int oNo, string nm="latitude"){
					btNo = oNo;
					btNm = nm;
        }
        public void setDu(int oNo, string nm="height"){
					duNo = oNo;
					duNm = nm;
        }

        public  object  par1 {                   // значения нек. параметров в точке.
           get{
                return _p1;
           }
        }
        public  object  par2 {
           get{
                return _p2;
           }
        }
        public  object  par3 {
           get{
                return _p3;
           }
        }
        public  object  par4 {
           get{
                return _p4;
           }
        }
        public void setP1(int oNo, string nm="", object t = null){
					p1No = oNo;
					if (t!=null)
						p1Type = t;
					else
  					p1Type = "";
					if (nm.Length > 0)
						p1Nm = nm;
        }
        public void setP2(int oNo, string nm="", object t = null){
					p2No = oNo;
					if (t!=null)
						p2Type = t;
					else
  					p2Type = "";
					if (nm.Length > 0)
						p2Nm = nm;
//          WriteLine (IMPORTANCELEVEL.Spam, "setP2: type/p2No/p2Nm: {0}/{1}/'{2}'", p2Type.GetType(), p2No, p2Nm);

        }
        public void setP3(int oNo, string nm="", object t = null){
					p3No = oNo;
					if (t!=null)
						p3Type = t;
					else
  					p3Type = "";
					if (nm.Length > 0)
						p3Nm = nm;
        }
        public void setP4(int oNo, string nm="", object t = null){
					p4No = oNo;
					if (t!=null)
						p4Type = t;
					else
  					p4Type = "";
					if (nm.Length > 0)
						p4Nm = nm;
          WriteLine (IMPORTANCELEVEL.Spam, "setP4: type/pNo/pNm: {0}/{1}/'{2}'", p4Type.GetType(), p4No, p4Nm);

        }
        public void toggleMultySep (){
           multySep = ! multySep;      
        }

        public void setSep (string sep1){
           sep[0]  = sep1;
        }

        public string txt(){  /// экспорт значенний
           string val = string.Format ("{0:0.000000} {1:0.000000}", bt , lr) ;
           if(duNm!=null)  val += " " +du.ToString();
           if(dtNm!=null)  val += " " +tm.ToString();
           if(p1Nm!=null)  val += " " +par1.ToString();
           if(p2Nm!=null)  val += " " +par2.ToString();
           if(p3Nm!=null)  val += " " +par3.ToString();
           if(p4Nm!=null)  val += " " +par4.ToString();
           return val;

        }
        public  string cmt(){  /// названия полей
           string val = string.Format ("# {0} {1} ", btNm, lrNm) ;
           if(duNm!=null)  val += " " +duNm;
           if(dtNm!=null)  val += " " +dtNm;
           if(p1Nm!=null)  val += " " +p1Nm;
           if(p2Nm!=null)  val += " " +p2Nm;
           if(p3Nm!=null)  val += " " +p3Nm;
           if(p4Nm!=null)  val += " " +p4Nm;
           return val;
        }

        public tuple make(){
          tuple val = new tuple();
           if(duNm!=null)  val.Add(duNm, du);
           if(dtNm!=null)  val.Add(dtNm, tm );
           if(p1Nm!=null)  val.Add(p1Nm, par1);
           if(p2Nm!=null)  val.Add(p2Nm, par2);
           if(p3Nm!=null)  val.Add(p3Nm, par3);
           if(p4Nm!=null)  val.Add(p4Nm, par4);
           return val;
        }

        
        public static implicit operator string (rec r) {  //  записи в строчку 
           string val =  string.Format("{0}",  mk.txt(r.bt, r.lr)) ;
           if(duNm!=null)  val += string.Format("; {0}: {1}", duNm, r.du.ToString());
           if(dtNm!=null)  val += string.Format("; {0}: {1}", dtNm, r.tm.ToString());
           if(p1Nm!=null)  val += string.Format("; {0}: {1}", p1Nm, r.par1.ToString());
           if(p2Nm!=null)  val += string.Format("; {0}: {1}", p2Nm, r.par2.ToString());
           if(p3Nm!=null)  val += string.Format("; {0}: {1}", p3Nm, r.par3.ToString());
           if(p4Nm!=null)  val += string.Format("; {0}: {1}", p4Nm, r.par4.ToString());
/*           if(r.du >= 0   )  val += string.Format("; {0}: {1}", duNm, r.du.ToString());
           if(dtNm  !=null)  val += string.Format("; {0}: {1}", dtNm, r.tm.ToString());
           if(r.par1!=null)  val += string.Format("; {0}: {1}", p1Nm, r.par1.ToString());
           if(r.par2!=null)  val += string.Format("; {0}: {1}", p2Nm, r.par2.ToString());
           if(r.par3!=null)  val += string.Format("; {0}: {1}", p3Nm, r.par3.ToString());
           if(r.par4!=null)  val += string.Format("; {0}: {1}", p4Nm, r.par4.ToString());
*/           return val;
        }

        public rec(string whereToGet, Loger log, char decPnt='.') : base (log) {
          lrNo =-1;
          btNo =-1;
          duNo =-1;
          p1No =-1;
          p2No =-1;
          p3No =-1;
          p4No =-1;
          dtNo =-1;
          dt2No =-1;
          p1Nm ="param1";
          p2Nm ="param2";
          p3Nm ="param3";
          p3Nm ="param4";
          _lr = 0.0;   
          _bt = 0.0;   
          _du = 0;   
          _tm =DateTime.Now ;   
          _p1 = null;   
          _p2 = null;   
          _p3 = null;   
          _p4 = null;   

          this.dP     = decPnt;
          this.whereToGet = whereToGet;
          WriteLine(IMPORTANCELEVEL.Debug, "ctor: decimal point is '{0}'"
                                , dP, decPnt.ToString());
          NumberDecimalSeparator
              = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
          lastErrLine = 0;
          lastError = "";
          Open();
        }

        public bool Open(Encoding encoding){ 
          if(File.Exists(whereToGet)){
	          file = new StreamReader(whereToGet, encoding);
            lineno = 0;
            errors = 0;
	          return true;
          }
          return false;
        }
        object   parse (string v, object t  ){
          object rc=null;
          Type tt = t.GetType();
          if (tt.Equals(typeof(double)) ) {
             double vv;
             if (NumberDecimalSeparator != dP)
             {
                 v = v.Replace(dP, NumberDecimalSeparator);
             }

             WriteLine (IMPORTANCELEVEL.Spam, "parse: type/val: {0}/{1}", tt, v);
             if (double.TryParse(v, out vv)){
                WriteLine (IMPORTANCELEVEL.Spam, "parse: type/vv: {0}/{1}", tt, vv);
                rc = vv;
						}
             else 
                WriteLine (IMPORTANCELEVEL.Spam, "cannot parse: type/val: {0}/{1}", tt, v);

          }
          else if (tt.Equals(typeof(int)))  {
             int vv;
             if (int.TryParse(v, out vv))
                rc = vv;

          }
          else if (tt.Equals(typeof(string)))
            rc = v;
          return rc;
        }
        public bool Open(){ 
          if(File.Exists(whereToGet)){
	          file = new StreamReader(whereToGet);
	          if (file !=null) {
               WriteLine(IMPORTANCELEVEL.Debug, "file '{0}' was opened",  whereToGet );
	             return true;
	          }
          }
          WriteLine(IMPORTANCELEVEL.Error, "cannot open file '{0}'",  whereToGet );
          return false;
        }

        public bool ReadLine(){
          string x = "";
          string y = "";
          string z = "";
          string t = "";
          string d = "";

          string str;
          WriteLine(IMPORTANCELEVEL.Debug, "ReadLine: {0}/{1}: {2}/{3}"
                                , lrNm, btNm, lrNo,  btNo
          );
          if (file!=null && lrNo >= 0 && btNo >= 0 ){

            bool justOnce = true;

            while (errors < 200 && (str=file.ReadLine())!=null) {
              lineno++;
//              WriteLine(IMPORTANCELEVEL.Spam, "ReadLine: lineno/line: {0}/'{1}'"
              WriteLine( IMPORTANCELEVEL.Spam ,"ReadLine: lineno/line: {0}/'{1}'"
                       , lineno, str);

              if(str.Length > 1 && str[0] != '#'){

                record = str.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                WriteLine( "ReadLine: lineno/record Length/: {0}/{1}"
                         , lineno, record.Length);
                if ( lrNo < record.Length && btNo < record.Length) {
                  x = record[lrNo];
                  y = record[btNo];

                  try
                  {
                       if (NumberDecimalSeparator != dP)
                       {
                           x = x.Replace(dP, NumberDecimalSeparator);
                           y = y.Replace(dP, NumberDecimalSeparator);
                       }
                       _lr = Convert.ToDouble(x);
                       _bt = Convert.ToDouble(y);
                       if (dtNo >=0 &&  dtNo < record.Length) {
                          d = record[dtNo];
                        	if (dt2No >=0&& dt2No < record.Length) {
                        	  t = record[dt2No];
                        	  d += ' '+t;
                        	}
                        	_tm = Convert.ToDateTime(d);
                       }
                       if (duNo >=0 &&  duNo < record.Length) {
                          z = record[duNo];
                          if (NumberDecimalSeparator != dP)
                          {
                              z = z.Replace(dP, NumberDecimalSeparator);
                          }
                          _du = (int)Convert.ToDouble(z);
                       }
                       if (p1No >=0 &&  p1No < record.Length) {
                          z = record[p1No];
                          _p1 = parse(z, p1Type);
                       }
                       if (p2No >=0 &&  p2No < record.Length) {
                          z = record[p2No];
                          _p2 = parse(z, p2Type);
                       }
                       if (p3No >=0 &&  p3No < record.Length) {
                          z = record[p3No];
                          _p3 = parse(z, p3Type);
                       }
                       if (p4No >=0 &&  p4No < record.Length) {
                          z = record[p4No];
                          _p4 = parse(z, p4Type);
                       }

                       WriteLine( "ReadLine: lineno/{0}/{1}: {2}/{3}/{4}"
                                       , lrNm, btNm, lineno, _lr, _bt);

       	              return true;
                  }
                  catch (Exception ex)
                  {
                      errors++;
                      lastErrLine = lineno;
                      lastError = ex.Message;
                      WriteLine(IMPORTANCELEVEL.Error, "LineNo/x/y/error: {0}/{1}/{2}/'{3}'"
                                   , lineno, x, y, ex.Message);

      	              return false;
                  }

	        }
	        else {
	                 errors++;
	                 WriteLine("{0} : to few fields: lineNo/length/{3}No/{5}No :{1}/{2}/{4}/{6} "
	                    ,"ReadLine"
	                      , lineno
	                        , record.Length
	                          , lrNm, lrNo 
	                            , btNm, btNo );

	                 for (int jj = 0; jj < record.Length && justOnce; jj++) {
                             WriteLine(IMPORTANCELEVEL.Spam, "fldNo/text: {0}/'{1}'"
                                 ,jj, record[jj]
                             );
                         }
                         justOnce = false;
	        }
              }
            }

          }
          return false;
        }
        public void Close(){ Dispose();}

        public  void Dispose(){
          if (file!=null)
	          file.Dispose();
        }
    }

}
