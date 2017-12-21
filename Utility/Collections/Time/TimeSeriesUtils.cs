using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Collections;

namespace insp.Utility.Collections.Time
{
    /// <summary>
    /// SLOPE实现
    /// </summary>
    public static class TimeSeriesSLOPEUtils
    {
        /// <summary>
        /// y=a+b*x;
        ///最小平方法求出估计值a,b,代入得估计直线
        ///b1:=∑(x(i)-avr(x,30))* (y(i)-avr(y,30));
        ///b2:=∑(x(i)-avr(x,30))^2;
        ///b:=b1/b2;
        ///a:=avr(y,30)-b*avr(x,30);SLOPE=(X,N)
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> SLOPE(this TimeSeries<ITimeSeriesItem<double>> ts, int num)
        {
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();

            for (int i=num-1;i<ts.Count;i++)
            {
                List<double> list = ts[i - num + 1, i].ConvertAll<double>(x => x.Value).ToList();
                double slope = list.SLOPE();

                TimeSeriesItem<double> item = new TimeSeriesItem<double>()
                {
                    Date = ts[i].Date,
                    Value = slope
                };

                results.Add(item);
            }
            return results;
            
        }
        
    }
    public static class TimeSeriesHHVUtils
    {
        
        public static TimeSeries<ITimeSeriesItem<double>> HHV(this TimeSeries<ITimeSeriesItem<double>> ts, int num)
        {
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();

            for (int i = num - 1; i < ts.Count; i++)
            {
                double max = 0;
                for (int j = i - num + 1; j <= i; j++)
                {
                    if (max < ts[j].Value)
                        max = ts[j].Value;
                }
                TimeSeriesItem<double> item = new TimeSeriesItem<double>()
                {
                    Date = ts[i].Date,
                    Value = max
                };
                results.Add(item);
            }
            return results;
        }
    }

    public static class TimeSeriesLLVUtils
    { 
        public static TimeSeries<ITimeSeriesItem<double>> LLV(this TimeSeries<ITimeSeriesItem<double>> ts, int num)
        {
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();

            for (int i = num - 1; i < ts.Count; i++)
            {
                double min = double.MaxValue;
                for (int j = i - num + 1; j <= i; j++)
                {
                    if (min > ts[j].Value)
                        min = ts[j].Value;
                }
                TimeSeriesItem<double> item = new TimeSeriesItem<double>()
                {
                    Date = ts[i].Date,
                    Value = min
                };
                results.Add(item);
            }
            return results;
        }
    }
    public static class TimeSeriesMAUtils
    {
        /// <summary>
        /// MA(X,N)简单算术平均
        /// 求X的N日移动平均值，不分轻重，平均算。算法是：
        /// (X1+X2+X3+…..+Xn)/N
        /// 例如：MA(C，20)表示20日的平均收盘价。C表示CLOSE。
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> MA(this TimeSeries<ITimeSeriesItem<double>> ts,int num)
        {
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();
            for(int i=num-1;i<ts.Count;i++)
            {
                ITimeSeriesItem t = ts[i];
                TimeSeriesItem<double> item = new TimeSeriesItem<double>()
                {
                    Date = t.Date,
                    Value = ts[i - num + 1, i].Sum(x => x.Value) / num
                };
                results.Add(item);
            }
            return results;
        }

