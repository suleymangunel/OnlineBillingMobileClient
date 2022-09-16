using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OnlineFaturaMobileClient
{
    public partial class BaglantiTuruFRM : NonFullscreenForm
    {
        static BaglantiTuruFRM newMessageBox;
        static string Button_id;
        static bool BaglantiDahilimi;

        public BaglantiTuruFRM()
        {
            InitializeComponent();
        }

        private void BaglantiTuruFRM_Load(object sender, EventArgs e)
        {
            if (!AyarlariYukleRegistry())
                MessageBox.Show("Ayarlar okunurken hata oluştu !");
        }

        public static string ShowBox(string txtMessage)
        {
            newMessageBox = new BaglantiTuruFRM();
            newMessageBox.CenterFormOnScreen = true;
            newMessageBox.MesajLBL.Text = txtMessage;
            newMessageBox.ShowDialog();
            return Button_id;
        }

        public static string ShowBox(string txtMessage, string txtTitle,string txtStatus)
        {
            newMessageBox = new BaglantiTuruFRM();
            newMessageBox.CenterFormOnScreen = true;
            newMessageBox.MesajLBL.Text = txtMessage;
            newMessageBox.Text = txtTitle;
            newMessageBox.statusBar1.Text = txtStatus;
            newMessageBox.ShowDialog();
            return Button_id;
        }

        private void TamamBTN_Click(object sender, EventArgs e)
        {
            Button_id = "TAMAM";
            if (!AyarlariKaydetRegistry())
                MessageBox.Show("Ayarlar kaydedilirken hata oluştu !");
            else
                MessageBox.Show("Ayarlar kaydedildi");
            newMessageBox.Visible = false;
            newMessageBox.Dispose();
        }

        private void IptalBTN_Click(object sender, EventArgs e)
        {
            Button_id = "IPTAL";
            newMessageBox.Visible = false;
            newMessageBox.Dispose();
        }

        private bool AyarlariYukleRegistry()
        {
            try
            {
                RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\OFMC", true);
                BaglantiDahilimi = (int)Key.GetValue("BaglantiDahilimi") == 1 ? true : false;
                Key.Close();
                if (BaglantiDahilimi)
                    DahiliRB.Checked = true;
                else
                    HariciRB.Checked = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool AyarlariKaydetRegistry()
        {
            try
            {
                if (!Program.RegistryKayitlariOlustur())
                {
                    MessageBox.Show("Registry kayıtları oluşturulamadı !");
                    Application.Exit();
                }

                RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\OFMC", true);
                Key.SetValue("BaglantiDahilimi", DahiliRB.Checked ? 1 : 0);
                Key.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}