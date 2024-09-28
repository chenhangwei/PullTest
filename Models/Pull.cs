using MiniExcelLibs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PullTest.Models
{
  public   class Pull
    {
        [ExcelColumn(Name ="时间",Format = "yyyy-MM-dd HH:mm:ss")]
        public DateTime CurrentDateTime { get; set; }

        [ExcelColumn(Name ="拉力")]
        public double CurrentValue {  get; set; }  
    }
}