        /// <summary>
        /// 例如：MA(C，20)表示20日的平均收盘价。C表示CLOSE。
        ///EMA(X, N)指数平滑移动平均
        ///求X的N日指数平滑移动平均，它真正的公式表达是：当日指数平均值=平滑系数*（当日指数值-昨日指数平均值）+昨日指数平均值；平滑系数=2/（周期单位+1）；由以上公式推导开，得到：EMA(C, N)=2*C/(N+1)+(N-1)/(N+1)*昨天的指数收盘平均值；
        ///算法是：若Y=EMA(X，N)，则Y=［2*X+(N-1)*Y’］/(N+1)，其中Y’表示上一周期的Y值。
        ///EMA引用函数在计算机上使用递归算法很容易实现，但不容易理解。例举分析说明EMA函数。
        ///X是变量，每天的X值都不同，从远到近地标记，它们分别记为X1，X2，X3，….，Xn
        ///如果N = 1，则EMA(X，1)=［2*X1+(1-1)*Y’］/(1+1)=X1
        ///如果N = 2，则EMA(X，2)=［2*X2+(2-1)*Y’］/(2+1)=(2/3)*X2+(1/3)X1
        ///如果N = 3，则EMA(X，3)=［2*X3+(3-1)*Y’］/(3+1)=［2*X3+2*((2/3)*X2+(1/3)*X1)］/4=(1 /2)*X3+(1/3)*X2+(1/6)*X1
        ///如果N = 4，则EMA(X，4)=［2*X4+(4-1)*Y’］/(4+1)=2/5*X4+3/5*((1/2)*X3+(1/3)*X2+(1 /6)*X1)=2/5*X4+3/10*X3+1/5*X2+1/10*X1
        ///如果N = 5，则EMA(X，5)=2/(5+1)*X5+(5-1)/(5+1)(2/5*X4+3/10*X3+3/15*X2+3/30*X1)=(1/3)*X5+(4/15)*X4+(3/15)*X3+(2/15)*X2+(1/15)*X1
        ///任何时候系数之和恒为1。如果X是常量，每天的X值都不变，则EMA(X, N)=MA(X, N).
        ///从以上的例举分析中，我们可以看到时间周期越近的X值它的权重越大，说明EMA函数对近期的X值加强了权重比，更能及时反映近期X值的波动情况。所以EMA比Ma更具参考价值，而ema 也不容易出现死叉和金叉，所以一旦出现要立即作出反映！对周线处理，ema就更加稳定了
        public static TimeSeries<ITimeSeriesItem<double>> EMA(this TimeSeries<ITimeSeriesItem<double>> ts, int num)
        {
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();
            if (ts == null || ts.Count <= 0)
                return results;
            TimeSeriesItem<double> obj = new TimeSeriesItem<double>()
            {
                Date = ts[0].Date,
                Value = ts[0].Value
            };
            results.Add(obj);
            double prevValue = obj.Value;
            for(int i = 1; i < ts.Count; i++)
            {
                double value = (2 * ts[i].Value + (num - 1) * prevValue) / (num + 1);
                obj = new TimeSeriesItem<double>()
                {
                    Date = ts[i].Date,
                    Value = value
                };
                results.Add(obj);
                prevValue = value;
            }
            return results;
        }

        /// <summary>
        /// 例如：MA(C，20)表示20日的平均收盘价。C表示CLOSE。
        ///EMA(X, N)指数平滑移动平均
        ///求X的N日指数平滑移动平均，它真正的公式表达是：当日指数平均值=平滑系数*（当日指数值-昨日指数平均值）+昨日指数平均值；平滑系数=2/（周期单位+1）；由以上公式推导开，得到：EMA(C, N)=2*C/(N+1)+(N-1)/(N+1)*昨天的指数收盘平均值；
        ///算法是：若Y=EMA(X，N)，则Y=［2*X+(N-1)*Y’］/(N+1)，其中Y’表示上一周期的Y值。
        ///EMA引用函数在计算机上使用递归算法很容易实现，但不容易理解。例举分析说明EMA函数。
        ///X是变量，每天的X值都不同，从远到近地标记，它们分别记为X1，X2，X3，….，Xn
        ///如果N = 1，则EMA(X，1)=［2*X1+(1-1)*Y’］/(1+1)=X1
        ///如果N = 2，则EMA(X，2)=［2*X2+(2-1)*Y’］/(2+1)=(2/3)*X2+(1/3)X1
        ///如果N = 3，则EMA(X，3)=［2*X3+(3-1)*Y’］/(3+1)=［2*X3+2*((2/3)*X2+(1/3)*X1)］/4=(1 /2)*X3+(1/3)*X2+(1/6)*X1
        ///如果N = 4，则EMA(X，4)=［2*X4+(4-1)*Y’］/(4+1)=2/5*X4+3/5*((1/2)*X3+(1/3)*X2+(1 /6)*X1)=2/5*X4+3/10*X3+1/5*X2+1/10*X1
        ///如果N = 5，则EMA(X，5)=2/(5+1)*X5+(5-1)/(5+1)(2/5*X4+3/10*X3+3/15*X2+3/30*X1)=(1/3)*X5+(4/15)*X4+(3/15)*X3+(2/15)*X2+(1/15)*X1
        ///任何时候系数之和恒为1。如果X是常量，每天的X值都不变，则EMA(X, N)=MA(X, N).
        ///从以上的例举分析中，我们可以看到时间周期越近的X值它的权重越大，说明EMA函数对近期的X值加强了权重比，更能及时反映近期X值的波动情况。所以EMA比Ma更具参考价值，而ema 也不容易出现死叉和金叉，所以一旦出现要立即作出反映！对周线处理，ema就更加稳定了
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        /*public static TimeSeries<ITimeSeriesItem<double>> EMA(this TimeSeries<ITimeSeriesItem<double>> ts, int num)
        {
            int denominator = num * (num + 1) / 2;
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();
            for(int i=num-1;i<ts.Count;i++)
            {
                double sum = 0;
                for(int j=num;j>=1;j--)                
                    sum += (j*1.0 / denominator) * ts[i + j - num].Value;

                TimeSeriesItem<double> item = new TimeSeriesItem<double>()
                {
                    Date = ts[i].Date,
                    Value = sum
                };

                results.Add(item);
            }
            return results;
        }*/

