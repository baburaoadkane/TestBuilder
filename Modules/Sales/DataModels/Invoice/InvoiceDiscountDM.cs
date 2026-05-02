using System;
using System.Collections.Generic;
using System.Text;

namespace Enfinity.ERP.Automation.Modules.Sales.DataModels.Invoice
{
    public class InvoiceDiscountDM
    {
        /// <summary>Discount applied in percent.</summary>
        public decimal DiscountInPercent { get; set; }

        /// <summary>Discount applied in absolute value.</summary>
        public decimal DiscountValue { get; set; }

        public bool HasData()
        {
            return DiscountInPercent > 0 || DiscountValue > 0;
        }
    }
}
