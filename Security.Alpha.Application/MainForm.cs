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
using Security.Alpha.Application.State;

namespace Security.Alpha.Application
{
    public partial class MainForm : Form
    {
        Dispatcher dispatcher;
        List<EventControl> eventControls = new List<EventControl>();
        
        class EventControl
        {
            public String name;
            public Button btn;
            public Label lblState;
            public Label lblResult;
        }
        public MainForm()
        {
            InitializeComponent();
            dateTimePicker1.Value = DateTime.Today;
            dispatcher = new Dispatcher(dateTimePicker1.Value, doHandleMessage);
            init();
        }
        public  void init()
        {
            eventControls.Clear();
            eventControls.Add(new EventControl() { name= "下载日线", btn = button1,lblState= label2,lblResult= label3 });
            eventControls.Add(new EventControl() { name = "日线数据入库", btn = button2, lblState = label5, lblResult = label4 });
            eventControls.Add(new EventControl() { name = "生成指标", btn = button7, lblState = label7, lblResult = label6 });
            eventControls.Add(new EventControl() { name = "选择可卖出股票", btn = button3, lblState = label9, lblResult = label8 });
            eventControls.Add(new EventControl() { name = "选择可买入股票", btn = button4, lblState = label11, lblResult = label10 });
            eventControls.Add(new EventControl() { name = "等待确认", btn = button8, lblState = label13, lblResult = label12 });
            eventControls.Add(new EventControl() { name = "等待下单", btn = button9, lblState = label15, lblResult = label14 });
            eventControls.Add(new EventControl() { name = "执行下单", btn = button10, lblState = label17, lblResult = label16 });

            eventControls.ForEach(x => { x.btn.Enabled = true; x.lblState.Text = "等待..."; x.lblResult.Text = "状态..."; });

        }

        private void doHandleMessage(LogRecord state, int beginorend, int error = 0, String msg = "", String detailMsg = "")
        {
            toolStripStatusLabel1.Text = msg;

            EventControl eventControl = eventControls.FirstOrDefault(x => x.name == state.eventName);
            if (eventControl == null)
                return;

            if (beginorend == Dispatcher.BEGIN)
            {
                eventControl.lblState.Text = "执行中";
                eventControl.lblResult.Text = "";
            }                
            else if(beginorend == Dispatcher.PROGRESS)
            {
                eventControl.lblState.Text = "执行中";
                eventControl.lblResult.Text = state.data;
            }else if(beginorend == Dispatcher.END)
            {
                eventControl.lblState.Text = (error ==0?"完成":"失败");
                eventControl.lblResult.Text = state.msg+(state.detailMsg==""?"":","+state.detailMsg);
            }

            if (beginorend == Dispatcher.BEGIN && state.eventName == LogRecord.EVENT_DOWNLOADING)
                this.Hide();
            if (beginorend == Dispatcher.END && state.eventName == LogRecord.EVENT_DOWNLOADING)
                this.Show();


        }
        /// <summary>
        /// 一键执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            eventControls.ForEach(x => x.btn.Enabled = false);
            try
            {
                dispatcher.Execute();
            }finally
            {
                eventControls.ForEach(x => x.btn.Enabled = true);
            }
        }
        /// <summary>
        /// 下载日线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        
    

        

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            dispatcher.adminEnter = Dispatcher.ADMIN_OK;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            dispatcher.adminEnter = Dispatcher.ADMIN_CANCEL;
        }
    }
}
