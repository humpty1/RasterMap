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
    public class ASCRec      : rec
    {
        public ASCRec(string whereToGet, Loger log, char decPnt='.') : base (whereToGet, log, decPnt) {
                      setP1(0, "photo");
                      setBt(1,"latitude");
                      setLr(2,"longitude");
                      setDu(3, "height");
                      setP2(4, "yaw",   0.0);
                      setP3(5, "pitch", 0.0);
                      setP4(6, "roll",  0.0);
        }

    }
}