        public static void EMA(this TimeSeries<ITimeSeriesItem<double>> output, TimeSeries<ITimeSeriesItem> input,String valueName, int num,DateTime begin,DateTime end)
        {
            if (input == null || output == null) return;
            int tBegin = input.IndexOf(begin);

            for (DateTime t = begin;t<=end;t=t.AddDays(1))
            {
                ITimeSeriesItem item = input[t];
                if (item == null) continue;
                int index = output.IndexOf(t);
                if (output[index - 1] == null) continue;
                double p1 = 0;
                if (tBegin == 0)
                    p1 = item.GetValue<double>(valueName);
                else
                    p1 = output[index - 1].GetDefaultValue<double>();

                double value = 0;
                if (valueName == null || valueName == "")
                    value = item.GetDefaultValue<double>();
                else
                    value = item.GetValue<double>(valueName);
                //Y=［2*X+(N-1)*Y’］/(N+1)
                double y = (2 * value + (num - 1) * p1) / (num + 1);

                TimeSeriesItem<double> obj = new TimeSeriesItem<double>()
                {
                    Date = t,
                    Value = y
                };

                output[t] = obj;
            }            
        }

        /// <summary>
        /// 假设日期：1，2，3，4，5，6，7，8，9，10，11，12，13，14，15，16，17，18，19，20.。。。。。。。
        /// 如果在10这个地方统计MA（C,5）=（6+7+8+9+10）/5;这是以10为终点,向前统计法;
        /// 如果在10这个地方统计XMA（C,5）=（8+9+10+11+12）/5;这是以10为中点,从中间向前和向后统计法;
        /// 如果在10这个地方统计MA（C,7）=（4+5+6+7+8+9+10）/7;这是以10为终点,向前统计法;
        /// 如果在10这个地方统计XMA（C,7）=（7+8+9+10+11+12+13）/7;这是以10为中点,从中间向前和向后统计法;
        /// 如果在10这个地方统计MA（C,9）=（2+3+4+5+6+7+8+9+10）/9;这是以10为终点,向前统计法;
        /// 如果在10这个地方统计XMA（C,9）=（6+7+8+9+10+11+12+13+14）/9;这是以10为中点,从中间向前和向后统计法;
        /// 假如今天就是10号收盘,前面的数据已经发生了,但是11号、12、13、14、15、16、17、18、19、20......没有发生，
        /// 没有发生就没数据，但XMA如何给没发生的赋值数据呢？
        /// 原理很简单：假如是XMA（C,7）,以10号收盘来统计XMA（C,7）,7、8、9、10数据已经有了，
        /// 但11、12、13还没发生，数据没有，怎么办？就是这用7、8、9、10这4天的平均价赋值分别给11、12、13；然后全部求平均。
        /// 如果是N天呢？N天后的没有发生的怎么赋值？那就用N天前的包括N天的（（N+1)/2）天的平均价赋值；
        /// 但是有一点，XMA还怪在这里：当今天10号已经过去，11号变成今天，昨天对11号没发生的赋值又会用今天的实际值来取代。
        /// 用11号实际发生的数值取代昨天对今天的赋值；
        /// XMA（C,N）里的一般为奇数，当N设定为偶数时候，它怎么办呢？就是自动采用N+1法自动调整为奇数。
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> XMA(this TimeSeries<ITimeSeriesItem<double>> ts, int num)
        {
            if (num % 2 == 0) num += 1;
            int len = (num - 1) / 2;

            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();

            for(int i=len;i<ts.Count;i++)
            {
                double sum = 0,temp=0;
                for(int j=i-len;j<=i+len;j++)
                {
                    if (j < ts.Count)
                    {
                        sum += ts[j].Value;                        
                    }                        
                    else
                    {
                        if (temp == 0)
                            temp = sum / (j - i + len);
                        sum += temp;
                    }
                }


                TimeSeriesItem<double> item = new TimeSeriesItem<double>()
                {
                    Date = ts[i].Date,
                    Value = sum / num
                };
                results.Add(item);
            }
            return results;
        }
    }
}
