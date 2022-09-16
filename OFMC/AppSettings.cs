using System;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace OnlineFaturaMobileClient
{
    class AppSettings
    {
        public class Settings
        {
            private static NameValueCollection m_settings;
            private static string m_settingsPath;

            static Settings()
            {
                m_settingsPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                m_settingsPath += @"\app.config";

                if (!File.Exists(m_settingsPath))
                    throw new FileNotFoundException(m_settingsPath + " bulunamadı.");

                System.Xml.XmlDocument xdoc = new XmlDocument();
                xdoc.Load(m_settingsPath);
                XmlElement root = xdoc.DocumentElement;
                System.Xml.XmlNodeList nodeList = root.ChildNodes.Item(0).ChildNodes;

                // Add settings to the NameValueCollection.
                m_settings = new NameValueCollection();
                m_settings.Add("ServisURLdahili", nodeList.Item(0).Attributes["value"].Value);
                m_settings.Add("ServisURLharici", nodeList.Item(1).Attributes["value"].Value);
            }

            public static void Update()
            {
                XmlTextWriter tw = new XmlTextWriter(m_settingsPath, System.Text.UTF8Encoding.UTF8);
                tw.WriteStartDocument();
                tw.WriteStartElement("configuration");
                tw.WriteStartElement("appSettings");

                for (int i = 0; i < m_settings.Count; ++i)
                {
                    tw.WriteStartElement("add");
                    tw.WriteStartAttribute("key", string.Empty);
                    tw.WriteRaw(m_settings.GetKey(i));
                    tw.WriteEndAttribute();

                    tw.WriteStartAttribute("value", string.Empty);
                    tw.WriteRaw(m_settings.Get(i));
                    tw.WriteEndAttribute();
                    tw.WriteEndElement();
                }

                tw.WriteEndElement();
                tw.WriteEndElement();

                tw.Close();
            }

            public static string ServisURLdahili
            {
                get { return m_settings.Get("ServisURLdahili"); }
                set { m_settings.Set("ServisURLdahili", value); }
            }

            public static string ServisURLharici
            {
                get { return m_settings.Get("ServisURLharici"); }
                set { m_settings.Set("ServisURLharici", value); }
            }
        }
    }
}
