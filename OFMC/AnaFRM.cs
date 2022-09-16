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
using System.Threading;
using System.IO;
using Microsoft.Win32;
using Microsoft.WindowsMobile.Samples.Location;
using Microsoft.WindowsMobile.PocketOutlook;
using Microsoft.WindowsMobile.PocketOutlook.MessageInterception;
using Microsoft.WindowsMobile.Status;
using Calib;
using CalibCs;
using GoogleMaps;

namespace OnlineFaturaMobileClient
{
    public partial class AnaFRM : Form
    {
        [DllImport("coredll.dll")]
        private extern static int GetDeviceUniqueID([In, Out] byte[] appdata, int cbApplictionData, int dwDeviceIDVersion, [In, Out] byte[] deviceIDOuput, out uint pcbDeviceIDOutput);

        [DllImport("coredll.dll", SetLastError = true)] //Ben ekledim
        public static extern int LineTo(IntPtr hdc, int nXEnd, int nYEnd); //Ben ekledim

        [DllImport("coredll.dll", SetLastError = true)] //Ben ekledim
        private static extern IntPtr MoveToEx(IntPtr hdc, int x, int y, IntPtr lpPoint); //Ben ekledim

        [DllImport("coredll", EntryPoint = "ExtTextOutW")]
        static extern bool ExtTextOut(IntPtr hdc, int X, int Y, uint fuOptions, [In] ref RECT lprc, [MarshalAs(UnmanagedType.LPWStr)] string lpString, uint cbCount, [In] int[] lpDx);

        OnlineFaturaServisi.Service1 FaturaServisi = new OnlineFaturaMobileClient.OnlineFaturaServisi.Service1();

        Gps GPSaygiti = new Gps();

        Int32 pError;
        string pMessage;
        string pCodeID = "  ";
        string pAimID = "   ";
        string pSymModifier = "   ";
        Int32 pLength;
        Keys TriggerLeft = (Keys)234;
        Keys TriggerRight = (Keys)233;
        bool KameraAcikmi = false;
        bool TriggerTusuBasildimi = false;
        int SolTriggerFonksiyonu = 0;
        int SagTriggerFonksiyonu = 0;

        int GPSOkumaAraligi = 300 * 1000;
        int GSMSinyaliOkumaAraligi = 3500; //Milisaniye WanGetRssi fonksiyonu minimum 3 san aralıklarla çağrılmalı 
        const string HataIconAsterisk = "Asterisk";
        const string HataIconExclamation = "Exclamation";
        const string HataIconHand = "Hand";
        const string HataIconNone = "None";
        const string HataIconQuestion = "Question";
        Size EbatBildirimler = new Size(139, 138);
        Size EbatFaturaPB = new Size(142, 260);
        Size EbatFotoZoomPNL = new Size(345, 524);
        string FotograflarKlasoru = "-1";

        DateTime SonSinyalZamani = DateTime.Now;
        int GSMSinyalGucuYuzde = -1;

        private MessageInterceptor SMSEvents = new MessageInterceptor(InterceptionAction.NotifyAndDelete, true);
        private MessageCondition SMSFilter = new MessageCondition();

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(Rectangle rc)
            {
                Left = rc.Left; Top = rc.Top; Right = rc.Right; Bottom = rc.Bottom;
            }
            public RECT(int l, int t, int r, int b)
            {
                Left = l; Top = t; Right = r;
                Bottom = b;
            }
            public override string ToString()
            {
                return String.Format("RECT({0},{1})-({2},{3})", Left, Top, Right, Bottom);
            }
        }

        public struct FaturaBilgileri
        {
            public string TuketiciTuru;
            public string AboneNo;
            public string AboneAd;
            public string Adres;
            public string FaturaNo;
            public string TebligTarihiSaati;
            public string IlkOkumaTarihi;
            public string SonOkumaTarihi;
            public string IlkEndeks;
            public string SonEndeks;
            public string FaturaGunSayisi;
            public string TerminalNo;
            public string SayacNo;
            public string SayacBasinci;
            public string SayactanOkunanHacim;
            public string BasincDuzeltmeKatsayisi;
            public string FatEsDuzHacim;
            public string OrtFiilUstIsilDeg;
            public string TukEnerMik;
            public string OrtPerSatFiyati;
            public string GecikmeBedeli;
            public string GecikmeKDV;
            public string Guvence;
            public string OzelHizmetBedeli;
            public string OzelHizmetKDV;
            public string TuketimBedeli;
            public string TuketimKDV;
            public string Tenzilat;
            public string Yuvarlama;
            public string IlkOdemeTarihi;
            public string SonOdemeTarihi;
            public string Toplam;
            public string KDVOrani;
            public string ParaBirimi;
        }
        FaturaBilgileri FaturaBilgi = new FaturaBilgileri();

        private static string m_sPrinter = "BuiltIn";
        private static string m_sPort = "BuiltIn";
        private static int m_hPrinter;
        private static int m_hDC;
        private static Cp780LibCS.CPDEVMODE m_CpDevMode, m_CpDevMode_Null;
        private static Cp780LibCS.CPDOCINFO m_CpDocInfo;

        DataTable dtBorclar = new DataTable("BorclarTablosu");
        DataTable dtBasilacakMesajlar = new DataTable("BasilacakMesajlarTablosu");
        DataTable dtTahakkukluFaturalar = new DataTable("SuretFaturalarTablosu");
        int TahakkukluFaturalarGRDSatir = 0;

        float KarakterEn_BorcGRD = 11, KarakterYukseklik_BorcGRD;
        float KarakterEn_TahakkukGRD = 11, KarakterYukseklik_TahakkukGRD; 
        string[] BasilacakHataKodu = new string[100];
        string[] BasilacakHataMesaji = new string[100];
        string[] YetkiliGSM = new string[100];
        char Ussu3 = (char)179;

        public AnaFRM()
        {
            if (!Program.LoginOK) Application.Exit();
            InitializeComponent();
            //GPSEventRegister();
        }

