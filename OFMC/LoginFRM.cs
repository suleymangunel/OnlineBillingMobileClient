using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Globalization;
using Microsoft.Win32;
using Microsoft.WindowsMobile.Status;
using Calib;
using CalibCs;

namespace OnlineFaturaMobileClient
{
    public partial class LoginFRM : Form
    {
        [DllImport("coredll.dll", EntryPoint = "CeRunAppAtEvent", SetLastError = true)]
        private static extern bool CeRunAppAtEvent(string pwszAppName, int lWhichEvent);

        [DllImport("coredll.dll")]
        private extern static int GetDeviceUniqueID([In, Out] byte[] appdata, int cbApplictionData, int dwDeviceIDVersion, [In, Out] byte[] deviceIDOuput, out uint pcbDeviceIDOutput);

        OnlineFaturaServisi.Service1 FaturaServisi = new OnlineFaturaMobileClient.OnlineFaturaServisi.Service1();
        LUSServisi.Service1 LUS_Servisi = new OnlineFaturaMobileClient.LUSServisi.Service1();

        string MS_DevID = "";
        string CASIO_DevID = "";
        //IntPtr phSIM = new IntPtr();
        static int logo = 0;
        Size EbatBildirimler = new Size(139, 138);
        
        //Fonksiyon tuşlarını Microsoft API ile tanımlamak için prosedür düzgün çalışıyor
        [DllImport("coredll.dll")]
        private static extern bool UnregisterFunc1(KeyModifiers modifiers, int keyID);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers Modifiers, int key);

        public enum KeyModifiers
        {
            MOD_NONE = 0x0000,
            MOD_ALT = 0x0001,
            MOD_CONTROL = 0x0002,
            MOD_SHIFT = 0x0004,
            MOD_WIN = 0x0008,
            MOD_KEYUP = 0x1000,
            MOD_NOREPEAT = 0x4000,
        }
        //-----------------------------------------------------
        

        public LoginFRM()
        {
            InitializeComponent();
        }

