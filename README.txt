1.行情库的使用方法
1.1 初始化
IndicatorRepository repository = new IndicatorRepository("d:\\repository"); //行情库本地缓存位置
repository.Initilization();
//其中行情库本地缓存目录结构如下
securities.csv   股票基本信息
kline            日K线和周K线数据
cube文件夹       买卖线数据
fundtrend文件夹  主力线散户线数据
fundcross文件夹  主力线金叉点数据

1.2 行情库中查询股票基本信息
List<SecurityProperties> list = repository.Securities.ToList();
foreach ( SecurityProperties sp in list)
{
	Console.WriteLine(sp.ToString());
}

1.3 行情库中查询或者时序指标数据

//查询，每个时序数据有个名称，如k线是"kline",参见IndicatorMetaCollection
TimeSerialsDataSet tds = repository["330101"];
ITimeSeries ts = tds["kline", TimeUnit.day];
//创建(范型函数)
KLine kline = tds.CreateOrLoad<KLine>("kline", TimeUnit.week);
//创建(非范型函数),最后一个参数为false会先到文件里面找，找到了就不创建了
KLine kline = (KLine)tds.Create("kline", TimeUnit.week, false);

1.4 行情库中放入新时序指标
第一步：需创建一个IndicatorMeta对象
如资金动向的创建：
IndicatorMeta META_FUND_TREND = new IndicatorMeta(NAME_FUND_TREND, "fundtrend", typeof(TimeSeries<ITimeSeriesItem<List<double>>>), IndicatorClass.TimeSeries, null);
第二步：为新的时序指标数据设定生成方法
META_FUND_TREND.Geneartor = props =>
{
                TimeSerialsDataSet tsd = (TimeSerialsDataSet)props["TimeSerialsDataSet"];
                TimeUnit tu = (TimeUnit)props["timeunit"];
                String dataname = (String)props["name"];
                String code = (String)props["code"];

                return tsd.FundTrendCreate(tu);
};
第三步：注册IndicatorMeta对象
repository.RegisteMeta(META_FUND_TREND);

