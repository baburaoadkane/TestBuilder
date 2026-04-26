using System;
using System.Collections.Generic;
using System.Text;

namespace Enfinity.ERP.Automation.Modules.Sales.DataModels
{
    public class InvoiceTotalsResponse
    {
        public decimal GrossValue { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal NetValue { get; set; }
        public decimal TaxValue { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
