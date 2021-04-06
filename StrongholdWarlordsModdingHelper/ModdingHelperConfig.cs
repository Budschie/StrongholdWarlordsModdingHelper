using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace StrongholdWarlordsModdingHelper
{
    class ModdingHelperConfig
    {
        private string lastStrongholdWarlordsLocation = null;
        private string lastModLocation = null;

        public string LastStrongholdWarlordsLocation { get => lastStrongholdWarlordsLocation; set => lastStrongholdWarlordsLocation = value; }
        public string LastModLocation { get => lastModLocation; set => lastModLocation = value; }

        public ModdingHelperConfig()
        {

        }

        public void Serialize()
        {
            XmlDocument config = new XmlDocument();
            config.AppendChild(config.CreateElement("Config"));

            XmlElement lastStrongholdWarlordsLocation = config.CreateElement("StrongholdWarlordsLocation");
            lastStrongholdWarlordsLocation.InnerText = this.lastStrongholdWarlordsLocation;

            XmlElement lastModLocation = config.CreateElement("ModLocation");
            lastModLocation.InnerText = this.lastModLocation;

            config.FirstChild.AppendChild(lastStrongholdWarlordsLocation);
            config.FirstChild.AppendChild(lastModLocation);

            config.Save("config.xml");
        }

        public void Deserialize()
        {
            if(File.Exists("config.xml"))
            {
                XmlDocument config = new XmlDocument();
                config.Load("config.xml");

                XmlElement rootElement = config.DocumentElement;

                XmlNodeList lastStrongholdWarlordsLocationList = rootElement.GetElementsByTagName("StrongholdWarlordsLocation");
                
                if(lastStrongholdWarlordsLocationList.Count == 1)
                {
                    this.lastStrongholdWarlordsLocation = lastStrongholdWarlordsLocationList[0].InnerText;
                }

                XmlNodeList lastModLocationList = rootElement.GetElementsByTagName("ModLocation");

                if (lastModLocationList.Count == 1)
                {
                    this.lastModLocation = lastModLocationList[0].InnerText;
                }
            }
        }
    }
}
