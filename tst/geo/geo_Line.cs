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
namespace geo
{                                                     

    public enum line
    {    track
        ,isoline
        ,other
    };

    public class tuple :  Dictionary <string, object>
    {                       // третья целая, я не плавающая, так как по ней не будет поиска в хештаблице
        public int z;              // третья координата в пикселях, тут высота 
                            //  все остальное реальные широта, долгота, скорость, фотка, все остальное 
                            //  лежит в списке
        public  string  val {                      
           get{
             string rc =  mk.txt((double)this["y"], (double)this["x"]);
             foreach (string j in Keys)
                if (j != "x" && j != "y")
                  rc += string.Format(" {0}:{1} ", j,this[j]);

              return rc;
           }
        }
    }


    public class Line   : usingLogger
    {                                 // класс для рисования треков или изолиний на карте
        readonly public string   nm;  // название трека
        readonly public line     ln;  // вид трек или изолиния

        readonly public Pen      pen; // этим карандашем рисуем 
        public PointF[] ps;        //  координаты точки в пикселях
        List<PointF> tmp;             //
        Dictionary <PointF, tuple> ts;  //   плавающие точки, чтобы различать точки в одном пикселе для поиска.
                                       // точки трека в исходном виде
        mappingF xM ;                //  это пропорции для отображения
        mappingF yM ;                //  реальных значений в пиксели экрана по трем измерениям
   public     mapping  test ;      
        mapping  zM ;                //   надо  задать диапазон значений в реальности и на пикселях
        readonly public string   xNm; //  название иксов - широта
        readonly public string   yNm; //  название игреков - долготоа
        readonly public string   zNm; //  высота


        public string descr( int oNo, bool verbose = false){
        
           if (0 <= oNo && oNo < ps.Length){
              int z = 0;
              tuple t = ts[ps[oNo]];
              if (t != null)
                z += t.z;

              string rc =
               string.Format("oNo/x/y/z(pixel): {0}/{1:0.00}/{2:0.00}/{3}::"
                     , oNo
                     , ps[oNo].X
                     , ps[oNo].Y 
                     , z
               );
              if (t != null)
                rc += " "+t.val;
              else
                rc += "null tuple";
              return rc ; 
           }

           return "nothing to return";
        }

        public Line(string name
           , line     l
           , Pen      pen
           , mappingF x 
           , string xName
           , mappingF y 
           , string  yName
           , mapping  z 
           , string zName
           , Loger log
        ): base (log) {
          nm     = name;
          ln     = l;
          this.pen = pen;
          this.xM = x;
          this.yM = y;
          this.zM = z;
          xNm = xName;
          yNm = yName;
          zNm = zName;
          tmp = new List<PointF>();
          ts  = new Dictionary <PointF, tuple>();
        }
        public void begin(){
          ps = null  ;
          tmp.Clear();
          ts.Clear() ;
        }
        public void commit(){
          ps = tmp.ToArray();
          tmp.Clear();   /// ???
        }

        public void add (  double x, double y, double z, tuple tail){
           PointF v =  new PointF( xM.val(x), yM.val(y) );
 //          Point  t =  new Point( test.val(x), test.val(y));
           tmp.Add(v);
           tuple head = new tuple();
           head.z = zM.val(z);  // эта точка нужна для трех мерного вращения 
           head.Add("x", x);
           head.Add("y", y);
       //    head.Add(zNm, z);
                // Получить коллекцию ключей
                ICollection<string> keys = tail.Keys;
                foreach (string  j in keys)
                  head.Add(j, tail[j]);
            ts.Add(v, head);
//            WriteLine("mapping results: v:[{0}..{1}] test:[{2}..{3}]",  v.X, v.Y, t.X, t.Y );
        }

        public string info (bool verbose=false){  ///для вывода информации о Line
           string xm = xM;
           string ym = yM;
           string zm = zM;
//           string rc = string.Format( "{0} Info:", "ddd");
    //         string rc  = string.Format( "{O} Info:"// length/pen: {1}/{2}"
    //         ,nm
         //    , ps.Length
          //   , pen.Color
      //       );


           string rc  = string.Format( "{0} Info: length/pen: {1}/{2} \n x mapping: {3}\n y mapping: {4}\n z mapping {5}"
              , nm, ps.Length, pen.Color
              , xm, ym, zm
           );
           return rc;
        }

        void export (string flNM){  ///для вывода объекта текущего класса

        }

    }
}
