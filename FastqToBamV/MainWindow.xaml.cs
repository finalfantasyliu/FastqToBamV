using System;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using RadioButton = System.Windows.Controls.RadioButton;

namespace FastqToBamV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        BackgroundWorker BW = new BackgroundWorker();
        Process process = new Process();
        DataText DT = new DataText();
        AnalysisPath Path = new AnalysisPath();
        ObservableCollection<Files> samples = new ObservableCollection<Files>();
        SequenceSetForToBam forFastqToBam = new SequenceSetForToBam();
        string checkOutputFolderPath = "";




        public MainWindow()
        {
            InitializeComponent();


            //samples.Add(new Files { FileName = "402321", Read = "R1", ReadBackground = (Brush)converter.ConvertFromString("#1098ad"), FilePath = "D:/Chen/1344.fastq" });
            //samples.Add(new Files { FileName = "402321", Read = "R2", ReadBackground = (Brush)converter.ConvertFromString("#1e88e5"), FilePath = "D:/Chen/1344.fastq" });
            //samples.Add(new Files { FileName = "402321", Read = "R1", ReadBackground = (Brush)converter.ConvertFromString("#1098ad"), FilePath = "D:/Chen/1344.fastq" });
            //samples.Add(new Files { FileName = "402321", Read = "R2", ReadBackground = (Brush)converter.ConvertFromString(" #1e88e5"), FilePath = "D:/Chen/1344.fastq" });
            //samples.Add(new Files { FileName = "402321", Read = "R1", ReadBackground = (Brush)converter.ConvertFromString("#1098ad"), FilePath = "D:/Chen/1344.fastq" });
            //samples.Add(new Files { FileName = "402321", Read = "R2", ReadBackground = (Brush)converter.ConvertFromString("#1e88e5"), FilePath = "D:/Chen/1344.fastq" });
            //samples.Add(new Files { FileName = "402321", Read = "R1", ReadBackground = (Brush)converter.ConvertFromString(" #1098ad"), FilePath = "D:/Chen/1344.fastq" });
            //samples.Add(new Files { FileName = "402321", Read = "R2", ReadBackground = (Brush)converter.ConvertFromString(" #1e88e5"), FilePath = "D:/Chen/1344.fastq" });
            //samples.Add(new Files { FileName = "402321", Read = "R1", ReadBackground = (Brush)converter.ConvertFromString(" #1098ad"), FilePath = "D:/Chen/1344.fastq" });

            List<string> ReferenceIndex = new List<string>();
            ReferenceIndex.Add("GRCh38 p13");
            ReferenceCombobox.ItemsSource = ReferenceIndex;
            ProcessButton.Background = (Brush)converter.ConvertFromString("Transparent");
            SettingButton.Background = (Brush)converter.ConvertFromString("Transparent");
            FolderButton.Background = (Brush)converter.ConvertFromString("#7b5cd6");
            processStay = false;
            settingStay = false;
            folderStay = true;
            TerminalContent.DataContext = DT;

            Settings.DataContext = forFastqToBam;
            forFastqToBam.path = new List<AnalysisPath>();



        }
        private void SubReadCall(object sender, DoWorkEventArgs e)
        {
            int samplesCount = samples.Count;
            List<Files> sampleCheck = new List<Files>();

            for (int i = 0; i < samplesCount; i++)
            {
                if (samples[i].Check == true)
                {
                    sampleCheck.Add(samples[i]);

                }
            }
            for (int i = 0; i < sampleCheck.Count; i++)
            {
                if (sampleCheck[i].Read == "R1")
                {
                    string Read1Name = sampleCheck[i].FileName.Substring(0, sampleCheck[i].FileName.IndexOf('_'));
                    for (int j = 0; j < sampleCheck.Count; j++)
                    {

                        if (sampleCheck[j].Read == "R2")
                        {
                            string Read2Name = sampleCheck[j].FileName.Substring(0, sampleCheck[j].FileName.IndexOf('_'));
                            if (Read1Name == Read2Name)
                            {
                                forFastqToBam.path.Add(new AnalysisPath { FileName = Read1Name, Read1 = sampleCheck[i].FilePath, Read2 = sampleCheck[j].FilePath });
                            }
                        }
                    }
                }
            }

            string desktopOutputPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FastqToBam");


            if (String.IsNullOrEmpty(checkOutputFolderPath))
            {
                if (!System.IO.Directory.Exists(desktopOutputPath))
                    System.IO.Directory.CreateDirectory(desktopOutputPath);
                forFastqToBam.OutputFolder = desktopOutputPath;

            }
            else
            {
                if (System.IO.Directory.Exists(checkOutputFolderPath))
                {
                    forFastqToBam.OutputFolder = checkOutputFolderPath;
                }
                else
                {
                    if (!System.IO.Directory.Exists(desktopOutputPath))
                        System.IO.Directory.CreateDirectory(desktopOutputPath);
                    forFastqToBam.OutputFolder = desktopOutputPath;
                }
            }

            string drivePath = System.IO.Path.GetPathRoot(Environment.CurrentDirectory);
            drivePath = drivePath.Replace("\\", "");
            string subreadPath = System.IO.Path.Combine(Environment.CurrentDirectory, "subread-2.0.3-Windows-x86_64", "bin");
            string arguments = null;
            process = new Process();


            process.StartInfo.FileName = "CMD.exe";
            if (!string.IsNullOrEmpty(arguments))
                process.StartInfo.Arguments = arguments;

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //process.StartInfo.UseShellExecute = true;
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            string outPutCommand = "";

            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                process.OutputDataReceived += (sendera, e1) =>
                {
                    if (e1.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        if (!e1.Data.Contains(outPutCommand) && !e1.Data.Contains(drivePath) && !e1.Data.Contains(subreadPath) && !e1.Data.Contains("Microsoft"))
                        {
                            DT.Data += e1.Data + "\r\n";
                        }




                    }
                };
                process.ErrorDataReceived += (sendera, e1) =>
                {
                    if (e1.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        if (!e1.Data.Contains(outPutCommand) && !e1.Data.Contains(drivePath) && !e1.Data.Contains(subreadPath))
                        {
                            DT.Data += e1.Data + "\r\n";
                        }

                    }
                };

                string baiIndex = "";
                string SV = "";
                if (forFastqToBam.DetectSV == true)
                {
                    SV = "--sv";
                }
                else
                {
                    SV = String.Empty;
                }
                if (forFastqToBam.SortByCoordinate == true)
                {
                    baiIndex = "--sortReadsByCoordinates";
                }
                else
                {
                    baiIndex = String.Empty;
                }
                process.Start();
                process.StandardInput.WriteLine($"{drivePath}");
                process.StandardInput.WriteLine($"cd {subreadPath}");

                if (forFastqToBam.path.Count > 0)
                {






                    for (int i = 0; i < forFastqToBam.path.Count; i++)
                    {

                        string outputPath = System.IO.Path.Combine(forFastqToBam.OutputFolder, forFastqToBam.path[i].FileName);
                        if (!System.IO.Directory.Exists(outputPath))
                            System.IO.Directory.CreateDirectory(outputPath);
                        outPutCommand = $"subread-align.exe -d 30 -D 200 -i GRCH38_index -r {forFastqToBam.path[i].Read1} -R {forFastqToBam.path[i].Read2} -o {System.IO.Path.Combine(outputPath, forFastqToBam.path[i].FileName + ".bam")} -t {forFastqToBam.SeqMode} -T {forFastqToBam.CPUThreads} {SV} {baiIndex}";
                        process.StandardInput.WriteLine($"subread-align.exe -d 30 -D 200 -i GRCH38_index -r {forFastqToBam.path[i].Read1} -R {forFastqToBam.path[i].Read2} -o {System.IO.Path.Combine(outputPath, forFastqToBam.path[i].FileName + ".bam")} -t {forFastqToBam.SeqMode} -T {forFastqToBam.CPUThreads} {SV} {baiIndex}");

                    }













                }
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.StandardInput.WriteLine("Exit");
                process.WaitForExit();



                BW.DoWork -= SubReadCall;







            }


        }
        private void RunCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (forFastqToBam.path.Count == 0)
            {
                DT.Data += "There is no files to be processed";
            }
            DT.Data = DT.Data + "\r\nRun Completed";

            BW.RunWorkerCompleted -= RunCompleted;
            RunButon.IsEnabled = true;
            Settings.IsEnabled = true;
            RunButon.Opacity = 1.0;

        }
        private void Run(object sender, RoutedEventArgs e)
        {
            checkOutputFolderPath = OutputFolderPath.Text;
            forFastqToBam.path.Clear();
            forFastqToBam.path.TrimExcess();
            DT.Data = String.Empty;
            DT.Data = "FastqToBam v 1.0.0 created by Frank Liu";
            RunButon.IsEnabled = false;
            Settings.IsEnabled = false;
            RunButon.Opacity = 0.5;
            BW.WorkerReportsProgress = true;
            BW.DoWork += SubReadCall;
            BW.RunWorkerCompleted += RunCompleted;
            BW.RunWorkerAsync();

        }

        private Boolean AutoScroll = true;
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (HistorySV.VerticalOffset == HistorySV.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set auto-scroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset auto-scroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                HistorySV.ScrollToVerticalOffset(HistorySV.ExtentHeight);
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private bool IsMaximized = false;
        public static BrushConverter converter = new BrushConverter();


        private void FullScreenClick(object sender, RoutedEventArgs e)
        {
            if (IsMaximized == false)
            {
                this.WindowState = WindowState.Maximized;
                IsMaximized = true;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.Height = 720;
                this.Width = 1080;
                IsMaximized = false;
            }
        }

        private void FolderClick(object sender, RoutedEventArgs e)
        {
            Folder.Visibility = Visibility.Visible;
            Settings.Visibility = Visibility.Collapsed;
            Process.Visibility = Visibility.Collapsed;
            if (folderStay == false)
            {

                ProcessButton.Background = (Brush)converter.ConvertFromString("Transparent");
                SettingButton.Background = (Brush)converter.ConvertFromString("Transparent");
                FolderButton.Background = (Brush)converter.ConvertFromString("#7b5cd6");
                processStay = false;
                settingStay = false;
                folderStay = true;

            }
        }

        private void SettingClick(object sender, RoutedEventArgs e)
        {
            Folder.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Visible;
            Process.Visibility = Visibility.Collapsed;
            if (settingStay == false)
            {

                ProcessButton.Background = (Brush)converter.ConvertFromString("Transparent");
                SettingButton.Background = (Brush)converter.ConvertFromString("#7b5cd6");
                FolderButton.Background = (Brush)converter.ConvertFromString("Transparent");
                processStay = false;
                settingStay = true;
                folderStay = false;

            }
        }

        private bool processStay = false;
        private bool settingStay = false;
        private bool folderStay = false;

        private void ProcessClick(object sender, RoutedEventArgs e)
        {
            Folder.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Collapsed;
            Process.Visibility = Visibility.Visible;
            if (processStay == false)
            {

                ProcessButton.Background = (Brush)converter.ConvertFromString("#7b5cd6");
                SettingButton.Background = (Brush)converter.ConvertFromString("Transparent");
                FolderButton.Background = (Brush)converter.ConvertFromString("Transparent");
                processStay = true;
                settingStay = false;
                folderStay = false;

            }
        }

        private void NextStepSetting(object sender, RoutedEventArgs e)
        {
            Folder.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Visible;
            Process.Visibility = Visibility.Collapsed;
            ProcessButton.Background = (Brush)converter.ConvertFromString("Transparent");
            SettingButton.Background = (Brush)converter.ConvertFromString("#7b5cd6");
            FolderButton.Background = (Brush)converter.ConvertFromString("Transparent");
            processStay = false;
            settingStay = true;
            folderStay = false;
        }

        private void NextStepProcess(object sender, RoutedEventArgs e)
        {
            Folder.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Collapsed;
            Process.Visibility = Visibility.Visible;

            ProcessButton.Background = (Brush)converter.ConvertFromString("#7b5cd6");
            SettingButton.Background = (Brush)converter.ConvertFromString("Transparent");
            FolderButton.Background = (Brush)converter.ConvertFromString("Transparent");
            processStay = true;
            settingStay = false;
            folderStay = false;
        }

        private void AddOutputFolder(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result.ToString() == "OK")
                {

                    OutputFolderPath.Text = dialog.SelectedPath;

                }
            }

        }

        private void AddInputFolder(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {

                DialogResult result = dialog.ShowDialog();
                if (result.ToString() == "OK")
                {

                    string[] inputFile = System.IO.Directory.GetFiles(dialog.SelectedPath);
                    for (int i = 0; i < inputFile.Length; i++)
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(inputFile[i]).Replace(".fastq", "");
                        if (fileName.Contains("R1"))
                        {
                            samples.Add(new Files { FileName = fileName, Check = false, FilePath = inputFile[i], Read = "R1", ReadBackground = (Brush)converter.ConvertFromString("#1098ad") });
                        }
                        else if (fileName.Contains("R2"))
                        {
                            samples.Add(new Files { FileName = fileName, Check = false, FilePath = inputFile[i], Read = "R2", ReadBackground = (Brush)converter.ConvertFromString("#1e88e5") });
                        }
                    }
                    fileDataGrid.ItemsSource = samples;
                }
            }

        }

        private void CloseFastqToBam(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddFilesToSamples(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;

            Nullable<bool> result = dlg.ShowDialog();

            int indexOfInsertion = fileDataGrid.SelectedIndex;

            if (result == true)
            {
                foreach (string item in dlg.FileNames)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(item).Replace(".fastq", "");
                    if (fileName.Contains("R1"))
                    {
                        samples.Insert(indexOfInsertion, new Files { FileName = fileName, Check = false, FilePath = item, Read = "R1", ReadBackground = (Brush)converter.ConvertFromString("#1098ad") });
                        indexOfInsertion++;
                    }
                    else if (fileName.Contains("R2"))
                    {
                        samples.Insert(indexOfInsertion, new Files { FileName = fileName, Check = false, FilePath = item, Read = "R2", ReadBackground = (Brush)converter.ConvertFromString("#1e88e5") });
                        indexOfInsertion++;
                    }
                }

            }
        }

        private void RemoveFilesFromSamples(object sender, RoutedEventArgs e)
        {
            var fileSelected = fileDataGrid.SelectedItems;
            Files[] copy = new Files[fileSelected.Count];

            fileSelected.CopyTo(copy, 0);
            for (int i = 0; i < copy.Count(); i++)
            {
                samples.RemoveAt(samples.IndexOf((Files)copy[i]));

            }
            copy = null;
            fileDataGrid.Items.Refresh();

        }

        private void SeqModeChecked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                switch (radioButton.Content)
                {
                    case "DNA-Seq":
                        {
                            forFastqToBam.SeqMode = 1;
                            break;
                        }
                    case "RNA-Seq":
                        {
                            forFastqToBam.SeqMode = 0;
                            break;
                        }
                    default:
                        {
                            forFastqToBam.SeqMode = 1;
                            break;
                        }
                }
            }
        }

        private void AnalysisCheck(object sender, RoutedEventArgs e)
        {
            var selected = fileDataGrid.SelectedItems;
            for (int i = 0; i < selected.Count; i++)
            {
                samples[samples.IndexOf((Files)selected[i])].Check = true;
            }
        }

        private void AnalysisUnCheck(object sender, RoutedEventArgs e)
        {
            var selected = fileDataGrid.SelectedItems;
            for (int i = 0; i < selected.Count; i++)
            {
                samples[samples.IndexOf((Files)selected[i])].Check = false;
            }
        }
    }
    public class Files : ViewBase
    {
        private bool _Check = false;

        public bool Check { get { return _Check; } set { _Check = value; OnPropertyChanged(); } }

        private string _FileName = "";
        public string FileName { get { return _FileName; } set { _FileName = value; OnPropertyChanged(); } }

        private string _Read = "";
        public string Read { get { return _Read; } set { _Read = value; OnPropertyChanged(); } }
        private Brush _ReadBackground = null;
        public Brush ReadBackground { get { return _ReadBackground; } set { _ReadBackground = value; OnPropertyChanged(); } }

        private string _FilePath { get { return _FilePath; } set { _FilePath = value; OnPropertyChanged(); } }
        public string FilePath { get; set; }
    }

    public class DataText : ViewBase
    {
        string _data = "";
        public string Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged(nameof(Data));
            }
        }

    }
    public class AnalysisPath : ViewBase
    {

        private string _FileName = "";
        public string FileName { get { return _FileName; } set { _FileName = value; OnPropertyChanged(); } }
        private string _Read1 = "";
        public string Read1 { get { return _Read1; } set { _Read1 = value; OnPropertyChanged(); } }

        private string _Read2 = "";
        public string Read2 { get { return _Read2; } set { _Read2 = value; OnPropertyChanged(); } }
    }
    public class ViewBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

    }
    public class SequenceSetForToBam : ViewBase
    {

        public List<AnalysisPath> path { get; set; }

        private string _ReferenceIndex = "GRCH38_index";
        public string ReferenceIndex { get { return _ReferenceIndex; } set { _ReferenceIndex = value; OnPropertyChanged(); } }
        private bool _SortByCoordinate = true;
        public bool SortByCoordinate { get { return _SortByCoordinate; } set { _SortByCoordinate = value; OnPropertyChanged(); } }

        private bool _DetectSV = false;
        public bool DetectSV { get { return _DetectSV; } set { _DetectSV = value; OnPropertyChanged(); } }

        private int _SeqMode = 1;

        public int SeqMode { get { return _SeqMode; } set { _SeqMode = value; OnPropertyChanged(); } }

        private string _CPUThreads = "1";
        public string CPUThreads { get { return _CPUThreads; } set { _CPUThreads = value; OnPropertyChanged(); } }

        private string _OutputFolder = "";

        public string OutputFolder { get { return _OutputFolder; } set { _OutputFolder = value; OnPropertyChanged(); } }


    }

}
