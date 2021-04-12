using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Xml;

namespace StrongholdWarlordsModdingHelper
{
    class ModApplierHandler
    {
        private string assetsFolder;

        public ModApplierHandler(string assetsFolder)
        {
            this.assetsFolder = assetsFolder;
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public void CreateBackup()
        {
            if(!File.Exists(assetsFolder + "/textures.ultrahigh.pak.backup"))
                File.Copy(assetsFolder + "/textures.ultrahigh.pak", assetsFolder + "/textures.ultrahigh.pak.backup");

            if (!File.Exists(assetsFolder + "/config.xml.backup"))
                File.Copy(assetsFolder + "/config.xml", assetsFolder + "/config.xml.backup");

            if (!Directory.Exists(assetsFolder + "/castlesBACKUP"))
            {
                Copy(assetsFolder + "/castles", assetsFolder + "/castlesBACKUP");
            }

            if(!Directory.Exists(assetsFolder + "/interfaceBACKUP"))
            {
                Copy(assetsFolder + "/interface", assetsFolder + "/interfaceBACKUP");
            }
        }

        public void ApplyBackup()
        {
            if(!File.Exists(assetsFolder + "/textures.ultrahigh.pak.backup") || !File.Exists(assetsFolder + "/config.xml.backup") || !Directory.Exists(assetsFolder + "/castlesBACKUP"))
            {
                MessageBox.Show("There is no backup that could be applied.", "Backup", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                File.Copy(assetsFolder + "/textures.ultrahigh.pak.backup", assetsFolder + "/textures.ultrahigh.pak", true);
                File.Copy(assetsFolder + "/config.xml.backup", assetsFolder + "/config.xml", true);
                
                if(Directory.Exists(assetsFolder + "/ModAssets"))
                    Directory.Delete(assetsFolder + "/ModAssets", true);

                Directory.Delete(assetsFolder + "/castles", true);
                Copy(assetsFolder + "/castlesBACKUP", assetsFolder + "/castles");

                Directory.Delete(assetsFolder + "/interface", true);
                Copy(assetsFolder + "/interfaceBACKUP", assetsFolder + "/interface");
            }

        }

        public delegate void StartTask(int task);
        public delegate void FinishTask(int task);

        public void ApplyMods(List<Mod> mods, StartTask startTask, FinishTask finishTask)
        {
            HashSet<string> appliedFiles = new HashSet<string>();
            Dictionary<string, LanguageMerger> computedEntries = new Dictionary<string, LanguageMerger>();

            bool skipLanguage = false;

            string xmlFile = File.ReadAllText(assetsFolder + "/config.xml.backup");

            // We have to modify the file directly as the file in stronghold:warlords isn't well formed.
            int indexToInsert = xmlFile.IndexOf("<directories>") + 13;

            Directory.CreateDirectory(assetsFolder + "/ModAssets");

            startTask(2);

            // This looks very interesting. First, we open the assets file. Then, we open the mod zip archive. And finally, we are opening a stream, which is used to copy the files in the assets directory of the game.
            using (ZipArchive assetsFile = ZipFile.Open(assetsFolder + "/textures.ultrahigh.pak", ZipArchiveMode.Update))
            {
                finishTask(2);
                startTask(3);

                foreach(Mod mod in mods)
                {
                    foreach(string additionalPath in mod.DirectoryAdditions)
                    {
                        //XmlElement element = xmlDocument.CreateElement("directory");
                        //element.SetAttribute("path", "../../assets/ModAssets/" + additionalPath);
                        //directoryTag.AppendChild(element);
                        xmlFile = xmlFile.Insert(indexToInsert + 1, "\n<directory path=\"..\\..\\assets\\ModAssets\\" + additionalPath + "\" />");
                    }

                    if (mod.Enabled)
                    {
                        using (ZipArchive modArchive = ZipFile.OpenRead(mod.FilePath))
                        {
                            foreach (ZipArchiveEntry entry in modArchive.Entries)
                            {
                                if (entry.FullName.StartsWith("textures") && !entry.FullName.EndsWith("/") && !appliedFiles.Contains(entry.FullName))
                                {
                                    appliedFiles.Add(entry.FullName);

                                    using (Stream zipStream = entry.Open())
                                    {
                                        ZipArchiveEntry assetsEntry = assetsFile.GetEntry(entry.FullName);
                                        if (assetsEntry == null)
                                            assetsEntry = assetsFile.CreateEntry(entry.FullName);

                                        using (Stream assetsEntryTargetStream = assetsEntry.Open())
                                        {
                                            zipStream.CopyTo(assetsEntryTargetStream);
                                        }
                                    }
                                }
                                else if(entry.FullName == entry.Name && !entry.FullName.EndsWith("/") && entry.FullName != "ModInfo.xml")
                                {
                                    using (Stream zipStream = entry.Open())
                                    {
                                        using (FileStream assetFileStream = new FileStream(assetsFolder + "/ModAssets/" + entry.Name, FileMode.Create))
                                        {
                                            zipStream.CopyTo(assetFileStream);
                                        }
                                    }
                                }
                                else if(entry.FullName.StartsWith("castles") && !entry.FullName.EndsWith("/") && !appliedFiles.Contains(entry.FullName))
                                {
                                    appliedFiles.Add(entry.FullName);

                                    using (Stream zipStream = entry.Open())
                                    {
                                        using (FileStream assetFileStream = new FileStream(assetsFolder + "/" + entry.FullName, FileMode.Create))
                                        {
                                            zipStream.CopyTo(assetFileStream);
                                        }
                                    }
                                }
                                else if(entry.FullName.StartsWith("interface/i18n") && entry.FullName.EndsWith(".xml") && !skipLanguage)
                                {
                                    using (Stream zipStream = entry.Open())
                                    {
                                        XmlDocument srcDocument = new XmlDocument();
                                        srcDocument.Load(zipStream);

                                        XmlDocument targetDocument = new XmlDocument();
                                        targetDocument.Load(assetsFolder + "/" + entry.FullName);

                                        if (!computedEntries.ContainsKey(assetsFolder + "/" + entry.FullName))
                                        {
                                            LanguageMerger merger = new LanguageMerger();
                                            if (!merger.LoadOriginal(targetDocument))
                                                skipLanguage = true;
                                            computedEntries.Add(assetsFolder + "/" + entry.FullName, merger);
                                        }

                                        if (!skipLanguage)
                                        {
                                            LanguageMerger merger = computedEntries[assetsFolder + "/" + entry.FullName];

                                            merger.Merge(srcDocument);
                                        }
                                        else
                                            computedEntries.Clear();
                                    }
                                }
                                else if(entry.FullName.StartsWith("interface") && !entry.FullName.EndsWith("/"))
                                {
                                    appliedFiles.Add(entry.FullName);

                                    using (Stream zipStream = entry.Open())
                                    {
                                        using (FileStream assetFileStream = new FileStream(assetsFolder + "/" + entry.FullName, FileMode.Create))
                                        {
                                            zipStream.CopyTo(assetFileStream);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                finishTask(3);
            }

            startTask(4);
            //xmlDocument.Save(assetsFolder + "/config.xml");
            File.WriteAllText(assetsFolder + "/config.xml", xmlFile);
            finishTask(4);

            startTask(5);

            if(!skipLanguage)
            {
                foreach(string path in computedEntries.Keys)
                {
                    LanguageMerger merger = computedEntries[path];

                    XmlDocument loadedXMLDocument = new XmlDocument();
                    loadedXMLDocument.Load(path);

                    merger.ApplyXMLChanges(loadedXMLDocument);

                    loadedXMLDocument.Save(path);
                }
            }

            finishTask(5);

            // We have to do this as else the memory consumption would be enormous, at around 2 GB AFTER the operation. And if we were to invoke this method more than once, the memory consumption would only rise. So, as a safety measure, we put this statement in.
            System.GC.Collect();
        }
    }
}
