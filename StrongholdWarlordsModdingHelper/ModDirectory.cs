using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Windows;

namespace StrongholdWarlordsModdingHelper
{
    class ModDirectory
    {
        private string modDirectory;
        private bool isDirty = true;
        private List<Mod> mods = new();

        public bool IsDirty { get => isDirty; set => isDirty = value; }
        internal List<Mod> Mods { get => mods; set => mods = value; }

        public ModDirectory(string modDirectory)
        {
            this.modDirectory = modDirectory;
        }

        public void InsertMod(Mod mod, int index)
        {
            if (mods.Any(iteratorMod => iteratorMod.ModId == mod.ModId))
                MessageBox.Show("A mod with the id " + mod.ModId + " is already present.", "Mod already present", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else
            {
                mods.Insert(index, mod);
                isDirty = true;
            }
        }

        public void AddMod(Mod mod)
        {
            if (mods.Any(iteratorMod => iteratorMod.ModId == mod.ModId))
                MessageBox.Show("A mod with the id \"" + mod.ModId + "\" is already present.", "Mod already present", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else
            {
                mods.Insert(0, mod);
                isDirty = true;
            }
        }

        public void RemoveMod(Mod mod)
        {
            mods.RemoveAll(modPred => modPred.ModId == mod.ModId);
            isDirty = true;
        }

        public void RemoveMod(int index)
        {
            mods.RemoveAt(index);
            isDirty = true;
        }

        public void LoadModConfiguration()
        {
            XmlDocument xmlDocument = new XmlDocument();

            using(FileStream fileStream = new FileStream(modDirectory + "/ModConfig.xml", FileMode.Open))
            {
                xmlDocument.Load(fileStream);
            }

            XmlNodeList list = xmlDocument.DocumentElement.GetElementsByTagName("ModEntry");

            List<ModDataStruct> modData = new List<ModDataStruct>();

            foreach(XmlNode xmlNode in list)
            {
                string path = xmlNode.Attributes.GetNamedItem("Path").Value;
                bool isEnabled = bool.Parse(xmlNode.Attributes.GetNamedItem("Enabled").Value);
                int modIndex = int.Parse(xmlNode.Attributes.GetNamedItem("ModIndex").Value);

                ModDataStruct modDataStruct = new ModDataStruct();
                modDataStruct.path = path;
                modDataStruct.isEnabled = isEnabled;
                modDataStruct.modIndex = modIndex;

                modData.Add(modDataStruct);
            }

            modData.Sort((x, y) => x.modIndex - y.modIndex);

            for(int i = 0; i < modData.Count; i++)
            {
                ModDataStruct modDataStruct = modData[i];

                if(!File.Exists(modDataStruct.path))
                {
                    MessageBox.Show("The mod file at the path " + modDataStruct + " wasn't found. Removing mod from mod list...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    try
                    {
                        Mod mod = Mod.LoadMod(modDataStruct.path);
                        mod.Enabled = modDataStruct.isEnabled;
                        mods.Add(mod);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("The mod at the location " + modDataStruct.path + " is malformed.\n\nReason:\n" + ex.Message, "Mod loading error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            isDirty = true;
        }

        public void SaveModConfiguration()
        {
            Directory.CreateDirectory(modDirectory);

            XmlDocument xmlDocument = new XmlDocument();

            XmlElement element = xmlDocument.CreateElement("ModConfig");
            xmlDocument.AppendChild(element);

            for(int i = 0; i < mods.Count; i++)
            {
                Mod currentMod = mods[i];
                XmlElement modEntry = xmlDocument.CreateElement("ModEntry");
                modEntry.SetAttribute("Path", currentMod.FilePath);
                modEntry.SetAttribute("ModIndex", i.ToString());
                modEntry.SetAttribute("Enabled", currentMod.Enabled.ToString());

                element.AppendChild(modEntry);
            }

            xmlDocument.Save(modDirectory + "/ModConfig.xml");
            isDirty = false;
        }
    }

    struct ModDataStruct
    {
        public string path;
        public bool isEnabled;
        public int modIndex;
    }
}
