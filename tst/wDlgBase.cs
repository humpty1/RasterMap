//#pragma warning disable 219

//
#define PANEL
//#define DEBUG
//#define LAYOUT
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Args;
using Logger;
using ut;



namespace wnd {

    ///  \brief ввод нескольких безтипных полей. 
    /**

       Безтипный означает, что не производится никаких проверок
       при вводе полей.
       Количество полей окна задается списком переменных в конструкторе.
    */

		public abstract class BaseDialog : DialogModel {
        protected _TextBox t1;           ///< поле ввода 
        protected Label l1;           ///< метка для поля ввода
		protected ComboBox cb1;
        protected int txtNo;         ///< здесь записано кол-во полей ввода
        public bool dbg;
        protected System.Drawing.Size bSz2; ///<это размер поля ввода, в два раза шире стандартной кнопки.
        ///
        public Loger l;
		protected bool justShow = false;

        protected abstract void DoStuff(); // Insert code before base
		public _Button OK_but;	

		protected Panel mainPanel;
		protected Arg[] ps;

		public BaseDialog(               ///< для наследования функции addFld в окно для ввода многих строчек
                 string name           ///<  имя окна ввода
                   , Loger ll = null     ///<  журнал для отладки окна
                       )
            : base(name, 2) {

            bSz2 = new System.Drawing.Size(ut.SZ.X_BUTTON * 2, ut.SZ.Y);   /// ширина для поля ввода
            Name = "InputText window";
            l = ll;
            txtNo = 0;
            if (l != null)
                l.WriteLine("{0} ctor  title '{1}'", Name, name);

            Load += loadH;
        }
                 
        public BaseDialog      ///этот конструктор для создания нескольких полей ввода
               (
                 string name          ///заголовок окна
                  , Loger ll
                 , params  Arg[] ps  ///список переменных для создания полей ввода
                       )
            : this(name, ll) {
            init(ps);
				
			DoStuff();
			this.ps = ps;
        }

        public BaseDialog(string name   ///< заголовок окна
                 , Arg p                ///< переменная для ввода
                    , Loger ll = null   ///< журнал для отладки

                         )
            : this(name, ll) {

                DoStuff();
	    
            initMainPanel();	
            addFld(1, p);
            setWidth();
        }
	
		void initMainPanel() {
			mainPanel = new Panel();
			mainPanel.MaximumSize = new Size(700, SystemInformation.VirtualScreen.Height * 6 / 10 - 50);
			mainPanel.AutoScroll = true;
			mainPanel.AutoSize = true;
			mainPanel.Dock = DockStyle.Top;
			mainPanel.Padding = new Padding(20, 0, 20, 0);
			Controls.Add(mainPanel);
		}
 	 
        void init(Arg[] ps) {
	    
	    initMainPanel();		    		
            this.AcceptButton = null;       /// в констукторе отменяется  нажатие клавиши Enter как на  кнопку Ok

            for (int i = ps.Length - 1; i >= 0; i--) {
                addFld(i, ps[i]);       ///
            }

            setWidth();
        }

        public void addFld          /// метод добавляет в окно новое поле ввода
          (int n, Arg par) {

            Padding pd = new Padding(0, 1 * SZ.X_SPC, 0, 1 * SZ.X_SPC);   ///
            Panel p = new Panel();               /// панель содержит метку и поле ввода
            p.Name = string.Format("p{0}", n);
            if (dbg)
                p.BorderStyle = BorderStyle.FixedSingle;
            p.Size = new Size(1, bSz.Height + 1 * SZ.X_SPC);
            p.Dock = DockStyle.Top;
            //p.Padding = pd;    ///

            mainPanel.Controls.Add(p);
		    
	    
	    if (par.vals == null) {
		//if (par.Equals(typeof(ArgFlg))) {
		if (par.GetType() == typeof(ArgFlg) && justShow) {
			addComboBox(n, par, p, new string[] {"True", "False"});
		}
		else {
	    		addBox(n, par, p);
		}
	    } 
	    else {
		addComboBox(n, par, p, par.vals);
		
	    }
	    	   
            addLabel(n, par, p);            
            
  
            txtNo++;
        }
	
	protected void addComboBox(int n, Arg par, Panel p, string[] values) {
		cb1 = new ComboBox();
//		cb1.TabIndex = n;
            	cb1.Name = string.Format("p{0}", n); 
		cb1.Width = bSz2.Width / 2;
            	cb1.Dock = DockStyle.Right;
		cb1.Text = par.val();		
		
		foreach (string val in values) {
			cb1.Items.Add(val);
		}	
		
		p.Controls.Add(cb1);	
		
	}
	
        protected void addBox (int n, Arg par, Panel p) {
            t1 = new _TextBox(par);
//            t1.TabIndex = n;
            t1.Name = string.Format("p{0}", n); ;
            //t1.Size = bSz2;
            t1.Width = bSz2.Width / 2;
            t1.Dock = DockStyle.Right;
            t1.restore();
            t1.KeyDown += _KeyDown;

            t1.txtChanged = par.txtChanged;
            t1.TextChanged += new EventHandler(_TextChanged);

            if (par.isPassword) {
                t1.UseSystemPasswordChar = true;
            }

            if (!par.edit) {
                t1.ReadOnly = true;
            }

            p.Controls.Add(t1);

            
        }

