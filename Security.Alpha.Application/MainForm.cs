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

using insp.Utility.Command;
using insp.Utility.IO;
using insp.Utility.Collections;
using insp.Utility.Collections.Tree;
using insp.Utility.Collections.Time;

using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Security.Data.Indicator;

using insp.Utility.Bean;
using insp.Utility.Common;

using insp.Security.Strategy;
using insp.Security.Strategy.Alpha;

namespace Security.Alpha.Application
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            dateTimePicker1.Value = DateTime.Today;
            init();
        }

        void init()
        {
            //注册转换器
            ConvertUtils.RegisteConvertor<String, TradeInfo>(ConvertUtils.strToObject<TradeInfo>);
            ConvertUtils.RegisteConvertor<TradeInfo, String>(ConvertUtils.objectToStr);
            ConvertUtils.RegisteConvertor<String, TradeDirection>(ConvertUtils.strtoenum<TradeDirection>);
            ConvertUtils.RegisteConvertor<TradeDirection, String>(ConvertUtils.enumtostr<TradeDirection>);
            ConvertUtils.RegisteConvertor<String, TradeIntent>(ConvertUtils.strtoenum<TradeIntent>);
            ConvertUtils.RegisteConvertor<TradeIntent, String>(ConvertUtils.enumtostr<TradeIntent>);

        }
        /// <summary>
        /// 下载日线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            String filename = FileUtils.GetDirectory() + "download.cmd";
            String[] lines = File.ReadAllLines(filename);
            CommandContext context = new CommandContext();

            CommandInterpreter interpreter = new CommandInterpreter(filename,context);

            interpreter.Parse();

            this.Hide();

            interpreter.Execute();

            this.Show();
        }

        private insp.Utility.Bean.Properties fileprops;
        private insp.Utility.Bean.Properties commonProps;
        private insp.Utility.Bean.Properties alphaProps;
        private AlphaStrategy4 alpha;
        private IStrategyInstance instance;
        private log4net.ILog logger;
        private IndicatorRepository repository;
        
        private void createStrategyInstance()
        {
            if (alpha == null)
                alpha = new AlphaStrategy4();
            fileprops = insp.Utility.Bean.Properties.Load(FileUtils.GetDirectory() + "\\alpha.properties", Encoding.UTF8);
            Dictionary<String, insp.Utility.Bean.Properties> spilts = fileprops.Spilt();
            commonProps = spilts["common"];
            alphaProps = spilts["alpha"];

            String id = DateTime.Today.ToString("yyyyMMdd");            
            instance = alpha.CreateInstance(id, alphaProps);

        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            createStrategyInstance();

            logger = log4net.LogManager.GetLogger(alpha.Name);
            commonProps.Put("logger", logger);

            
            if(repository == null)
            {
                statusStrip1.Text = "加载行情...";
                repository = new IndicatorRepository(commonProps.Get<String>("repository"));
                repository.Initilization();
            }
            commonProps.Put("repository", repository);

            statusStrip1.Text = "启动执行...";
            instance.Run(commonProps);
            statusStrip1.Text = "";

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }
    }
}
