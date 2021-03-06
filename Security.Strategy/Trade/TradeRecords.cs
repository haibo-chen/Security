﻿using insp.Utility.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 交易记录
    /// </summary>
    public class TradeRecords : IListOperator
    {
        #region 回合信息
        private String code;

        public String Code { get { return code; } set { code = value; } }
        /// <summary>
        /// 回合
        /// </summary>
        private List<TradeBout> bouts = new List<TradeBout>();
        /// <summary>
        /// 回合
        /// </summary>
        public List<TradeBout> Bouts { get { return bouts; } }

        public TradeRecords() { }
        public TradeRecords(String code){ this.code = code; }
        public TradeRecords(String code, List<TradeBout> bouts)
        {
            this.code = code;
            this.bouts = bouts;
        }

        /// <summary>
        /// 删除未完成的测试回合
        /// </summary>
        /// <param name="bouts"></param>
        public void RemoveUnCompeletedBouts()
        {
            if (bouts == null) return;
            for (int i = 0; i < bouts.Count; i++)
            {
                if (bouts[i].Completed)
                    continue;
                bouts.RemoveAt(i--);
            }
        }

        public void Print(log4net.ILog log,bool detailed = false)
        {
            if (bouts != null && bouts.Count > 0)
            {
                if(detailed)
                {
                    foreach (TradeBout bout in bouts)
                    {
                        log.Info("  " + bout.ToString());
                    }
                }
                

                double totalProfilt = bouts.Sum(x => x.Profit);
                double totalCost = bouts.Sum(x => x.BuyInfo.TradeCost);
                log.Info(Code + ":回合数=" + bouts.Count.ToString() +
                                   ",胜率=" + (bouts.Count(x => x.Win) * 1.0 / bouts.Count).ToString("F2") +
                                   ",盈利=" + bouts.Sum(x => x.Profit).ToString("F2") +                                   
                                   ",平均盈利率=" + (totalProfilt / totalCost).ToString("F3"));

                

            }
        }
        #endregion

        #region 读取
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="filename"></param>
        public void Save(String filename)
        {
            List<String> lines = new List<string>();
            bouts.ForEach(x => lines.Add(x.ToText()));
            System.IO.File.WriteAllLines(filename, lines.ToArray());
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static TradeRecords Load(String filename)
        {
            String[] lines = System.IO.File.ReadAllLines(filename);
            if (lines == null || lines.Length <= 0)
                return new TradeRecords();

            TradeRecords records = new TradeRecords();
            foreach (String line in lines)
            {
                TradeBout bout = TradeBout.Parse(line);
                if (bout != null)
                    records.bouts.Add(bout);
            }
            return records;
        }
        #endregion

        #region 回合数量信息
        /// <summary>
        /// 总回合数
        /// </summary>
        public int Count
        {
            get { return bouts.Count; }
        }
        /// <summary>
        /// 完成的回合数
        /// </summary>
        public int CountCompleted
        {
            get { return bouts.Count(x=>x.Completed); }
        }
        /// <summary>
        /// 赢数
        /// </summary>
        public int WinCount
        {
            get { return bouts.Count(x => x.Profit > 0); }
        }
        /// <summary>
        /// 数
        /// </summary>
        public int LossCount
        {
            get { return bouts.Count(x => x.Profit < 0); }
        }
        /// <summary>
        /// 持仓天数
        /// </summary>
        public int PositionDaysAverage
        {
            get
            {
                if (bouts.Count <= 0) return 0;
                return (int)bouts.Average(x => x.PositionDays);                
            }
        }
        /// <summary>
        /// 最大持仓天数
        /// </summary>
        public int PositionDaysMax
        {
            get
            {
                if (bouts.Count <= 0) return 0;
                return bouts.Max(x => x.PositionDays);
            }
        }
        #endregion

        #region 过程信息
        /// <summary>
        /// 最大持仓天数
        /// </summary>
        public int MaxHoldDays
        {
            get { return bouts.Max(x => x.PositionDays); }
        }
        /// <summary>
        /// 平均持仓天数
        /// </summary>
        public int AverageHoldDays
        {
            get { return (int)bouts.Average(x => (double)x.PositionDays); }
        }
        
        #endregion


        #region 盈利信息
        /// <summary>
        /// 胜率
        /// </summary>
        public double WinRate
        {
            get { return WinCount*(1.0) / CountCompleted; }
        }

        /// <summary>
        /// 盈利额
        /// </summary>
        public double Profit
        {
            get { return bouts.Sum(x => x.Profit); }
        }
        /// <summary>
        /// 平均盈利率
        /// </summary>
        public double EarningsRateAverage
        {
            get
            {
                return bouts.Average(x => x.EarningsRate);
            }
        }

        #endregion

        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "胜率=" + WinRate.ToString("F2") + "(" + WinCount + "/" + Count + ")" + ",盈利=" + Profit.ToString("F2")+",持仓天数(平均/最长)="+ PositionDaysAverage+"/"+ PositionDaysMax;
        }
    }
}
