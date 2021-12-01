#pragma warning disable 642
// параметры ell  303

// малая полуось  6356752.3142
// эксцентриситет 0.0818191908426

// перечисление эллипсоидов стр. 11 серпинас
//

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Globalization;
using Args;
using Logger;

///


namespace geo
{                                                       //
                                                        //   r = d * pI/180
    public class math    {                              //   r * 180 = d * PI
                                                        //   r * 180 / pi = d
         static public double degree2radian(double d ){ // to remember
           return  d*(Math.PI/180.0);  
         }
         static public double radian2degree(double r ){ // to remember
           return  r*(180.0/Math.PI);       //???
         }

         static public double sin( int degree){
            return sin ((double) degree);
         }
         static public double sin( double degree){
            return Math.Sin(degree*(Math.PI/180.0));
         }                                  

         static public double cos( int degree){
            return cos ((double) degree);
         }
         static public double cos( double degree){
            return Math.Cos(degree*(Math.PI/180.0));
         }
                
    }
    // real segment
    public struct sgmDouble {
       public double min;
       public double max;
       public  sgmDouble (double mi, double ma) {
            min = mi; max = ma;
       }
       public  sgmDouble (int i) {
            min = double.MaxValue; max = double.MinValue;
       }
    } 
    //  screen segment
    public struct sgmInt {
       public int min;
       public int max;
       public  sgmInt (int mi, int ma) {
            min = mi; max = ma;
       }
    } 

    public struct sgmFloat {
       public float min;
       public float max;
       public  sgmFloat (float mi, float ma) {
            min = mi; max = ma;
       }
    } 

    ///  отображение  оси координат из реального мира в пиксели
    public struct mappingF {
       public sgmDouble  B;    // входной отрезок, реальный мир	       big
       public sgmFloat     s;    // выходной отрезок, пиксели на экране  small
       public mappingF ( sgmFloat oo): this(oo, new sgmDouble(1)){
       // o = oo;
       // i = 
       }
       public mappingF (sgmFloat oo, sgmDouble ii){
         B = ii;
         s = oo;
       }
       public float val ( double Val) {
          return (float) ( 
              ((Val - B.min) /(B.max - B.min)) 
                               * (s.max - s.min) + s.min
          )	;
       }

       public double Val ( float v) {
          return  ( 
              ((((double)v) - s.min) /((double)(s.max - s.min)))
                               * (B.max - B.min) + B.min
          )	;
       }

       public static implicit operator string (mappingF m) {
           return  String.Format("in[{0}..{1}]:outF[{2}..{3})]",m.B.min,m.B.max, m.s.min,m.s.max);
       }
    }     
    ///  отображение  оси координат из реального мира в пиксели
    public struct mapping {
       public sgmDouble  B;    // входной отрезок, реальный мир	       big
       public sgmInt     s;    // выходной отрезок, пиксели на экране  small
       public mapping ( sgmInt oo): this(oo, new sgmDouble(1)){
       // o = oo;
       // i = 
       }
       public mapping (sgmInt oo, sgmDouble ii){
         B = ii;
         s = oo;
       }
       public int val ( double Val) {
          return (int)Math.Round  ( 
              ((Val - B.min) /(B.max - B.min)) 
                               * (s.max - s.min) + s.min
          )	;
       }

       public double Val ( int v) {
          return  ( 
              ((((double)v-0.5) - s.min) /((double)(s.max - s.min)))
                               * (B.max - B.min) + B.min
          )	;
       }

       public static implicit operator string (mapping m) {
           return  String.Format("in[{0}..{1}]:out[{2}..{3}]",m.B.min,m.B.max, m.s.min,m.s.max);
       }
    }     
}