        private void LoginFRM_Load(object sender, EventArgs e)
        {
            ProgramLogoPB.Image = imageList3.Images[1];
            ProgramLogoPB.Update();
            
            Program.KeyboardLightOnOff(false);

            if (!SetBacklightLevel()) MessageBox.Show("Aydınlatma seviyesi ayarlanamadı");

            //if (!Program.SetFunctionKeys_CASIO()) MessageBox.Show("Fonsyion tuşları tanımlanamadı, sadece ekran tuşlarını kullanabilirsiniz.", "UYARI", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
            if (!SetFunctionKeys_MS()) MessageBox.Show("Fonksiyon tuşları tanımlanamadı, sadece ekran tuşlarını kullanabilirsiniz.", "UYARI", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);

            if (!AyarlariYukleRegistry())
            {
                MessageBox.Show("Ayarlar okunurken hata oluştu !");
            }
            
            if (!Program.WWANCalistir())
            {
                MessageBox.Show("WWAN çalıştırılamadı ! Terminal yeniden başlatılacak", "KRİTİK HATA", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                Calib.SystemLibNet.Api.SysSoftReset();
                Application.Exit();
            }

            /*
            if (!TerminalCraddleUstundemi())
                if (MessageBox.Show("Bağlantı yerel mi ?", "Bağlantı Türü", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    Program.ServisURL = Program.ServisURLdahili;
                else
                    Program.ServisURL = Program.ServisURLharici;
            */

            if (!BolgeselAyarlarKontrol())
            {
                Program.LoginOK = false;
                DialogResult = DialogResult.No;
                this.Close();
                Application.Exit();
            }

            UserTXT.Focus();
        }

        private void ProgramLogoPB_MouseDown(object sender, MouseEventArgs e)
        {
            Program.Titret(120);
            string StatusText = (TerminalCraddleUstundemi() ? "Craddle: Bağlı" : "Craddle: Yok");
            StatusText += " / " + (InternetBaglantisiVarmi() ? "Internet: Var" : "Internet: Yok");
            string Secim = BaglantiTuruFRM.ShowBox("Servis bağlantısı sağlanamadı, lütfen bağlantı türünüzü doğrulayın", "Bağlantı Seçimi", StatusText);
            this.Visible = true;
            if (!AyarlariYukleRegistry())
                MessageBox.Show("Ayarlar okunurken hata oluştu !");
        }

        private bool AyarlariYukleRegistry()
        {
            try
            {
                RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\OFMC", true);
                Program.ServisURL =(int)Key.GetValue("BaglantiDahilimi") == 1 ? Program.ServisURLdahili : Program.ServisURLharici;
                Key.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public static bool InternetBaglantisiVarmi()
        {
            try
            {
                System.Net.Sockets.TcpClient client =
                    new System.Net.Sockets.TcpClient("www.diyargaz.com.tr", 80);
                client.Close();
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        
        private string LUSAdresi()
        {
            IAsyncResult iar1 = FaturaServisi.BeginGetLUSadress(Program.SvcUser, Program.SvcPass, null, null);
            iar1.AsyncWaitHandle.WaitOne();
            return FaturaServisi.EndGetLUSadress(iar1);
        }

        private bool BolgeselAyarlarKontrol()
        {
            string OndalikSimgesi = ",", GrupSimgesi = ".", TarihFormati = "DD.MM.YY", TarihAyiraci = ".", SaatFormati = "HH:MM:SS", SaatAyiraci = ":";

            if (CultureInfo.CurrentCulture.Name.ToUpper() != "TR-TR")
            {
                MessageBox.Show("Lütfen bölgesel ayarları Türkiye/Turkish yapın");
                return false;
            }

            if (CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator != OndalikSimgesi)
            {
                MessageBox.Show("Para ondalik simgesi '" + OndalikSimgesi + "' olmalı");
                return false;
            }

            if (CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator != GrupSimgesi)
            {
                MessageBox.Show("Para basamak simgesi '" + GrupSimgesi + "' olmalı");
                return false;
            }

            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != OndalikSimgesi)
            {
                MessageBox.Show("Sayı ondalik simgesi '" + OndalikSimgesi + "' olmalı");
                return false;
            }

            if (CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator != GrupSimgesi)
            {
                MessageBox.Show("Sayı basamak simgesi '" + GrupSimgesi + "' olmalı");
                return false;
            }

            if (CultureInfo.CurrentCulture.NumberFormat.PercentDecimalSeparator != OndalikSimgesi)
            {
                MessageBox.Show("Yüzde ondalik simgesi '" + OndalikSimgesi + "' olmalı");
                return false;
            }

            if (CultureInfo.CurrentCulture.NumberFormat.PercentGroupSeparator != GrupSimgesi)
            {
                MessageBox.Show("Yüzde basamak simgesi '" + GrupSimgesi + "' olmalı");
                return false;
            }

            if (CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToUpper() != TarihFormati)
            {
                MessageBox.Show("Tarih formatı '" + TarihFormati + "' olmalı");
                return false;
            }

            if (CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator != TarihAyiraci)
            {
                MessageBox.Show("Tarih ayıracı '" + TarihAyiraci + "' olmalı");
                return false;
            }

            if (CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.ToUpper() != SaatFormati)
            {
                MessageBox.Show("Saat formatı '" + SaatFormati + "' olmalı");
                return false;
            }

            if (CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator != SaatAyiraci)
            {
                MessageBox.Show("Zaman ayıracı '" + SaatAyiraci + "' olmalı");
                return false;
            }
            return true;
        }

        private byte[] GetDeviceID(string AppString)
        {
            byte[] AppData = new byte[AppString.Length];
            for (int count = 0; count < AppString.Length; count++)
                AppData[count] = (byte)AppString[count];
            int appDataSize = AppData.Length;
            byte[] DeviceOutput = new byte[20];
            uint SizeOut = 20;
            GetDeviceUniqueID(AppData, appDataSize, 1, DeviceOutput, out SizeOut);
            return DeviceOutput;
        }

        private void TerminalKimliginiOku()
        {
            //Microsoft coredll fonksiyonu----------------------
            byte[] buffer = GetDeviceID("MyAppString");
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < buffer.Length; x++)
            {
                //sb.Append('{');
                sb.Append(string.Format("{0:x2}", buffer[x]));
                //sb.Append("} ");
            }
            MS_DevID = sb.ToString();

            //Casio SysLibNet fonksiyonu------------------------
            char[] chrDevID = new char[32];
            Calib.SystemLibNet.Api.SysGetDeviceIDCode(chrDevID);
            CASIO_DevID = new string(chrDevID).Replace("\0", string.Empty);
        }

        private string TerminalYetkilimi(string CASIOSysGetDeviceIDCode, string MSGetDeviceUniqueID, string User, string Pass)
        {
            string ServisDonusKodu = "";
            
            try
            {
                int sServisTarihi = 0;
                int sLisansTarihi = 1;
                int sGPS = 2;
                int sSMS = 3;

                LUSServisi.AuthenticationHeader LUSkimlik = new LUSServisi.AuthenticationHeader();
                LUSkimlik.UserName = "_CobraSpaceAdventure_";
                LUSkimlik.Password = "_TimeMachine_";
                LUS_Servisi.AuthenticationHeaderValue = LUSkimlik;

                string[] strLUS = LUS_Servisi.TerminalTanimlimi(CASIOSysGetDeviceIDCode.Trim(), MSGetDeviceUniqueID.Trim()).Split('#');
                if (strLUS[0] != "YOK")
                {
                    CultureInfo arSA = new CultureInfo("tr-TR");
                    if (strLUS[sServisTarihi] != DateTime.Today.ToString("dd.MM.yyyy")) return "-2";
                    if (DateTime.Today.Subtract(DateTime.Parse(strLUS[sLisansTarihi], arSA)).Days >= -7 && DateTime.Today.Subtract(DateTime.Parse(strLUS[sLisansTarihi], arSA)).Days <= 0) ServisDonusKodu = "-3#" + Math.Abs(DateTime.Today.Subtract(DateTime.Parse(strLUS[sLisansTarihi], arSA)).Days).ToString();
                    if (DateTime.Today > DateTime.Parse(strLUS[sLisansTarihi], arSA)) return "-4";
                    string[] LisansliModuller = new string[10];
                    int sayac = -1;
                    if (strLUS[sGPS] == "1")
                    {
                        Program.GPSLisansli = true;
                        sayac++;
                        LisansliModuller[sayac] = "GPS";
                    }
                    if (strLUS[sSMS] == "1")
                    {
                        Program.SMSLisansli = true;
                        sayac++;
                        LisansliModuller[sayac] = "RemoteSMS";
                    }
                    for (int s = 0; s <= sayac; s++)
                    {
                        Program.LisansliModuller += LisansliModuller[s];
                        if (s != sayac && sayac > 0) Program.LisansliModuller += ",";
                    }
                }
                else
                {
                    return "-5";
                }


                IAsyncResult iar1 = FaturaServisi.BeginTerminalKullaniciKontrol(Program.SvcUser, Program.SvcPass, CASIOSysGetDeviceIDCode.Trim(), MSGetDeviceUniqueID.Trim(), User.Trim(), Pass.Trim(), null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndTerminalKullaniciKontrol(iar1);

                if (xe.SelectSingleNode("HataMesaji").InnerXml != "YOK")
                {
                    if (xe.SelectSingleNode("HataMesaji").InnerXml == "2")
                        Program.TerminalNo = xe.SelectSingleNode("TerminalNo").InnerXml;
                    return Convert.ToString(xe.SelectSingleNode("HataMesaji").InnerXml) + "#" + ServisDonusKodu;
                }
                Program.TerminalNo = xe.SelectSingleNode("TerminalNo").InnerXml;
                Program.PersonelKod = xe.SelectSingleNode("PersonelKod").InnerXml;
                Program.PersonelSicilNo = xe.SelectSingleNode("PersonelSicilNo").InnerXml;
                Program.PersonelAd = xe.SelectSingleNode("PersonelAd").InnerXml;
                Program.PersonelSoyad = xe.SelectSingleNode("PersonelSoyad").InnerXml;
                return "0" + "#" + ServisDonusKodu;
            }
            catch
            {
                return "-1" + "#" + ServisDonusKodu;
            }
        }

        private void KlavyePB_MouseDown(object sender, MouseEventArgs e)
        {
            Program.Titret(120);
            KlavyePB.Image = imageList1.Images[1];
            KlavyePB.Update();
            KlavyeBackLBL.BackColor = Color.Navy;
        }

        private void KlavyePB_MouseUp(object sender, MouseEventArgs e)
        {
            KlavyePB.Image = imageList1.Images[0];
            KlavyePB.Update();
            KlavyeBackLBL.BackColor = Color.White;
            inputPanel1.Enabled = !inputPanel1.Enabled;
        }

        private void UserTXT_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                PassTXT.Focus();
        }

        private void PassTXT_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                TamamPB.Focus();
                //TamamPB_MouseUp(null, null);
        }

        private void LoginFRM_Closing(object sender, CancelEventArgs e)
        {
            Calib.SystemLibNet.Api.SysSetUserDefineKeyState(Program.UserDefineKeyState);
            Program.KeyboardLightOnOff(false);
            if (!Program.WWANDurdur()) MessageBox.Show("WWAN kapatılamadı !LF");
        }

        private void LoginFRM_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Program.F1Tusu) MessageBox.Show("F1");
            if (e.KeyCode == Program.F2Tusu) MessageBox.Show("F2");
            if (e.KeyCode == Program.F3Tusu) MessageBox.Show("F3");
            if (e.KeyCode == Program.F4Tusu) MessageBox.Show("F4");
            */
            if (e.KeyCode == Keys.Enter && TamamPB.Focused)
            {
                TamamPB_MouseDown(null, null);
                TamamPB_MouseUp(null, null);
            }
        }
        
