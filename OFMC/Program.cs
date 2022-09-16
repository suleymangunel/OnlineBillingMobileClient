using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Configuration;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Calib;


namespace OnlineFaturaMobileClient
{
    static class Program
    {
        public static string TerminalNo = "-1";
        public static string PersonelKod = "1";
        public static string PersonelSicilNo = "1";
        public static string PersonelAd = "1";
        public static string PersonelSoyad = "1";
        public static string ServisURLdahili = "";
        public static string ServisURLharici = "";
        public static string ServisURL = ServisURLdahili;
        public static bool GPSLisansli = false;
        public static bool SMSLisansli = false;
        public static string LisansliModuller = "";
        public static bool LoginOK = false;
        public static IntPtr phSIM = new IntPtr();
        public static IntPtr phSMS = new IntPtr();
        public static IntPtr phSMSMessageAvailableEvent = new IntPtr();
        public static int MenuKey = 236;
        public static string SvcUser = "_CobraSpaceAdventure_";
        public static string SvcPass = "_TimeMachine_";
        public static bool UserDefineKeyState = false;

        [MTAThread]
        [DllImport("coredll")]
        private extern static void SignalStarted(uint dword);
        
        [DllImport("coredll.dll", SetLastError = true)]
        private static extern IntPtr CreateEvent(IntPtr securityAttributes, bool IsManual, bool initialState, string name);
        
