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
        private CameraPlusSetup _cameraPlusSetup;
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

            MainCameraSpoutNameTextBox.Text = _settings.MainCamSpoutName;
            MainCameraHeightTextBox.Text = _settings.MainCamOutputHeight.ToString();
            MainCameraWidthTextBox.Text = _settings.MainCamOutputWidth.ToString();

            UseMirrorCheckBox.IsChecked = _settings.UseMirror;
            MirrorResolutionTextBox.Text = _settings.MirrorResolution.ToString();
            MirrorWidthTextBox.Text = _settings.MirrorWidth.ToString();
            MirrorHeightTextBox.Text = _settings.MirrorHeight.ToString();

            UseMirrorFollowCheckBox.IsChecked = _settings.FollowMirrorPosition;
            MirrorIntensityTextBox.Text = _settings.MirrorIntensity.ToString();
            MirrorCenterPositionXTextBox.Text = _settings.MirrorPositionX.ToString();
            MirrorCenterPositionYTextBox.Text = _settings.MirrorPositionY.ToString();
            MirrorCenterPositionZTextBox.Text = _settings.MirrorPositionZ.ToString();

            MirrorCenterRotationYTextBox.Text = _settings.MirrorRotationY.ToString();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.MainCamSpoutName = MainCameraSpoutNameTextBox.Text;
            _settings.MainCamOutputWidth = int.Parse(MainCameraWidthTextBox.Text);
            _settings.MainCamOutputHeight = int.Parse(MainCameraHeightTextBox.Text);

            _settings.UseMirror = UseMirrorCheckBox.IsChecked.Value;
            _settings.MirrorIntensity = float.Parse(MirrorIntensityTextBox.Text);
            _settings.MirrorResolution = int.Parse(MirrorResolutionTextBox.Text);
            _settings.MirrorWidth = float.Parse(MirrorWidthTextBox.Text);
            _settings.MirrorHeight = float.Parse(MirrorHeightTextBox.Text);

            _settings.FollowMirrorPosition = UseMirrorFollowCheckBox.IsChecked.Value;
            _settings.MirrorPositionX = float.Parse(MirrorCenterPositionXTextBox.Text);
            _settings.MirrorPositionY = float.Parse(MirrorCenterPositionYTextBox.Text); 
            _settings.MirrorPositionZ = float.Parse(MirrorCenterPositionZTextBox.Text); 
            _settings.MirrorRotationY = float.Parse(MirrorCenterRotationYTextBox.Text);

            _settings.AdditionalCameras = new CameraSetting[_cameraSettings.Count];
            for(int i=0; i < _cameraSettings.Count; i++)
            {
                _settings.AdditionalCameras[i] = _cameraSettings[i];
            }
            string startupPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Environment.GetCommandLineArgs()[0]));
            File.WriteAllText(Path.Combine(startupPath, "VMCSpoutSetting.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));

            Application.Current.Shutdown();
        }

        private void CameraPlusSetupButton_Click(object sender, RoutedEventArgs e)
        {
            _cameraPlusSetup = new CameraPlusSetup();
            _cameraPlusSetup.Owner = this;
            if (_cameraPlusSetup.ShowDialog() == true)
            {
                int spoutCount = _cameraPlusSetup.ResultSpoutCount;
                _cameraSettings.Clear();
                for (int i = 0; i < spoutCount; i++)
                {
                    _cameraSettings.Add(new CameraSetting()
                    {
                        SpoutName = $"VMC Spout {i + 1}",
                        OutputWidth = int.Parse(MainCameraWidthTextBox.Text),
                        OutputHeight = int.Parse(MainCameraHeightTextBox.Text),
                        Port = 39640 + i
                    });
                }
                SpoutDataGrid.ItemsSource = _cameraSettings;

            }
        }
    }
}
