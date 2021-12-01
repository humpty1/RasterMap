#pragma warning disable 219

// #define DEBUG
using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Args;
using Logger;
using ut;
	

namespace wnd {

    public class OkDlg : BaseDialog {

       // public _Button OK_but;
        Panel p = new Panel();
		protected int current; 				// временная переманая (пока я не разберусь с рекурсией в setValues)

        protected override void DoStuff() {
            EnableTextBoxes(false, null);
        }

        public OkDlg(string name, Loger ll = null)
            : base(name, ll) {

               initBtn();
        }

        public OkDlg(string name, Loger ll, params  Arg[] ps)
            : base(name, ll, ps) {

            initBtn();
           
            OK_but.TabIndex = ps.Length;
            OK_but.KeyDown += _KeyDown;
			//DoStuff(ps);
			
        }

        public OkDlg(string name, Arg p, Loger ll = null)
            : base(name, p, ll) {

              initBtn();
              OK_but.TabIndex = 2;
        }

        private void initBtn() {

            OK_but = new _Button();
            OK_but.Dock = DockStyle.Right;
            this.AcceptButton = null;
            OK_but.Focus();
			OK_but.Click += new System.EventHandler(OK_but_Click);
            initBtn(OK_but, "OK_but", CNST._OK, DialogResult.OK);
            
        }
	
	 protected void initBtn(_Button b, string name, string text, DialogResult result) { 
			
            //p = new Panel();
            p.Size = new Size(100, 30);
	   // p.Location = new Point(10, 30);
            p.Dock = DockStyle.Bottom;    
	    //p.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
            Controls.Add(p);
		
            b.Name = name;
            b.Text = text;
            b.Size = new Size(92, 24);
            b.DialogResult = result;
	    //	
	    int pd = (this.Width - b.Width) / 2;
	    p.Padding = new Padding(pd, 0, pd, 0);
            p.Controls.Add(b);
        }

        void OK_but_Click(object sender, System.EventArgs e) {
			current = ps.Length - 1;
			setValues(this, ps.Length - 1);			
        }

		private void setValues(Control parent, int index)
		{			
			foreach (Control c in parent.Controls) {
				if (c.GetType() == typeof(_TextBox)) {
					ps[current].set((c as _TextBox).Text);
					current--;
				} else if (c.GetType() == typeof(ComboBox)) {
					ps[current].set((c as ComboBox).Text);
					current--;
				} else {
					setValues(c, current);
				}
			}
		}		
    }

    public class OkCancelDlg : OkDlg {
        public _Button ESC_but;

        protected override void DoStuff() {
			justShow = true;
		}

        public OkCancelDlg(string name, Loger ll = null)
            : base(name, ll) {

            initBtn();
            ESC_but.Click += new System.EventHandler(ESC_but_Click);
        }

        public OkCancelDlg(string name, Loger ll, params  Arg[] ps)
            : base(name, ll, ps) {
		
            initBtn();

            ESC_but.TabIndex = ps.Length + 1;
            ESC_but.KeyDown += _KeyDown;
        }

        public OkCancelDlg(string name, Arg p, Loger ll = null)
            : base(name, p, ll) {

            initBtn();
            ESC_but.TabIndex = 3;
        }

        private void initBtn() {
            ESC_but = new _Button();

            initBtn(ESC_but, "ESC_but", CNST._ESC, DialogResult.Cancel);

	    
            OK_but.Dock = DockStyle.Left;
            ESC_but.Dock = DockStyle.Right;

	    OK_but.Parent.Padding = new Padding(20, 0, 20, 0);

            this.CancelButton = ESC_but;
        }
        private void ESC_but_Click(object sender, System.EventArgs e) {
            if (l != null)
                l.WriteLine(
                    "IT:Esc button pressed    text/save: this '{0}'", Name);
        }
    }
}
