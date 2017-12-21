using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using insp.Utility.View;
using insp.Utility.Collections;
using insp.Utility.Date;
using insp.Utility.Collections.Time;

namespace Security.View
{
    public partial class StockTimeSeriesViewContainer : UserControl
    {
        #region 缺省值
        /// <summary>
        /// 缺省背景色
        /// </summary>
        public readonly Color DEFAULT_BACK_COLOR = Color.Black;
        
        #endregion

        #region 初始化
        /// <summary>
        /// 构造方法
        /// </summary>
        public StockTimeSeriesViewContainer()
        {
            InitializeComponent();
            initDefault();


        }
        /// <summary>
        /// 初始化缺省设置
        /// </summary>
        public void initDefault()
        {
            this.BackColor = DEFAULT_BACK_COLOR;
            this.origin = DEFAULT_ORIGON_POSTION;
            this.asixColor = DEFAULT_XAXIS_COLOR;
            viewInit();
        }
        #endregion

        #region 坐标轴显示参数
        /// <summary>
        /// 坐标轴原点位置
        /// </summary>
        public readonly Point DEFAULT_ORIGON_POSTION = new Point(70, 20);

        /// <summary>
        /// X轴缺省颜色 
        /// </summary>
        public readonly Color DEFAULT_XAXIS_COLOR = Color.FromArgb(64, 64, 64);
        /// <summary>
        /// X轴颜色 
        /// </summary>
        private Color asixColor;
        /// <summary>
        /// X轴颜色
        /// </summary>
        public Color AsixColor { get { return asixColor; } set { asixColor = value; xasixPen = new Pen(new SolidBrush(asixColor)); } }
        /// <summary>
        /// X轴画笔
        /// </summary>
        private Pen xasixPen;
        /// <summary>
        /// 坐标轴原点位置
        /// </summary>
        private Point origin;
        /// <summary>
        /// X轴水平线距底部高度
        /// </summary>
        public Point Origin { get { return origin; } set { origin = value; if (origin.X <= 0 || origin.Y<=0) origin = DEFAULT_ORIGON_POSTION; } }
        /// <summary>
        /// 坐标轴X长度
        /// </summary>
        public int AxisXLength { get { return this.Width - origin.X; } }
        /// <summary>
        /// 坐标轴Y长度
        /// </summary>
        public int AxisYLength { get { return this.Height - origin.Y; } }
        /// <summary>
        /// 画坐标轴
        /// </summary>
        /// <param name="g"></param>
        private void drawAsix(Graphics g)
        {
            //画X轴
            g.DrawLine(xasixPen, origin.X, origin.Y, this.Width, origin.Y);
            //画Y轴
            g.DrawLine(xasixPen, origin.X, origin.Y, origin.X, 0);
            //找到所有时间单位
            List<DateTime> datetimes = null;
            if(timeUnit == TimeUnit.day)//如果数据时间单位是日
            {
                datetimes = CalendarUtils.WorkDayList(this.beginTime, this.endTime);
            }else if(timeUnit == TimeUnit.week)
            {
                datetimes = CalendarUtils.WorkWeekList(this.beginTime, this.endTime);
            }
            if (datetimes == null) return;
            //计算每个时间单位的像素数
            int pixelNumPerTimeunit = AxisXLength / datetimes.Count;

        }
        #endregion

        


        #region 代码和时间设置
        /// <summary>
        /// 代码
        /// </summary>
        private String code;
        /// <summary>
        /// 时间单位
        /// </summary>
        private TimeUnit timeUnit = TimeUnit.day;
        /// <summary>
        /// 开始时间
        /// </summary>
        protected DateTime beginTime;
        /// <summary>
        /// 结束时间
        /// </summary>
        protected DateTime endTime;
        /// <summary>
        /// 代码
        /// </summary>
        public String Code { get { return code; } set { code = value; } }
        /// <summary>
        /// 时间单位
        /// </summary>
        public TimeUnit TimeUnit { get { return timeUnit; } set { timeUnit = value; } }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginTime { get { return beginTime; } set { beginTime = value;Refresh(); } }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get { return endTime; } set { endTime = value;Refresh(); } }
        #endregion



        #region 显示方法 
        public override void Refresh()
        {
            base.Refresh();
        }

        #endregion

        #region 内容设置
        
        /// <summary>
        /// 视图集合 
        /// </summary>
        private List<PartView> views = new List<PartView>();
        /// <summary>
        /// 创建缺省试图
        /// </summary>
        /// <returns></returns>
        private List<PartView> viewInit()
        {
            views.Clear();

            PartView view = new PartView(this);
            view.SerinalNo = 1;
            view.Height.Type = LengthType.Percent;
            view.Height.Value = 60;
            views.Add(view);

            view = new PartView(this);
            view.SerinalNo = 2;
            view.Height.Type = LengthType.Percent;
            view.Height.Value = 40;
            views.Add(view);

            return views;
        }

        /// <summary>
        /// 视图数量
        /// </summary>
        public int ViewCount
        {
            get { return views.Count; }
        }


        /// <summary>
        /// 视图清除
        /// </summary>
        public void ViewClear() { viewInit(); }
        /// <summary>
        /// 视图增加
        /// </summary>
        /// <param name="dataNames"></param>
        public void ViewInc(params String[] dataNames) { throw new NotImplementedException(); }
        /// <summary>
        /// 视图删除
        /// </summary>
        public void ViewDec(int xh=0) { throw new NotImplementedException(); }


        /// <summary>
        /// 视图信息
        /// </summary>
        public class PartView : IListOperator
        {
            /// <summary>
            /// 指标名称集
            /// </summary>
            private readonly List<String> indicatorNames = new List<string>();
            /// <summary>
            /// 指标集名称
            /// </summary>
            public String IndicatorNames
            {
                get { return indicatorNames.Aggregate((a,b)=>a+","+b); }
            }

            /// <summary>
            /// 容器
            /// </summary>
            private StockTimeSeriesViewContainer container;
            /// <summary>
            /// 构造方法 
            /// </summary>
            /// <param name="container"></param>
            public PartView(StockTimeSeriesViewContainer container)
            {
                this.container = container;
            }

            private int serinalNo;
            /// <summary>
            /// 序号
            /// </summary>
            public int SerinalNo { get { return serinalNo; } set { serinalNo = value; } }

            private readonly ViewLength height = new ViewLength();
            /// <summary>
            /// 高度
            /// </summary>
            public ViewLength Height { get {return height; } }

            
            private int horizontalSpiltLineDistance;
            /// <summary>
            /// 水平分割线距离(像素)
            /// </summary>
            public int HorizontalSpiltLineDistance { get { return horizontalSpiltLineDistance; } set { horizontalSpiltLineDistance = value; Refresh(); } }

           
            /// <summary>
            /// 刷新
            /// </summary>
            public void Refresh()
            {
                Graphics g = container.CreateGraphics();
                try
                {
                    drawHorizontalLine(g);
                    drawTitle(g);
                    drawData(g);
                }
                finally
                {
                    g.Dispose();
                }
            }
            /// <summary>
            /// 画水平线
            /// </summary>
            /// <param name="g"></param>
            private void drawHorizontalLine(Graphics g)
            {

            }
            /// <summary>
            /// 画标题
            /// </summary>
            /// <param name="g"></param>
            private void drawTitle(Graphics g)
            {

            }
            /// <summary>
            /// 画出数据
            /// </summary>
            /// <param name="g"></param>
            private void drawData(Graphics g)
            {

            }
        }
        #endregion
    }
}
