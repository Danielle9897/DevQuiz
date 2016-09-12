using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevQuiz.Models
{
    public class Stats
    {
        public int StatsId { get; set; }
        public int VisitorsCount { get; set; }
        public DateTime LastAccessTime { get; set; }        
    }
}