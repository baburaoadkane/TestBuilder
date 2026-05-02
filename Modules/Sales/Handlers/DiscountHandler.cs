using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels.Invoice;
using OpenQA.Selenium;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers
{
    public class DiscountHandler : BaseHandler
    {
        // ── Static Fields ──────────────────────────────────────────────────        
        private static readonly By DiscountInPercent = By.XPath("//input[contains(@id, '.DiscountInPercent_I')]");
        private static readonly By DiscountValue = By.XPath("//input[contains(@id, '.DiscountValue_I')]");

        public DiscountHandler(IWebDriver driver, WaitHelper wait, ReportHelper report) : base(driver, wait, report) { }

        // ── PUBLIC ENTRY ──────────────────────────────────────────────────
        public void Fill(InvoiceDiscountDM header)
        {
            // 🔥 Fully Dynamic (NO FieldMap)
            Type(DiscountInPercent, header.DiscountInPercent);
            Type(DiscountValue, header.DiscountValue);

            WaitForLoader();
        }        
    }
}
