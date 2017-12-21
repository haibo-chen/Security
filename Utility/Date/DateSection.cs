using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Utility.Date
{
    public class DateSection
    {
        public readonly DateTime Begin;
        public readonly DateTime End;

        public DateSection()
        {
            Begin = new DateTime(DateTime.Today.Year, 1, 1);
            End = DateTime.Today;
        }
        public DateSection(DateTime Begin)
        {
            this.Begin = Begin;
            this.End = DateTime.Today;
        }
        public DateSection(DateTime Begin, DateTime End)
        {
            this.Begin = Begin;
            this.End = End;
        }

        public bool isIn(DateTime date)
        {
            return date >= Begin && date <= End;
        }

    }

    public class DateSection<T> : DateSection
    {
        
        public T Data;

        
        public DateSection(DateTime Begin, DateTime End,T data):base(Begin,End)
        {
            this.Data = data;
        }
    }
}