        [DllImport("coredll.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
 
        static void Main(string[] args)
        {
            EnableAutoStart(args);

            if (!RegistryKayitlariOlustur())
            {
                MessageBox.Show("Registry kayıtları oluşturulamadı !");
                Application.Exit();
            }

            switch (AppConfigOlustur())
            {
                case 0:
                    ServisURLdahili = AppSettings.Settings.ServisURLdahili;
                    ServisURLharici = AppSettings.Settings.ServisURLharici;
                    ServisURL = ServisURLdahili;

                    var LoginForm = new LoginFRM();
                    if (LoginForm.ShowDialog() == DialogResult.OK && Program.LoginOK)
                    {
                        Application.Run(new AnaFRM());
                    }
                    else
                    {
                        KeyboardLightOnOff(false);
                        Application.Exit();
                    }

                    break;

                case 1:
                    MessageBox.Show("App.Config dosyası yeniden oluşturuldu, lütfen dosyadaki bağlantıları düzenleyip, programı tekrar çalıştırın.");
                    Application.Exit();
                    break;

                case -1:
                    MessageBox.Show("App.Config dosyası bulunamadı ve yeniden oluşturulamadı !");
                    break;
            }

        }

        public static void EnableAutoStart(string[] args)
        {
            if (args.Length > 0)
                SignalStarted(uint.Parse(args[0]));

            RegistryKey key = Registry.LocalMachine.OpenSubKey("init", true);

            if (key.GetValue("Launch99") == null)
                key.SetValue("Launch99", System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            key.Close();
        }

        public static XElement ToXElement(this XmlElement xml)
        {
            //Bu sınıf sayesinde XElement türüne "ToXElement" fonksiyonu eklenmiş olacak.
            //Bu sayede XmlElement nesnelerinin XElement'e dönüşümü sağlanabilecek.
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.ImportNode(xml, true));
            return XElement.Parse(doc.InnerXml);
        }

        public static XmlElement ToXmlElement(this XElement xelement)
        {
            return new XmlDocument().ReadNode(xelement.CreateReader()) as XmlElement;
        }

        public static bool Sayimi(this String Deger)
        {
            Deger = Deger.Trim();
            if (Deger.Length <= 0) return false;
            string Sayilar = "0123456789";
            foreach (char chrD in Deger)
                if (Sayilar.IndexOf(chrD) < 0) return false;
            return true;
        }

        public static bool WWANCalistir()
        {
            try
            {
                phSIM = new IntPtr(0);
                if (!WWAN_PowerOn()) return false;
                if (!WWAN_HandleSIM()) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WWANDurdur()
        {
            try
            {
                if (!WWAN_ReleaseSIM()) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WWAN_PowerOn()
        {
            try
            {
                string HataMesaji = "";
                WANGPRSLibNet.Def.PowerStatus GucDurumu;
                int sonuc = WANGPRSLibNet.Api.WanGetPowerStatus(out GucDurumu);
                if (sonuc == WANGPRSLibNet.Def.WAN_ERROR_SUCCESS)
                {
                    if (GucDurumu != WANGPRSLibNet.Def.PowerStatus.WAN_MODULE_POWER_ON)
                    {
                        sonuc = WANGPRSLibNet.Api.WanSetPowerStatus(WANGPRSLibNet.Def.PowerStatus.WAN_MODULE_POWER_ON);
                        System.Threading.Thread.Sleep(5000);
                        if (sonuc != WANGPRSLibNet.Def.WAN_ERROR_SUCCESS)
                        {
                            MessageBox.Show("WAN_ERROR_FAIL", "SET POWER STATE", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                            return false;
                        }
                    }
                }
                else
                {
                    if (GucDurumu == WANGPRSLibNet.Def.PowerStatus.WAN_MODULE_POWER_ON)
                        HataMesaji = "WAN_MODULE_POWER_ON";
                    if (GucDurumu == WANGPRSLibNet.Def.PowerStatus.WAN_MODULE_POWER_OFF)
                        HataMesaji = "WAN_MODULE_POWER_OFF";
                    if (GucDurumu == WANGPRSLibNet.Def.PowerStatus.WAN_MODULE_POWER_UNKNOWN)
                        HataMesaji = "WAN_MODULE_POWER_UNKNOWN";

                    if (sonuc == WANGPRSLibNet.Def.WAN_ERROR_SUCCESS)
                        HataMesaji += "/WAN_ERROR_SUCCESS";
                    if (sonuc == WANGPRSLibNet.Def.WAN_ERROR_BADPARAM)
                        HataMesaji += "/WAN_ERROR_BADPARAM";
                    if (sonuc == WANGPRSLibNet.Def.WAN_ERROR_FAIL)
                        HataMesaji += "/WAN_ERROR_FAIL";
                    MessageBox.Show(HataMesaji, "GET POWER STATE", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WWAN_PowerOff()
        {
            try
            {
                string HataMesaji = "";
                WANGPRSLibNet.Def.PowerStatus GucDurumu;
                int sonuc = WANGPRSLibNet.Api.WanGetPowerStatus(out GucDurumu);
                if (sonuc == WANGPRSLibNet.Def.WAN_ERROR_SUCCESS)
                {
                    if (GucDurumu != WANGPRSLibNet.Def.PowerStatus.WAN_MODULE_POWER_OFF)
                    {
                        sonuc = WANGPRSLibNet.Api.WanSetPowerStatus(WANGPRSLibNet.Def.PowerStatus.WAN_MODULE_POWER_OFF);
                        System.Threading.Thread.Sleep(5000);
                        if (sonuc != WANGPRSLibNet.Def.WAN_ERROR_SUCCESS)
                        {
                            MessageBox.Show("WAN_ERROR_FAIL", "SET POWER STATE", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                            return false;
                        }
                    }
                }
                else
                {
                    if (GucDurumu == WANGPRSLibNet.Def.PowerStatus.WAN_MODULE_POWER_ON)
                        HataMesaji = "WAN_MODULE_POWER_ON";
                    if (GucDurumu == WANGPRSLibNet.Def.PowerStatus.WAN_MODULE_POWER_OFF)
                        HataMesaji = "WAN_MODULE_POWER_OFF";
                    if (GucDurumu == WANGPRSLibNet.Def.PowerStatus.WAN_MODULE_POWER_UNKNOWN)
                        HataMesaji = "WAN_MODULE_POWER_UNKNOWN";

                    if (sonuc == WANGPRSLibNet.Def.WAN_ERROR_SUCCESS)
                        HataMesaji += "/WAN_ERROR_SUCCESS";
                    if (sonuc == WANGPRSLibNet.Def.WAN_ERROR_BADPARAM)
                        HataMesaji += "/WAN_ERROR_BADPARAM";
                    if (sonuc == WANGPRSLibNet.Def.WAN_ERROR_FAIL)
                        HataMesaji += "/WAN_ERROR_FAIL";
                    MessageBox.Show(HataMesaji, "GET POWER STATE", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WWAN_HandleSIM()
        {
            try
            {
                int sonuc = WANGPRSLibNet.Api.WanSimInitialize(out Program.phSIM);
                if (sonuc != WANGPRSLibNet.Def.WAN_ERROR_SUCCESS || Convert.ToInt64(Program.phSIM.ToString()) <= 0)
                {
                    if (sonuc == WANGPRSLibNet.Def.WAN_ERROR_BADPARAM)
                        MessageBox.Show("WAN_ERROR_BADPARAM / " + Program.phSIM.ToString());
                    if (sonuc == WANGPRSLibNet.Def.WAN_ERROR_BUSY)
                        MessageBox.Show("WAN_ERROR_BUSY / " + Program.phSIM.ToString());
                    if (sonuc == WANGPRSLibNet.Def.WAN_ERROR_FAIL)
                        MessageBox.Show("WAN_ERROR_FAIL / " + Program.phSIM.ToString());
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WWAN_ReleaseSIM()
        {
            try
            {
                if (Convert.ToInt64(Program.phSIM.ToString()) > 0)
                {
                    if (WANGPRSLibNet.Api.WanSimDeinitialize(Program.phSIM) != WANGPRSLibNet.Def.WAN_ERROR_SUCCESS)
                        return false;
                    else
                        phSIM = new IntPtr(0);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool KeyboardLightState()
        {
            try
            {
                //MessageBox.Show(Calib.SystemLibNet.Api.SysGetKeyBackLightState().ToString());
                if (Calib.SystemLibNet.Api.SysGetKeyBackLightState() == 0)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }

        public static void KeyboardLightOnOff(bool State)
        {
            try
            {
                if (State) 
                    Calib.SystemLibNet.Api.SysKeyBackLightOn();
                else
                    Calib.SystemLibNet.Api.SysKeyBackLightOff();
            }
            catch
            {
                return;
            }
        }

        public static bool SetFunctionKeys_CASIO()
        {
            try
            {
                int[] F1Buff = new int[16] { 201, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int[] F2Buff = new int[16] { 202, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int[] F3Buff = new int[16] { 203, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int[] F4Buff = new int[16] { 204, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_NUM, Calib.SystemLibNet.Def.KEYID_F1, F1Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_NUM, Calib.SystemLibNet.Def.KEYID_F2, F2Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_NUM, Calib.SystemLibNet.Def.KEYID_F3, F3Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_NUM, Calib.SystemLibNet.Def.KEYID_F4, F4Buff);

                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHA, Calib.SystemLibNet.Def.KEYID_F1, F1Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHA, Calib.SystemLibNet.Def.KEYID_F2, F2Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHA, Calib.SystemLibNet.Def.KEYID_F3, F3Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHA, Calib.SystemLibNet.Def.KEYID_F4, F4Buff);

                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHAS, Calib.SystemLibNet.Def.KEYID_F1, F1Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHAS, Calib.SystemLibNet.Def.KEYID_F2, F2Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHAS, Calib.SystemLibNet.Def.KEYID_F3, F3Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHAS, Calib.SystemLibNet.Def.KEYID_F4, F4Buff);

                Calib.SystemLibNet.Api.SysSetUserDefineKeyState(true);

                Calib.SystemLibNet.Api.SysSetFnKeyLock(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool RestoreFunctionKeys_CASIO()
        {
            try
            {
                int[] F1Buff = new int[16] { 0x70, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int[] F2Buff = new int[16] { 0x71, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int[] F3Buff = new int[16] { 0x72, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int[] F4Buff = new int[16] { 0x73, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_NUM, Calib.SystemLibNet.Def.KEYID_F1, F1Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_NUM, Calib.SystemLibNet.Def.KEYID_F2, F2Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_NUM, Calib.SystemLibNet.Def.KEYID_F3, F3Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_NUM, Calib.SystemLibNet.Def.KEYID_F4, F4Buff);

                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHA, Calib.SystemLibNet.Def.KEYID_F1, F1Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHA, Calib.SystemLibNet.Def.KEYID_F2, F2Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHA, Calib.SystemLibNet.Def.KEYID_F3, F3Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHA, Calib.SystemLibNet.Def.KEYID_F4, F4Buff);

                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHAS, Calib.SystemLibNet.Def.KEYID_F1, F1Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHAS, Calib.SystemLibNet.Def.KEYID_F2, F2Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHAS, Calib.SystemLibNet.Def.KEYID_F3, F3Buff);
                Calib.SystemLibNet.Api.SysSetUserDefineKey(Calib.SystemLibNet.Def.KEY_MODE_ALPHAS, Calib.SystemLibNet.Def.KEYID_F4, F4Buff);

                Calib.SystemLibNet.Api.SysSetUserDefineKeyState(false);

                Calib.SystemLibNet.Api.SysSetFnKeyLock(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Uyar()
        {
            //Calib.SystemLibNet.Api.SysSetVibratorMute(Calib.SystemLibNet.Def.B_USERDEF, false);
            //Calib.SystemLibNet.Api.SysPlayVibrator(Calib.SystemLibNet.Def.B_USERDEF, 2, 1000, 1000);

            Calib.SystemLibNet.Api.SysSetBuzzerMute(Calib.SystemLibNet.Def.B_USERDEF, false);
            Calib.SystemLibNet.Api.SysSetBuzzerVolume(Calib.SystemLibNet.Def.B_USERDEF, Calib.SystemLibNet.Def.BUZZERVOLUME_MAX);
            for (int i = 3500; i > 1000; i -= 5)
                Calib.SystemLibNet.Api.SysPlayBuzzer(Calib.SystemLibNet.Def.B_USERDEF, i, 10);
            Calib.SystemLibNet.Api.SysSetBuzzerVolume(Calib.SystemLibNet.Def.B_USERDEF, Calib.SystemLibNet.Def.BUZZERVOLUME_MID);
        }

        public static void Titret(int Sure)
        {
            Calib.SystemLibNet.Api.SysSetVibratorMute(Calib.SystemLibNet.Def.B_USERDEF, false);
            Calib.SystemLibNet.Api.SysPlayVibrator(Calib.SystemLibNet.Def.B_USERDEF, 1, Sure, 0);
        }

        public static int AppConfigOlustur()
        {
            try
            {
                string DosyaAdi = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\app.config";

                if (File.Exists(DosyaAdi))
                {
                    return 0;
                }
                else
                {
                    string[] app = new string[10];
                    app[0] = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
                    app[1] = "<configuration>";
                    app[2] = "  <appSettings>";
                    app[3] = "    <add key=\"ServisURLdahili\" value=\"http://127.0.0.1/onlinefatura/service1.svc\"/>";
                    app[4] = "    <add key=\"ServisURLharici\" value=\"http://127.0.0.1:0000/onlinefatura/service1.svc\"/>";
                    app[5] = "  </appSettings>";
                    app[6] = "</configuration>";

                    FileStream fStream = File.Open(DosyaAdi, FileMode.OpenOrCreate);
                    StreamWriter sWriter = new StreamWriter(fStream);
                    int i = 0;
                    while (i <= 6)
                    {
                        sWriter.WriteLine(app[i]);
                        i++;
                    }
                    sWriter.Close();
                    fStream.Close();
                    return 1;
                }
            }
            catch
            {
                return -1;
            }
        }

        public static bool RegistryKayitlariOlustur()
        {
            try
            {
                RegistryKey KeyKontrol = Registry.CurrentUser.OpenSubKey("OFMC");
                if (KeyKontrol == null)
                    Registry.CurrentUser.CreateSubKey(@"Software\OFMC");
                RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\OFMC", true);
                if (Key.GetValue("SolTrigger") == null)
                    Key.SetValue("SolTrigger", 1);
                if (Key.GetValue("SagTrigger") == null)
                    Key.SetValue("SagTrigger", 1);
                if (Key.GetValue("BaglantiDahilimi") == null)
                    Key.SetValue("BaglantiDahilimi", 0);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}