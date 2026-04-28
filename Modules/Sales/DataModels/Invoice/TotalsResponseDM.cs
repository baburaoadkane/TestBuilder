using System;
using System.Collections.Generic;
using System.Text;

namespace Enfinity.ERP.Automation.Modules.Sales.DataModels.Invoice
{
    public class TotalsResponseDM
    {
        public decimal GrossValue { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal DiscountValueLC { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal NetValue { get; set; }
        public decimal NetValueLC { get; set; }
        public decimal TaxValue { get; set; }
        public decimal TaxValueLC { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
