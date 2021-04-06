using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace StrongholdWarlordsModdingHelper
{
    class Mod
    {
        private string name;
        private string modId;
        private string description;
        private string filePath;
        private bool enabled;

        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }
        public string FilePath { get => filePath; set => filePath = value; }
        public bool Enabled { get => enabled; set => enabled = value; }
        public string ModId { get => modId; set => modId = value; }

        public Mod(string name, string description, string modId, string filePath)
        {
            this.name = name;
            this.description = description;
            this.filePath = filePath;
            this.modId = modId;
            this.enabled = false;
        }

        public static Mod LoadMod(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The given file " + path + " doesn't exist.");

            string modName = null;
            string modDescription = null;
            string modId = null;

            using(ZipArchive file = ZipFile.OpenRead(path))
            {
                if (!file.Entries.Any(archive => archive.FullName == "ModInfo.xml"))
                    throw new FileNotFoundException("The given archive " + path + " does exist, but it doesn't have a mod info.");

                XmlDocument modInfo = new XmlDocument();
                using(Stream modInfoStream = file.GetEntry("ModInfo.xml").Open())
                {
                    modInfo.Load(modInfoStream);
                }

                XmlNodeList nodeList = modInfo.DocumentElement.GetElementsByTagName("ModMetadata");
                if (nodeList.Count != 1)
                    throw new MalformedModFileException("The mod file is malformed. Reason: The mod metadata is not complete.");

                XmlNode node = nodeList[0];

                modName = node.Attributes.GetNamedItem("ModName").Value;
                // Escaped description
                modDescription = node.Attributes.GetNamedItem("ModDescription").Value.Replace("\\n", "\n");

                modId = node.Attributes.GetNamedItem("ModId").Value;
            }

            return new Mod(modName, modDescription, modId, path);
        }
    }
}
