// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Management;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Natural_Graphics_Primitives_UI.Models;
using Natural_Graphics_Primitives_UI.Services;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using Clipboard = System.Windows.Clipboard;
using IDataObject = System.Windows.IDataObject;

namespace Natural_Graphics_Primitives_UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDataObject _dataObject;
        private SettingService _settingService;
        private SettingsModel _settingsModel;

        private readonly Dictionary<int, CheckBox> _cbs;
        private List<Process> _processes = new List<Process>();
        private List<Button> _buttons;

        public MainWindow()
        {
            InitializeComponent();
            _cbs = new Dictionary<int, CheckBox>
                { { 1, cbMinimal }, { 2, cbSmall }, { 3, cbMedium }, { 8, cbLarge }, { 16, cbMax } };
            
            _buttons = new List<Button>
            {
                buildButton, generateButton, cancelButton
            };
        }

        private void WindowLoaded(object sender, RoutedEventArgs args)
        {
            _settingService = new SettingService();
            _settingsModel = _settingService.Settings;
            UpdateButtons();
            UpdateCheckBoxes(_settingsModel.SelectedResolution);
            CheckStatus(null, null);
        }

        private void UpdateCheckBoxes(int select)
        {
            if (select == 0)
            {
                foreach (var cb in _cbs.Values)
                {
                    cb.IsChecked = false;
                }
                _buttons.ForEach(x => x.IsEnabled = false);
            }
            else
            {
                foreach (var cb in _cbs.Values)
                {
                    cb.IsChecked = false;
                }
                _buttons.ForEach(x => x.IsEnabled = true);
                _cbs[select].IsChecked = true;
            }

            _settingsModel.SelectedResolution = select;
            _settingService.Settings = _settingsModel;
        }

        private void GetClipboard(object sender, RoutedEventArgs args)
        {
            _dataObject = Clipboard.GetDataObject();
            CheckCurrentDataObject();
        }

        private void CheckStatus(object sender, RoutedEventArgs args)
        {
            // cbContainsAudio.IsChecked = Clipboard.ContainsAudio();
            // cbContainsFileDropList.IsChecked = Clipboard.ContainsFileDropList();
            // cbContainsImage.IsChecked = Clipboard.ContainsImage();
            // cbContainsText.IsChecked = Clipboard.ContainsText();
        }

        private void ClearClipboard(object sender, RoutedEventArgs args)
        {
            Clipboard.Clear();
            CheckStatus(null, null);
            GetClipboard(null, null);
        }

        private void CheckCurrentDataObject()
        {
            CheckStatus(null, null);

            tbInfo.Clear();

            if (_dataObject == null)
            {
                tbInfo.Text = "DataObject is null.";
            }
            else
            {
                tbInfo.AppendText("Clipboard DataObject Type: ");
                tbInfo.AppendText(_dataObject.GetType().ToString());

                tbInfo.AppendText("\n\n****************************************************\n\n");

                var formats = _dataObject.GetFormats();

                tbInfo.AppendText(
                    "The following data formats are present in the data object obtained from the clipboard:\n");

                if (formats.Length > 0)
                {
                    foreach (var format in formats)
                    {
                        if (_dataObject.GetDataPresent(format, false))
                            tbInfo.AppendText("\t- " + format + " (native)\n");
                        else tbInfo.AppendText("\t- " + format + " (autoconvertable)\n");
                    }
                }
                else tbInfo.AppendText("\t- no data formats are present\n");

                foreach (var format in formats)
                {
                    tbInfo.AppendText("\n\n****************************************************\n");
                    tbInfo.AppendText(format + " data:\n");
                    tbInfo.AppendText("****************************************************\n\n");
                    tbInfo.AppendText(_dataObject.GetData(format, true).ToString());
                }
            }
        }

        private void Cb_OnClick(object sender, RoutedEventArgs e)
        {
            var s = sender as CheckBox;
            UpdateCheckBoxes(_cbs.FirstOrDefault(x => x.Value == s).Key);
        }

        private void UpdatePath(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if (string.IsNullOrWhiteSpace(folderBrowser.SelectedPath)) return;
            if (!Directory.Exists(folderBrowser.SelectedPath))
            {
                tbInfo.AppendText("\n\n****************************************************\n");
                tbInfo.AppendText("Директории не существует\n");
                tbInfo.AppendText("****************************************************\n\n");
                return;
            }
            _settingsModel.NGPPath = folderBrowser.SelectedPath;
            _settingService.Settings = _settingsModel;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (Directory.Exists(_settingsModel.NGPPath))
            {
                pathButton.Content = "Изменить расположение нейросети";
                pathButton.Background = Brushes.Chartreuse;
                tbInfo.AppendText("****************************************************\n");
                tbInfo.AppendText("Расположение сети задано\n");
                tbInfo.AppendText("****************************************************\n\n");
                _buttons.ForEach(x => x.IsEnabled = true);
                _cbs.Values.ToList().ForEach(x => x.IsEnabled = true);
            }
            else
            {
                pathButton.Content = "Задать расположение нейросети";
                pathButton.Background = Brushes.Red;
                tbInfo.AppendText("****************************************************\n");
                tbInfo.AppendText("Расположение сети не задано\n");
                tbInfo.AppendText("****************************************************\n\n");
                _buttons.ForEach(x => x.IsEnabled = false);
                _cbs.Values.ToList().ForEach(x => x.IsEnabled = false);
            }
        }

        private void GenerateErrorMap(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if (string.IsNullOrWhiteSpace(folderBrowser.SelectedPath)) return;
            if (!Directory.Exists(folderBrowser.SelectedPath))
            {
                tbInfo.AppendText("\n\n****************************************************\n");
                tbInfo.AppendText("Директории не существует\n");
                tbInfo.AppendText("****************************************************\n\n");
                return;
            }
            var a =
                $"/C python {_settingsModel.NGPPath}/scripts/colmap2nerf.py --colmap_matcher exhaustive --run_colmap --aabb_scale {_settingsModel.SelectedResolution} --images {folderBrowser.SelectedPath}";
 
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = a,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            var process = new Process();
            process.StartInfo = startInfo;


            process.OutputDataReceived += (sender, args) => { Dispatcher.Invoke((Action)(() => { tbInfo.AppendText(args.Data + "\n"); })); };

            process.Exited += (sender, args) =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    tbInfo.AppendText("****************************************************\n\n");
                    tbInfo.AppendText("Операция завершена успешно\n");
                    tbInfo.AppendText("****************************************************\n\n");
                    CreateRightStructure(folderBrowser.SelectedPath);
                }));
            };
            process.Start();
            process.StandardInput.WriteLine("Y");
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _processes.Add(process);
        }

        private void CreateRightStructure(string path)
        {
            var directoryName = path[(path.LastIndexOf('\\')+1)..];
            string[] files = Directory.GetFiles(path);
            DirectoryInfo dirInfo = new DirectoryInfo(path+"\\"+directoryName);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            foreach (var file in files)
            {
                var fileName = file[(file.LastIndexOf('\\') + 1)..];
                File.Move(file, path + "\\" + directoryName + "\\" + fileName);
            }

            string transforms = Directory.GetFiles(_settingsModel.NGPPath).FirstOrDefault(x => x.Substring(x.LastIndexOf('\\') + 1) == "transforms.json");
            
            if (transforms != null)
            {
                File.Move(transforms, path + "\\transforms.json");
            }
        }

        private void WindowClosing(object? sender, CancelEventArgs e)
        {
            CancelAllProcess();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            CancelAllProcess();
        }

        private void CancelAllProcess()
        {
            tbInfo.AppendText("****************************************************\n");
            if (_processes.Count == 0)
            {
                tbInfo.AppendText("Нет активных операций\n");
            }
            else
            {
                _processes.ForEach(x => KillProcessAndChildrens(x.Id));
                tbInfo.AppendText("Операция отменена\n");
                _processes.Clear();
            }
            tbInfo.AppendText("****************************************************\n\n");
        }

        private static void KillProcessAndChildrens(int pid)
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
                ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();

            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }

            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"]));
                }
            }
        }


        private void BuildModel(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var folderBrowser = new OpenFileDialog();
            folderBrowser.ShowDialog();
            if (string.IsNullOrWhiteSpace(folderBrowser.FileName)) return;
            if (!File.Exists(folderBrowser.FileName))
            {
                tbInfo.AppendText("\n\n****************************************************\n");
                tbInfo.AppendText("Директории не существует\n");
                tbInfo.AppendText("****************************************************\n\n");
                return;
            }
            var disk = folderBrowser.FileName.Substring(folderBrowser.FileName.IndexOf('\\')  -2, 1);
            var path =  $@"/C {disk}: & cd {_settingsModel.NGPPath} & {_settingsModel.NGPPath}/build/testbed --scene {folderBrowser.FileName}";
 
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = path,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            var process = new Process();
            process.StartInfo = startInfo;


            process.OutputDataReceived += (sender, args) => { Dispatcher.Invoke((Action)(() => { tbInfo.AppendText(args.Data + "\n"); })); };
            process.ErrorDataReceived += (sender, args) => { Dispatcher.Invoke((Action)(() => { tbInfo.AppendText(args.Data + "\n"); })); };
            process.Disposed += (sender, args) =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    tbInfo.AppendText("****************************************************\n\n");
                    tbInfo.AppendText("Операция завершена успешно\n");
                    tbInfo.AppendText("****************************************************\n\n");
                }));
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _processes.Add(process);
           
        }

        private void TbInfo_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            tbInfo.SelectionStart = tbInfo.Text.Length;

            tbInfo.ScrollToEnd();

        }
    }
}