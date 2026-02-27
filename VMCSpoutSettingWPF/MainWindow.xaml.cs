using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace VMCSpoutSettingWPF
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private VMCSpoutSetting _settings = new VMCSpoutSetting();
        private ObservableCollection<CameraSetting> _cameraSettings;

        public MainWindow()
        {
            InitializeComponent();

            string startupPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Environment.GetCommandLineArgs()[0]));

            if (File.Exists(Path.Combine(startupPath, "VMCSpoutSetting.json")))
                _settings = JsonConvert.DeserializeObject<VMCSpoutSetting>(File.ReadAllText(Path.Combine(startupPath, "VMCSpoutSetting.json")));
            else
            {
                _settings = new VMCSpoutSetting();
                File.WriteAllText(Path.Combine(startupPath, "VMCSpoutSetting.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
            }
            _cameraSettings = new ObservableCollection<CameraSetting>();

            foreach(var cs in _settings.AdditionalCameras)
            {
                _cameraSettings.Add(cs);
            }

            SpoutDataGrid.ItemsSource = _cameraSettings;

            ScaleSyncCheckBox.IsChecked = _settings.ScaleSyncWithCamera;
            MainCameraSpoutNameTextBox.Text = _settings.MainCamSpoutName;
            MainCameraHeightTextBox.Text = _settings.MainCamOutputHeight.ToString();
            MainCameraWidthTextBox.Text = _settings.MainCamOutputWidth.ToString();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.ScaleSyncWithCamera = ScaleSyncCheckBox.IsChecked.Value;
            _settings.MainCamSpoutName = MainCameraSpoutNameTextBox.Text;
            _settings.MainCamOutputWidth = int.Parse(MainCameraWidthTextBox.Text);
            _settings.MainCamOutputHeight = int.Parse(MainCameraHeightTextBox.Text);
            _settings.AdditionalCameras = new CameraSetting[_cameraSettings.Count];
            for(int i=0; i < _cameraSettings.Count; i++)
            {
                _settings.AdditionalCameras[i] = _cameraSettings[i];
            }
            string startupPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Environment.GetCommandLineArgs()[0]));
            File.WriteAllText(Path.Combine(startupPath, "VMCSpoutSetting.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));

            Application.Current.Shutdown();
        }
    }
}
