using System;
using System.Collections.Generic;
using System.Text;

namespace Enfinity.ERP.Automation.Modules.Sales.DataModels
{
    public class InvoiceTotalsResponse
    {
        public decimal SubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
