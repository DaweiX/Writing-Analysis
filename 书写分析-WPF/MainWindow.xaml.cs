using System.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Globalization;

namespace 书写分析_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //stk.Visibility = Visibility.Collapsed;
            cb_ports.Items.Add("刷新");
            c_output.Items.Add("路径");
            cb_rate.Items.Add("2400");
            cb_rate.Items.Add("4800");
            cb_rate.Items.Add("9600");
            cb_rate.Items.Add("38400");
            cb_rate.Items.Add("115200");
            c_count.Items.Add("10");
            c_count.Items.Add("12");
            c_count.Items.Add("15");
            c_count.Items.Add("20");
            c_rate.Items.Add("1.0 %");
            c_rate.Items.Add("1.5 %");
            c_rate.Items.Add("1.8 %");
            c_output.Items.Add("开");
            c_output.Items.Add("关");
            foreach (string name in SerialPort.GetPortNames())
            {
                cb_ports.Items.Add(name);
            }
            if (SerialPort.GetPortNames().Length != 0)
            {
                sp.PortName = cb_ports.Text = SerialPort.GetPortNames()[0];
            }
        }

        #region 定义        
        StringBuilder Str = new StringBuilder();
        string data = "";
        SerialPort sp = new SerialPort();
        bool Isstart = false;
        bool Set = false;
        bool isNameMode = false;
        int nn = 0;
        int error = 0;
        int a = 0;
        StringBuilder s = new StringBuilder();

        List<int> time = new List<int>();
        List<int> acc_X = new List<int>();
        List<int> acc_Y = new List<int>();
        List<int> acc_Z = new List<int>();

        List<int> gyr_X = new List<int>();
        List<int> gyr_Y = new List<int>();
        List<int> gyr_Z = new List<int>();

        List<double> ang_X = new List<double>();
        List<double> ang_Y = new List<double>();
        List<double> ang_Z = new List<double>();

        List<int> isWrite = new List<int>();

        List<int> arr = new List<int>();

        List<int[]> A_Name = new List<int[]>();
        #endregion

        private void listClear()
        {
            ang_X.Clear();
            ang_Y.Clear();
            ang_Z.Clear();
            gyr_X.Clear();
            gyr_Y.Clear();
            gyr_Z.Clear();
            acc_X.Clear();
            acc_Y.Clear();
            acc_Z.Clear();
            time.Clear();
            isWrite.Clear();
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {  
            Isstart = !Isstart;
            if (!sp.IsOpen && !string.IsNullOrEmpty(cb_ports.Text))
            {
                sp.PortName = cb_ports.Text;
                if (!string.IsNullOrEmpty(cb_rate.Text) && cb_rate.Text != "N/A")  
                    sp.BaudRate = Convert.ToInt32(cb_rate.Text);
            }
            Thread th2 = new Thread(new ThreadStart(run));            
            if (Isstart == true)//开始
            {
                if (string.IsNullOrEmpty(textBox_name.Text) && isNameMode) 
                {
                    MessageBox.Show("请输入用户名", "提示");
                    return;
                }
                if (!isNameMode)
                    ani_fade(st1, 1);
                st1.IsEnabled = false;
                listClear();
                textBox1.Visibility = Visibility.Visible;
                textBox1.Text = "";
                var aabb = th2.ThreadState.ToString();
                th2.Start();
            }
            if (Isstart == false)//停止
            {
                if (!isNameMode) 
                    ani_fade(st1, 0);
                st1.IsEnabled = isNameMode ? false : true;
                textBox1.Visibility = Visibility.Collapsed;
                th2.Abort();
                read();
                StringBuilder ss = new StringBuilder();
                for (int i = 0; i < time.Count; i++)
                {
                    ss.AppendLine(time[i].ToString());
                }
                MessageBox.Show(ss.ToString());
                //if (c_output.Text == "开") 
                //    output();
                analysis(out a);
                if (isNameMode) 
                {
                    th2.Abort();
                    label_name.Text = "当前姓名：" + textBox_name.Text;
                    label_count.Text = "已录入组数：" + (nn + 1).ToString() + "/" + c_count.Text;
                    arr.Add(a);
                    nn++;
                    if (nn.ToString() == c_count.Text)
                    {
                        nn = 0;
                        listBox_name.Items.Add(textBox_name.Text);
                        comboBox3.Items.Add(textBox_name.Text);
                        int[] temp = new int[arr.Count];
                        arr.CopyTo(temp);
                        A_Name.Add(temp);
                        MessageBox.Show("当前姓名书写信息采集完毕！", "提示");
                    }
                }

            }
        }

        void run()
        {
            //Thread th3 = new Thread(new ThreadStart(read));
            //if (th3.ThreadState != ThreadState.Running) 
            //    th3.Start();
            try
            {
                if (!sp.IsOpen)
                    sp.Open();
                byte[] b = new byte[5];
                while (Isstart)
                { 
                    sp.Read(b, 0, 5);
                    data = Convert.ToInt32(Convert.ToString(b[0])).ToString("X2") + " " +
                    Convert.ToInt32(Convert.ToString(b[1])).ToString("X2") + " " +
                    Convert.ToInt32(Convert.ToString(b[2])).ToString("X2") + " " +
                    Convert.ToInt32(Convert.ToString(b[3])).ToString("X2") + " " +
                    Convert.ToInt32(Convert.ToString(b[4])).ToString("X2") + " ";
                    Str.Append(data);
                    //read(Str.ToString());
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        textBox1.AppendText(Convert.ToInt32(Convert.ToString(b[0])).ToString("X2") + " " +
                           Convert.ToInt32(Convert.ToString(b[1])).ToString("X2") + " " +
                           Convert.ToInt32(Convert.ToString(b[2])).ToString("X2") + " " +
                           Convert.ToInt32(Convert.ToString(b[3])).ToString("X2") + " " +
                           Convert.ToInt32(Convert.ToString(b[4])).ToString("X2") + " ");
                    }));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "错误", MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        void read()
        {
            listClear();
            //string[] str = textBox1.Text.Split(' ');
            string[] str = textBox1.ToString().Split(' ');
            int n = 0;
            while (n < str.Length - 32)
            {
                if (str[n] + str[n + 1] == "AA01")
                {
                    /*
                     -----------------------------------------------------------------------
                     包头  角度  角加速度  加速度  时间  布尔  保留位  校验位  包尾
                     AA01  2 *3     2* 3    2* 3   2（u）  1     8     sum_end   5A
                     0                                                          
                     -----------------------------------------------------------------------
                    */
                    int sum = 0;
                    int[] array = new int[11];

                    array[1] = Convert.ToInt32(str[n + 2] + str[n + 3], 16);
                    array[2] = Convert.ToInt32(str[n + 4] + str[n + 5], 16);
                    array[3] = Convert.ToInt32(str[n + 6] + str[n + 7], 16);
                    array[4] = Convert.ToInt32(str[n + 8] + str[n + 9], 16);
                    array[5] = Convert.ToInt32(str[n + 10] + str[n + 11], 16);
                    array[6] = Convert.ToInt32(str[n + 12] + str[n + 13], 16);
                    array[7] = Convert.ToInt32(str[n + 14] + str[n + 15], 16);
                    array[8] = Convert.ToInt32(str[n + 16] + str[n + 17], 16);
                    array[9] = Convert.ToInt32(str[n + 18] + str[n + 19], 16);
                    array[0] = Convert.ToInt32(str[n + 20] + str[n + 21], 16);
                    array[10] = Convert.ToInt32(str[n + 22], 16);

                    for (int i = n + 2; i < n + 31; i++)
                    {
                        sum += Convert.ToInt32(str[i], 16);
                    }

                    var b = sum.ToString("X2").ToCharArray();
                    if (str[n + 32] == "5A" && b[b.Length - 2].ToString() + b[b.Length - 1].ToString() == str[n + 31])
                    {
                        ang_X.Add(reverse(array[1]) * 0.01);
                        ang_Y.Add(reverse(array[2]) * 0.01);
                        ang_Z.Add(reverse(array[3]) * 0.01);
                        gyr_X.Add(reverse(array[4]));
                        gyr_Y.Add(reverse(array[5]));
                        gyr_Z.Add(reverse(array[6]));
                        acc_X.Add(reverse(array[7]));
                        acc_Y.Add(reverse(array[8]));
                        acc_Z.Add(reverse(array[9]));
                        time.Add(array[0]);
                        isWrite.Add(array[10]);
                    }
                    else
                    {
                        error++;
                    }
                }
                n++;
            }
        }

        int reverse(int a)
        {
            if (a > 32767)
                return -(65535 - a);
            else return a;
        }

        void check(int[] array, double a)
        {
            int count = 0;
            double b = (Convert.ToInt32((c_rate.Text.ToString().Split(' ')[0]).ToString().Split('.')[0]) * 1.0 +
                Convert.ToInt32((c_rate.Text.ToString().Split(' ')[0]).ToString().Split('.')[1]) * 0.1) * 0.01;
            int c = Convert.ToInt32(c_count2.Text.ToString());
            string str = "";
            foreach (int num in array)
            {
                double min = num * (1 - b);
                double max = num * (1 + b);
                if (a < max && a > min)
                {
                    count++;
                    str += num.ToString() + "……通过" + Environment.NewLine;
                }
                else
                {
                    str += num.ToString() + "……不通过" + Environment.NewLine;
                }
            }
            //tb_report.Text = str;
            if (count > c)
                MessageBox.Show(this, "验证成功!" + a.ToString() + count.ToString(), "欢迎");
            else
                MessageBox.Show(this, "验证失败!" + a.ToString() + count.ToString(), "抱歉");
        }

        private void ani_fade(UIElement a, int b)
        {
            ///<param name="b">
            ///1是淡出，其他为淡入
            /// </param>
            Storyboard Sty = new Storyboard();
            DoubleAnimation da = new DoubleAnimation();
            da.Duration = TimeSpan.FromSeconds(0.8);
            Storyboard.SetTarget(da, a);
            Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));
            if (b == 1)
            {
                da.From = 1;
                da.To = 0;
            }
            else
            {
                da.From = 0;
                da.To = 1;
            }
            Sty.Children.Add(da);
            Sty.Begin();
        }

        void analysis(out int a)
        {
            double[] value = new double[isWrite.Count];
            for (int i = 0; i < isWrite.Count; i++)
            {
                value[i] = Math.Pow(acc_X[i], 2) + Math.Pow(acc_Y[i], 2) + Math.Pow(acc_Z[i], 2);
                value[i] = Math.Sqrt(value[i]);
            }
            a = (int)avg_1(value);
        }

        double avg_1(double[] a)
        {
            double sum = 0;
            int n = 0;
            for (int i = 0; i < isWrite.Count; i++)
            {
                //if (isWrite[i] == 1)
                //{
                sum += a[i];
                n++;
                //}
            }
            return sum / n;
        }

        void output()
        {
            string[] a = DateTime.Now.ToShortTimeString().Split(':');
            try
            {
                string path = a[0] + "：" + a[1] + "：" + DateTime.Now.Second.ToString() + ".csv";
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.WriteLine(",,,■  欢迎使用本上位机o(^▽^)o  ■,,,,");
                        sw.WriteLine("ANG_X,ANG_Y,ANG_Z,GYR_X,GYR_Y,GYR_Z,ACC_X,ACC_Y,ACC_Z,时间,Bool");
                        for (int i = 0; i < time.Count; i++)
                        {
                            sw.WriteLine(ang_X[i] + "," + ang_Y[i] + "," + ang_Z[i] + "," +
                                gyr_X[i] + "," + gyr_Y[i] + "," + gyr_Z[i] + "," +
                                acc_X[i] + "," + acc_Y[i] + "," + acc_Z[i] + "," + time[i] + "," + isWrite[i]);
                        }
                        sw.Flush();
                        sw.Close();
                        fs.Close();
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message.ToString(), "错误");
            }
        }

        private void setting_Click(object sender, RoutedEventArgs e)
        {
            Set = !Set;
            if (Set)
            {
                ani_rotate(setting, 45, 100, 0.8);
                ani_fade(st2, 1);
                ani_fade(st3, 1);
                st2.IsEnabled = false;
                st3.IsEnabled = false;
                sets.Visibility = Visibility.Visible;
                ani_fade(sets, 0);
                sets.IsEnabled = true;
            }
           else
            {
                ani_rotate(setting, -45, 100, 0.8);
                ani_fade(st2, 0);
                ani_fade(st3, 0);
                ani_fade(sets,1);
                st2.IsEnabled = true;
                st3.IsEnabled = true;
                sets.Visibility = Visibility.Collapsed;
                sets.IsEnabled = false;
            }
        }

        private void ani_rotate(UIElement a, int ang, int center, double time)
        {
            RotateTransform rtf = new RotateTransform();
            a.RenderTransform = rtf;
            rtf.CenterX = center;
            rtf.CenterY = center;
            DoubleAnimation angle = new DoubleAnimation(0, ang, new Duration(TimeSpan.FromSeconds(time)));
            rtf.BeginAnimation(RotateTransform.AngleProperty, angle);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void exit(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void c_count_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            c_count2.Items.Clear();
            for (int a = Convert.ToInt32(c_count.SelectedItem.ToString()); a > 0; a--)
            {
                c_count2.Items.Add(a.ToString());
            }
        }

        private void rec_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ani_rotate(ext, 90, 30, 0.4);
        }
        
        private void mode_Click(object sender, RoutedEventArgs e)
        {
            isNameMode = !isNameMode;
            st2.IsEnabled = true;
            if (isNameMode == true) 
            {
                txt_mode.Text = "基准录入";
                plus.Visibility = Visibility.Visible;
                ani_fade(st1, 1);
                st1.IsEnabled = false;
                st_mode.Visibility = Visibility.Visible;
                st_mode.IsEnabled = true;
                ani_fade(st_mode, 0);
            }
            if(isNameMode == false)
            {
                txt_mode.Text = "验证模式";
                plus.Visibility = Visibility.Collapsed;
                ani_fade(st_mode, 1);
                st_mode.IsEnabled = false;
                ani_fade(st1, 0);
                st1.IsEnabled = true;
                st_mode.Visibility = Visibility.Collapsed;
                st_mode.IsEnabled = false;
            }
        }

        private void textBox1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBox1.ScrollToEnd();
        }

        private void cb_ports_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cb_ports.SelectedIndex == 0)
            {
                var a = SerialPort.GetPortNames();
                cb_ports.Items.Clear();
                cb_ports.Items.Add("刷新");
                foreach (string name in SerialPort.GetPortNames())
                {
                    cb_ports.Items.Add(name);
                }
                if (SerialPort.GetPortNames().Length != 0)
                {
                    sp.PortName = cb_ports.Text = SerialPort.GetPortNames()[0];
                }
            }
            else
            {
                sp.PortName = cb_ports.Text;
            }
        }

        private void c_output_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (c_output.SelectedIndex == 0)
            {              
               // fd.ShowDialog();
                //textBox_path.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_name.SelectedItem != null)
            {
                comboBox3.Items.RemoveAt(listBox_name.SelectedIndex);
                listBox_name.Items.Remove(listBox_name.SelectedItem as string);
            }
        }

        private void btn_nameOK_Click(object sender, RoutedEventArgs e)
        {
            label_name.Text = "当前姓名：N/A";
            label_count.Text = "已录入组数：" + 0.ToString();
            if (!listBox_name.Items.Contains(textBox_name.Text))
            {
                arr.Clear();
            }
        }

        private void check_Click(object sender, RoutedEventArgs e)
        {
            //结果比对分析
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(a.ToString());
            MessageBox.Show(sb.ToString());
            check(A_Name[comboBox3.SelectedIndex], a);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var a in A_Name[listBox_name.Items.IndexOf(listBox_name.SelectedItem)]) 
            {
                sb.AppendLine(a.ToString());
            }
            MessageBox.Show(sb.ToString());
        }
    }
    public class Aaaa : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo a)
        {
            bool? b = (bool?)value;         
            if (b == true) return Visibility.Collapsed;
            return Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo a)
        {
            return null;
        }
    }

    public class Bbbb : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo a)
        {
            bool? b = (bool?)value;         
            if (b == true) return Visibility.Visible;
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo a)
        {
            return null;
        }
    }
}
