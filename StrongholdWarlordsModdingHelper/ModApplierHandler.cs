using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace StrongholdWarlordsModdingHelper
{
    class ModApplierHandler
    {
        private string assetsFolder;

        public ModApplierHandler(string assetsFolder)
        {
            this.assetsFolder = assetsFolder;
        }

        public void CreateBackup()
        {
            if(!File.Exists(assetsFolder + "/textures.ultrahigh.pak.backup"))
                File.Copy(assetsFolder + "/textures.ultrahigh.pak", assetsFolder + "/textures.ultrahigh.pak.backup");
        }

        public void ApplyBackup()
        {
            if(!File.Exists(assetsFolder + "/textures.ultrahigh.pak.backup"))
            {
                MessageBox.Show("There is no backup that could be applied.", "Backup", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                File.Copy(assetsFolder + "/textures.ultrahigh.pak.backup", assetsFolder + "/textures.ultrahigh.pak", true);
            }
        }

        public void ApplyMods(List<Mod> mods)
        {
            HashSet<string> appliedFiles = new HashSet<string>();

            // This looks very interesting. First, we open the assets file. Then, we open the mod zip archive. And finally, we are opening a stream, which is used to copy the files in the assets directory of the game.
            using(ZipArchive assetsFile = ZipFile.Open(assetsFolder + "/textures.ultrahigh.pak", ZipArchiveMode.Update))
            {
                foreach(Mod mod in mods)
                {
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
                            }
                        }
                    }
                }
            }

            // We have to do this as else the memory consumption would be enormous, at around 2 GB AFTER the operation. And if we were to invoke this method more than once, the memory consumption would only rise. So, as a safety measure, we put this statement in.
            System.GC.Collect();
        }
    }
}