        protected void addLabel(int n, Arg par, Control p) {
            l1 = new Label();
            l1.Name = string.Format("l{0}", n);
           // l1.Size = bSz2;
            l1.Width = bSz2.Width / 2;
//            l1.Text = par.lNm;  /// создать новую строку
            l1.Text = Names.Text( par.lNm);  /// создать новую строку
            l1.Dock = DockStyle.Left;
	    //l1.TextAlign = ContentAlignment.TopRight;
            if (dbg)
                l1.BackColor = Color.Blue;

            p.Controls.Add(l1);
        }

        public void loadH(object sender, System.EventArgs e) {
            if (l != null)
                l.WriteLine(
                     " '{0}': Load is here {1}", Name, (l == null) ? "null" : "not null");
                        

        }

        _TextBox getTextBox(int oNo) {
            _TextBox foo;
            Panel foo1;
            foreach (Control y in Controls) {
                if (y is Panel) {
                    foo1 = (Panel)y;
                    if (l != null)
                        l.WriteLine(
                                  "IT:Esc button pressed    text/save: first for '{0}'", foo1.Name);
                    foreach (Control x in foo1.Controls) {
                        if (x is _TextBox) {
                            foo = (_TextBox)x;
                            if (foo.ordNo() == oNo)
                                return foo;
                        }
                    }
                }
            }
            return null;
        }

        protected void _KeyDown(object sender, KeyEventArgs e) {
            Control s = (Control)sender;
            if (s is Button)
                SelectNextControl(ActiveControl, true, true, true, true);

            else if (s is TextBox) {
                int box = ((_TextBox)s).ordNo();
                if (l != null)
                    l.WriteLine(
                         " '{0}': _keyDown is here,sender Name/code:{1}/{2} ", Name, s.Name, e.KeyCode);
                switch (e.KeyCode) {
                    case Keys.Return:
                        box++;
                        if (box >= txtNo) {
                            //OK_but.Focus();
                            return;
                        }
                        break;

                    case Keys.Down:
                        box++;

                        //                   SelectNextControl(ActiveControl, true, true, true, true);
                        if (box >= txtNo)
                            box = 0;
                        break;
                    case Keys.Up:
                        box--;
                        if (box < 0)
                            box = txtNo - 1;
                        break;

                }
                if (l != null)
                    l.WriteLine(
                         "cur/next box No: {0}/{1} ", ((_TextBox)s).ordNo(), box);


                _TextBox next = getTextBox(box);
                //if (next != null) {
                    //next.Focus();
                    // next.Active();
                //}
            }
        }

        public void EnableTextBoxes(bool b, string name) {                         // NEW Блокирую OK_but и все поля ввода кроме поля с именем name 
            foreach (Control c in mainPanel.Controls) {
                bool isTextBox = c is Panel && c.Name != name;

                if (isTextBox) {
                    c.Enabled = b;
                }
            }
	    
            
        }
        void _TextChanged(Object sender, EventArgs e) {                     // NEW  Обработчик изминения текста поля ввода
            _TextBox current = (sender as _TextBox);
	    
            string tb_name = current.Parent.Name;                     // NEW  Имя поля ввода
            string tb_text = current.Text.Trim();              // NEW  Текст поля ввода
            var inType = current.txtChanged;             // NEW  Тип поля ввода.
            bool enable = 	inType(sender, tb_text);    

	
            this.EnableTextBoxes(enable, tb_name);
	    OK_but.Enabled = enable;
        }

        public void setWidth() {
            this.Width = bSz2.Width * 2 - 20;
        }
    }
    /**
      \brief Класс - просто демонстрация для создания модального окна сообщения с двумя кнопками.
      Причем это окно является родителем  для многих других модальных окон.

    */

    public class DialogModel : Form {

        protected Size bSz;
	
        public DialogModel(
            string q = "question for OkCancel window" /// заголовок окна
          , int www = 1            /// минимальное количество дополнительных панелей между кнопками ок, канел
        ) {
            Padding _pd;
            Name = "OkCancel";
            Text = q;

            MinimizeBox = false;
            MaximizeBox = false;
            ControlBox = false;
            AutoScroll = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            AutoSize = true;
            DialogResult = DialogResult.Cancel;

            bSz = countBaseSize();
            Size =  bSz;

            Panel p0 = new Panel();           // P0 эта панель задает высоту строки
            p0.Size = bSz;

            p0.Dock = DockStyle.Top;
            Controls.Add(p0);               //  первую панель положили вниз

            Panel p = new Panel();      // `p` дополнительная панель между кнопками ок, кансел

            SizeF sizef;                     //подстраиваем размер окна под заголовок
            Graphics g = CreateGraphics();
            StringFormat sf = new StringFormat(StringFormatFlags.DirectionRightToLeft);
            sizef = g.MeasureString(q, System.Drawing.SystemFonts.CaptionFont, Int32.MaxValue, sf);
            if (sizef.Width - bSz.Width * 2 > bSz.Width * www)
                p.Size = new Size((int)(sizef.Width) - bSz.Width * 2, bSz.Height);
            else
                p.Size = new Size(bSz.Width * www, bSz.Height);

            _pd = new Padding(SZ.X_SPC * p.Size.Width / 30 - 1, SZ.Y_SPC
                                , SZ.X_SPC * p.Size.Width / 30 - 1, SZ.Y_SPC);
				
            Padding = new Padding(0, 20, 0, 20);
              //                  , 10, SZ.Y_SPC);
            p.Dock = DockStyle.Right; ;
            Controls.Add(p);

            Location = new Point(ut.SZ.X_SPC * 20, ut.SZ.Y_SPC * 20);
            this.StartPosition = FormStartPosition.CenterParent;
        }

         protected Size countBaseSize () {
             return new Size(ut.SZ.X_BUTTON, ut.SZ.Y);
         }
    }
}