1.������ʹ�÷���
1.1 ��ʼ��
IndicatorRepository repository = new IndicatorRepository("d:\\repository"); //����Ȿ�ػ���λ��
repository.Initilization();
//��������Ȿ�ػ���Ŀ¼�ṹ����
securities.csv   ��Ʊ������Ϣ
kline            ��K�ߺ���K������
cube�ļ���       ����������
fundtrend�ļ���  ������ɢ��������
fundcross�ļ���  �����߽�������

1.2 ������в�ѯ��Ʊ������Ϣ
List<SecurityProperties> list = repository.Securities.ToList();
foreach ( SecurityProperties sp in list)
{
	Console.WriteLine(sp.ToString());
}

1.3 ������в�ѯ����ʱ��ָ������

//��ѯ��ÿ��ʱ�������и����ƣ���k����"kline",�μ�IndicatorMetaCollection
TimeSerialsDataSet tds = repository["330101"];
ITimeSeries ts = tds["kline", TimeUnit.day];
//����(���ͺ���)
KLine kline = tds.CreateOrLoad<KLine>("kline", TimeUnit.week);
//����(�Ƿ��ͺ���),���һ������Ϊfalse���ȵ��ļ������ң��ҵ��˾Ͳ�������
KLine kline = (KLine)tds.Create("kline", TimeUnit.week, false);

1.4 ������з�����ʱ��ָ��
��һ�����贴��һ��IndicatorMeta����
���ʽ���Ĵ�����
IndicatorMeta META_FUND_TREND = new IndicatorMeta(NAME_FUND_TREND, "fundtrend", typeof(TimeSeries<ITimeSeriesItem<List<double>>>), IndicatorClass.TimeSeries, null);
�ڶ�����Ϊ�µ�ʱ��ָ�������趨���ɷ���
META_FUND_TREND.Geneartor = props =>
{
                TimeSerialsDataSet tsd = (TimeSerialsDataSet)props["TimeSerialsDataSet"];
                TimeUnit tu = (TimeUnit)props["timeunit"];
                String dataname = (String)props["name"];
                String code = (String)props["code"];

                return tsd.FundTrendCreate(tu);
};
��������ע��IndicatorMeta����
repository.RegisteMeta(META_FUND_TREND);