        private void AnaFRM_Load(object sender, EventArgs e)
        {
            if (!Program.LoginOK) Application.Exit();

            SetKeyboardBacklightStateIcon();
            SetUserDefinedKeySate(false);

            LisansliModullerTXT.Text = Program.LisansliModuller;

            if (!AyarlariYukleRegistry())
            {
                MessageBox.Show("Ayarlar okunurken hata oluştu !");
            }

            if (!ResimKlasoruOlustur())
            {
                MessageBox.Show("Fotoğraflar klasörü sorunlu: " + FotograflarKlasoru);
                FotoCekPB.Visible = false;
                FotoCekLBL.Visible = false;
            }
            if (!Program.WWANCalistir())
            {
                MessageBox.Show("WWAN çalıştırılamadı !", "KRİTİK HATA", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                Application.Exit();
            }
            if (!SMSHazirla())
            {
                //MessageBox.Show("SMS servisi ayarlanamadı !", "KRİTİK HATA", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                //Application.Exit();
            }
            if (!ServisAyarla())
            {
                MessageBox.Show("Servis ayarlanamadı !", "KRİTİK HATA", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                Application.Exit();
            }
            if (!YetkiliGSMListesiniYukle())
            {
                MessageBox.Show("Yetkili GSM'ler okunamadı !", "KRİTİK HATA", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                Application.Exit();
            }
            if (!BildirimListesiniYukle())
            {
                MessageBox.Show("Bildirim listesi okunamadı !", "KRİTİK HATA", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                Application.Exit();
            }
            if (!TablolariOlustur())
            {
                MessageBox.Show("Tablolar oluşturulamadı !", "KRİTİK HATA", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                Application.Exit();
            }
            if (!ImagerOnOff(true))
            {
                MessageBox.Show("Barkod okuyucu çalıştırılamadı !", "KRİTİK HATA", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                Application.Exit();
            }
            AboneNoTXT.Focus();
            SayacNoTXT.Focus();
        }

        private int WWANCekiyormu()
        {
            try
            {
                if (Math.Abs(SonSinyalZamani.Subtract(DateTime.Now).TotalMilliseconds) >= GSMSinyaliOkumaAraligi)
                {
                    SonSinyalZamani = DateTime.Now;
                    WANGPRSLibNet.WanRssi GSMSinyalGucu;
                    WANGPRSLibNet.Api.WanGetRssi(out GSMSinyalGucu);
                    if ((object)GSMSinyalGucu != null)
                    {
                        double SinyalCur = GSMSinyalGucu.SignalStrength;
                        double SinyalMin = GSMSinyalGucu.SignalStrengthMin;
                        double SinyalMax = GSMSinyalGucu.SignalStrengthMax;
                        GSMSinyalGucuYuzde = Convert.ToInt32(((SinyalCur - SinyalMin) / (SinyalMax - SinyalMin)) * 100);
                        //listBox1.Items.Add(GSMSinyalGucuYuzde.ToString() + "/" + SinyalCur.ToString() + "/" + SinyalMin.ToString() + "/" + SinyalMax.ToString());
                    }
                }
                return GSMSinyalGucuYuzde;
            }
            catch
            {
                return -1;
            }
        }

        private bool ServisAyarla()
        {
            try
            {
                FaturaServisi.Url = Program.ServisURL;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TablolariOlustur()
        {
            try
            {
                dtBorclar.Columns.Add("FaturaNo", Type.GetType("System.String"));
                dtBorclar.Columns.Add("Tutar", Type.GetType("System.String"));
                dtBorclar.Columns.Add("FaizTutar", Type.GetType("System.String"));
                dtBorclar.Columns.Add("SonOdemeTarihi", Type.GetType("System.String"));

                dtTahakkukluFaturalar.Columns.Add("GazAboneNo", Type.GetType("System.String"));
                dtTahakkukluFaturalar.Columns.Add("GazSayacNo", Type.GetType("System.String"));
                dtTahakkukluFaturalar.Columns.Add("FaturaSeri", Type.GetType("System.String"));
                dtTahakkukluFaturalar.Columns.Add("FaturaNo", Type.GetType("System.String"));
                dtTahakkukluFaturalar.Columns.Add("SonOkumaTarihi", Type.GetType("System.String"));
                dtTahakkukluFaturalar.Columns.Add("SonEndeks", Type.GetType("System.String"));
                dtTahakkukluFaturalar.Columns.Add("Sarfiyat", Type.GetType("System.String"));
                dtTahakkukluFaturalar.Columns.Add("ToplamTL", Type.GetType("System.String"));
                dtTahakkukluFaturalar.Columns.Add("AdSoyad", Type.GetType("System.String"));

                dtBasilacakMesajlar.Columns.Add("Mesaj", Type.GetType("System.String"));

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ImagerOnOff(bool State)
        {
            try
            {
                if (State)
                {
                    string pFileName = "\\FlashDisk\\System Settings\\IMGSet.ini";
                    Calib.IMGLibNet.Api.IMGInit();						// IMGDRV open
                    Calib.IMGLibNet.Api.IMGLoadConfigFile(pFileName);	// ini File read default value set
                    Calib.IMGLibNet.Api.IMGConnect();					// IMGDRV mode will be ini File vallue 

                    for (int i = 0; i < 512; i++)
                        pMessage = pMessage + " ";  // allocate 512 pcs space for scanning data
                    return true;
                }
                else
                {
                    Calib.IMGLibNet.Api.IMGDisconnect();
                    Calib.IMGLibNet.Api.IMGDeinit();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void BilgiAl()
        {
            try
            {
                if (WWANCekiyormu() <= 10) SinyalSeviyesiDusuk(true);
                if (!SayacNoGecerlimi(SayacNoTXT.Text) && !AboneNoGecerlimi(AboneNoTXT.Text))
                {
                    MessageBox.Show("Sayaç no/Abone no geçersiz");
                    return;
                }
                AraniyorSimgesiGoster(true);
                IAsyncResult iar1 = FaturaServisi.BeginAboneBilgi(Program.SvcUser, Program.SvcPass, SayacNoTXT.Text.Trim(), (TriggerTusuBasildimi ? "" : AboneNoTXT.Text.Trim().ToUpper()), null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndAboneBilgi(iar1);

                if (xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml != "YOK")
                {
                    EkraniTemizle();
                    MessageBoxIcon MesajSimgesi = MessageBoxIcon.None;
                    switch (xe.SelectSingleNode("AboneBilgileri/HataIcon").InnerXml)
                    {
                        case HataIconAsterisk:
                            MesajSimgesi = MessageBoxIcon.Asterisk;
                            break;
                        case HataIconExclamation:
                            MesajSimgesi = MessageBoxIcon.Exclamation;
                            break;
                        case HataIconHand:
                            MesajSimgesi = MessageBoxIcon.Hand;
                            break;
                        case HataIconQuestion:
                            MesajSimgesi = MessageBoxIcon.Question;
                            break;
                        case HataIconNone:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                        default:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                    }
                    AraniyorSimgesiGoster(false);
                    MessageBox.Show(xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml, xe.SelectSingleNode("AboneBilgileri/HataBaslik").InnerXml, MessageBoxButtons.OK, MesajSimgesi, MessageBoxDefaultButton.Button1);
                    AboneNoTXT.Focus();
                    SayacNoTXT.Focus();
                    return;
                }

                TuketiciTuruTXT.Text = xe.SelectSingleNode("AboneBilgileri/TuketiciTuru").InnerXml;
                SayacNoTXT.Text = xe.SelectSingleNode("AboneBilgileri/SayacNo").InnerXml;
                AboneNoTXT.Text = xe.SelectSingleNode("AboneBilgileri/AboneNo").InnerXml;
                IsimTXT.Text = xe.SelectSingleNode("AboneBilgileri/Ad").InnerXml;
                AdresTXT.Text = xe.SelectSingleNode("AboneBilgileri/Adres").InnerXml;
                TesisatciTXT.Text = xe.SelectSingleNode("AboneBilgileri/Firma").InnerXml;

                //Ödenmemiş fatura bilgilerini okumak için 2 farklı yol======================================
                //1. Yol: Ödenmemiş faturaların bilgilerini okuma için. (Bu tercih edilmeli çünkü 2. yol tüm "Fatura" taglarını okuyor, 1. yol ise 
                //"OdenmemisFaturaBilgileri/Fatura" sayesinde başka "Fatura" tagları olsa bile OdenmemisFaturaBilgileri içindeki "Fatura"
                //taglarını okuyor.

                XmlNodeList nl_1 = xe.SelectNodes("OdenmemisFaturaBilgileri/Fatura");
                //MessageBox.Show(nl_1.Count.ToString());
                float SeriBoy = ((float)"Fatura No".Length + 1) * KarakterEn_BorcGRD;
                float TutarBoy = ((float)"Tutar (TL)".Length + 1) * KarakterEn_BorcGRD;
                float FaizTutarBoy = ((float)"Faiz (TL)".Length + 1) * KarakterEn_BorcGRD;
                float TarihBoy = ((float)"Son Tarih".Length + 1) * KarakterEn_BorcGRD;
                dtBorclar.Clear();
                foreach (XmlNode xn in nl_1)
                {
                    string Seri = xn.SelectSingleNode("Seri").InnerXml;
                    if (((float)Seri.Length + 1) * KarakterEn_BorcGRD > SeriBoy)
                        SeriBoy = ((float)Seri.Length + 1) * KarakterEn_BorcGRD;
                    string Tutar = xn.SelectSingleNode("Tutar").InnerXml;
                    if (((float)Tutar.Length + 1) * KarakterEn_BorcGRD > TutarBoy)
                        TutarBoy = ((float)Tutar.Length + 1) * KarakterEn_BorcGRD;
                    string FaizTutar = xn.SelectSingleNode("FaizTutar").InnerXml;
                    if (((float)FaizTutar.Length + 1) * KarakterEn_BorcGRD > FaizTutarBoy)
                        FaizTutarBoy = ((float)FaizTutar.Length + 1) * KarakterEn_BorcGRD;
                    string Tarih = xn.SelectSingleNode("Tarih").InnerXml;
                    if (((float)Tarih.Length + 1) * KarakterEn_BorcGRD > TarihBoy)
                        TarihBoy = ((float)Tarih.Length + 1) * KarakterEn_BorcGRD;

                    DataRow drBorc = dtBorclar.NewRow();
                    drBorc["FaturaNo"] = Seri;
                    drBorc["Tutar"] = Tutar;
                    drBorc["FaizTutar"] = FaizTutar;
                    drBorc["SonOdemeTarihi"] = Tarih;
                    dtBorclar.Rows.Add(drBorc);
                }
                if (nl_1.Count > 0) BorclariGoster(SeriBoy, TutarBoy, FaizTutarBoy, TarihBoy);


                //2. Yol: Ödenmemiş faturaların bilgilerini okuma için.
                /*XmlNodeList nl1 = xe.GetElementsByTagName("Fatura");
                System.Collections.IEnumerator ienum = nl1.GetEnumerator();
                MessageBox.Show(nl1.Count.ToString()); //Ödenmemiş fatura sayısı
                while (ienum.MoveNext() && nl1.Count>0)
                {
                    XmlNode OdenmemisFatura = (XmlNode)ienum.Current;
                    MessageBox.Show(OdenmemisFatura.SelectSingleNode("Seri").InnerXml);
                    MessageBox.Show(OdenmemisFatura.SelectSingleNode("Tarih").InnerXml);
                }*/
                //============================================================================================

                /*
                XmlNodeList nl1 = xe.ChildNodes;
                foreach (XmlNode xn in nl1)
                {
                    //MessageBox.Show(xn.OuterXml);
                    //foreach (XmlNode xn2 in xn)
                        MessageBox.Show(nl1.);
                }*/

                /*
                XmlNodeList nl = xe.ChildNodes;
                foreach (XmlNode xn in nl)
                {
                    //MessageBox.Show(xn.OuterXml);
                    foreach (XmlNode xn2 in xn)
                        MessageBox.Show(xn2.InnerXml);
                    MessageBox.Show(xn.InnerXml);
                }*/

                //AboneNoTXT.Text = xe.Descendants("AboneNo").First().Value;
                //AboneAdTXT.Text = xe.Descendants("Ad").First().Value;
                AboneNoTXT.Focus();
                SayacNoTXT.Focus();
                AraniyorSimgesiGoster(false);
            }
            catch
            {
                AraniyorSimgesiGoster(false);
                MessageBox.Show("Bilgi sorgulama sırasında hata oldu");
            }
        }

        private void FaturaKesiliyorSimgesiGoster(bool Durum)
        {
            EkranAktifPasif(Durum, Durum);
            FaturaPB.Size = EbatFaturaPB;
            FaturaPB.BringToFront();
            FaturaPB.Top = (int)((TemelBilgilerTAB.Height - FaturaPB.Height) / 2);
            FaturaPB.Left = (int)((TemelBilgilerTAB.Width - FaturaPB.Width) / 2);
            FaturaPB.Visible = Durum;
            this.Invalidate();
            this.Refresh();
            Application.DoEvents();
        }

        private void YazdiriliyorSimgesiGoster(bool Durum)
        {
            EkranAktifPasif(Durum, Durum);
            TekrarBasiliyorPB.Size = EbatBildirimler;
            TekrarBasiliyorPB.BringToFront();
            TekrarBasiliyorPB.Top = (int)((TemelBilgilerTAB.Height - TekrarBasiliyorPB.Height) / 2);
            TekrarBasiliyorPB.Left = (int)((TemelBilgilerTAB.Width - TekrarBasiliyorPB.Width) / 2);
            TekrarBasiliyorPB.Visible = Durum;
            this.Invalidate();
            this.Refresh();
            Application.DoEvents();
        }

        private void AraniyorSimgesiGoster(bool Durum)
        {
            EkranAktifPasif(Durum, Durum);
            AraniyorPB.Size = EbatBildirimler;
            AraniyorPB.BringToFront();
            AraniyorPB.Top = (int)((TemelBilgilerTAB.Height - AraniyorPB.Height) / 2);
            AraniyorPB.Left = (int)((TemelBilgilerTAB.Width - AraniyorPB.Width) / 2);
            AraniyorPB.Visible = Durum;
            this.Invalidate();
            this.Refresh();
            Application.DoEvents();
        }

        private void BildirimlerYenileniyorSimgesiGoster(bool Durum)
        {
            EkranAktifPasif(Durum, Durum);
            BildirimListesiYenileniyorPB.Size = EbatBildirimler;
            BildirimListesiYenileniyorPB.BringToFront();
            BildirimListesiYenileniyorPB.Top = (int)((TemelBilgilerTAB.Height - BildirimListesiYenileniyorPB.Height) / 2);
            BildirimListesiYenileniyorPB.Left = (int)((TemelBilgilerTAB.Width - BildirimListesiYenileniyorPB.Width) / 2);
            BildirimListesiYenileniyorPB.Visible = Durum;
            this.Invalidate();
            this.Refresh();
            Application.DoEvents();
        }

        private void SuretFaturalarListeleniyorSimgesiGoster(bool Durum)
        {
            EkranAktifPasif(Durum, Durum);
            SuretFaturalarListeleniyorPB.Size = EbatBildirimler;
            SuretFaturalarListeleniyorPB.BringToFront();
            SuretFaturalarListeleniyorPB.Top = (int)((TemelBilgilerTAB.Height - BildirimListesiYenileniyorPB.Height) / 2);
            SuretFaturalarListeleniyorPB.Left = (int)((TemelBilgilerTAB.Width - BildirimListesiYenileniyorPB.Width) / 2);
            SuretFaturalarListeleniyorPB.Visible = Durum;
            this.Invalidate();
            this.Refresh();
            Application.DoEvents();
        }

        private void ZoomlaniyorSimgesiGoster(bool Durum)
        {
            EkranAktifPasif(Durum, false);
            FotoZoomPNL.Size = EbatFotoZoomPNL;
            FotoZoomPNL.BringToFront();
            FotoZoomPNL.Top = (int)((AnaTAB.Height - FotoZoomPNL.Height) / 2);
            FotoZoomPNL.Left = (int)((AnaTAB.Width - FotoZoomPNL.Width) / 2);
            FotoZoomPNL.Visible = Durum;
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
                PrintScreenPB.Size = AnaTAB.Size;
                PrintScreenPB.Image = EkranGoruntusu;
                PrintScreenPB.Visible = true;
                if (TouchPanelOff)
                    Calib.SystemLibNet.Api.SysTouchPanelOff();
                else
                    Calib.SystemLibNet.Api.SysTouchPanelOn();
            }
            else
            {
                if (TouchPanelOff)
                    Calib.SystemLibNet.Api.SysTouchPanelOff();
                else
                    Calib.SystemLibNet.Api.SysTouchPanelOn();
                PrintScreenPB.Visible = false;
                PrintScreenPB.Image.Dispose();
                PrintScreenPB.SendToBack();
            }
        }

        private void FaturaKesPB_MouseDown(object sender, MouseEventArgs e)
        {
            FaturaKesPB.Image = imageList3.Images[1];
            FaturaKesPB.Update();
            Program.Titret(120);
        }

        private void FaturaKesPB_MouseUp(object sender, MouseEventArgs e)
        {
            FaturaKesPB.Image = imageList3.Images[0];
            FaturaKesPB.Update();
            FaturaKes();
        }

        private void TekrarBasPB_MouseDown(object sender, MouseEventArgs e)
        {
            TekrarBasPB.Image = imageList3.Images[3];
            TekrarBasPB.Update();
            Program.Titret(120);
        }

        private void TekrarBasPB_MouseUp(object sender, MouseEventArgs e)
        {
            TekrarBasPB.Image = imageList3.Images[2];
            TekrarBasPB.Update();
            if ((object)FaturaBilgi.AboneNo == null || FaturaBilgi.AboneNo.Trim() == "") { MessageBox.Show("Kayıtlı fatura bulunamadı"); return; }
            FaturaYaz();
        }

        private void AboneBilgisiPB_MouseDown(object sender, MouseEventArgs e)
        {
            AboneBilgisiPB.Image = imageList3.Images[5];
            AboneBilgisiPB.Update();
            Program.Titret(120);
        }

        private void AboneBilgisiPB_MouseUp(object sender, MouseEventArgs e)
        {
            AboneBilgisiPB.Image = imageList3.Images[4];
            AboneBilgisiPB.Update();
            TriggerTusuBasildimi = false;
            BilgiAl();
        }

        /*
        private void TuslarinEtrafiniTemizle()
        {
            using (Graphics g = this.FaturaKesTAB.CreateGraphics())
            {
                Pen pen = new Pen(Color.White, 10);
                g.DrawRectangle(pen, FaturaKesPB.Left, FaturaKesPB.Top, FaturaKesPB.Width, FaturaKesPB.Height);
                g.DrawRectangle(pen, TekrarBasPB.Left, TekrarBasPB.Top, TekrarBasPB.Width, TekrarBasPB.Height);
                g.DrawRectangle(pen, AboneBilgisiPB.Left, AboneBilgisiPB.Top, AboneBilgisiPB.Width, AboneBilgisiPB.Height);
                pen.Dispose();
            }

            using (Graphics g = this.BildirimTAB.CreateGraphics())
            {
                Pen pen = new Pen(Color.White, 10);
                g.DrawRectangle(pen, BildirimListeYenilePB.Left, BildirimListeYenilePB.Top, BildirimListeYenilePB.Width, BildirimListeYenilePB.Height);
                g.DrawRectangle(pen, BildirimBasPB.Left, BildirimBasPB.Top, BildirimBasPB.Width, BildirimBasPB.Height);
                pen.Dispose();
            }

            using (Graphics g = this.FotoTAB.CreateGraphics())
            {
                Pen pen = new Pen(Color.White, 10);
                g.DrawRectangle(pen, KameraAcKapatPB.Left, KameraAcKapatPB.Top, KameraAcKapatPB.Width, KameraAcKapatPB.Height);
                g.DrawRectangle(pen, FotoCekPB.Left, FotoCekPB.Top, FotoCekPB.Width, FotoCekPB.Height);
                pen.Dispose();
            }
        }
        */

        private void FaturaKes()
        {
            try
            {
                if (WWANCekiyormu() <= 10) SinyalSeviyesiDusuk(true);
                if (!SayacNoGecerlimi(SayacNoTXT.Text) && !AboneNoGecerlimi(AboneNoTXT.Text))
                {
                    MessageBox.Show("Sayaç no/Abone no geçersiz");
                    return;
                }
                if (EndeksTXT.Text.Trim() == "")
                {
                    MessageBox.Show("Endeks girmelisiniz");
                    return;
                }
                if ((object)FaturaBilgi.SayacNo != null)
                    if (Int64.Parse(SayacNoTXT.Text.Trim()) == Int64.Parse(FaturaBilgi.SayacNo))
                    {
                        string Secim = EvetHayirFRM.ShowBox("Son fatura zaten bu aboneye ait, yeniden fatura kesmek için EVET'e, son faturayı tekrar yazdırmak için önce HAYIR'a sonra F2'ye basınız", "Faturalandırma Seçimi", "Evet", "Hayır");
                        this.Focus();
                        if (Secim == "RIGHT") return;
                    }
 
                FaturaKesiliyorSimgesiGoster(true);
                IAsyncResult iar1 = FaturaServisi.BeginFaturaKes(Program.SvcUser, Program.SvcPass, SayacNoTXT.Text.Trim(), AboneNoTXT.Text.Trim().ToUpper(), EndeksTXT.Text.Trim(), Program.TerminalNo.Trim(), Program.PersonelSicilNo.Trim(), Program.PersonelKod.Trim(), FaturaMesajTXT.Text.Trim(), "0", null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndFaturaKes(iar1);

                if (xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml != "YOK")
                {
                    EkraniTemizle();
                    MessageBoxIcon MesajSimgesi = MessageBoxIcon.None;
                    switch (xe.SelectSingleNode("AboneBilgileri/HataIcon").InnerXml)
                    {
                        case HataIconAsterisk:
                            MesajSimgesi = MessageBoxIcon.Asterisk;
                            break;
                        case HataIconExclamation:
                            MesajSimgesi = MessageBoxIcon.Exclamation;
                            break;
                        case HataIconHand:
                            MesajSimgesi = MessageBoxIcon.Hand;
                            break;
                        case HataIconQuestion:
                            MesajSimgesi = MessageBoxIcon.Question;
                            break;
                        case HataIconNone:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                        default:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                    }
                    FaturaKesiliyorSimgesiGoster(false);
                    MessageBox.Show(xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml, xe.SelectSingleNode("AboneBilgileri/HataBaslik").InnerXml, MessageBoxButtons.OK, MesajSimgesi, MessageBoxDefaultButton.Button1);
                    AboneNoTXT.Focus();
                    SayacNoTXT.Focus();
                    return;
                }

                FaturaMesajTXT.Text = "";
                TesisatciTXT.Text = xe.SelectSingleNode("AboneBilgileri/Firma").InnerXml;

                FaturaBilgi.TuketiciTuru = xe.SelectSingleNode("AboneBilgileri/TuketiciTuru").InnerXml;
                FaturaBilgi.AboneNo = xe.SelectSingleNode("AboneBilgileri/AboneNo").InnerXml;
                FaturaBilgi.AboneAd = xe.SelectSingleNode("AboneBilgileri/Ad").InnerXml;
                FaturaBilgi.Adres = xe.SelectSingleNode("AboneBilgileri/Adres").InnerXml;
                FaturaBilgi.FaturaNo = xe.SelectSingleNode("AboneBilgileri/FaturaNo").InnerXml;
                FaturaBilgi.TebligTarihiSaati = xe.SelectSingleNode("AboneBilgileri/TebligTarihiSaati").InnerXml;
                FaturaBilgi.IlkOkumaTarihi = xe.SelectSingleNode("AboneBilgileri/SonOkumaTarihi").InnerXml;
                FaturaBilgi.SonOkumaTarihi = xe.SelectSingleNode("AboneBilgileri/FaturaTarihi").InnerXml;
                FaturaBilgi.IlkEndeks = xe.SelectSingleNode("AboneBilgileri/IlkEndeks").InnerXml;
                FaturaBilgi.SonEndeks = xe.SelectSingleNode("AboneBilgileri/SonEndeks").InnerXml;
                FaturaBilgi.FaturaGunSayisi = xe.SelectSingleNode("AboneBilgileri/FaturaGunSayisi").InnerXml;
                FaturaBilgi.TerminalNo = xe.SelectSingleNode("AboneBilgileri/TerminalNo").InnerXml;
                FaturaBilgi.SayacNo = xe.SelectSingleNode("AboneBilgileri/SayacNo").InnerXml;
                FaturaBilgi.SayacBasinci = xe.SelectSingleNode("AboneBilgileri/SayacBasinci").InnerXml;
                FaturaBilgi.SayactanOkunanHacim = xe.SelectSingleNode("AboneBilgileri/SayactanOkunanHacim").InnerXml;
                FaturaBilgi.BasincDuzeltmeKatsayisi = xe.SelectSingleNode("AboneBilgileri/BasincDuzeltmeKatsayisi").InnerXml;
                FaturaBilgi.FatEsDuzHacim = xe.SelectSingleNode("AboneBilgileri/FaturayaEsasDuzeltilmisHacim").InnerXml;
                FaturaBilgi.OrtFiilUstIsilDeg = xe.SelectSingleNode("AboneBilgileri/OrtalamaFiiliUstIsilDeger").InnerXml;
                FaturaBilgi.TukEnerMik = xe.SelectSingleNode("AboneBilgileri/TuketilenEnerjiMiktari").InnerXml;
                FaturaBilgi.OrtPerSatFiyati = xe.SelectSingleNode("AboneBilgileri/OrtalamaPerakendeSatisFiyati").InnerXml;
                FaturaBilgi.GecikmeBedeli = xe.SelectSingleNode("AboneBilgileri/GecikmeBedeli").InnerXml;
                FaturaBilgi.GecikmeKDV = xe.SelectSingleNode("AboneBilgileri/GecikmeBedeliKDV").InnerXml;
                FaturaBilgi.Guvence = xe.SelectSingleNode("AboneBilgileri/GuvenceDamgaVergisi").InnerXml;
                FaturaBilgi.OzelHizmetBedeli = xe.SelectSingleNode("AboneBilgileri/OzelHizmetBedeli").InnerXml;
                FaturaBilgi.OzelHizmetKDV = xe.SelectSingleNode("AboneBilgileri/OzelHizmetBedeliKDV").InnerXml;
                FaturaBilgi.TuketimBedeli = xe.SelectSingleNode("AboneBilgileri/TuketimBedeli").InnerXml;
                FaturaBilgi.TuketimKDV = xe.SelectSingleNode("AboneBilgileri/TuketimBedeliKDV").InnerXml;
                FaturaBilgi.Tenzilat = xe.SelectSingleNode("AboneBilgileri/TenzilatIlave").InnerXml;
                FaturaBilgi.Yuvarlama = xe.SelectSingleNode("AboneBilgileri/Yuvarlama").InnerXml;
                FaturaBilgi.IlkOdemeTarihi = xe.SelectSingleNode("AboneBilgileri/IlkOdemeTarihi").InnerXml;
                FaturaBilgi.SonOdemeTarihi = xe.SelectSingleNode("AboneBilgileri/SonOdemeTarihi").InnerXml;
                FaturaBilgi.Toplam = xe.SelectSingleNode("AboneBilgileri/Toplam").InnerXml;
                FaturaBilgi.KDVOrani = xe.SelectSingleNode("AboneBilgileri/KDVOrani").InnerXml;
                FaturaBilgi.ParaBirimi = xe.SelectSingleNode("AboneBilgileri/ParaBirimi").InnerXml;

                TuketiciTuruTXT.Text = FaturaBilgi.TuketiciTuru;
                AboneNoTXT.Text = FaturaBilgi.AboneNo;
                SayacNoTXT.Text = FaturaBilgi.SayacNo;
                IsimTXT.Text = FaturaBilgi.AboneAd;
                //SonEndeksTXT.Text = FaturaBilgi.SonEndeks;
                AdresTXT.Text = FaturaBilgi.Adres;

                XmlNodeList nl_AcikFaturalar = xe.SelectNodes("OdenmemisFaturaBilgileri/Fatura");
                //MessageBox.Show(nl_1.Count.ToString());
                float SeriBoy = ((float)"Fatura No".Length + 1) * KarakterEn_BorcGRD;
                float TutarBoy = ((float)"Tutar (TL)".Length + 1) * KarakterEn_BorcGRD;
                float FaizTutarBoy = ((float)"Faiz (TL)".Length + 1) * KarakterEn_BorcGRD;
                float TarihBoy = ((float)"Son Tarih".Length + 1) * KarakterEn_BorcGRD;
                dtBorclar.Clear();
                foreach (XmlNode xn in nl_AcikFaturalar)
                {
                    string Seri = xn.SelectSingleNode("Seri").InnerXml;
                    if (((float)Seri.Length + 1) * KarakterEn_BorcGRD > SeriBoy)
                        SeriBoy = ((float)Seri.Length + 1) * KarakterEn_BorcGRD;
                    string Tutar = xn.SelectSingleNode("Tutar").InnerXml;
                    if (((float)Tutar.Length + 1) * KarakterEn_BorcGRD > TutarBoy)
                        TutarBoy = ((float)Tutar.Length + 1) * KarakterEn_BorcGRD;
                    string FaizTutar = xn.SelectSingleNode("FaizTutar").InnerXml;
                    if (((float)FaizTutar.Length + 1) * KarakterEn_BorcGRD > FaizTutarBoy)
                        FaizTutarBoy = ((float)FaizTutar.Length + 1) * KarakterEn_BorcGRD;
                    string Tarih = xn.SelectSingleNode("Tarih").InnerXml;
                    if (((float)Tarih.Length + 1) * KarakterEn_BorcGRD > TarihBoy)
                        TarihBoy = ((float)Tarih.Length + 1) * KarakterEn_BorcGRD;

                    DataRow drBorc = dtBorclar.NewRow();
                    drBorc["FaturaNo"] = Seri;
                    drBorc["Tutar"] = Tutar;
                    drBorc["FaizTutar"] = FaizTutar;
                    drBorc["SonOdemeTarihi"] = Tarih;
                    dtBorclar.Rows.Add(drBorc);
                }
                if (nl_AcikFaturalar.Count > 0) BorclariGoster(SeriBoy, TutarBoy, FaizTutarBoy, TarihBoy);

                XmlNodeList nl_BasilacakMesajlar = xe.SelectNodes("BasilacakMesajlar/MesajBilgileri");
                dtBasilacakMesajlar.Clear();
                foreach (XmlNode xn in nl_BasilacakMesajlar)
                {
                    DataRow drMesaj = dtBasilacakMesajlar.NewRow();
                    drMesaj["Mesaj"] = xn.SelectSingleNode("Mesaj").InnerXml;
                    dtBasilacakMesajlar.Rows.Add(drMesaj);

                }

                FaturaKesiliyorSimgesiGoster(false);
                FaturaYaz();
                //Uyar("Fatura Kesildi", true);
                AboneNoTXT.Focus();
                SayacNoTXT.Focus();
            }
            catch(Exception Ex)
            {
                string h1 = (object)Ex != null ? "FaturaKes" + Ex.ToString() : "FaturaKes" + "h1";
                string h2 = (object)Ex.InnerException != null ? Ex.InnerException.Message : "h2";
                string h3 = (object)Ex.Message != null ? Ex.Message : "h3";
                HataDosyaKaydet(h1, h2, h3);
                FaturaKesiliyorSimgesiGoster(false);
                MessageBox.Show("Fatura kesilirken hata oldu");
            }
        }

        private void BorclarGRD_Paint(object sender, PaintEventArgs e)
        {
            SizeF YaziBoyu = e.Graphics.MeasureString("A", BorclarGRD.Font);
            KarakterEn_BorcGRD = YaziBoyu.Width;
            KarakterYukseklik_BorcGRD = YaziBoyu.Height;
        }

        public void BorclariGoster(float SeriBoy, float TutarBoy, float FaizTutarBoy, float TarihBoy)
        {
            DataGridTableStyle tableStyle = new DataGridTableStyle();
            tableStyle.MappingName = dtBorclar.TableName;

            GridColumnStylesCollection columnStyles = tableStyle.GridColumnStyles;
            columnStyles.Clear();

            DataGridTextBoxColumn columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "FaturaNo";
            columnStyle.HeaderText = "Fatura No";
            columnStyle.Width = (int)SeriBoy;
            columnStyles.Add(columnStyle);

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "Tutar";
            columnStyle.HeaderText = "Tutar (TL)";
            columnStyle.Width = (int)TutarBoy;
            columnStyles.Add(columnStyle);

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "SonOdemeTarihi";
            columnStyle.HeaderText = "Son Tarih";
            columnStyle.Width = (int)TarihBoy;
            columnStyles.Add(columnStyle);

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "FaizTutar";
            columnStyle.HeaderText = "Faiz (TL)";
            columnStyle.Width = (int)FaizTutarBoy;
            columnStyles.Add(columnStyle);

            GridTableStylesCollection tableStyles = BorclarGRD.TableStyles;
            tableStyles.Clear();
            tableStyles.Add(tableStyle);

            BorclarGRD.PreferredRowHeight = (int)KarakterYukseklik_BorcGRD;
            BorclarGRD.RowHeadersVisible = false;
            BorclarGRD.DataSource = dtBorclar;

            dtBorclar.Dispose();
            tableStyle.Dispose();
        }

        public void EkraniTemizle()
        {
            SayacNoTXT.Text = "";
            EndeksTXT.Text = "";
            IsimTXT.Text = "";
            AboneNoTXT.Text = "";
            //SonEndeksTXT.Text = "";
            TuketiciTuruTXT.Text = "";
            AdresTXT.Text = "";
            TesisatciTXT.Text = "";
            dtBorclar.Clear();
            BorclarGRD.DataSource = dtBorclar;
        }

        private bool BildirimListesiniYukle()
        {
            try
            {
                BildirimlerYenileniyorSimgesiGoster(true);
                IAsyncResult iar1 = FaturaServisi.BeginHataKodlariniOku(Program.SvcUser, Program.SvcPass, null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndHataKodlariniOku(iar1);

                if ((object)xe.SelectSingleNode("AboneBilgileri/HataMesaji") != null && xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml != "YOK")
                {
                    EkraniTemizle();
                    MessageBoxIcon MesajSimgesi = MessageBoxIcon.None;
                    switch (xe.SelectSingleNode("AboneBilgileri/HataIcon").InnerXml)
                    {
                        case HataIconAsterisk:
                            MesajSimgesi = MessageBoxIcon.Asterisk;
                            break;
                        case HataIconExclamation:
                            MesajSimgesi = MessageBoxIcon.Exclamation;
                            break;
                        case HataIconHand:
                            MesajSimgesi = MessageBoxIcon.Hand;
                            break;
                        case HataIconQuestion:
                            MesajSimgesi = MessageBoxIcon.Question;
                            break;
                        case HataIconNone:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                        default:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                    }
                    BildirimlerYenileniyorSimgesiGoster(false);
                    MessageBox.Show(xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml, xe.SelectSingleNode("AboneBilgileri/HataBaslik").InnerXml, MessageBoxButtons.OK, MesajSimgesi, MessageBoxDefaultButton.Button1);
                    return false;
                }

                if (xe.SelectSingleNode("Hata/HataAciklama").InnerXml == "YOK")
                {
                    BildirimlerYenileniyorSimgesiGoster(false);
                    return false;
                }
                else
                {
                    XmlNodeList nl_1 = xe.SelectNodes("Hata");
                    int i = 0;
                    if (nl_1.Count > 0)
                    {
                        BasilacakHataKodu = new string[nl_1.Count];
                        BasilacakHataMesaji = new string[nl_1.Count];
                        HataListesiLB.Items.Clear();
                        foreach (XmlNode xn in nl_1)
                        {
                            /*if (xn.SelectSingleNode("HataAciklama").InnerXml == "YOK") //Alternatif hata kontrolü
                            {
                                BildirimlerYenileniyorSimgesiGoster(false);
                                MessageBox.Show("Hata Kodları Okunamadı");
                                return;
                            }*/
                            HataListesiLB.Items.Add(xn.SelectSingleNode("HataAciklama").InnerXml);
                            BasilacakHataKodu[i] = xn.SelectSingleNode("HataKodu").InnerXml;
                            BasilacakHataMesaji[i] = xn.SelectSingleNode("BasilacakMesaj").InnerXml;
                            i++;
                        }
                    }
                    BildirimlerYenileniyorSimgesiGoster(false);
                    return true;
                }
            }
            catch
            {
                BildirimlerYenileniyorSimgesiGoster(false);
                return false;
            }
        }

        private bool YetkiliGSMListesiniYukle()
        {
            try
            {
                IAsyncResult iar1 = FaturaServisi.BeginYetkiliGSMler(Program.SvcUser, Program.SvcPass, null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndYetkiliGSMler(iar1);

                if ((object)xe.SelectSingleNode("AboneBilgileri/HataMesaji") != null && xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml != "YOK")
                {
                    EkraniTemizle();
                    MessageBoxIcon MesajSimgesi = MessageBoxIcon.None;
                    switch (xe.SelectSingleNode("AboneBilgileri/HataIcon").InnerXml)
                    {
                        case HataIconAsterisk:
                            MesajSimgesi = MessageBoxIcon.Asterisk;
                            break;
                        case HataIconExclamation:
                            MesajSimgesi = MessageBoxIcon.Exclamation;
                            break;
                        case HataIconHand:
                            MesajSimgesi = MessageBoxIcon.Hand;
                            break;
                        case HataIconQuestion:
                            MesajSimgesi = MessageBoxIcon.Question;
                            break;
                        case HataIconNone:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                        default:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                    }
                    MessageBox.Show(xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml, xe.SelectSingleNode("AboneBilgileri/HataBaslik").InnerXml, MessageBoxButtons.OK, MesajSimgesi, MessageBoxDefaultButton.Button1);
                    return false;
                }

                if (xe.SelectSingleNode("YetkiliGSM/GSMno").InnerXml == "YOK")
                {
                    return false;
                }
                else
                {
                    XmlNodeList nl_1 = xe.SelectNodes("YetkiliGSM");
                    int i = 0;
                    if (nl_1.Count > 0)
                    {
                        YetkiliGSM = new string[nl_1.Count];
                        foreach (XmlNode xn in nl_1)
                        {
                            YetkiliGSM[i] = xn.SelectSingleNode("GSMno").InnerXml;
                            i++;
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void HataListesiLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            BasilacakMesajTXT.Text = BasilacakHataKodu[HataListesiLB.SelectedIndex] + " - " + BasilacakHataMesaji[HataListesiLB.SelectedIndex];
            //BasilacakMesajTXT.Text = BasilacakHataMesaji[HataListesiLB.SelectedIndex];
        }

        private void SayacNoTXT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == TriggerLeft | e.KeyCode == TriggerRight)
            {
                TriggerTusuBasildimi = true;
                BarkodOku((e.KeyCode == TriggerLeft ? true : false));
            }
        }

        private void SayacNoTXT_GotFocus(object sender, EventArgs e)
        {
            SayacNoTXT.BackColor = Color.SkyBlue;
            SayacNoTXT.ForeColor = Color.Black;
            SayacNoTXT.SelectAll();
        }

        private void SayacNoTXT_LostFocus(object sender, EventArgs e)
        {
            SayacNoTXT.BackColor = Color.White;
            SayacNoTXT.ForeColor = Color.Black;
        }


        private void BarkodOku(bool SolTrigger)
        {
            try
            {
                pError = Calib.IMGLibNet.Api.IMGWaitForDecode(5000, pMessage, pCodeID, pAimID, pSymModifier, ref pLength, IntPtr.Zero);
                if (pError == Calib.IMGLibNet.Def.IMG_SUCCESS)
                {
                    SayacNoTXT.Text = pMessage;	// symbol data display
                    if (pLength > 0) SayacNoTXT.Text = SayacNoTXT.Text.Substring(0, pLength); //Bu satırı ben ekledim çünkü üstteki satır son okunan barkodu öncekinin ilk karatkterleri değiştirerek gösteriyor
                    if (SolTrigger)
                    {
                        switch (SolTriggerFonksiyonu)
                        {
                            case 0:
                                //SayacNoTXT'ye barkodu yansıttı zaten
                                break;
                            case 1:
                                BilgiAl();
                                break;
                            case 2:
                                FaturaKes();
                                break;
                        }
                    }
                    else
                    {
                        switch (SagTriggerFonksiyonu)
                        {
                            case 0:
                                //SayacNoTXT'ye barkodu yansıttı zaten
                                break;
                            case 1:
                                BilgiAl();
                                break;
                            case 2:
                                FaturaKes();
                                break;
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Barkod okuma hatası");
            }
        }
    
        

        private void FaturaYaz()
        {
            try
            {
                YazdiriliyorSimgesiGoster(true);
                StringFormat format = new StringFormat();
                int leftMargin = 15;
                int topMargin = 55;
                string FontAdi = "Arial Narrow";
                //UInt32 ETO_CLIPPED = 4;
                m_CpDevMode = new Cp780LibCS.CPDEVMODE();
                m_CpDevMode_Null = new Cp780LibCS.CPDEVMODE();
                m_CpDocInfo = new Cp780LibCS.CPDOCINFO();

                if(!Cp780LibCS.Api.CpOpenPrinter(m_sPrinter, m_sPort, ref m_hPrinter))
                    MessageBox.Show("Yazdırma hatası: 0", "Fatura Basılamadı");
                if (Cp780LibCS.Api.CpDocumentProperties(0, m_hPrinter, m_CpDevMode, m_CpDevMode_Null, Cp780LibCS.Def.CPDM_OUT_BUFFER) < 0)
                    MessageBox.Show("Yazdırma hatası: 1", "Fatura Basılamadı");

                //m_CpDevMode.PrintSpeed = Cp780LibCS.Def.PRINTSPEED_GRAPHIC;
                m_CpDevMode.PrintSpeed = Cp780LibCS.Def.PRINTSPEED_SLOW;
                m_CpDevMode.dmOrientation = Cp780LibCS.Def.CPDMORIENT_PORTRAIT;
                //m_CpDevMode.dmColor = 0;
                //m_CpDevMode.dmDitherType = Cp780LibCS.Def.CPDMDITHER_ERRORDIFFUSION;
                m_CpDevMode.bErrorContinuation = 1;
                m_CpDevMode.bPreHeat = 1;
                m_CpDevMode.bMarkerDetection = Cp780LibCS.Def.MARKER_END;
                m_CpDevMode.dmPaperSize = Cp780LibCS.Def.CPDMPAPER_FREE;
                m_CpDevMode.dmPaperWidth = 800;
                m_CpDevMode.dmPaperLength = 1465;

                m_hDC = Cp780LibCS.Api.CpCreateDC(m_hPrinter, m_CpDevMode);

                MoveToEx((IntPtr)m_hDC, leftMargin, 0, IntPtr.Zero);

                //RECT DestRect = new RECT();

                if (Cp780LibCS.Api.CpStartDoc(m_hPrinter, m_hDC, m_CpDocInfo) == Cp780LibCS.Def.CP_ERROR)
                    MessageBox.Show("Yazdırma hatası: 2", "Fatura Basılamadı");
                if(Cp780LibCS.Api.CpStartPage(m_hPrinter, m_hDC)==Cp780LibCS.Def.CP_ERROR)
                    MessageBox.Show("Yazdırma hatası: 3", "Fatura Basılamadı");

                Graphics g = Graphics.FromHdc((IntPtr)m_hDC);

                //SizeF YaziBoyu = g.MeasureString("DENEME İÇİN YAPILMIŞTIR", new Font("Arial", 8, FontStyle.Regular));
                //g.DrawRectangle(new Pen(Color.Black, 2), Convert.ToInt32(textBox10.Text), Convert.ToInt32(textBox13.Text), Convert.ToInt32(textBox14.Text), Convert.ToInt32(textBox15.Text));

                g.DrawString(FaturaBilgi.FaturaNo, new Font(FontAdi, 11, FontStyle.Bold), new SolidBrush(Color.Black), leftMargin + 385, topMargin + 50);
                g.DrawString(FaturaBilgi.SonOkumaTarihi, new Font(FontAdi, 11, FontStyle.Bold), new SolidBrush(Color.Black), leftMargin + 385, topMargin + 80);
                g.DrawString(FaturaBilgi.AboneNo, new Font(FontAdi, 11, FontStyle.Bold), new SolidBrush(Color.Black), leftMargin + 65, topMargin + 80);
                g.DrawString("Sayın," + FaturaBilgi.AboneAd, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin - 8, topMargin + 120, 328, 50));
                g.DrawString(FaturaBilgi.Adres, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin - 8, topMargin + 175, 328, 75));

                format = new StringFormat();
                format.Alignment = StringAlignment.Far;
                g.DrawString(".", new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 437, topMargin + 126, 137, 25), format);
                g.DrawString("#" + FaturaBilgi.TerminalNo + "#", new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 465, topMargin + 148, 85, 25), format);
                g.DrawString(FaturaBilgi.SayacNo, new Font(FontAdi, 10, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 430, topMargin + 170, 120, 25), format);

                g.DrawString(FaturaBilgi.GecikmeBedeli, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 465, topMargin + 245, 90, 25), format);
                g.DrawString(FaturaBilgi.GecikmeKDV, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 465, topMargin + 270, 90, 25), format);
                g.DrawString(FaturaBilgi.KDVOrani, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 320, topMargin + 268, 90, 25), format);

                g.DrawString(FaturaBilgi.Guvence, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 465, topMargin + 340, 90, 25), format);
                g.DrawString(FaturaBilgi.OzelHizmetBedeli, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 465, topMargin + 364, 90, 25), format);
                g.DrawString(FaturaBilgi.OzelHizmetKDV, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 465, topMargin + 388, 90, 25), format);
                g.DrawString(FaturaBilgi.KDVOrani, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 320, topMargin + 388, 90, 25), format);

                g.DrawString(FaturaBilgi.TuketimBedeli, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 465, topMargin + 458, 90, 25), format);
                g.DrawString(FaturaBilgi.TuketimKDV, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 465, topMargin + 482, 90, 25), format);
                g.DrawString(FaturaBilgi.KDVOrani, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 320, topMargin + 480, 90, 25), format);
                g.DrawString(FaturaBilgi.Tenzilat, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 465, topMargin + 505, 90, 25), format);
                g.DrawString(FaturaBilgi.Yuvarlama, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 465, topMargin + 528, 90, 25), format);

                format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                g.DrawString("GÜNLÜK HARCAMANIZ", new Font(FontAdi, 8, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 340, topMargin + 575, 200, 25), format);
                string GunlukTuketimBedeli = ((double.Parse(FaturaBilgi.TuketimBedeli) + double.Parse(FaturaBilgi.TuketimKDV)) / double.Parse(FaturaBilgi.FaturaGunSayisi)).ToString("###,##0.##");
                g.DrawString(GunlukTuketimBedeli + " ", new Font(FontAdi, 8, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 345, topMargin + 600, 200, 25), format);
                g.DrawString(FaturaBilgi.Toplam + " ", new Font(FontAdi, 13, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 345, topMargin + 660, 200, 35), format);
                float px0 = g.MeasureString(GunlukTuketimBedeli + " ", new Font(FontAdi, 8, FontStyle.Bold)).Width;
                float px1 = g.MeasureString(FaturaBilgi.Toplam + " ", new Font(FontAdi, 13, FontStyle.Bold)).Width;
                g.DrawString(((char)0xA8).ToString(), new Font("AbakuTLSymSans", 8 - 1, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 345 + 200 / 2 + (int)px0 / 2, topMargin + 600, 25, 25)); //TL simgesi sağda
                g.DrawString(((char)0xA8).ToString(), new Font("AbakuTLSymSans", 13 - 1, FontStyle.Regular), new SolidBrush(Color.Black), new Rectangle(leftMargin + 345 + 200 / 2 + (int)px1 / 2, topMargin + 660, 25, 35)); //TL simgesi sağda
                //g.DrawString(((char)0xA8).ToString(), new Font("AbakuTLSymSans", 8 - 1, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 360 + 200 / 2 - (int)px0 / 2 - 25, topMargin + 600, 25, 25)); //TL simgesi solda
                //g.DrawString(((char)0xA8).ToString(), new Font("AbakuTLSymSans", 13 - 1, FontStyle.Regular), new SolidBrush(Color.Black), new Rectangle(leftMargin + 350 + 200 / 2 - (int)px1 / 2 - 25, topMargin + 660, 25, 35)); //TL simgesi solda

                format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                g.DrawString(FaturaBilgi.IlkOkumaTarihi, new Font(FontAdi, 8, FontStyle.Regular), new SolidBrush(Color.Black), new Rectangle(leftMargin - 10, topMargin + 285, 140, 25), format);
                g.DrawString(FaturaBilgi.SonOkumaTarihi, new Font(FontAdi, 8, FontStyle.Regular), new SolidBrush(Color.Black), new Rectangle(leftMargin - 10, topMargin + 310, 140, 25), format);

                format = new StringFormat();
                format.Alignment = StringAlignment.Far;
                g.DrawString(FaturaBilgi.IlkEndeks, new Font(FontAdi, 8, FontStyle.Regular), new SolidBrush(Color.Black), new Rectangle(leftMargin + 160, topMargin + 285, 140, 25), format);
                g.DrawString(FaturaBilgi.SonEndeks, new Font(FontAdi, 8, FontStyle.Regular), new SolidBrush(Color.Black), new Rectangle(leftMargin + 160, topMargin + 310, 140, 25), format);

                format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                g.DrawString(FaturaBilgi.TebligTarihiSaati, new Font(FontAdi, 9, FontStyle.Regular), new SolidBrush(Color.Black), new Rectangle(leftMargin + 10, topMargin + 360, 310, 28), format);
                g.DrawString("Bu fatura " + FaturaBilgi.FaturaGunSayisi + " günlüktür", new Font(FontAdi, 9, FontStyle.Regular), new SolidBrush(Color.Black), new Rectangle(leftMargin + 10, topMargin + 386, 310, 28), format);

                format = new StringFormat();
                format.Alignment = StringAlignment.Far;
                g.DrawString(FaturaBilgi.SayacBasinci, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 213, topMargin + 448, 100, 20), format);
                g.DrawString(FaturaBilgi.SayactanOkunanHacim, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 213, topMargin + 468, 100, 20), format);
                g.DrawString(FaturaBilgi.BasincDuzeltmeKatsayisi, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 213, topMargin + 488, 100, 20), format);
                g.DrawString(FaturaBilgi.FatEsDuzHacim, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 213, topMargin + 508, 100, 20), format);
                g.DrawString(FaturaBilgi.OrtFiilUstIsilDeg, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 213, topMargin + 528, 100, 20), format);
                g.DrawString(FaturaBilgi.TukEnerMik, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 213, topMargin + 547, 100, 20), format);
                g.DrawString(".", new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 213, topMargin + 567, 100, 20), format);
                g.DrawString(".", new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 213, topMargin + 587, 100, 20), format);
                g.DrawString(FaturaBilgi.OrtPerSatFiyati, new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 213, topMargin + 607, 100, 20), format);

                g.DrawString(FaturaBilgi.IlkOdemeTarihi, new Font(FontAdi, 10, FontStyle.Regular), new SolidBrush(Color.Black), new Rectangle(leftMargin + 170, topMargin + 650, 140, 25));
                g.DrawString(FaturaBilgi.SonOdemeTarihi, new Font(FontAdi, 10, FontStyle.Regular), new SolidBrush(Color.Black), new Rectangle(leftMargin + 170, topMargin + 676, 140, 25));

                format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                int FaturaListesiIlkSatir = 783;
                foreach (DataRow drOdenmemisFatura in dtBorclar.Rows)
                {
                    g.DrawString(drOdenmemisFatura["FaturaNo"].ToString(), new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 25, topMargin + FaturaListesiIlkSatir, 170, 22), format);
                    g.DrawString(drOdenmemisFatura["SonOdemeTarihi"].ToString(), new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 185, topMargin + FaturaListesiIlkSatir, 130, 22), format);
                    g.DrawString(drOdenmemisFatura["Tutar"].ToString(), new Font(FontAdi, 7, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 330, topMargin + FaturaListesiIlkSatir, 220, 22), format);
                    FaturaListesiIlkSatir += 18;
                }

                //SizeF YaziBoyu = g.MeasureString("Bu bir denemedi", new Font(FontAdi, 7, FontStyle.Bold));
                int MesajListesiIlkSatir = FaturaListesiIlkSatir > 825 ? (FaturaListesiIlkSatir + 15) : 835;
                string BasilacakMesaj = "";
                char CR = (char)0x0D;
                char LF = (char)0x0A;
                //char FF = (char)0x0C;
                foreach (DataRow drMesaj in dtBasilacakMesajlar.Rows)
                {
                    BasilacakMesaj += drMesaj["Mesaj"].ToString() + CR + LF;
                }
                BasilacakMesaj = BasilacakMesaj.Replace(",", ", ");
                g.DrawString(BasilacakMesaj, new Font(FontAdi, ((1000 - MesajListesiIlkSatir) / 23.57 >= 6) ? 7 : 6, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 15, topMargin + MesajListesiIlkSatir, 525, 142));

                byte[] inBuf = new byte[2];
                inBuf[0] = 27;
                inBuf[1] = 0x45;
                byte[] outBuf = new byte[1];
                if(Cp780LibCS.Api.CpExtEscape(m_hPrinter, m_hDC, Cp780LibCS.Def.CPPASSTHROGH, 2, ref inBuf[0], 0, ref outBuf[0])==Cp780LibCS.Def.CP_ERROR)
                    MessageBox.Show("Yazdırma hatası: 4", "Fatura Basılamadı");

                if(Cp780LibCS.Api.CpEndPage(m_hPrinter, m_hDC)==Cp780LibCS.Def.CP_ERROR)
                    MessageBox.Show("Yazdırma hatası: 5", "Fatura Basılamadı");
                if(Cp780LibCS.Api.CpEndDoc(m_hPrinter, m_hDC)==Cp780LibCS.Def.CP_ERROR)
                    MessageBox.Show("Yazdırma hatası: 6", "Fatura Basılamadı");

                if(!Cp780LibCS.Api.CpDeleteDC(m_hPrinter, m_hDC))
                    MessageBox.Show("Yazdırma hatası: 7", "Fatura Basılamadı");
                if(!Cp780LibCS.Api.CpClosePrinter(m_hPrinter))
                    MessageBox.Show("Yazdırma hatası: 8", "Fatura Basılamadı");

                YazdiriliyorSimgesiGoster(false);
                EndeksTXT.Text = "";
            }
            catch(Exception Ex)
            {
                string h1 = (object)Ex != null ? "FaturaKes" + Ex.ToString() : "FaturaKes" + "h1";
                string h2 = (object)Ex.InnerException != null ? Ex.InnerException.Message : "h2";
                string h3 = (object)Ex.Message != null ? Ex.Message : "h3";
                HataDosyaKaydet(h1, h2, h3);

                YazdiriliyorSimgesiGoster(false);
                MessageBox.Show("Yazdırma hatası");
            }
        }

        private void BildirimYaz(string BildirimMesaji)
        {
            try
            {
                YazdiriliyorSimgesiGoster(true);
                StringFormat format = new StringFormat();
                int leftMargin = 1;
                int topMargin = 0;
                string FontAdi = "Arial Narrow";
                //UInt32 ETO_CLIPPED = 4;
                m_CpDevMode = new Cp780LibCS.CPDEVMODE();
                m_CpDevMode_Null = new Cp780LibCS.CPDEVMODE();
                m_CpDocInfo = new Cp780LibCS.CPDOCINFO();

                if(!Cp780LibCS.Api.CpOpenPrinter(m_sPrinter, m_sPort, ref m_hPrinter))
                    MessageBox.Show("Yazdırma hatası: 0", "Bildirim Basılamadı");
                if(Cp780LibCS.Api.CpDocumentProperties(0, m_hPrinter, m_CpDevMode, m_CpDevMode_Null, Cp780LibCS.Def.CPDM_OUT_BUFFER)<0)
                    MessageBox.Show("Yazdırma hatası: 1", "Bildirim Basılamadı");

                //m_CpDevMode.PrintSpeed = Cp780LibCS.Def.PRINTSPEED_GRAPHIC;
                m_CpDevMode.PrintSpeed = Cp780LibCS.Def.PRINTSPEED_SLOW;
                m_CpDevMode.dmOrientation = Cp780LibCS.Def.CPDMORIENT_PORTRAIT;
                //m_CpDevMode.dmColor = 0;
                //m_CpDevMode.dmDitherType = Cp780LibCS.Def.CPDMDITHER_ERRORDIFFUSION;
                m_CpDevMode.bPreHeat = 2;
                m_CpDevMode.bMarkerDetection = Cp780LibCS.Def.MARKER_END;
                m_CpDevMode.dmPaperSize = Cp780LibCS.Def.CPDMPAPER_FREE;
                m_CpDevMode.dmPaperWidth = 800;
                m_CpDevMode.dmPaperLength = 1465;

                m_hDC = Cp780LibCS.Api.CpCreateDC(m_hPrinter, m_CpDevMode);

                MoveToEx((IntPtr)m_hDC, leftMargin, 0, IntPtr.Zero);

                //RECT DestRect = new RECT();

                if(Cp780LibCS.Api.CpStartDoc(m_hPrinter, m_hDC, m_CpDocInfo)==Cp780LibCS.Def.CP_ERROR)
                    MessageBox.Show("Yazdırma hatası: 2", "Bildirim Basılamadı");
                if(Cp780LibCS.Api.CpStartPage(m_hPrinter, m_hDC)==Cp780LibCS.Def.CP_ERROR)
                    MessageBox.Show("Yazdırma hatası: 3", "Bildirim Basılamadı");

                Graphics g = Graphics.FromHdc((IntPtr)m_hDC);

                //SizeF YaziBoyu = g.MeasureString("DENEME İÇİN YAPILMIŞTIR", new Font("Arial", 8, FontStyle.Regular));
                //g.DrawRectangle(new Pen(Color.Black, 2), leftMargin + 395, topMargin, 50, 50);
                g.DrawLine(new Pen(Color.Black, 8), leftMargin + 250, topMargin, leftMargin + 250 + 320, topMargin);
                g.DrawLine(new Pen(Color.Black, 8), leftMargin + 250, topMargin + 15, leftMargin + 250 + 320, topMargin + 15);
                g.DrawLine(new Pen(Color.Black, 8), leftMargin + 250, topMargin + 30, leftMargin + 250 + 320, topMargin + 30);
                format = new StringFormat(); format.Alignment = StringAlignment.Center;
                g.DrawString("B İ L D İ R İ M", new Font(FontAdi, 17, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin + 250, topMargin + 37, 320, 60), format);
                g.DrawLine(new Pen(Color.Black, 8), leftMargin + 250, topMargin + 90, leftMargin + 250 + 320, topMargin + 90);
                g.DrawLine(new Pen(Color.Black, 8), leftMargin + 250, topMargin + 105, leftMargin + 250 + 320, topMargin + 105);
                g.DrawLine(new Pen(Color.Black, 8), leftMargin + 250, topMargin + 120, leftMargin + 250 + 320, topMargin + 120);

                g.DrawString(BildirimMesaji, new Font(FontAdi, 8, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(leftMargin, topMargin + 175, 328, 175));

                byte[] inBuf = new byte[2];
                inBuf[0] = 27;
                inBuf[1] = 0x45;
                byte[] outBuf = new byte[1];
                if(Cp780LibCS.Api.CpExtEscape(m_hPrinter, m_hDC, Cp780LibCS.Def.CPPASSTHROGH, 2, ref inBuf[0], 0, ref outBuf[0])==Cp780LibCS.Def.CP_ERROR)
                    MessageBox.Show("Yazdırma hatası: 4", "Bildirim Basılamadı");

                if(Cp780LibCS.Api.CpEndPage(m_hPrinter, m_hDC)==Cp780LibCS.Def.CP_ERROR)
                    MessageBox.Show("Yazdırma hatası: 5", "Bildirim Basılamadı");
                if(Cp780LibCS.Api.CpEndDoc(m_hPrinter, m_hDC)==Cp780LibCS.Def.CP_ERROR)
                    MessageBox.Show("Yazdırma hatası: 6", "Bildirim Basılamadı");

                if(!Cp780LibCS.Api.CpDeleteDC(m_hPrinter, m_hDC))
                    MessageBox.Show("Yazdırma hatası: 7", "Bildirim Basılamadı");
                if(!Cp780LibCS.Api.CpClosePrinter(m_hPrinter))
                    MessageBox.Show("Yazdırma hatası: 8", "Bildirim Basılamadı");

                YazdiriliyorSimgesiGoster(false);
            }
            catch(Exception Ex)
            {
                string h1 = (object)Ex != null ? "FaturaKes" + Ex.ToString() : "FaturaKes" + "h1";
                string h2 = (object)Ex.InnerException != null ? Ex.InnerException.Message : "h2";
                string h3 = (object)Ex.Message != null ? Ex.Message : "h3";
                HataDosyaKaydet(h1, h2, h3);

                YazdiriliyorSimgesiGoster(false);
                MessageBox.Show("Yazdırma hatası");
            }
        }

        public void TahakkukluFaturalariGoster(float GazAboneNoBoy, float GazSayacNoBoy, float FaturaSeriBoy, float FaturaNoBoy, float SonOkumaTarihiBoy, float SonEndeksBoy, float SarfiyatBoy, float ToplamTLBoy, float AdSoyadBoy)
        {
            DataGridTableStyle tableStyle = new DataGridTableStyle();
            tableStyle.MappingName = dtTahakkukluFaturalar.TableName;

            GridColumnStylesCollection columnStyles = tableStyle.GridColumnStyles;
            columnStyles.Clear();

            DataGridTextBoxColumn columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "SonOkumaTarihi";
            columnStyle.HeaderText = "Son Okuma Tarihi";
            columnStyle.Width = (int)SonOkumaTarihiBoy;
            columnStyles.Add(columnStyle);

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "FaturaSeri";
            columnStyle.HeaderText = "Seri";
            columnStyle.Width = (int)FaturaSeriBoy;
            columnStyles.Add(columnStyle);

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "FaturaNo";
            columnStyle.HeaderText = "Fatura No";
            columnStyle.Width = (int)FaturaNoBoy;
            columnStyles.Add(columnStyle);

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "ToplamTL";
            columnStyle.HeaderText = "Tutar(TL)";
            columnStyle.Width = (int)ToplamTLBoy;
            columnStyles.Add(columnStyle);

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "Sarfiyat";
            columnStyle.HeaderText = "Sarfiyat(m" + Ussu3 + ")";
            columnStyle.Width = (int)SarfiyatBoy;
            columnStyles.Add(columnStyle);
            
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "GazSayacNo";
            columnStyle.HeaderText = "Gaz Sayaç No";
            columnStyle.Width = (int)GazSayacNoBoy;
            columnStyles.Add(columnStyle);

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "GazAboneNo";
            columnStyle.HeaderText = "Gaz Abone No";
            columnStyle.Width = (int)GazAboneNoBoy;
            columnStyles.Add(columnStyle);

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "SonEndeks";
            columnStyle.HeaderText = "Son Endeks(m" + Ussu3 + ")";
            columnStyle.Width = (int)SonEndeksBoy;
            columnStyles.Add(columnStyle);

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.MappingName = "AdSoyad";
            columnStyle.HeaderText = "İsim".PadRight(30, ' ');
            columnStyle.Width = (int)AdSoyadBoy;
            columnStyles.Add(columnStyle);

            GridTableStylesCollection tableStyles = TahakkukluFaturalarGRD.TableStyles;
            tableStyles.Clear();
            tableStyles.Add(tableStyle);

            TahakkukluFaturalarGRD.PreferredRowHeight = (int)KarakterYukseklik_TahakkukGRD;
            TahakkukluFaturalarGRD.RowHeadersVisible = false;
            TahakkukluFaturalarGRD.DataSource = dtTahakkukluFaturalar;

            dtTahakkukluFaturalar.Dispose();
            tableStyle.Dispose();
        }

        private bool SuretFaturalariListele()
        {
            try
            {
                SuretFaturalarListeleniyorSimgesiGoster(true);
                IAsyncResult iar1 = FaturaServisi.BeginFaturaSureti(Program.SvcUser, Program.SvcPass, SuretSayacNoTXT.Text.Trim(), SuretAboneNoTXT.Text.Trim(), "", "", null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndFaturaSureti(iar1);

                if (xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml != "YOK")
                {
                    EkraniTemizle();
                    MessageBoxIcon MesajSimgesi = MessageBoxIcon.None;
                    switch (xe.SelectSingleNode("AboneBilgileri/HataIcon").InnerXml)
                    {
                        case HataIconAsterisk:
                            MesajSimgesi = MessageBoxIcon.Asterisk;
                            break;
                        case HataIconExclamation:
                            MesajSimgesi = MessageBoxIcon.Exclamation;
                            break;
                        case HataIconHand:
                            MesajSimgesi = MessageBoxIcon.Hand;
                            break;
                        case HataIconQuestion:
                            MesajSimgesi = MessageBoxIcon.Question;
                            break;
                        case HataIconNone:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                        default:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                    }
                    SuretFaturalarListeleniyorSimgesiGoster(false);
                    MessageBox.Show(xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml, xe.SelectSingleNode("AboneBilgileri/HataBaslik").InnerXml, MessageBoxButtons.OK, MesajSimgesi, MessageBoxDefaultButton.Button1);
                    return false;
                }
                SuretSayacNoTXT.Text = xe.SelectSingleNode("AboneBilgileri/SayacNo").InnerXml;
                SuretAboneNoTXT.Text = xe.SelectSingleNode("AboneBilgileri/AboneNo").InnerXml;


                XmlNodeList nl_1 = xe.SelectNodes("TahakkukluFaturaBilgisi/Fatura");
                //MessageBox.Show(nl_1.Count.ToString());
                float GazAboneNoBoy = ((float)"Gaz Abone No".Length + 1) * KarakterEn_TahakkukGRD;
                float GazSayacNoBoy = ((float)"Gaz Sayaç No".Length + 1) * KarakterEn_TahakkukGRD;
                float FaturaSeriBoy = ((float)"Seri".Length + 1) * KarakterEn_TahakkukGRD;
                float FaturaNoBoy = ((float)"Fatura No".Length + 1) * KarakterEn_TahakkukGRD;
                float SonOkumaTarihiBoy = ((float)"Son Okuma Tarihi".Length + 1) * KarakterEn_TahakkukGRD;
                float SonEndeksBoy = ((float)("Son Endeks(m" + Ussu3 + ")").Length + 1) * KarakterEn_TahakkukGRD;
                float SarfiyatBoy = ((float)("Sarfiyat(m" + Ussu3 + ")").Length + 1) * KarakterEn_TahakkukGRD;
                float ToplamTLBoy = ((float)"Tutar(TL)".Length + 1) * KarakterEn_TahakkukGRD;
                float AdSoyadBoy = ((float)"İsim".PadRight(30, ' ').Length + 1) * KarakterEn_TahakkukGRD;
                dtTahakkukluFaturalar.Clear();
                foreach (XmlNode xn in nl_1)
                {
                    string GazAboneNo = xn.SelectSingleNode("GazAboneNo").InnerXml;
                    if (((float)GazAboneNo.Length + 1) * KarakterEn_TahakkukGRD > GazAboneNoBoy)
                        GazAboneNoBoy = ((float)GazAboneNo.Length + 1) * KarakterEn_TahakkukGRD;

                    string GazSayacNo = xn.SelectSingleNode("GazSayacNo").InnerXml;
                    if (((float)GazSayacNo.Length + 1) * KarakterEn_TahakkukGRD > GazSayacNoBoy)
                        GazSayacNoBoy = ((float)GazSayacNo.Length + 1) * KarakterEn_TahakkukGRD;

                    string FaturaSeri = xn.SelectSingleNode("FaturaSeri").InnerXml;
                    if (((float)FaturaSeri.Length + 1) * KarakterEn_TahakkukGRD > FaturaSeriBoy)
                        FaturaSeriBoy = ((float)FaturaSeri.Length + 1) * KarakterEn_TahakkukGRD;

                    string FaturaNo = xn.SelectSingleNode("FaturaNo").InnerXml;
                    if (((float)FaturaNo.Length + 1) * KarakterEn_TahakkukGRD > FaturaNoBoy)
                        FaturaNoBoy = ((float)FaturaNo.Length + 1) * KarakterEn_TahakkukGRD;

                    string SonOkumaTarihi = xn.SelectSingleNode("SonOkumaTarihi").InnerXml;
                    if (((float)SonOkumaTarihi.Length + 1) * KarakterEn_TahakkukGRD > SonOkumaTarihiBoy)
                        SonOkumaTarihiBoy = ((float)SonOkumaTarihi.Length + 1) * KarakterEn_TahakkukGRD;

                    string SonEndeks = xn.SelectSingleNode("SonEndeks").InnerXml;
                    if (((float)SonEndeks.Length + 1) * KarakterEn_TahakkukGRD > SonEndeksBoy)
                        SonEndeksBoy = ((float)SonEndeks.Length + 1) * KarakterEn_TahakkukGRD;

                    string Sarfiyat = xn.SelectSingleNode("Sarfiyat").InnerXml;
                    if (((float)Sarfiyat.Length + 1) * KarakterEn_TahakkukGRD > SarfiyatBoy)
                        SarfiyatBoy = ((float)Sarfiyat.Length + 1) * KarakterEn_TahakkukGRD;

                    string ToplamTL = xn.SelectSingleNode("ToplamTL").InnerXml;
                    if (((float)ToplamTL.Length + 1) * KarakterEn_TahakkukGRD > ToplamTLBoy)
                        ToplamTLBoy = ((float)ToplamTL.Length + 1) * KarakterEn_TahakkukGRD;

                    string AdSoyad = xn.SelectSingleNode("AdSoyad").InnerXml;
                    if (((float)AdSoyad.Length + 1) * KarakterEn_TahakkukGRD > AdSoyadBoy)
                        AdSoyadBoy = ((float)AdSoyad.Length + 1) * KarakterEn_TahakkukGRD;

                    DataRow drTahakkukluFatura = dtTahakkukluFaturalar.NewRow();
                    drTahakkukluFatura["GazAboneNo"] = GazAboneNo;
                    drTahakkukluFatura["GazSayacNo"] = GazSayacNo;
                    drTahakkukluFatura["FaturaSeri"] = FaturaSeri;
                    drTahakkukluFatura["FaturaNo"] = FaturaNo;
                    drTahakkukluFatura["SonOkumaTarihi"] = SonOkumaTarihi;
                    drTahakkukluFatura["SonEndeks"] = SonEndeks;
                    drTahakkukluFatura["Sarfiyat"] = Sarfiyat;
                    drTahakkukluFatura["ToplamTL"] = ToplamTL;
                    drTahakkukluFatura["AdSoyad"] = AdSoyad;
                    dtTahakkukluFaturalar.Rows.Add(drTahakkukluFatura);
                }
                if (nl_1.Count > 0) TahakkukluFaturalariGoster(GazAboneNoBoy, GazSayacNoBoy, FaturaSeriBoy, FaturaNoBoy, SonOkumaTarihiBoy, SonEndeksBoy, SarfiyatBoy, ToplamTLBoy, AdSoyadBoy);
                SuretFaturalarListeleniyorSimgesiGoster(false);
                return true;
            }
            catch
            {
                SuretFaturalarListeleniyorSimgesiGoster(false);
                return false;
            }
        }

        private bool SuretFaturaYaz(string SayacNo,string AboneNo,string FaturaSeri,string FaturaNo)
        {
            try
            {
                AraniyorSimgesiGoster(true);
                IAsyncResult iar1 = FaturaServisi.BeginFaturaSureti(Program.SvcUser, Program.SvcPass, SayacNo, AboneNo, FaturaSeri, FaturaNo, null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndFaturaSureti(iar1);

                if (xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml != "YOK")
                {
                    EkraniTemizle();
                    MessageBoxIcon MesajSimgesi = MessageBoxIcon.None;
                    switch (xe.SelectSingleNode("AboneBilgileri/HataIcon").InnerXml)
                    {
                        case HataIconAsterisk:
                            MesajSimgesi = MessageBoxIcon.Asterisk;
                            break;
                        case HataIconExclamation:
                            MesajSimgesi = MessageBoxIcon.Exclamation;
                            break;
                        case HataIconHand:
                            MesajSimgesi = MessageBoxIcon.Hand;
                            break;
                        case HataIconQuestion:
                            MesajSimgesi = MessageBoxIcon.Question;
                            break;
                        case HataIconNone:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                        default:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                    }
                    AraniyorSimgesiGoster(false);
                    MessageBox.Show(xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml, xe.SelectSingleNode("AboneBilgileri/HataBaslik").InnerXml, MessageBoxButtons.OK, MesajSimgesi, MessageBoxDefaultButton.Button1);
                    return false;
                }

                FaturaBilgi.AboneNo = xe.SelectSingleNode("AboneBilgileri/AboneNo").InnerXml;
                FaturaBilgi.AboneAd = xe.SelectSingleNode("AboneBilgileri/Ad").InnerXml;
                FaturaBilgi.Adres = xe.SelectSingleNode("AboneBilgileri/Adres").InnerXml;
                FaturaBilgi.FaturaNo = xe.SelectSingleNode("AboneBilgileri/FaturaNo").InnerXml;
                FaturaBilgi.TebligTarihiSaati = xe.SelectSingleNode("AboneBilgileri/TebligTarihiSaati").InnerXml;
                FaturaBilgi.IlkOkumaTarihi = xe.SelectSingleNode("AboneBilgileri/IlkOkumaTarihi").InnerXml;
                FaturaBilgi.SonOkumaTarihi = xe.SelectSingleNode("AboneBilgileri/SonOkumaTarihi").InnerXml;
                FaturaBilgi.IlkEndeks = xe.SelectSingleNode("AboneBilgileri/IlkEndeks").InnerXml;
                FaturaBilgi.SonEndeks = xe.SelectSingleNode("AboneBilgileri/SonEndeks").InnerXml;
                FaturaBilgi.FaturaGunSayisi = xe.SelectSingleNode("AboneBilgileri/FaturaGunSayisi").InnerXml;
                FaturaBilgi.TerminalNo = xe.SelectSingleNode("AboneBilgileri/TerminalNo").InnerXml;
                FaturaBilgi.SayacNo = xe.SelectSingleNode("AboneBilgileri/SayacNo").InnerXml;
                FaturaBilgi.SayacBasinci = xe.SelectSingleNode("AboneBilgileri/SayacBasinci").InnerXml;
                FaturaBilgi.SayactanOkunanHacim = xe.SelectSingleNode("AboneBilgileri/SayactanOkunanHacim").InnerXml;
                FaturaBilgi.BasincDuzeltmeKatsayisi = xe.SelectSingleNode("AboneBilgileri/BasincDuzeltmeKatsayisi").InnerXml;
                FaturaBilgi.FatEsDuzHacim = xe.SelectSingleNode("AboneBilgileri/FaturayaEsasDuzeltilmisHacim").InnerXml;
                FaturaBilgi.OrtFiilUstIsilDeg = xe.SelectSingleNode("AboneBilgileri/OrtalamaFiiliUstIsilDeger").InnerXml;
                FaturaBilgi.TukEnerMik = xe.SelectSingleNode("AboneBilgileri/TuketilenEnerjiMiktari").InnerXml;
                FaturaBilgi.OrtPerSatFiyati = xe.SelectSingleNode("AboneBilgileri/OrtalamaPerakendeSatisFiyati").InnerXml;
                FaturaBilgi.GecikmeBedeli = xe.SelectSingleNode("AboneBilgileri/GecikmeBedeli").InnerXml;
                FaturaBilgi.GecikmeKDV = xe.SelectSingleNode("AboneBilgileri/GecikmeBedeliKDV").InnerXml;
                FaturaBilgi.Guvence = xe.SelectSingleNode("AboneBilgileri/GuvenceDamgaVergisi").InnerXml;
                FaturaBilgi.OzelHizmetBedeli = xe.SelectSingleNode("AboneBilgileri/OzelHizmetBedeli").InnerXml;
                FaturaBilgi.OzelHizmetKDV = xe.SelectSingleNode("AboneBilgileri/OzelHizmetBedeliKDV").InnerXml;
                FaturaBilgi.TuketimBedeli = xe.SelectSingleNode("AboneBilgileri/TuketimBedeli").InnerXml;
                FaturaBilgi.TuketimKDV = xe.SelectSingleNode("AboneBilgileri/TuketimBedeliKDV").InnerXml;
                FaturaBilgi.Tenzilat = xe.SelectSingleNode("AboneBilgileri/TenzilatIlave").InnerXml;
                FaturaBilgi.Yuvarlama = xe.SelectSingleNode("AboneBilgileri/Yuvarlama").InnerXml;
                FaturaBilgi.IlkOdemeTarihi = xe.SelectSingleNode("AboneBilgileri/IlkOdemeTarihi").InnerXml;
                FaturaBilgi.SonOdemeTarihi = xe.SelectSingleNode("AboneBilgileri/SonOdemeTarihi").InnerXml;
                FaturaBilgi.Toplam = xe.SelectSingleNode("AboneBilgileri/Toplam").InnerXml;
                FaturaBilgi.KDVOrani = xe.SelectSingleNode("AboneBilgileri/KDVOrani").InnerXml;
                FaturaBilgi.ParaBirimi = xe.SelectSingleNode("AboneBilgileri/ParaBirimi").InnerXml;

                AraniyorSimgesiGoster(false);
                FaturaYaz();
                return true;
            }
            catch
            {
                AraniyorSimgesiGoster(false);
                return false;
            }
        }

        private void SayacNoTXT_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                EndeksTXT.Focus();
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void EndeksTXT_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                FaturaKesPB.Focus();
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private bool GPSCalistir()
        {
            if (!Program.GPSLisansli) return false;
            try
            {
                GPSTimer.Interval = GPSOkumaAraligi;
                GPSTimer.Enabled = true;
                if (!GPSaygiti.Opened)
                    GPSaygiti.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool GPSDurdur()
        {
            if (!Program.GPSLisansli) return true;
            try
            {
                GPSTimer.Enabled = false;
                if (GPSaygiti.Opened)
                    GPSaygiti.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool GPSKonumEslestirKaydet()
        {
            try
            {
                string GSMPos = LBS.RIL.GetCellTowerInfo().Replace(',', '.');
                string Latitude = "-1";
                string Longitude = "-1";

                GpsPosition GPSposition = GPSaygiti.GetPosition();
                if (GPSposition.LatitudeValid)
                    Latitude = GPSposition.Latitude.ToString().Replace(',', '.');
                if (GPSposition.LongitudeValid)
                    Longitude = GPSposition.Longitude.ToString().Replace(',', '.');

                //listBox1.Items.Add(WWANCekiyormu().ToString() + "-" + Latitude + "-" + Longitude + " " + GSMPos);
                string Zaman = DateTime.Now.Year.ToString() + '-' + DateTime.Now.Month.ToString().PadLeft(2, '0') + '-' + DateTime.Now.Day.ToString().PadLeft(2, '0') + ' ' + DateTime.Now.Hour.ToString().PadLeft(2, '0') + ':' + DateTime.Now.Minute.ToString().PadLeft(2, '0') + ':' + DateTime.Now.Second.ToString().PadLeft(2, '0');

                IAsyncResult iar1 = FaturaServisi.BeginTerminalGPSKonumKaydet(Program.SvcUser, Program.SvcPass, Program.TerminalNo, Latitude, Longitude, GSMPos, Zaman, null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndTerminalGPSKonumKaydet(iar1);

                if ((object)xe.SelectSingleNode("AboneBilgileri/HataMesaji") != null && xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml != "YOK")
                {
                    EkraniTemizle();
                    MessageBoxIcon MesajSimgesi = MessageBoxIcon.None;
                    switch (xe.SelectSingleNode("AboneBilgileri/HataIcon").InnerXml)
                    {
                        case HataIconAsterisk:
                            MesajSimgesi = MessageBoxIcon.Asterisk;
                            break;
                        case HataIconExclamation:
                            MesajSimgesi = MessageBoxIcon.Exclamation;
                            break;
                        case HataIconHand:
                            MesajSimgesi = MessageBoxIcon.Hand;
                            break;
                        case HataIconQuestion:
                            MesajSimgesi = MessageBoxIcon.Question;
                            break;
                        case HataIconNone:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                        default:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                    }
                    MessageBox.Show(xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml, xe.SelectSingleNode("AboneBilgileri/HataBaslik").InnerXml, MessageBoxButtons.OK, MesajSimgesi, MessageBoxDefaultButton.Button1);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool GPSKonumKaydet()
        {
            try
            {
                GpsPosition position = GPSaygiti.GetPosition();

                string Latitude = "", Longitude = "", Heading = "";

                if (position.LatitudeValid)
                    Latitude = position.Latitude.ToString().Replace(',', '.');
                if (position.LongitudeValid)
                    Longitude = position.Longitude.ToString().Replace(',', '.');
                if (position.HeadingValid)
                    Heading = position.Heading.ToString().Replace(',', '.');

                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.Append("Latitude = ");
                sb.AppendLine(Latitude);
                sb.Append("Longitude = ");
                sb.AppendLine(Longitude);
                sb.Append("Heading = ");
                sb.AppendLine(Heading);

                IAsyncResult iar1 = FaturaServisi.BeginTerminalGPSKonumKaydet(Program.SvcUser, Program.SvcPass, Program.TerminalNo, Latitude, Longitude, Heading, DateTime.Now.ToString(), null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndTerminalGPSKonumKaydet(iar1);

                if ((object)xe.SelectSingleNode("AboneBilgileri/HataMesaji") != null && xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml != "YOK")
                {
                    EkraniTemizle();
                    MessageBoxIcon MesajSimgesi = MessageBoxIcon.None;
                    switch (xe.SelectSingleNode("AboneBilgileri/HataIcon").InnerXml)
                    {
                        case HataIconAsterisk:
                            MesajSimgesi = MessageBoxIcon.Asterisk;
                            break;
                        case HataIconExclamation:
                            MesajSimgesi = MessageBoxIcon.Exclamation;
                            break;
                        case HataIconHand:
                            MesajSimgesi = MessageBoxIcon.Hand;
                            break;
                        case HataIconQuestion:
                            MesajSimgesi = MessageBoxIcon.Question;
                            break;
                        case HataIconNone:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                        default:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                    }
                    MessageBox.Show(xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml, xe.SelectSingleNode("AboneBilgileri/HataBaslik").InnerXml, MessageBoxButtons.OK, MesajSimgesi, MessageBoxDefaultButton.Button1);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool GPSKonumKaydet_RIL()
        {
            try
            {
                //MessageBox.Show(LBS.RIL.GetCellTowerInfo());
                string GSMPos = LBS.RIL.GetCellTowerInfo();
                string[] cellidFields = GSMPos.Split('-');
                //string[] cellidFields = "15582-41-525".Split('-'); //Örnek
                // [0] - CID
                // [1] - LAC
                // [2] – MCC
                // [3] - Time
                //---Arguments for GetLatLng(MCC MNC LAC CID)---
                string[] args = {cellidFields[2], // MCC
                             "0", // MNC – don’t need it here
                             cellidFields[1], // LAC
                             cellidFields[0] // CID
                            };

                string[] LatLng = GMM.GetLatLng(args).Split('|');

                string Latitude = "-1";
                string Longitude = "-1";

                if (LatLng.Length > 1)
                {
                    Latitude = LatLng[0].Replace(',', '.');
                    Longitude = LatLng[1].Replace(',', '.');
                }

                MessageBox.Show(Latitude + "-" + Longitude + " " + LBS.RIL.GetCellTowerInfo());

                IAsyncResult iar1 = FaturaServisi.BeginTerminalGPSKonumKaydet(Program.SvcUser, Program.SvcPass, Program.TerminalNo, Latitude, Longitude, GSMPos, DateTime.Now.ToString(), null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndTerminalGPSKonumKaydet(iar1);

                if ((object)xe.SelectSingleNode("AboneBilgileri/HataMesaji") != null && xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml != "YOK")
                {
                    EkraniTemizle();
                    MessageBoxIcon MesajSimgesi = MessageBoxIcon.None;
                    switch (xe.SelectSingleNode("AboneBilgileri/HataIcon").InnerXml)
                    {
                        case HataIconAsterisk:
                            MesajSimgesi = MessageBoxIcon.Asterisk;
                            break;
                        case HataIconExclamation:
                            MesajSimgesi = MessageBoxIcon.Exclamation;
                            break;
                        case HataIconHand:
                            MesajSimgesi = MessageBoxIcon.Hand;
                            break;
                        case HataIconQuestion:
                            MesajSimgesi = MessageBoxIcon.Question;
                            break;
                        case HataIconNone:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                        default:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                    }
                    MessageBox.Show(xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml, xe.SelectSingleNode("AboneBilgileri/HataBaslik").InnerXml, MessageBoxButtons.OK, MesajSimgesi, MessageBoxDefaultButton.Button1);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool SMSHazirla()
        {
            if (!Program.SMSLisansli) return false;
            try
            {
                SMSEvents.MessageReceived += new MessageInterceptorEventHandler(SMSAlindi);
                SMSFilter.Property = MessageProperty.Sender;
                SMSFilter.ComparisonType = MessagePropertyComparisonType.Equal;
                SMSFilter.CaseSensitive = true;
                SMSFilter.ComparisonValue = "+905327235807";
                //SMSEvents.MessageCondition = SMSFilter;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SMSAlindi(object sender, MessageInterceptorEventArgs e)
        {
            SmsMessage GelenSMS = (SmsMessage)e.Message;
            Recipient Gonderen = GelenSMS.From;
            GelenSMS.Read = true;
            string SMStext = GelenSMS.Body;
            string SMSisim = (object)Gonderen.Name == null ? "" : Gonderen.Name;
            string SMSadres = (object)Gonderen.Address == null ? "" : Gonderen.Address;
            if (SMSisim == "") SMSisim = SMSadres;
            if (SMSadres == "") SMSadres = SMSisim;
            if ((SMSisim.Trim().Length < 10 && SMSadres.Trim().Length < 10) || SMStext.Trim().Length <= 0) return;

            string Zaman = DateTime.Now.Year.ToString() + '-' + DateTime.Now.Month.ToString().PadLeft(2, '0') + '-' + DateTime.Now.Day.ToString().PadLeft(2, '0') + ' ' + DateTime.Now.Hour.ToString().PadLeft(2, '0') + ':' + DateTime.Now.Minute.ToString().PadLeft(2, '0') + ':' + DateTime.Now.Second.ToString().PadLeft(2, '0');
            if (!GSMLog(Program.TerminalNo, SMSadres, SMSisim, SMStext, Zaman, (Array.IndexOf(YetkiliGSM, SMSadres) >= 0 ? "1" : "0")))
                MessageBox.Show("GSMLog Hatası");

            if (Array.IndexOf(YetkiliGSM, SMSadres) >= 0)
                SMSKomutIsle(SMStext);
        }

        private void SMSKomutIsle(string SMStext)
        {
            string[] Parametreler = SMStext.Split('#');
            int ParametreSayisi = Parametreler.GetUpperBound(0);
            if (ParametreSayisi <= 0)
                return;
            string Komut = Parametreler[0].ToUpper();
            switch (Komut)
            {
                case "GPS":
                    int Periyot = 0;
                    if (!Parametreler[1].Sayimi())
                        return;
                    else
                        Periyot = Convert.ToInt32(Parametreler[1]) * 1000;
                    if (Periyot == 0)
                    {
                        GPSDurdur();
                    }
                    else
                    {
                        GPSOkumaAraligi = Periyot;
                        GPSCalistir();
                    }
                    break;

                case "FAT":
                    string FaturaKomut = Parametreler[1].ToUpper();
                    if (FaturaKomut == "TB")
                        if ((object)FaturaBilgi.AboneNo != null && FaturaBilgi.AboneNo.Trim() != "")
                            FaturaYaz();
                    break;

                case "RESET":
                    Calib.SystemLibNet.Api.SysSoftReset();
                    Application.Exit();
                    break;
            }
        }

        private bool GSMLog(string TerminalNo, string GSMno, string GSMisim, string Mesaj, string Zaman, string Aktif)
        {
            try
            {
                IAsyncResult iar1 = FaturaServisi.BeginGSMLogKaydet(Program.SvcUser, Program.SvcPass, TerminalNo, GSMno, GSMisim, Mesaj, Zaman, Aktif, null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndGSMLogKaydet(iar1);

                if ((object)xe.SelectSingleNode("AboneBilgileri/HataMesaji") != null && xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml != "YOK")
                {
                    EkraniTemizle();
                    MessageBoxIcon MesajSimgesi = MessageBoxIcon.None;
                    switch (xe.SelectSingleNode("AboneBilgileri/HataIcon").InnerXml)
                    {
                        case HataIconAsterisk:
                            MesajSimgesi = MessageBoxIcon.Asterisk;
                            break;
                        case HataIconExclamation:
                            MesajSimgesi = MessageBoxIcon.Exclamation;
                            break;
                        case HataIconHand:
                            MesajSimgesi = MessageBoxIcon.Hand;
                            break;
                        case HataIconQuestion:
                            MesajSimgesi = MessageBoxIcon.Question;
                            break;
                        case HataIconNone:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                        default:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                    }
                    MessageBox.Show(xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml, xe.SelectSingleNode("AboneBilgileri/HataBaslik").InnerXml, MessageBoxButtons.OK, MesajSimgesi, MessageBoxDefaultButton.Button1);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool OlayLog(string TerminalNo, string Olay, string Zaman)
        {
            try
            {
                IAsyncResult iar1 = FaturaServisi.BeginOlayLogKaydet(Program.SvcUser, Program.SvcPass, TerminalNo, Olay, Zaman, null, null);
                iar1.AsyncWaitHandle.WaitOne();
                XmlElement xe = FaturaServisi.EndOlayLogKaydet(iar1);

                if ((object)xe.SelectSingleNode("AboneBilgileri/HataMesaji") != null && xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml != "YOK")
                {
                    EkraniTemizle();
                    MessageBoxIcon MesajSimgesi = MessageBoxIcon.None;
                    switch (xe.SelectSingleNode("AboneBilgileri/HataIcon").InnerXml)
                    {
                        case HataIconAsterisk:
                            MesajSimgesi = MessageBoxIcon.Asterisk;
                            break;
                        case HataIconExclamation:
                            MesajSimgesi = MessageBoxIcon.Exclamation;
                            break;
                        case HataIconHand:
                            MesajSimgesi = MessageBoxIcon.Hand;
                            break;
                        case HataIconQuestion:
                            MesajSimgesi = MessageBoxIcon.Question;
                            break;
                        case HataIconNone:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                        default:
                            MesajSimgesi = MessageBoxIcon.None;
                            break;
                    }
                    MessageBox.Show(xe.SelectSingleNode("AboneBilgileri/HataMesaji").InnerXml, xe.SelectSingleNode("AboneBilgileri/HataBaslik").InnerXml, MessageBoxButtons.OK, MesajSimgesi, MessageBoxDefaultButton.Button1);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /*
        private void GPSEventRegister()
        {
            GPSaygiti.DeviceStateChanged += new DeviceStateChangedEventHandler(gps_DeviceStateChanged);
            GPSaygiti.LocationChanged += new LocationChangedEventHandler(gps_LocationChanged);
        }

        private void GPSEventUnRegister()
        {
            GPSaygiti.DeviceStateChanged -= new DeviceStateChangedEventHandler(gps_DeviceStateChanged);
            GPSaygiti.LocationChanged -= new LocationChangedEventHandler(gps_LocationChanged);
        }

        private void gps_LocationChanged(object sender, LocationChangedEventArgs args)
        {
            GpsPosition position = GPSaygiti.GetPosition();
            string Latitude = string.Empty;
            string Longitude = string.Empty;
            string Heading = string.Empty;
            if (position.LatitudeValid)
                Latitude = position.Latitude.ToString();
            if (position.LongitudeValid)
                Longitude = position.Longitude.ToString();
            if (position.HeadingValid)
                Heading = position.Heading.ToString();
            StringBuilder strdisplay = new StringBuilder();
            strdisplay.AppendLine();
            strdisplay.Append("GPS Latitude = ");
            strdisplay.AppendLine(Latitude);
            strdisplay.Append("GPS Longitude = ");
            strdisplay.AppendLine(Longitude);
            strdisplay.Append("GPS Heading = ");
            strdisplay.AppendLine(Heading);
            MessageBox.Show(strdisplay.ToString());
        }


        private void gps_DeviceStateChanged(object sender, DeviceStateChangedEventArgs args)
        {
            GpsDeviceState device = args.DeviceState;
            StringBuilder strdisplay = new StringBuilder();
            strdisplay.AppendLine();
            strdisplay.Append("Device Name = ");
            strdisplay.Append(device.FriendlyName.ToString());
            strdisplay.Append("Device State = ");
            strdisplay.Append(device.DeviceState.ToString());
            strdisplay.Append("Service State = ");
            strdisplay.Append(device.ServiceState.ToString());
            MessageBox.Show(strdisplay.ToString());
            //lbldevicedisplay.Text = strdisplay.ToString();
        }
        */

        private void OnlineFaturaMobileClient_Activated(object sender, EventArgs e)
        {
            if (!Program.LoginOK) Application.Exit();
            /*
            if (GPSaygiti.Opened)
            {
                GPSaygiti.DeviceStateChanged += gps_DeviceStateChanged;
                GPSaygiti.LocationChanged += gps_LocationChanged;
            }
            */
        }

        private void OnlineFaturaMobileClient_Deactivate(object sender, EventArgs e)
        {
            /*
            if (GPSaygiti.Opened)
            {
                GPSaygiti.DeviceStateChanged -= gps_DeviceStateChanged;
                GPSaygiti.LocationChanged -= gps_LocationChanged;
            }
            */
        }

        private void AnaFRM_Closing(object sender, CancelEventArgs e)
        {
            SetUserDefinedKeySate(Program.UserDefineKeyState);
            CameraPreviewOnOff(false);
            if (!ImagerOnOff(false)) MessageBox.Show("BarkodOku okuyucu kapatılamadı !");
            if (!Program.WWANDurdur()) MessageBox.Show("WWAN kapatılamadı !");
            if (!GPSDurdur()) MessageBox.Show("GPS kapatılamadı !");
        }

        private void AnaFRM_Closed(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GPSKonumEslestirKaydet();
        }

        private void GPSTimer_Tick(object sender, EventArgs e)
        {
            GPSKonumEslestirKaydet();
        }

        private void FaturaMesajTXT_TextChanged(object sender, EventArgs e)
        {
            KalanKarakterLBL.Text = (40 - FaturaMesajTXT.Text.Length).ToString();
        }

        private void KlavyePB_MouseDown(object sender, MouseEventArgs e)
        {
            KlavyePB.Image = imageList1.Images[1];
            KlavyePB.Update();
            Program.Titret(120);
        }

        private void KlavyePB_MouseUp(object sender, MouseEventArgs e)
        {
            KlavyePB.Image = imageList1.Images[0];
            KlavyePB.Update();
            inputPanel1.Enabled = !inputPanel1.Enabled;
            FaturaMesajTXT.Focus();
        }

        private void SetKeyboardBacklightStateIcon()
        {
            if (Program.KeyboardLightState())
            {
                KlavyeBackLightPB.Image = imageList2.Images[0];
                KlavyeBackLightPB.Update();
            }
            else
            {
                KlavyeBackLightPB.Image = imageList2.Images[1];
                KlavyeBackLightPB.Update();
            }
        }

        private void SetUserDefinedKeySate(bool durum)
        {
            Calib.SystemLibNet.Api.SysSetUserDefineKeyState(durum);
        }

        private void KlavyeBackLightPB_MouseDown(object sender, MouseEventArgs e)
        {
            Program.Titret(120);
            Program.KeyboardLightOnOff(!Program.KeyboardLightState());
            SetKeyboardBacklightStateIcon();
        }

        private void Uyar(string Mesaj, bool Durum)
        {
            MesajLBL.BringToFront();
            MesajLBL.Text = Mesaj;
            MesajLBL.Visible = Durum;
            if (Durum) MesajTimerCalistir();
        }

        private void SinyalSeviyesiDusuk(bool Durum)
        {
            SinyalLBL.BringToFront();
            SinyalLBL.Visible = Durum;
            if (Durum) GSMTimerCalistir();
        }

        private void GSMTimerCalistir()
        {
            GSMTimer.Interval = 10000;
            GSMTimer.Enabled = true;
        }

        private void GSMTimer_Tick(object sender, EventArgs e)
        {
            SinyalSeviyesiDusuk(false);
            GSMTimer.Enabled = false;
        }

        private void MesajTimerCalistir()
        {
            MesajTimer.Interval = 3000;
            MesajTimer.Enabled = true;
        }

        private void MesajTimer_Tick(object sender, EventArgs e)
        {
            Uyar("", false);
            MesajTimer.Enabled = false;
        }

        private void AnaFRM_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if ((e.KeyCode == TriggerLeft | e.KeyCode == TriggerRight) && GetFocusedControl(this).Name != SayacNoTXT.Name)
                {
                    bool Yenile = false;
                    if (AnaTAB.SelectedIndex != 0)
                    {
                        AnaTAB.SelectedIndex = 0;
                        Yenile = true;
                    }
                    if (FaturaBilgileriTAB.SelectedIndex != 0)
                    {
                        FaturaBilgileriTAB.SelectedIndex = 0;
                        Yenile = true;
                    }
                    if (Yenile)
                    {
                        this.Invalidate();
                        this.Update();
                        this.Refresh();
                    }
                    AboneNoTXT.Focus();
                    SayacNoTXT.Focus();
                    TriggerTusuBasildimi = true;
                    BarkodOku((e.KeyCode == TriggerLeft ? true : false));
                }

                if ((int)e.KeyCode == Program.MenuKey) AnaTAB.Focus();
                switch (AnaTAB.TabPages[AnaTAB.SelectedIndex].Name)
                {
                    case "FaturaKesTAB":
                        {
                            switch (e.KeyCode)
                            {
                                case Keys.F1:
                                    inputPanel1.Enabled = false;
                                    FaturaKesPB_MouseDown(null, null);
                                    Thread.Sleep(100);
                                    FaturaKesPB_MouseUp(null, null);
                                    //FaturaKes();
                                    break;
                                case Keys.F2:
                                    inputPanel1.Enabled = false;
                                    //if ((object)FaturaBilgi.AboneNo == null || FaturaBilgi.AboneNo.Trim() == "") { MessageBox.Show("Kayıtlı fatura bulunamadı"); return; }
                                    TekrarBasPB_MouseDown(null, null);
                                    Thread.Sleep(100);
                                    TekrarBasPB_MouseUp(null, null);
                                    //FaturaYaz();
                                    break;
                                case Keys.F4:
                                    inputPanel1.Enabled = false;
                                    TriggerTusuBasildimi = false;
                                    AboneBilgisiPB_MouseDown(null, null);
                                    Thread.Sleep(100);
                                    AboneBilgisiPB_MouseUp(null, null);
                                    //BilgiAl();
                                    break;
                            }
                            break;
                        }

                    case "BildirimTAB":
                        {
                            switch (e.KeyCode)
                            {
                                case Keys.F2:
                                    inputPanel1.Enabled = false;
                                    BildirimListeYenilePB_MouseDown(null, null);
                                    Thread.Sleep(100);
                                    BildirimListeYenilePB_MouseUp(null, null);
                                    break;
                                case Keys.F3:
                                    inputPanel1.Enabled = false;
                                    BildirimBasPB_MouseDown(null, null);
                                    Thread.Sleep(100);
                                    BildirimBasPB_MouseUp(null, null);
                                    break;
                            }
                            break;
                        }

                    case "SuretTAB":
                        {
                            switch (e.KeyCode)
                            {
                                case Keys.F2:
                                    inputPanel1.Enabled = false;
                                    SuretListelePB_MouseDown(null, null);
                                    Thread.Sleep(100);
                                    SuretListelePB_MouseUp(null, null);
                                    break;
                                case Keys.F3:
                                    inputPanel1.Enabled = false;
                                    SuretBasPB_MouseDown(null, null);
                                    Thread.Sleep(100);
                                    SuretBasPB_MouseUp(null, null);
                                    break;
                            }
                            break;
                        }

                    case "FotoTAB":
                        {
                            switch (e.KeyCode)
                            {
                                case Keys.F1:
                                    if (IsikCMB.SelectedIndex + 1 > IsikCMB.Items.Count - 1)
                                        IsikCMB.SelectedIndex = 0;
                                    else
                                        IsikCMB.SelectedIndex++;
                                    break;
                                case Keys.F2:
                                    KameraAcKapatPB_MouseDown(null, null);
                                    Thread.Sleep(100);
                                    KameraAcKapatPB_MouseUp(null, null);
                                    break;
                                case Keys.F3:
                                    FotoCekPB_MouseDown(null, null);
                                    Thread.Sleep(100);
                                    FotoCekPB_MouseUp(null, null);
                                    break;
                                case Keys.F4:
                                    IlluminationCHK.Checked = !IlluminationCHK.Checked;
                                    break;
                            }
                            if (KameraAcikmi && (e.KeyCode == Keys.F1 || e.KeyCode == Keys.F2))
                                CameraSetStates(true);
                            break;
                        }
                }
            }
            catch
            {
                MessageBox.Show("KeyDown hatası");
            }
        }

        private Control GetFocusedControl(Control parent)
        {
            if (parent.Focused)
            {
                return parent;
            }
            foreach (Control ctrl in parent.Controls)
            {
                Control temp = GetFocusedControl(ctrl);
                if (temp != null)
                {
                    return temp;
                }
            }
            return null;
        }

        public bool AboneNoGecerlimi(string strAboneNo)
        {
            if (strAboneNo.Trim() == "" || strAboneNo.Trim().Length < 9) return false;
            string Harfler = "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZQWX";
            string Sayilar = "0123456789";
            strAboneNo = strAboneNo.ToUpper().Trim();
            foreach (char chrA in strAboneNo.Substring(0, 1))
                if (Harfler.IndexOf(chrA) < 0) return false;
            foreach (char chrA in strAboneNo.Substring(1, 6))
                if (Sayilar.IndexOf(chrA) < 0) return false;
            if (strAboneNo.Substring(7, 1) != "-") return false;
            foreach (char chrA in strAboneNo.Substring(8, strAboneNo.Length - 8))
                if (Harfler.IndexOf(chrA) < 0 && Sayilar.IndexOf(chrA) < 0) return false;
            return true;
        }

        public bool SayacNoGecerlimi(string strSayacNo)
        {
            if (strSayacNo.Trim() == "") return false;
            string Sayilar = "0123456789";
            foreach (char chrA in strSayacNo)
                if (Sayilar.IndexOf(chrA) < 0) return false;
            return true;
        }

        private void Klavye2PB_MouseDown(object sender, MouseEventArgs e)
        {
            Klavye2PB.Image = imageList1.Images[1];
            Klavye2PB.Update();
            Program.Titret(120);
        }

        private void Klavye2PB_MouseUp(object sender, MouseEventArgs e)
        {
            Klavye2PB.Image = imageList1.Images[0];
            Klavye2PB.Update();
            inputPanel1.Enabled = !inputPanel1.Enabled;
            AboneNoTXT.Focus();
        }

        private void AboneNoTXT_GotFocus(object sender, EventArgs e)
        {
            AboneNoTXT.BackColor = Color.SkyBlue;
            AboneNoTXT.ForeColor = Color.Black;
            AboneNoTXT.SelectAll();
        }

        private void AboneNoTXT_LostFocus(object sender, EventArgs e)
        {
            AboneNoTXT.BackColor = Color.White;
            AboneNoTXT.ForeColor = Color.Black;
        }

        private void CameraPreviewOnOff(bool State)
        {
            if (State)
            {
                ImagerOnOff(false);
                CameraLibNet.Api.CAMOpen();
                CameraLibNet.Api.CAMStartPreview(FotoOnizlemePB.Handle, 0, 0, CameraLibNet.Def.CAM_4PER9VGA | CameraLibNet.Def.CAM_ROTATE_180);
                KameraAcikmi = true;
            }
            else
            {
                CameraLibNet.Api.CAMStopPreview();
                CameraLibNet.Api.CAMClose();
                ImagerOnOff(true);
                KameraAcikmi = false;
            }
            CameraSetStates(State);
        }

        private void CameraSetStates(bool State)
        {
            CameraLibNet.Api.CAMSetIllumination(100);
            CameraLibNet.Api.CAMIlluminationOn(State ? IlluminationCHK.Checked : false);

            switch (IsikCMB.SelectedIndex)
            {
                case 0:
                    CameraLibNet.Api.CAMSetLightMode(CameraLibNet.Def.CAM_LIGHT_AUTO);
                    break;
                case 1:
                    CameraLibNet.Api.CAMSetLightMode(CameraLibNet.Def.CAM_OUTDOOR);
                    break;
                case 2:
                    CameraLibNet.Api.CAMSetLightMode(CameraLibNet.Def.CAM_FLUORESCENT);
                    break;
                case 3:
                    CameraLibNet.Api.CAMSetLightMode(CameraLibNet.Def.CAM_INCANDESCE);
                    break;
                case 4:
                    CameraLibNet.Api.CAMSetLightMode(CameraLibNet.Def.CAM_DIMLIGHT);
                    break;
            }

            switch (IrisCMB.SelectedIndex)
            {
                case 0:
                    CameraLibNet.Api.CAMSetIris(CameraLibNet.Def.CAM_IRIS_35);
                    break;
                case 1:
                    CameraLibNet.Api.CAMSetIris(CameraLibNet.Def.CAM_IRIS_70);
                    break;
            }

            switch (ZoomTB.Value)
            {
                case 0:
                    CameraLibNet.Api.CAMSetDigitalZoom(CameraLibNet.Def.CAM_ZOOM_NONE);
                    break;
                case 1:
                    CameraLibNet.Api.CAMSetDigitalZoom(CameraLibNet.Def.CAM_ZOOM_15);
                    break;
                case 2:
                    CameraLibNet.Api.CAMSetDigitalZoom(CameraLibNet.Def.CAM_ZOOM_20);
                    break;
                case 3:
                    CameraLibNet.Api.CAMSetDigitalZoom(CameraLibNet.Def.CAM_ZOOM_30);
                    break;
            }
        }

        private void IlluminationCHK_CheckStateChanged(object sender, EventArgs e)
        {
            if (KameraAcikmi)
                CameraSetStates(true);
        }

        private void ZoomTB_ValueChanged(object sender, EventArgs e)
        {
            if (KameraAcikmi)
                CameraSetStates(true);
        }

        private void FotoTAB_LostFocus(object sender, EventArgs e)
        {
            CameraPreviewOnOff(false);
        }

        private void IsikCMB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (KameraAcikmi)
                CameraSetStates(true);
        }

        private void IrisCMB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (KameraAcikmi)
                CameraSetStates(true);
        }

        private void FotoAciklaTemizleBTN_Click(object sender, EventArgs e)
        {
            FotografAciklamaTXT.Text = "";
        }

        private void FotoAciklaKaydetBTN_Click(object sender, EventArgs e)
        {
            if (FotografListesiLB.SelectedIndex >= 0)
                if (FotoAciklamaDosyaKaydet())
                    MessageBox.Show("Dosya kaydedildi");
                else
                    MessageBox.Show("Dosya kaydedilemedi");
        }

        private void FotoZoomSilBTN_Click(object sender, EventArgs e)
        {
            if (FotografListesiLB.SelectedIndex >= 0)
            {
                if (MessageBox.Show(FotografListesiLB.Text + " silinecek, eminmisiniz", "Fotoğraf Silinecek", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    string DosyaFotograf = FotograflarKlasoru + "\\" + FotografListesiLB.Text;
                    string DosyaAciklama = FotograflarKlasoru + "\\" + FotografListesiLB.Text.Replace(".JPG", ".TXT"); //.Substring(0, FotografListesiLB.Text.Length - "JPG".Length) + "TXT"; 

                    try
                    {
                        if (File.Exists(DosyaFotograf))
                        {
                            File.Delete(DosyaFotograf);
                            if (File.Exists(DosyaFotograf))
                            {
                                throw new System.UnauthorizedAccessException();
                            }
                            else
                            {
                                MessageBox.Show("Fotoğraf silindi " + DosyaFotograf);
                                ThumbnailPB.Image = new Bitmap(ThumbnailPB.Width, ThumbnailPB.Height);
                                Graphics g = Graphics.FromImage(ThumbnailPB.Image);
                                //g.DrawString("Silindi", new Font("Tahoma", 8, FontStyle.Regular), new SolidBrush(Color.White), new Rectangle(0, 0, ThumbnailPB.Width, ThumbnailPB.Height));
                                g.DrawLine(new Pen(Color.Red), 1, 1, ThumbnailPB.Width - 1, ThumbnailPB.Height - 1);
                                g.DrawLine(new Pen(Color.Red), ThumbnailPB.Width - 1, 1, 1, ThumbnailPB.Height - 1);
                            }
                        }
                        if (!FotografListesiniYukle())
                            MessageBox.Show("Fotoğraf listesi yenilenemedi,silinmiş resim halen listede " + DosyaFotograf);
                    }
                    catch
                    {
                        MessageBox.Show("Fotoğraf dosyası silinemedi " + DosyaFotograf, "Hata !", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    }

                    try
                    {
                        if (File.Exists(DosyaAciklama))
                        {
                            File.Delete(DosyaAciklama);
                            if (File.Exists(DosyaAciklama))
                            {
                                throw new System.UnauthorizedAccessException();
                            }
                            else
                            {
                                FotografAciklamaTXT.Text = "";
                                MessageBox.Show("Açıklama silindi " + DosyaAciklama);
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Açıklama dosyası silinemedi " + DosyaAciklama, "Hata !", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    }
                    ZoomlaniyorSimgesiGoster(false);
                }
            }
        }

        private void FotoZoomKapatBTN_Click(object sender, EventArgs e)
        {
            ZoomlaniyorSimgesiGoster(false);
        }

        private bool FotoAciklamaDosyaYukle()
        {
            try
            {
                string Dosya = FotograflarKlasoru + "\\" + FotografListesiLB.Text.Replace(".JPG", ".TXT"); //.Substring(0, FotografListesiLB.Text.Length - "JPG".Length) + "TXT";
                if (File.Exists(Dosya))
                {
                    using (TextReader tr = new StreamReader(Dosya, System.Text.Encoding.GetEncoding("windows-1254")))
                        FotografAciklamaTXT.Text = tr.ReadLine();
                    return true;
                }
                else
                {
                    FotografAciklamaTXT.Text = "Açıklama dosyası mevcut değil";
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool FotoAciklamaDosyaKaydet()
        {
            try
            {
                string Dosya = FotograflarKlasoru + "\\" + FotografListesiLB.Text.Replace(".JPG", ".TXT"); //.Substring(0, FotografListesiLB.Text.Length - "JPG".Length) + "TXT";
                if (File.Exists(Dosya))
                    File.Delete(Dosya);

                using (TextWriter tr = new StreamWriter(Dosya, false, System.Text.Encoding.GetEncoding("windows-1254")))
                    tr.Write(FotografAciklamaTXT.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void FotografListesiLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ThumbnailPB.Image != null)
                    ThumbnailPB.Image.Dispose();
                ThumbnailPB.SizeMode = PictureBoxSizeMode.StretchImage;
                ThumbnailPB.Image = new Bitmap(FotograflarKlasoru + "\\" + FotografListesiLB.Text);
                FotoAciklamaDosyaYukle();
                GC.Collect(); //OutOfMemoryException durumunda GarbaceCollector'u tetiklemek için kullanılabilir.
            }
            catch
            {
                FotografAciklamaTXT.Text = "Resim önizleme sırasında hata oldu";
            }
        }

        private void HataDosyaKaydet(string Hata,string HataKaynagi,string HataMesaji)
        {
            try
            {
                string Dosya = FotograflarKlasoru + "\\T" + Program.TerminalNo + "_ERR_" + DateTime.Today.Day.ToString().PadLeft(2, '0') + DateTime.Today.Month.ToString().PadLeft(2, '0') + DateTime.Today.Year.ToString() + "_" + DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') + DateTime.Now.Second.ToString().PadLeft(2, '0') + ".TXT";
                if (File.Exists(Dosya))
                    File.Delete(Dosya);

                using (TextWriter tr = new StreamWriter(Dosya, false, System.Text.Encoding.GetEncoding("windows-1254")))
                {
                    tr.WriteLine(Hata);
                    tr.WriteLine(HataKaynagi);
                    tr.WriteLine(HataMesaji);
                }
                return;
            }
            catch
            {
                return;
            }
        }

        private void AnaTAB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AnaTAB.TabPages[AnaTAB.SelectedIndex].Name != "FotoTAB")
                CameraPreviewOnOff(false);
        }

        private void KameraAcKapatPB_MouseDown(object sender, MouseEventArgs e)
        {
            KameraAcKapatPB.Image = imageList3.Images[13];
            KameraAcKapatPB.Update();
            Program.Titret(120);
        }

        private void KameraAcKapatPB_MouseUp(object sender, MouseEventArgs e)
        {
            KameraAcKapatPB.Image = imageList3.Images[12];
            KameraAcKapatPB.Update();
            CameraPreviewOnOff(!KameraAcikmi);
        }

        private void FotoCekPB_MouseDown(object sender, MouseEventArgs e)
        {
            FotoCekPB.Image = imageList3.Images[15];
            FotoCekPB.Update();
            Program.Titret(120);
        }

        private void FotoCekPB_MouseUp(object sender, MouseEventArgs e)
        {
            FotoCekPB.Image = imageList3.Images[14];
            FotoCekPB.Update();

            try
            {
                if (!ResimKlasoruOlustur())
                {
                    MessageBox.Show("Fotoğraflar klasörü sorunlu: " + FotograflarKlasoru);
                    FotoCekPB.Visible = false;
                    FotoCekLBL.Visible = false;
                    return;
                }

                if (KameraAcikmi)
                {
                    //string DosyaAdi = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\T" + Program.TerminalNo + "G_" + DateTime.Today.Day.ToString().PadLeft(2, '0') + DateTime.Today.Month.ToString().PadLeft(2, '0') + DateTime.Today.Year.ToString() + "_" + DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') + DateTime.Now.Second.ToString().PadLeft(2, '0') + ".JPG";
                    string DosyaAdi = FotograflarKlasoru + "\\T" + Program.TerminalNo + "G_" + DateTime.Today.Day.ToString().PadLeft(2, '0') + DateTime.Today.Month.ToString().PadLeft(2, '0') + DateTime.Today.Year.ToString() + "_" + DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') + DateTime.Now.Second.ToString().PadLeft(2, '0') + ".JPG";
                    if (CameraLibNet.Api.CAMSingleCapture(DosyaAdi, CameraLibNet.Def.CAM_JPEG, CameraLibNet.Def.CAM_UXGA) != CameraLibNet.Def.CAM_SUCCESS)
                        MessageBox.Show("Fotoğraf çekilemedi !");
                }
            }
            catch
            {
                MessageBox.Show("Fotoğraf çekilemedi !");
            }
        }

        private void ThumbnailPB_MouseDown(object sender, MouseEventArgs e)
        {
            ThumbnailPB.Left += 5;
            ThumbnailPB.Top += 5;
            ThumbnailCerceveLBL.Left += 5;
            ThumbnailCerceveLBL.Top += 5;
            ThumbnailPB.Update();
            panel1.Invalidate();
            panel1.Update();
            Program.Titret(120);
        }

        private void ThumbnailPB_MouseUp(object sender, MouseEventArgs e)
        {
            ThumbnailPB.Left -= 5;
            ThumbnailPB.Top -= 5;
            ThumbnailCerceveLBL.Left -= 5;
            ThumbnailCerceveLBL.Top -= 5;
            ThumbnailPB.Update();
            if (ThumbnailPB.Image == null)
                return;
            if (FotoZoomPB.Image != null)
                FotoZoomPB.Image.Dispose();
            FotoZoomPB.SizeMode = PictureBoxSizeMode.StretchImage;
            FotoZoomPB.Image = new Bitmap(ThumbnailPB.Image);
            FotoZoomLBL.Text = FotografListesiLB.Text;
            panel1.Invalidate();
            panel1.Update();
            ZoomlaniyorSimgesiGoster(true);
        }

        private void FotoDetayTAB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FotoDetayTAB.TabPages[FotoDetayTAB.SelectedIndex].Name != "FotoGoruntuTAB")
                CameraPreviewOnOff(false);

            if (FotoDetayTAB.TabPages[FotoDetayTAB.SelectedIndex].Name == "FotoEkBilgiTAB")
                FotografListesiniYukle();
        }

        private void BildirimListeYenilePB_MouseDown(object sender, MouseEventArgs e)
        {
            BildirimListeYenilePB.Image = imageList3.Images[9];
            BildirimListeYenilePB.Update();
            Program.Titret(120);
        }

        private void BildirimListeYenilePB_MouseUp(object sender, MouseEventArgs e)
        {
            BildirimListeYenilePB.Image = imageList3.Images[8];
            BildirimListeYenilePB.Update();
            if (!BildirimListesiniYukle())
                MessageBox.Show("Hata Kodları Okunamadı");
            else
                MessageBox.Show("Bildirim listesi yüklendi", "Durum Raporu", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
        }

        private void BildirimBasPB_MouseDown(object sender, MouseEventArgs e)
        {
            BildirimBasPB.Image = imageList3.Images[11];
            BildirimBasPB.Update();
            Program.Titret(120);
        }

        private void BildirimBasPB_MouseUp(object sender, MouseEventArgs e)
        {
            BildirimBasPB.Image = imageList3.Images[10];
            BildirimBasPB.Update();
            if ((object)BasilacakMesajTXT.Text != null && BasilacakMesajTXT.Text.Trim() != "" && BasilacakMesajTXT.Text.Length > 10)
            {
                BildirimYaz(BasilacakMesajTXT.Text.Substring(BasilacakMesajTXT.Text.IndexOf("- ") + 2, BasilacakMesajTXT.Text.Length - (BasilacakMesajTXT.Text.IndexOf("- ") + 2)));
            }
        }

        private void FaturaMesajTemizlePB_MouseDown(object sender, MouseEventArgs e)
        {
            FaturaMesajTemizlePB.Image = imageList3.Images[7];
            FaturaMesajTemizlePB.Update();
            Program.Titret(120);
        }

        private void FaturaMesajTemizlePB_MouseUp(object sender, MouseEventArgs e)
        {
            FaturaMesajTemizlePB.Image = imageList3.Images[6];
            FaturaMesajTemizlePB.Update();
            FaturaMesajTXT.Text = "";
        }

        private void EndeksTXT_GotFocus(object sender, EventArgs e)
        {
            EndeksTXT.BackColor = Color.SkyBlue;
            EndeksTXT.ForeColor = Color.Black;
            EndeksTXT.SelectAll();
        }

        private void EndeksTXT_LostFocus(object sender, EventArgs e)
        {
            EndeksTXT.BackColor = Color.White;
            EndeksTXT.ForeColor = Color.Black;
        }

        private bool ResimKlasoruOlustur()
        {
            try
            {
                FotograflarKlasoru = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Fotograflar";
                if (Directory.Exists(FotograflarKlasoru))
                    return true;
                else
                    Directory.CreateDirectory(FotograflarKlasoru);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool FotografListesiniYukle()
        {
            try
            {
                FotografListesiLB.Enabled = true;
                FotografAciklamaTXT.Enabled = true;
                FotografListesiLB.Items.Clear();

                string[] FotografDosyalari = Directory.GetFiles(FotograflarKlasoru + "\\", "*.JPG");
                if (FotografDosyalari.GetUpperBound(0) < 0)
                {
                    FotografListesiLB.Enabled = false;
                    FotografAciklamaTXT.Text = "Herhangi bir fotoğraf dosyası bulunamadı";
                    FotografAciklamaTXT.Enabled = false;
                    return true;
                }
                else
                {
                    Array.Sort(FotografDosyalari);
                    foreach (string Dosya in FotografDosyalari)
                        FotografListesiLB.Items.Add(Dosya.Substring((FotograflarKlasoru + "\\").Length, Dosya.Length - (FotograflarKlasoru + "\\").Length));
                    FotografAciklamaTXT.Text = "";
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AyarKaydetPB_MouseDown(object sender, MouseEventArgs e)
        {
            AyarKaydetPB.Image = imageList3.Images[19];
            AyarKaydetPB.Update();
            Program.Titret(120);
        }

        private void AyarKaydetPB_MouseUp(object sender, MouseEventArgs e)
        {
            AyarKaydetPB.Image = imageList3.Images[18];
            AyarKaydetPB.Update();
            if(!AyarlariKaydetRegistry())
                MessageBox.Show("Ayarlar kaydedilirken hata oluştu !");
            else
                MessageBox.Show("Ayarlar kaydedildi");
        }

        private void AyarOkuPB_MouseDown(object sender, MouseEventArgs e)
        {
            AyarOkuPB.Image = imageList3.Images[21];
            AyarOkuPB.Update();
            Program.Titret(120);
        }

        private void AyarOkuPB_MouseUp(object sender, MouseEventArgs e)
        {
            AyarOkuPB.Image = imageList3.Images[20];
            AyarOkuPB.Update();
            if (!AyarlariYukleRegistry())
                MessageBox.Show("Ayarlar okunurken hata oluştu !");
            else
                MessageBox.Show("Ayarlar okundu");
        }

        private bool AyarlariYukleRegistry()
        {
            try
            {
                RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\OFMC", true);
                SolTriggerCMB.SelectedIndex = (int)Key.GetValue("SolTrigger");
                SagTriggerCMB.SelectedIndex = (int)Key.GetValue("SagTrigger");
                Key.Close();
                SolTriggerFonksiyonu = SolTriggerCMB.SelectedIndex;
                SagTriggerFonksiyonu = SagTriggerCMB.SelectedIndex;
                return true;
            }
            catch
            {
                SolTriggerFonksiyonu = SolTriggerCMB.SelectedIndex;
                SagTriggerFonksiyonu = SagTriggerCMB.SelectedIndex;
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
                Key.SetValue("SolTrigger", SolTriggerCMB.SelectedIndex);
                Key.SetValue("SagTrigger", SagTriggerCMB.SelectedIndex);
                Key.Close();
                SolTriggerFonksiyonu = SolTriggerCMB.SelectedIndex;
                SagTriggerFonksiyonu = SagTriggerCMB.SelectedIndex;
                return true;
            }
            catch
            {
                SolTriggerFonksiyonu = SolTriggerCMB.SelectedIndex;
                SagTriggerFonksiyonu = SagTriggerCMB.SelectedIndex;
                return false;
            }
        }

        private void Klavye3PB_MouseDown(object sender, MouseEventArgs e)
        {
            Klavye3PB.Image = imageList1.Images[1];
            Klavye3PB.Update();
            Program.Titret(120);
        }

        private void Klavye3PB_MouseUp(object sender, MouseEventArgs e)
        {
            Klavye3PB.Image = imageList1.Images[0];
            Klavye3PB.Update();
            inputPanel1.Enabled = !inputPanel1.Enabled;
            FotografAciklamaTXT.Focus();
        }

        private void TahakkukluFaturalarGRD_CurrentCellChanged(object sender, EventArgs e)
        {
            TahakkukluFaturalarGRDSatir = TahakkukluFaturalarGRD.CurrentCell.RowNumber;
            TahakkukluFaturalarGRD.Select(TahakkukluFaturalarGRDSatir);
        }

        private void SuretListelePB_MouseDown(object sender, MouseEventArgs e)
        {
            SuretListelePB.Image = imageList3.Images[23];
            SuretListelePB.Update();
            Program.Titret(120);
        }

        private void SuretListelePB_MouseUp(object sender, MouseEventArgs e)
        {
            SuretListelePB.Image = imageList3.Images[22];
            SuretListelePB.Update();
            if (!SuretFaturalariListele())
                MessageBox.Show("Fatura Listesi Yüklenemedi");
            else
                MessageBox.Show("Faturalar Listelendi", "Durum Raporu", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
        }

        private void SuretBasPB_MouseDown(object sender, MouseEventArgs e)
        {
            SuretBasPB.Image = imageList3.Images[25];
            SuretBasPB.Update();
            Program.Titret(120);
        }

        private void SuretBasPB_MouseUp(object sender, MouseEventArgs e)
        {
            SuretBasPB.Image = imageList3.Images[24];
            SuretBasPB.Update(); 
            int satir = TahakkukluFaturalarGRDSatir;
            SuretFaturaYaz(TahakkukluFaturalarGRD[satir, 5].ToString(), TahakkukluFaturalarGRD[satir, 6].ToString(), TahakkukluFaturalarGRD[satir, 1].ToString(), TahakkukluFaturalarGRD[satir, 2].ToString());
            TahakkukluFaturalarGRD.Select(satir);
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

        /*************************Contrast********************************************/
        public static bool ApplyContrast(Bitmap b, sbyte nContrast)
        {
            if (nContrast < -100) return false;
            if (nContrast > 100) return false;

            double pixel = 0, contrast = (100.0 + nContrast) / 100.0;

            contrast *= contrast;

            int red, green, blue;

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - b.Width * 3;
                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];

                        pixel = red / 255.0;
                        pixel -= 0.5;
                        pixel *= contrast;
                        pixel += 0.5;
                        pixel *= 255;
                        if (pixel < 0) pixel = 0;
                        if (pixel > 255) pixel = 255;
                        p[2] = (byte)pixel;

                        pixel = green / 255.0;
                        pixel -= 0.5;
                        pixel *= contrast;
                        pixel += 0.5;
                        pixel *= 255;
                        if (pixel < 0) pixel = 0;
                        if (pixel > 255) pixel = 255;
                        p[1] = (byte)pixel;

                        pixel = blue / 255.0;
                        pixel -= 0.5;
                        pixel *= contrast;
                        pixel += 0.5;
                        pixel *= 255;
                        if (pixel < 0) pixel = 0;
                        if (pixel > 255) pixel = 255;
                        p[0] = (byte)pixel;

                        p += 3;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);
            return true;
        }
        /*************************Contrast********************************************/

        /*************************Brightness********************************************/
        public static bool ApplyBrightness(Bitmap b, int nBrightness)
        {
            if (nBrightness < -255 || nBrightness > 255)
                return false;

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            int nVal = 0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 3;
                int nWidth = b.Width * 3;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        nVal = (int)(p[0] + nBrightness);

                        if (nVal < 0) nVal = 0;
                        if (nVal > 255) nVal = 255;

                        p[0] = (byte)nVal;

                        ++p;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);

            return true;
        }
        /*************************Brightness********************************************/
    }
}
