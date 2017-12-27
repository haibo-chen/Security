using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using insp.Security.Import.Info;
using insp.Security.Data;
using insp.Security.Data.Security;
using insp.Security.Data.kline;
using insp.Utility.Collections.Time;
using insp.Security.Data.Indicator.Macd;

namespace insp.Security.Import
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 行情库
        /// </summary>
        private IndicatorRepository repository;
        public Form1()
        {
            InitializeComponent();
            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker2.Value = DateTime.Today;                        
        }

        #region 基本信息
        private void button3_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            textBox2.Text = dlg.SelectedPath;

        }
        private void button4_Click_1(object sender, EventArgs e)
        {
            repository = new IndicatorRepository(textBox2.Text);
            repository.Initilization();
        }
        /// <summary>
        /// 浏览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            this.textBox1.Text = dlg.FileName;
        }
        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (repository == null)
            {
                repository = new IndicatorRepository(textBox2.Text);
                repository.Initilization();
            }
                
            List<SecurityProperties> sps = null;
            if (radioButton1.Checked)
            {
                sps = new SHTrader().LoadStockAInfo(this.textBox1.Text);
            }
            else if(radioButton2.Checked)
            {
                sps = new SZTrader().LoadStockInfo(this.textBox1.Text,"主板");
            }
            else if(radioButton3.Checked)
            {
                sps = new SZTrader().LoadStockInfo(this.textBox1.Text, "创业板");
            }
            else if (radioButton4.Checked)
            {
                sps = new SZTrader().LoadStockInfo(this.textBox1.Text, "中小板");
            }else
            {
                MessageBox.Show("需选择导入类型");
                return;
            }
            if (sps == null)
            {
                MessageBox.Show("没有需要导入的数据");
                return;
            }

            SecurityPropertiesSet securities = repository.Securities;
            securities.Merge(sps);
            repository.SaveSecuritiesInfo();
            MessageBox.Show("导入完成");

        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            textBox2.Text = dlg.SelectedPath;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.repository = new IndicatorRepository(textBox2.Text);
            repository.Initilization();
        }
        #endregion

        #region K线
        private void button6_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            textBox3.Text = dlg.SelectedPath;

        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (repository == null)
            {
                repository = new IndicatorRepository(textBox2.Text);
                repository.Initilization();
            }

            if (radioButton5.Checked)//飞狐交易师CSV格式
            {
                DirectoryInfo dInfo = new DirectoryInfo(textBox3.Text);
                FileInfo[] fInfos = dInfo.GetFiles("*.csv");
                int num = 0;
                for (int i = 0; i < fInfos.Length; i++)
                {
                    FileInfo x = fInfos[i];
                    KLine kline = new KLine(x.Name.Substring(2, 6), Utility.Collections.Time.TimeUnit.day);
                    kline.Load(x.FullName, false, ",", new String[] { "时间", "开盘", "最高", "最低", "收盘", "成交量", "成交额" });

                    toolStripStatusLabel1.Text = "导入" + kline.Code + "...";
                    Application.DoEvents();
                    TimeSerialsDataSet ds = repository[kline.Code];
                    if (ds == null) continue;

                    ds.DayKLine = kline;
                    num += 1;
                }
                MessageBox.Show("导入完成,共有" + num + "个日线完成导入");

            }
            else if (radioButton6.Checked)//通达信金融终端TXT格式
            {
                String[] columns = {"时间", "开盘", "最高", "最低", "收盘", "成交量", "成交额" };
                DirectoryInfo dInfo = new DirectoryInfo(textBox3.Text);
                FileInfo[] fInfos = dInfo.GetFiles("*.txt");
                int num = 0;
                for (int i = 0; i < fInfos.Length; i++)
                {
                    FileInfo x = fInfos[i];
                    String code = x.Name.Substring(3, 6);
                    KLine kline = new KLine(code, Utility.Collections.Time.TimeUnit.day);
                    String[] lines = File.ReadAllLines(x.FullName);
                    if (lines == null || lines.Length <= 0)
                        continue;
                    List<String> strs = lines.ToList();
                    strs.RemoveAt(0);
                    strs.RemoveAt(0);
                    strs.RemoveAt(strs.Count-1);                    
                    kline.Load(strs.ToArray(),columns);
                    kline.ForEach(y => y["code"] = code);

                    toolStripStatusLabel1.Text = "导入" + kline.Code + "...";
                    Application.DoEvents();
                    TimeSerialsDataSet ds = repository[kline.Code];
                    if (ds == null) continue;

                    ds.DayKLine = kline;
                    num += 1;
                }
                MessageBox.Show("导入完成,共有" + num + "个日线完成导入");
            }
        }
        /// <summary>
        /// 周K
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button14_Click(object sender, EventArgs e)
        {
            if (repository == null)
            {
                repository = new IndicatorRepository(textBox2.Text);
                repository.Initilization();
            }
            SecurityPropertiesSet securities = repository.Securities;
            List<String> codes = securities.Codes;
            int num = 0;
            foreach (String code in codes)
            {
                if (code == null || code == "") continue;
                TimeSerialsDataSet tsd = repository[code];
                if (tsd == null || tsd.DayKLine == null || tsd.DayKLine.Count <= 0)
                    continue;
                showText(code + "...");

                KLine weekLine = (KLine)tsd.Create("kline", TimeUnit.week, checkBox2.Checked);                
            }
        }
        /// <summary>
        /// 月k
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button13_Click(object sender, EventArgs e)
        {
            if (repository == null)
            {
                repository = new IndicatorRepository(textBox2.Text);
                repository.Initilization();
            }
            SecurityPropertiesSet securities = repository.Securities;
            List<String> codes = securities.Codes;
            int num = 0;
            foreach (String code in codes)
            {
                if (code == null || code == "") continue;
                TimeSerialsDataSet tsd = repository[code];
                if (tsd == null || tsd.DayKLine == null || tsd.DayKLine.Count <= 0)
                    continue;
                showText(code + "...");

                
                KLine monthKline = (KLine)tsd.Create("kline", TimeUnit.month, checkBox2.Checked);
            }
        }
        #endregion

        #region Alpha
        private void button7_Click(object sender, EventArgs e)
        {
            if (repository == null)
            {
                repository = new IndicatorRepository(textBox2.Text);
                repository.Initilization();
            }
            SecurityPropertiesSet securities = repository.Securities;
            List<String> codes = securities.Codes;
            int num = 0;
            foreach(String code in codes)
            {
                if (code == null || code == "") continue;
                TimeSerialsDataSet tsd = repository[code];
                if (tsd == null || tsd.DayKLine == null || tsd.DayKLine.Count <= 0)
                    continue;
                showText(code + "...");
                tsd.CubeCreate(Utility.Collections.Time.TimeUnit.day);
                tsd.FundTrendCreate(Utility.Collections.Time.TimeUnit.day);
                tsd.CubeCreate(Utility.Collections.Time.TimeUnit.week);
                tsd.FundTrendCreate(Utility.Collections.Time.TimeUnit.week);
                num += 1;
            }
            showText("");
            MessageBox.Show("生成完成,共有"+num.ToString()+"个股票生成数据");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (repository == null)
            {
                repository = new IndicatorRepository(textBox2.Text);
                repository.Initilization();
            }
            SecurityPropertiesSet securities = repository.Securities;
            List<String> codes = securities.Codes;
            int num = 0;
            foreach (String code in codes)
            {
                if (code == null || code == "") continue;
                TimeSerialsDataSet tsd = repository[code];
                if (tsd == null || tsd.DayKLine == null || tsd.DayKLine.Count <= 0)
                    continue;
                showText(code + "...");
                tsd.CubeCreate(Utility.Collections.Time.TimeUnit.month);
                tsd.FundTrendCreate(Utility.Collections.Time.TimeUnit.month);
                num += 1;
            }
            showText("");
            MessageBox.Show("生成完成,共有" + num.ToString() + "个股票生成数据");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (repository == null)
            {
                repository = new IndicatorRepository(textBox2.Text);
                repository.Initilization();
            }
            DateTime beginDate = this.dateTimePicker1.Value;
            DateTime endDate = this.dateTimePicker2.Value;

            SecurityPropertiesSet securities = repository.Securities;
            List<String> codes = securities.Codes;
            int num = 0;
            foreach (String code in codes)
            {
                if (code == null || code == "") continue;
                TimeSerialsDataSet tsd = repository[code];
                if (tsd == null || tsd.DayKLine == null || tsd.DayKLine.Count <= 0)
                    continue;
                showText(code + "...");

                tsd.CubeCreate(Utility.Collections.Time.TimeUnit.day);
                tsd.FundTrendCreate(Utility.Collections.Time.TimeUnit.day);
                tsd.CubeCreate(Utility.Collections.Time.TimeUnit.week);
                tsd.FundTrendCreate(Utility.Collections.Time.TimeUnit.week);
                num += 1;
            }
            showText("");
            MessageBox.Show("生成完成,共有" + num.ToString() + "个股票生成数据");

        }
        /// <summary>
        /// 生成MACD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            if (repository == null)
            {
                repository = new IndicatorRepository(textBox2.Text);
                repository.Initilization();
            }
            SecurityPropertiesSet securities = repository.Securities;
            List<String> codes = securities.Codes;
            int num = 0;
            foreach (String code in codes)
            {
                if (code == null || code == "") continue;
                TimeSerialsDataSet tsd = repository[code];
                if (tsd == null) continue;
                KLine dayLine = tsd.DayKLine;
                if(dayLine != null)
                {
                    tsd.Create("macd", TimeUnit.day, checkBox1.Checked);
                }
                KLine weekLine = tsd.WeekKLine;
                if(weekLine != null)
                {
                    tsd.Create("macd", TimeUnit.week, checkBox1.Checked);
                }
                if (tsd == null || tsd.DayKLine == null || tsd.DayKLine.Count <= 0)
                    continue;
                showText(code + "...");
                
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (repository == null)
            {
                repository = new IndicatorRepository(textBox2.Text);
                repository.Initilization();
            }
            SecurityPropertiesSet securities = repository.Securities;
            List<String> codes = securities.Codes;
            int num = 0;
            foreach (String code in codes)
            {
                if (code == null || code == "") continue;
                TimeSerialsDataSet tsd = repository[code];
                if (tsd == null) continue;

                KLine dayLine = tsd.DayKLine;
                if (dayLine == null) continue;

                MACD macd = (MACD)tsd.Create("macd", TimeUnit.day, false);
                if (macd == null) continue;



            }
        }
        #endregion

        public void showText(String text)
        {
            this.toolStripStatusLabel1.Text = text;
            Application.DoEvents();
        }

        
    }
}
