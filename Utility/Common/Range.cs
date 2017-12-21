using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Utility.Common
{
    /// <summary>
    /// 范围数据
    /// </summary>   
    public class Range
    {
        private const double STEPBASE = 10;

        private readonly double begin;
        private readonly double end;
        private readonly double step;
        public Range()
        {
            begin = 0;
            end = 1;
            step = 0.1;
        }
        public Range(double begin,double end)
        {
            this.begin = begin;
            this.end = end;
            this.step = (end - begin) / STEPBASE;
        }
        public Range(double begin, double end,double step)
        {
            this.begin = begin;
            this.end = end;
            this.step =step;
        }
        public List<int> ToIntList()
        {
            List<int> list = new List<int>();
            double t = begin;
            while(true)
            {
                list.Add((int)t);
                t += step;
                if (t > end)
                    break;
            }
            return list;
        }
        public List<double> ToList()
        {
            List<double> list = new List<double>();
            double t = begin;
            while (true)
            {
                list.Add(t);
                t += step;
                if (t > end)
                    break;
            }
            return list;
        }
    }

    public class Range<T> where T : IComparable
    {
        public readonly T begin;
        public readonly T end;
        
        public Range()
        {
            begin = end = default(T);
        }
        public Range(T begin,T end)
        {
            this.begin = begin;
            this.end = end;
        }

        public bool IsIn(T value)
        {
            return value.CompareTo(begin)>=0 && value.CompareTo(end)<=0;
        }
    }
}
