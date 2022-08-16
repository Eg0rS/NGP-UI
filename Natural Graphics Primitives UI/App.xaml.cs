using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Natural_Graphics_Primitives_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private void Start()
        {
            string output = "";
            using (var process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF32;
                process.OutputDataReceived += OutputHandler;
                process.ErrorDataReceived += OutputHandler;
                string? var = Environment.GetEnvironmentVariable("NGP");
// process.StartInfo.Arguments =
// $"/C d: & cd {var} & {var}/build/testbed —scene D:/NGP/instant-ngp/data/nerf/fox";
                process.StartInfo.Arguments =
                    $"/C d: & cd {var} & python scripts/colmap2nerf.py —colmap_matcher exhaustive —run_colmap —aabb_scale 1 —images D:/NGP/instant-ngp/data/amogus";
                process.EnableRaisingEvents = true;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();


//process.StandardInput.WriteLine("Y");
            }
        }

        void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
// Console.WriteLine(outLine.Data);
                Dispatcher.BeginInvoke(new ThreadStart(delegate { TextBlock.Text += outLine.Data + "\n"; }));
            }
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            Start();
        }
    }
}