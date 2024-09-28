using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using MiniExcelLibs;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
using PdfSharp;
using PullTest.Models;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Quality;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
namespace PullTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private volatile CancellationTokenSource _cancellationTokenSource;
        private volatile CancellationTokenSource _cancellationTokenSource2;
        private SerialPort serialPort = new SerialPort();
        public ObservableCollection<Pull> pullList;
        public ObservableCollection<double> doubleList;
        ASCIIEncoding encoding = new ASCIIEncoding();
        double MaxPullValue = 0.0;
        double CurrentPullValue = 0.0;
        double BreakValue = 100;
        int JumpTime = 3;
        string testedname = "";
        byte[] sendRunBuffer = new byte[1];
        byte[] sendUpBuffer = new byte[1];
        byte[] sendDownBuffer = new byte[1];
        byte[] sendStopBuffer = new byte[1];
        SolidColorBrush brush;
        TestObject testObject;
        bool IsUpRun = false;
        bool IsDownRun = false;
        bool IsRun = false;

        public MainWindow()
        {
            InitializeComponent();
            sendRunBuffer[0] = 0xA1;
            sendStopBuffer[0] = 0xA2;
            sendUpBuffer[0] = 0xA3;
            sendDownBuffer[0] = 0xA4;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource2 = new CancellationTokenSource();
            brush = new SolidColorBrush();
            testObject = new TestObject();
            pullList = new ObservableCollection<Pull>();
            doubleList = new ObservableCollection<double>();
            pullListView.ItemsSource = pullList;
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            PortComboBox.ItemsSource = ports;
            PortComboBox.SelectedIndex = 0;
            var _Series = new ISeries[]
            {
           new LineSeries<double>
            {
                Values=doubleList,
                Name= "Pull",
                Stroke= new SolidColorPaint(SKColors.Red) { StrokeThickness = 2 },
                Fill=new SolidColorPaint(SKColors.Red.WithAlpha(90)),
                GeometryStroke=new SolidColorPaint(SKColors.Red) { StrokeThickness = 1 },
            }
            };
            Chart.Series = _Series;
            JumpTime = Settings.Default.JumpTime;
            JumpTextBox.Text = JumpTime.ToString();
            BreakValue = Settings.Default.BreakValue;
            SetBreakValueTextBox.Text = BreakValue.ToString();
            testedname = Settings.Default.TestedName;
            TestedTextBox.Text = testedname;


        }
        private async void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    await StopProgramAsync();
                    serialPort.Close();
                    if (!serialPort.IsOpen)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource2.Cancel();
                        brush.Color = Colors.Black;
                        State.Content = " 串口已经关闭";
                        State.Foreground = brush;
                        ConnectButton.Content = "\ue63f" + " " + "连接";
                        PortComboBox.IsEnabled = true;
                        BaudRateComboBox.IsEnabled = true;
                        DataBitsComboBox.IsEnabled = true;
                        ParityComboBox.IsEnabled = true;
                        StopBitsComboBox.IsEnabled = true;
                    }
                }
                else
                {
                    if (!(PortComboBox.SelectedItem == null))
                    {
                        serialPort.PortName = PortComboBox.SelectedItem.ToString();
                        switch (BaudRateComboBox.SelectedIndex)
                        {
                            case 0:
                                serialPort.BaudRate = 9600;
                                break;
                            case 1:
                                serialPort.BaudRate = 19200;
                                break;
                            case 2:
                                serialPort.BaudRate = 38400;
                                break;
                            case 3:
                                serialPort.BaudRate = 115200;
                                break;
                            default:
                                serialPort.BaudRate = 9600;
                                break;
                        }
                        switch (DataBitsComboBox.SelectedIndex)
                        {
                            case 0:
                                serialPort.DataBits = 7;
                                break;
                            case 1:
                                serialPort.DataBits = 8;
                                break;
                            default:
                                serialPort.DataBits = 8;
                                break;
                        }
                        switch (StopBitsComboBox.SelectedIndex)
                        {
                            case 0:
                                serialPort.StopBits = StopBits.One;
                                break;
                            case 1:
                                serialPort.StopBits = StopBits.OnePointFive;
                                break;
                            case 2:
                                serialPort.StopBits = StopBits.Two;
                                break;
                            case 3:
                                serialPort.StopBits = StopBits.None;
                                break;
                            default:
                                serialPort.StopBits = StopBits.One;
                                break;
                        }
                        switch (ParityComboBox.SelectedIndex)
                        {
                            case 0:
                                serialPort.Parity = Parity.None;
                                break;
                            case 1:
                                serialPort.Parity = Parity.Odd;
                                break;
                            case 2:
                                serialPort.Parity = Parity.Even;
                                break;
                            case 3:
                                serialPort.Parity = Parity.Mark;
                                break;
                            case 4:
                                serialPort.Parity = Parity.Space;
                                break;
                            default:
                                serialPort.Parity = Parity.None;
                                break;
                        }
                        if (serialPort.PortName is not null)
                        {
                            serialPort.Open();
                            _cancellationTokenSource.Cancel();
                            _cancellationTokenSource2.Cancel();
                            if (serialPort.IsOpen)
                            {
                                ConnectButton.Content = "\ue7df" + " " + "断开";
                                serialPort.DataReceived += SerialPort_DataReceived;
                                PortComboBox.IsEnabled = false;
                                BaudRateComboBox.IsEnabled = false;
                                DataBitsComboBox.IsEnabled = false;
                                ParityComboBox.IsEnabled = false;
                                StopBitsComboBox.IsEnabled = false;
                                brush.Color = Colors.Black;
                                State.Content = " 串口已经打开";
                                State.Foreground = brush;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            byte[] data = new byte[serialPort.BytesToRead];
            serialPort.Read(data, 0, data.Length);
            AddData(data);
        }
        private void AddData(byte[] data)
        {
            try
            {
                string ResultValue = Encoding.UTF8.GetString(data);
                Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (char c in ResultValue)
                {
                    if (Convert.ToInt32(c) >= 48 && Convert.ToInt32(c) <= 57)
                    {
                        sb.Append(c);
                    }
                }
                string str = sb.ToString();
                if (str == "")
                {
                }
                else
                {
                    if (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        CurrentPullValue = Convert.ToDouble(str);
                        doubleList.Add(CurrentPullValue);
                        Pull pull = new Pull();
                        pull.CurrentValue = CurrentPullValue;
                        pull.CurrentDateTime = DateTime.Now;
                        int a = pullList.Count - 1;
                        if (a > 0)
                        {
                            double b = Math.Abs(CurrentPullValue - pullList[a].CurrentValue);
                            if (b > BreakValue)
                            {
                                await StopProgramAsync();
                            }
                        }
                        pullList.Add(pull);
                        //series.Values.Add(CurrentPullValue);
                        if (pullList.Count > 0)
                        {
                            var maxpull = pullList.OrderByDescending(pull => pull.CurrentValue).FirstOrDefault();
                            MaxPullValue = maxpull.CurrentValue;
                            MaxPullLabel.Content = MaxPullValue;
                            CurrentPullLabel.Content = CurrentPullValue;
                            pullListView.ScrollIntoView(pullListView.Items[pullListView.Items.Count - 1]);
                        }
                    }
                }
            }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void RunButtonClick(object sender, RoutedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var str = MessageBox.Show("自动运行将要开始", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                    if (str == MessageBoxResult.OK)
                    {
                        _cancellationTokenSource = new CancellationTokenSource();
                        brush.Color = Colors.Green;
                        State.Content = " 自动运行中";
                        State.Foreground = brush;
                        serialPort.Write(sendRunBuffer, 0, sendRunBuffer.Length);
                    }
                }
               

            }
            else
            {
                brush.Color = Colors.Red;
                State.Content = " 串口未打开";
                State.Foreground = brush;
            }
        }
        private async void StopButtonClick(object sender, RoutedEventArgs e)
        {
            await StopProgramAsync();
        }
        private async Task StopProgramAsync()
        {
            if (serialPort.IsOpen)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource2 = new CancellationTokenSource();
                brush.Color = Colors.Red;
                State.Content = " 停止";
                State.Foreground = brush;
                for (int i = 0; i < 3; i++)
                {
                    await Task.Delay(100);
                    serialPort.Write(sendStopBuffer, 0, sendStopBuffer.Length);
                    IsUpRun = false;
                    IsDownRun = false;
                    IsRun = false;

                }
            }
            else
            {
                brush.Color = Colors.Red;
                State.Content = " 串口未连接";
                State.Foreground = brush;
            }
        }
        private async void UpButtonClick(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested && !IsDownRun && !IsRun)
            {
                if (serialPort.IsOpen)
                {
                    _cancellationTokenSource2.Cancel();
                    await UPRun();
                }
            }
            else
            {
                brush.Color = Colors.Red;
                State.Content = "不能点击向上";
                State.Foreground = brush;
            }
        }
        private async Task UPRun()
        {

            int temp = JumpTime + 1;
            serialPort.Write(sendUpBuffer, 0, sendUpBuffer.Length);

            IsUpRun = true;
            IsRun = true;
            brush.Color = Colors.Green;
            State.Content = "向上运行";
            State.Foreground = brush;
            for (int i = 0; i < JumpTime; i++)
            {
                if (_cancellationTokenSource2.Token.IsCancellationRequested)
                {
                    temp--;
                    brush.Color = Colors.Green;
                    State.Content = $"向上运行...{temp}";
                    State.Foreground = brush;
                    await Task.Delay(1000);


                }
                else
                {
                    IsUpRun = false;
                    break;

                }

            }
            serialPort.Write(sendStopBuffer, 0, sendStopBuffer.Length);
            IsUpRun = false;
            IsRun = false;
            brush.Color = Colors.Red;
            State.Content = "向上停止";
            State.Foreground = brush;
        }
        private async void DownButtonClick(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested && !IsUpRun && !IsRun)
            {
                if (serialPort.IsOpen)
                {
                    _cancellationTokenSource2.Cancel();
                    await DownRun();
                }
            }
            else
            {
                brush.Color = Colors.Red;
                State.Content = "不能点击向下";
                State.Foreground = brush;
            }
        }
        private async Task DownRun()
        {
            int temp = JumpTime + 1;
            serialPort.Write(sendDownBuffer, 0, sendDownBuffer.Length);
            IsDownRun = true;
            IsRun = true;
            brush.Color = Colors.Green;
            State.Content = "向下运行";
            State.Foreground = brush;
            for (int i = 0; i < JumpTime; i++)
            {
                if (_cancellationTokenSource2.Token.IsCancellationRequested)
                {
                    temp--;
                    brush.Color = Colors.Green;
                    State.Content = $"向下运行...{temp}";
                    State.Foreground = brush;
                    await Task.Delay(1000);
                }
                else
                {
                    IsDownRun = false;
                    break;
                }

            }
            serialPort.Write(sendStopBuffer, 0, sendStopBuffer.Length);
            IsDownRun = false;
            IsRun = false;
            brush.Color = Colors.Red;
            State.Content = "向下停止";
            State.Foreground = brush;
        }
        private void ExportToExcelButtonClick(object sender, RoutedEventArgs e)
        {
            testObject.Name = NameTextBox.Text ?? "无名称";
            testObject.Description = DescriptionTextBox.Text ?? "无描述";
            testObject.Thickness = ThicknessTextBox.Text ?? "无厚度";
            testObject.Material = MaterialTextBox.Text ?? "无内容";
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                string str = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (str is not null)
                {
                    string now = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    string name = $"{str}\\抗拉强度测试-{now}-{testObject.Name}.xlsx";
                    MiniExcel.SaveAs(name, pullList, overwriteFile: true);
                }
            }
            else
            {
                brush.Color = Colors.Red;
                State.Content = "不能导出Excel文件";
                State.Foreground = brush;
            }
        }
        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var rel = MessageBox.Show("你是否要删除当前的数据吗？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (rel == MessageBoxResult.OK)
                {
                    MaxPullValue = 0.0;
                    CurrentPullValue = 0.0;
                    MaxPullLabel.Content = "0.0";
                    CurrentPullLabel.Content = "0.0";
                    pullList.Clear();
                    doubleList.Clear();
                }
            }
            else
            {
                brush.Color = Colors.Red;
                State.Content = "不能清除数据";
                State.Foreground = brush;
            }
        }
        private void ExportToPDFButtonClick(object sender, RoutedEventArgs e)
        {
            testedname = TestedTextBox.Text == "" ? "无签名" : TestedTextBox.Text;
            Settings.Default.TestedName = testedname;
            Settings.Default.Save();
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (Capabilities.Build.IsCoreBuild)
                {
                    GlobalFontSettings.FontResolver = new CustomFontResolver();
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = "抗拉强度测试";
                    document.Info.Author = $"{testedname}";
                    document.Info.Subject = "拉力值曲线图记录";
                    var page = document.AddPage();
                    page.Size = PageSize.A3;
                    page.Orientation = PageOrientation.Landscape;
                    //绘制文本或图形，请创建一个XGraphics对象。
                    var gfx = XGraphics.FromPdfPage(page);
                    RenderTargetBitmap rtb = new RenderTargetBitmap(
                        (int)DrawBorder.ActualWidth,
                        (int)DrawBorder.ActualHeight,
                        96d,
                      96d,
                        PixelFormats.Pbgra32
                        );
                    rtb.Render(Chart);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(rtb));
                        encoder.Save(ms);
                        // Bitmap bitmap = new Bitmap(ms);
                        ms.Position = 0;
                        // 将位图转换为XImage
                        XImage ximage = XImage.FromStream(ms);// XImage.FromGdiPlusImage(bitmap);
                        // 绘制图像到PDF
                        gfx.DrawImage(ximage, 0, 0, page.Width, page.Height);
                    }
                    //要绘制文本，我们首先必须创建字体。
                    XFont font = new XFont("simfang", 15, XFontStyleEx.Bold);
                    // XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
                    //var font = new XFont("Microsoft YaHei UI", 20, XFontStyleEx.BoldItalic,options);
                    string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    testObject.Name = NameTextBox.Text == "" ? "无名称" : NameTextBox.Text;
                    gfx.DrawString($"记录时间：{time}，名称：{testObject.Name}，测试人：{testedname}", font, XBrushes.Black,
                 new XRect(0, 0, page.Width, page.Height), XStringFormats.BottomLeft);
                    string str = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    if (str is not null)
                    {
                        string now = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                        string name = $"{str}\\抗拉强度测试记录-{now}-{testObject.Name}.PDF";
                        document.Save(name);
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(name) { UseShellExecute = true });
                    }
                }
                else
                {
                    brush.Color = Colors.Red;
                    State.Content = "不能导出PDF文件";
                    State.Foreground = brush;
                }
            }
        }
        private void SetButtonClick(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (JumpTextBox.Text is not null)
                {
                    int temp = 3;
                    int.TryParse(JumpTextBox.Text, out temp);
                    JumpTime = temp;
                    Settings.Default.JumpTime = JumpTime;
                    Settings.Default.Save();
                    brush.Color = Colors.Green;
                    State.Content = "修改完成";
                    State.Foreground = brush;
                }
                if (SetBreakValueTextBox.Text is not null)
                {
                    double.TryParse(SetBreakValueTextBox.Text, out BreakValue);
                    Settings.Default.BreakValue = BreakValue;
                    Settings.Default.Save();
                    brush.Color = Colors.Green;
                    State.Content = "修改完成";
                    State.Foreground = brush;
                }
                if (TestedTextBox.Text is not null)
                {
                    Settings.Default.TestedName = TestedTextBox.Text;
                    Settings.Default.Save();
                    brush.Color = Colors.Green;
                    State.Content = "修改完成";
                    State.Foreground = brush;
                }
            }
            else
            {
                brush.Color = Colors.Red;
                State.Content = "不能修改";
                State.Foreground = brush;
            }
        }
    }
}
