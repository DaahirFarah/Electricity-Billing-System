using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBS.viewModels
{
    public class invWrapper
    {
       public invoiceVM invoice { get; set; }

       public List<invoicevmList> invoiceList { get; set; }
    }
}