        private bool SetFunctionKeys_MS()
        {
            //0x70=F1, 0x71=F2, 0x72=F3, 0x73=F4
            try
            {
                UnregisterFunc1(KeyModifiers.MOD_NONE, 0x72);
                RegisterHotKey(this.Handle, 0x72, KeyModifiers.MOD_NONE|KeyModifiers.MOD_NOREPEAT, 0x72);

                UnregisterFunc1(KeyModifiers.MOD_NONE, 0x73);
                RegisterHotKey(this.Handle, 0x73, KeyModifiers.MOD_NONE|KeyModifiers.MOD_NOREPEAT, 0x73);
                Program.UserDefineKeyState = Calib.SystemLibNet.Api.SysGetUserDefineKeyState() == -1 ? true : false;
                Calib.SystemLibNet.Api.SysSetUserDefineKeyState(false);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private bool TerminalCraddleUstundemi()
        {
            try
            {
                if (Calib.SystemLibNet.Api.SysCheckIOBOX(2000) == 0)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        private bool SetBacklightLevel()
        {
            try
            {
                Calib.SystemLibNet.Api.SysSetBLBattery(4); //Default 6
                Calib.SystemLibNet.Api.SysSetBLExpower(8); //Default 8
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void TamamPB_GotFocus(object sender, EventArgs e)
        {
            using (Graphics g = this.LoginPNL.CreateGraphics())
            {
                Pen pen = new Pen(Color.ForestGreen, 1);
                g.DrawRectangle(pen, TamamPB.Left - 5, TamamPB.Top - 5, TamamPB.Width + 10, TamamPB.Height + 10);
                pen.Dispose();
            }
        }

        private void TamamPB_LostFocus(object sender, EventArgs e)
        {
            using (Graphics g = this.LoginPNL.CreateGraphics())
            {
                Pen pen = new Pen(LoginPNL.BackColor, 1);
                g.DrawRectangle(pen, TamamPB.Left - LoginPNL.Left - 10, TamamPB.Top - LoginPNL.Top - 10, TamamPB.Width + 20, TamamPB.Height + 20);
                pen.Dispose();
            }
        }

        private void TamamPB_MouseDown(object sender, MouseEventArgs e)
        {
            TamamPB.Image = imageList4.Images[1];
            TamamPB.Update();
            Program.Titret(120);
        }

        private void TamamPB_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                string Zaman = DateTime.Now.Year.ToString() + '-' + DateTime.Now.Month.ToString().PadLeft(2, '0') + '-' + DateTime.Now.Day.ToString().PadLeft(2, '0') + ' ' + DateTime.Now.Hour.ToString().PadLeft(2, '0') + ':' + DateTime.Now.Minute.ToString().PadLeft(2, '0') + ':' + DateTime.Now.Second.ToString().PadLeft(2, '0');
                TamamPB.Image = imageList4.Images[0];
                TamamPB.Update();
                ProgramLogoPB.Image = imageList3.Images[1];
                ProgramLogoPB.Update();
                inputPanel1.Enabled = false;
                LoginCheckSimgesiGoster(true);
                FaturaServisi.Url = Program.ServisURL;
                LUS_Servisi.Url = LUSAdresi();

                TerminalKimliginiOku();

                string[] TerminalYetkiDurum = TerminalYetkilimi(CASIO_DevID, MS_DevID, UserTXT.Text, PassTXT.Text).Split('#');
                LoginCheckSimgesiGoster(false);
                
                if (TerminalYetkiDurum.GetUpperBound(0) > 0)
                    if (TerminalYetkiDurum[1] == "-3")
                    {
                        ProgramLogoPB.Image = imageList3.Images[0];
                        ProgramLogoPB.Update();
                        MessageBox.Show("Lisans süresi dolmak üzere. Son " + (TerminalYetkiDurum[2] == "0" ? "" : TerminalYetkiDurum[2] + " ") + "gün");
                    }

                if (TerminalYetkiDurum[0] == "0")
                {
                    OlayLog(Program.TerminalNo, "Giriş başarılı#" + Program.PersonelSicilNo + "#" + Program.PersonelAd + "#" + Program.PersonelSoyad, Zaman);
                    ProgramLogoPB.Image = imageList3.Images[0];
                    ProgramLogoPB.Update();
                    MessageBox.Show("Merhaba " + Program.PersonelAd + " " + Program.PersonelSoyad);
                    MessageBox.Show("Yapacağınız tüm işlemler yasal delil olarak kaydedilecektir.", "UYARI!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    Program.LoginOK = true;
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                else if (TerminalYetkiDurum[0] == "1")
                {
                    OlayLog(Program.TerminalNo, "Giriş başarısız#Terminal yetkisiz#" + CASIO_DevID + "#" + MS_DevID, Zaman);
                    MessageBox.Show("Terminal Yetkisiz");
                    Program.LoginOK = false;
                    DialogResult = DialogResult.No;
                    this.Close();
                    Application.Exit();
                }
                else if (TerminalYetkiDurum[0] == "2")
                {
                    OlayLog(Program.TerminalNo, "Giriş başarısız#Kullanıcı adı veya şifresi hatalı", Zaman);
                    MessageBox.Show("Kullanıcı adı veya şifresi hatalı");
                    Program.LoginOK = false;
                    DialogResult = DialogResult.No;
                    this.Close();
                    Application.Exit();
                }
                else if (TerminalYetkiDurum[0] == "-1")
                {
                    MessageBox.Show("Doğrulama işlemi sonuçlanamadı");
                    MessageBox.Show(CASIO_DevID);
                    MessageBox.Show(MS_DevID);
                    Program.LoginOK = false;
                    DialogResult = DialogResult.No;
                    this.Close();
                    Application.Exit();
                }
                else if (TerminalYetkiDurum[0] == "-2")
                {
                    OlayLog(Program.TerminalNo, "Giriş başarısız#Terminal tarihi sorunlu", Zaman);
                    MessageBox.Show("Terminal tarihi sorunlu");
                    Program.LoginOK = false;
                    DialogResult = DialogResult.No;
                    this.Close();
                    Application.Exit();
                }
                else if (TerminalYetkiDurum[0] == "-4")
                {
                    OlayLog(Program.TerminalNo, "Giriş başarısız#Terminal lisans süresi dolmuş", Zaman);
                    MessageBox.Show("Terminal lisans süresi dolmuş");
                    Program.LoginOK = false;
                    DialogResult = DialogResult.No;
                    this.Close();
                    Application.Exit();
                }
                else if (TerminalYetkiDurum[0] == "-5")
                {
                    OlayLog(Program.TerminalNo, "Giriş başarısız#Terminal lisansı mevcut değil", Zaman);
                    MessageBox.Show("Terminal lisansı mevcut değil");
                    MessageBox.Show(CASIO_DevID);
                    MessageBox.Show(MS_DevID);
                    Program.LoginOK = false;
                    DialogResult = DialogResult.No;
                    this.Close();
                    Application.Exit();
                }
            }
            catch
            {
                LoginCheckSimgesiGoster(false);
                MessageBox.Show("Bağlantı Hatası");
            }
        }

        private void IptalPB_GotFocus(object sender, EventArgs e)
        {
            using (Graphics g = this.LoginPNL.CreateGraphics())
            {
                Pen pen = new Pen(Color.Red, 1);
                g.DrawRectangle(pen, IptalPB.Left - LoginPNL.Left - 10, IptalPB.Top - LoginPNL.Top - 10, IptalPB.Width + 20, IptalPB.Height + 20);
                pen.Dispose();
            }
        }

        private void IptalPB_LostFocus(object sender, EventArgs e)
        {
            using (Graphics g = this.LoginPNL.CreateGraphics())
            {
                Pen pen = new Pen(LoginPNL.BackColor, 1);
                g.DrawRectangle(pen, IptalPB.Left - LoginPNL.Left - 10, IptalPB.Top - LoginPNL.Top - 10, IptalPB.Width + 20, IptalPB.Height + 20);
                pen.Dispose();
            }
        }
        
        private void IptalPB_MouseDown(object sender, MouseEventArgs e)
        {
            IptalPB.Image = imageList4.Images[3];
            IptalPB.Update();
            Program.Titret(120);
        }

        private void IptalPB_MouseUp(object sender, MouseEventArgs e)
        {
            IptalPB.Image = imageList4.Images[2];
            IptalPB.Update();
            this.Close();
        }

        private bool OlayLog(string TerminalNo, string Olay, string Zaman)
        {
            try
            {
                IAsyncResult iar1 = FaturaServisi.BeginOlayLogKaydet(Program.SvcUser, Program.SvcPass, TerminalNo, Olay, Zaman, null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndOlayLogKaydet(iar1);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void LoginCheckSimgesiGoster(bool Durum)
        {
            EkranAktifPasif(Durum, Durum);
            LoginCheckPB.Size = EbatBildirimler;
            LoginCheckPB.BringToFront();
            LoginCheckPB.Top = (int)((this.Height - LoginCheckPB.Height) / 2);
            LoginCheckPB.Left = (int)((this.Width - LoginCheckPB.Width) / 2);
            LoginCheckPB.Visible = Durum;
            this.Invalidate();
            this.Refresh();
            Application.DoEvents();
        }

        public void EkranAktifPasif(bool Durum, bool TouchPanelOff)
        {
            if (Durum)
            {
                PrintScreenPB.BringToFront();
                Bitmap EkranGoruntusu = MakeGrayscale(GetScreenCapture());
                //ApplyContrast(EkranGoruntusu, -50);
                //ApplyBrightness(EkranGoruntusu, 50);
                PrintScreenPB.Left = 0;
                PrintScreenPB.Top = 0;
                PrintScreenPB.BringToFront();
                PrintScreenPB.Size = this.Size;
                PrintScreenPB.Image = EkranGoruntusu;
                PrintScreenPB.Visible = true;
                if (TouchPanelOff)
                    Calib.SystemLibNet.Api.SysTouchPanelOff();
            }
            else
            {
                if (!TouchPanelOff)
                    Calib.SystemLibNet.Api.SysTouchPanelOn();
                PrintScreenPB.Visible = false;
                PrintScreenPB.Image.Dispose();
                PrintScreenPB.SendToBack();
            }
        }
        
        /*************************Ekran görüntüsünü yakalamak için********************************************/
        enum RasterOperation : uint
        {
            SRC_COPY = 0x00CC0020
        }

        [DllImport("coredll.dll")]
        static extern int BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, RasterOperation rasterOperation);

        [DllImport("coredll.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("coredll.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, DeviceCapsIndex nIndex);

        enum DeviceCapsIndex : int
        {
            HORZRES = 8,
            VERTRES = 10,
        }

        public static Size PixelDimensions
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                return new Size(GetDeviceCaps(hdc, DeviceCapsIndex.HORZRES), GetDeviceCaps(hdc, DeviceCapsIndex.VERTRES));
            }
        }

        public static Bitmap GetScreenCapture()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            Size size = PixelDimensions;
            Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format16bppRgb565);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                IntPtr dstHdc = graphics.GetHdc();
                BitBlt(dstHdc, 0, 0, size.Width, size.Height, hdc, 0, 0 + 52, RasterOperation.SRC_COPY);
                graphics.ReleaseHdc(dstHdc);
            }
            return bitmap;
        }
        /*************************Ekran görüntüsünü yakalamak için********************************************/

        /*************************Gri tonlama********************************************/
        public static Bitmap MakeGrayscale(Bitmap original)
        {
            unsafe
            {
                //create an empty bitmap the same size as original
                Bitmap newBitmap = new Bitmap(original.Width, original.Height);

                //lock the original bitmap in memory
                BitmapData originalData = original.LockBits(
                   new Rectangle(0, 0, original.Width, original.Height),
                   ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                //lock the new bitmap in memory
                BitmapData newData = newBitmap.LockBits(
                   new Rectangle(0, 0, original.Width, original.Height),
                   ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                //set the number of bytes per pixel
                int pixelSize = 3;

                for (int y = 0; y < original.Height; y++)
                {
                    //get the data from the original image
                    byte* oRow = (byte*)originalData.Scan0 + (y * originalData.Stride);

                    //get the data from the new image
                    byte* nRow = (byte*)newData.Scan0 + (y * newData.Stride);

                    for (int x = 0; x < original.Width; x++)
                    {
                        //create the grayscale version
                        byte grayScale =
                           (byte)((oRow[x * pixelSize] * .11) + //B
                           (oRow[x * pixelSize + 1] * .59) +  //G
                           (oRow[x * pixelSize + 2] * .3)); //R

                        //set the new image's pixel to the grayscale version
                        nRow[x * pixelSize] = grayScale; //B
                        nRow[x * pixelSize + 1] = grayScale; //G
                        nRow[x * pixelSize + 2] = grayScale; //R
                    }
                }

                //unlock the bitmaps
                newBitmap.UnlockBits(newData);
                original.UnlockBits(originalData);

                return newBitmap;
            }
        }
        /*************************Gri tonlama********************************************/
    }
}