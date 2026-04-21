using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace VMCSpoutSettingWPF
{
    /// <summary>
    /// CameraPlusSetup.xaml の相互作用ロジック
    /// </summary>
    public partial class CameraPlusSetup : Window
    {
        public int ResultSpoutCount { get; private set; }
        public ObservableCollection<CameraPlusProfile> Profiles;

        private const string _profilePath = "UserData/CameraPlus/Profiles";
        public CameraPlusSetup()
        {
            InitializeComponent();

            Profiles = new ObservableCollection<CameraPlusProfile>();

            ProfileListBox.ItemsSource = Profiles;
        }

        private void BeatSaberFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog()
            {
                Description = "Select Beat Saber Folder",
                ShowNewFolderButton = false,
            })
            {
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    BeatSaberFolderTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void BeatSaberFolderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var path = Path.Combine(BeatSaberFolderTextBox.Text, _profilePath);
            if (Directory.Exists(path))
            {
                var profileFolders = Directory.GetDirectories(path);
                Profiles.Clear();
                foreach(var folder in profileFolders)
                {
                    var p = new CameraPlusProfile { Name = Path.GetFileName(folder), IsEnabled = true };
                    Profiles.Add(p);
                }
            }
        }

        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            int maxSpoutCount = 0;

            if (Profiles.Count == 0)
            {
                System.Windows.MessageBox.Show("No CameraPlus profiles found. Please select the correct Beat Saber folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            bool isChecked = false;
            foreach(var p in Profiles)
            {
                if (p.IsEnabled)
                {
                    isChecked = true;
                    break;
                }
            }
            if (!isChecked)
            {
                System.Windows.MessageBox.Show("Please select at least one profile to enable.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var p in Profiles)
            {
                if (p.IsEnabled)
                {
                    var path = Path.Combine(BeatSaberFolderTextBox.Text, _profilePath, p.Name);
                    var files = Directory.GetFiles(path);
                    for (int i = 0; i < files.Length; i++)
                    {
                        string json = File.ReadAllText(files[i]);
                        dynamic jsonDynamic = JsonConvert.DeserializeObject(json);

                        var vmcElement = jsonDynamic["VMCProtocol"];
                        vmcProtocolElements vmc = vmcElement?.ToObject<vmcProtocolElements>() ?? new vmcProtocolElements();
                        vmc.mode = VMCProtocolMode.Sender;
                        vmc.port = 39640 + i;
                        jsonDynamic.VMCProtocol = JObject.FromObject(vmc);

                        var spoutElement = jsonDynamic["Spout"];
                        SpoutCameraElements spout = spoutElement?.ToObject<SpoutCameraElements>() ?? new SpoutCameraElements();
                        spout.reciverName = "VMC Spout " + (i + 1);
                        spout.reciverAutoConnect = true;
                        jsonDynamic.Spout = JObject.FromObject(spout);

                        File.WriteAllText(files[i], JsonConvert.SerializeObject(jsonDynamic, Formatting.Indented));
                    }
                    if (maxSpoutCount < files.Length)
                        maxSpoutCount = files.Length;
                }
            }

            ResultSpoutCount = maxSpoutCount;
            System.Windows.MessageBox.Show("Configured Spout and VMCProtocol in CameraPlus settings.", "Setup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }
    }
}
