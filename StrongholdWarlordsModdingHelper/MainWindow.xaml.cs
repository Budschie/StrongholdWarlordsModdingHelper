using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using ListViewItem = System.Windows.Controls.ListViewItem;
using CheckBox = System.Windows.Controls.CheckBox;

namespace StrongholdWarlordsModdingHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentOpenedPath = null;
        private volatile bool lockBool = false;
        private ModDirectory modDirectory = null;
        private ModApplierHandler modApplierHandler = null;
        private ModdingHelperConfig config;

        public MainWindow()
        {
            config = new ModdingHelperConfig();
            config.Deserialize();
            InitializeComponent();

            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(modDirectory != null && modDirectory.IsDirty)
            {
                if (MessageBox.Show("Are you sure that you want to close? There is unsaved data.", "Unsaved data", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                    e.Cancel = true;
            }

            if(!e.Cancel)
                config.Serialize();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (lockBool)
                return;

            lockBool = true;

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = config.LastStrongholdWarlordsLocation + "/";
            folderBrowserDialog.Description = "Please choose your Stronghold Warlords root directory.";
            folderBrowserDialog.UseDescriptionForTitle = true;
            DialogResult result = folderBrowserDialog.ShowDialog();

            if(result == System.Windows.Forms.DialogResult.OK)
            {
                string potentialDirectory = folderBrowserDialog.SelectedPath;

                // Very interesting formatting indeed
                if (   !File.Exists(potentialDirectory + "/assets/textures.low.pak")
                    || !File.Exists(potentialDirectory + "/assets/textures.med.pak")
                    || !File.Exists(potentialDirectory + "/assets/textures.high.pak")
                    || !File.Exists(potentialDirectory + "/assets/textures.ultrahigh.pak")
                    )
                {
                    MessageBox.Show("The directory that was selected didn't contain the game.\n\nPlease select a valid game directory.", "Verification failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("The directory was verified successfully.", "Verification succeeded", MessageBoxButton.OK, MessageBoxImage.Information);

                    currentOpenedPath = folderBrowserDialog.SelectedPath;
                    currentPath.Text = "Current bound asset path: " + currentOpenedPath;
                    modDirectory = new ModDirectory(currentOpenedPath + "/Mods");
                    modApplierHandler = new ModApplierHandler(currentOpenedPath + "/assets");
                    config.LastStrongholdWarlordsLocation = currentOpenedPath;

                    if (Directory.Exists(currentOpenedPath + "/Mods"))
                        modDirectory.LoadModConfiguration();

                    UpdateListView();
                }
            }

            lockBool = false;
        }

        private void UpdateListView()
        {
            modListView.Items.Clear();

            foreach(Mod mod in modDirectory.Mods)
            {
                modListView.Items.Add(mod);
            }
        }

        private void ImportMod_Click(object sender, RoutedEventArgs e)
        {
            if (lockBool)
                return;

            lockBool = true;

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Please open your mod file.";
            fileDialog.Filter = "Stronghold Warlords Mod file (*.shwmod)|*.shwmod";
            fileDialog.CheckPathExists = true;
            fileDialog.CheckFileExists = true;
            fileDialog.InitialDirectory = config.LastModLocation;
            DialogResult result = fileDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (modDirectory == null)
                {
                    MessageBox.Show("The game directory isn't bound yet. You have to bind the directory first, and then you can import mods.", "Import failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    Mod mod = null;

                    string lastModDirectoryLocation = fileDialog.FileName.Replace('/', '\\');
                    lastModDirectoryLocation = lastModDirectoryLocation.Substring(0, lastModDirectoryLocation.LastIndexOf('\\'));
                    config.LastModLocation = lastModDirectoryLocation;

                    try
                    {
                        mod = Mod.LoadMod(fileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("There was an error during loading the mod:\n" + ex.Message, "Mod loading error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    if (mod != null)
                    {
                        modDirectory.InsertMod(mod, 0);
                        UpdateListView();
                    }
                }
            }

            lockBool = false;
        }

        private void SaveModConfig_Click(object sender, RoutedEventArgs e)
        {
            if (modDirectory == null)
            {
                MessageBox.Show("There is currently nothing that could be saved, as the game directory isn't properly bound.", "Mod config error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                modDirectory.SaveModConfiguration();
                MessageBox.Show("Successfully saved mod data.", "Save successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void modListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                Mod selectedMod = (Mod)e.AddedItems[0];

                outliner.Visibility = Visibility.Visible;

                title.Text = selectedMod.Name;
                description.Text = selectedMod.Description;
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (lockBool)
                return;

            lockBool = true;

            if(modListView.SelectedItems.Count > 0)
            {
                if(MessageBox.Show("Do you really want to remove this mod from the list?", "Removal of mod", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Mod mod = (Mod)modListView.SelectedItems[0];
                    modDirectory.RemoveMod(mod);
                    outliner.Visibility = Visibility.Hidden;
                    UpdateListView();
                }
            }

            lockBool = false;
        }

        private void ApplyMods_Click(object sender, RoutedEventArgs e)
        {
            if (lockBool)
                return;

            lockBool = true;

            if (modApplierHandler == null)
            {
                MessageBox.Show("The game directory isn't properly bound yet.", "Mod config error", MessageBoxButton.OK, MessageBoxImage.Error);
                lockBool = false;
            }
            else
            {
                if (MessageBox.Show("Do you want to apply the mods?", "Mods", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {

                    ModdingApplicationTaskList moddingApplicationTaskList = new ModdingApplicationTaskList(new ModdingProgressTask("Creating backup"), new ModdingProgressTask("Applying backup"), new ModdingProgressTask("Loading assets of Stronghold:Warlords"), new ModdingProgressTask("Applying mods"), new ModdingProgressTask("Merging config.xml files"), new ModdingProgressTask("Saving merged language files"));
                    moddingApplicationTaskList.Show();

                    Thread executionThread = new Thread(() => 
                    {
                        ModApplierHandler.StartTask startTask = (task) => moddingApplicationTaskList.Dispatcher.Invoke(() => moddingApplicationTaskList.StartTask(task));
                        ModApplierHandler.FinishTask finishTask = (task) => moddingApplicationTaskList.Dispatcher.Invoke(() => moddingApplicationTaskList.FinishTask(task));

                        startTask(0);
                        modApplierHandler.CreateBackup();
                        finishTask(0);
                        startTask(1);
                        modApplierHandler.ApplyBackup();
                        finishTask(1);
                        modApplierHandler.ApplyMods(modDirectory.Mods, startTask, finishTask);

                        Dispatcher.Invoke(() =>
                        {
                            modListView.IsEnabled = true;
                            lockBool = false;
                            moddingApplicationTaskList.SetDone();
                            MessageBox.Show("Successfully added " + modDirectory.Mods.Count + " mods!", "Mod loading", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        });
                    });

                    modListView.IsEnabled = false;

                    executionThread.Start();
                }
                else
                {
                    lockBool = false;
                }
            }
        }
    }

}
