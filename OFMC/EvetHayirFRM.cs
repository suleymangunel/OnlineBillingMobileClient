using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OnlineFaturaMobileClient
{
    public partial class EvetHayirFRM : NonFullscreenForm
    {
        static EvetHayirFRM newMessageBox;
        static string Button_id;
        
        public EvetHayirFRM()
        {
            InitializeComponent();
        }

        public static string ShowBox(string txtMessage)
        {
            newMessageBox = new EvetHayirFRM();
            newMessageBox.CenterFormOnScreen = true;
            newMessageBox.MesajLBL.Text = txtMessage;
            newMessageBox.ShowDialog();
            return Button_id;
        }

        public static string ShowBox(string txtMessage, string txtTitle, string txtStatus)
        {
            newMessageBox = new EvetHayirFRM();
            newMessageBox.CenterFormOnScreen = true;
            newMessageBox.MesajLBL.Text = txtMessage;
            newMessageBox.Text = txtTitle;
            newMessageBox.ShowDialog();
            return Button_id;
        }

        public static string ShowBox(string txtMessage, string txtTitle, string ButtonLeftText,string ButtonRightText)
        {
            newMessageBox = new EvetHayirFRM();
            newMessageBox.CenterFormOnScreen = true;
            newMessageBox.MesajLBL.Text = txtMessage;
            newMessageBox.Text = txtTitle;
            newMessageBox.LeftBTN.Text = ButtonLeftText;
            newMessageBox.RightBTN.Text = ButtonRightText;
            newMessageBox.ShowDialog();
            return Button_id;
        }

        private void LeftBTN_Click(object sender, EventArgs e)
        {
            Button_id = "LEF";
            newMessageBox.Dispose();
        }

        private void RightBTN_Click(object sender, EventArgs e)
        {
            Button_id = "RIGHT";
            newMessageBox.Dispose();
        }
    }
}