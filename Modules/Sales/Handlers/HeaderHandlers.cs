using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;
using OpenQA.Selenium;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers
{
    public class HeaderHandlers : BaseHandler
    {
        // ── Common Lookup Locator ───────────────────────────────────────────
        private static readonly By LookupText = By.XPath("//div[@class='lookup-text']");

        // ── Field Mapping (🔥 CORE) ─────────────────────────────────────────
        private static readonly Dictionary<string, string> FieldMap = new()
        {
            { "Customer", "CustomerId" },
            { "Currency", "CurrencyId" },
            { "PriceList", "PriceListId" },
            { "Warehouse", "WarehouseId" },
            { "Salesman", "SalesmanId" },
            { "PaymentMethod", "PaymentMethodId" },
            { "PaymentTerm", "PaymentTermId" }
        };

        // ── Static (Non-Lookup) Fields ──────────────────────────────────────
        private static readonly By TxnDateDropdown = By.XPath("//td[contains(@id, '.TxnDate_B-1')]");
        private static readonly By TxnDateInput = By.XPath("//input[contains(@id, '.TxnDate_I')]");

        private static readonly By ReferenceNumInput = By.XPath("//input[contains(@id, '.ReferenceNum_I')]");
        private static readonly By CustomerPONumInput = By.XPath("//input[contains(@id, '.CustomerPONum_I')]");
        private static readonly By DisplayNameInput = By.XPath("//input[contains(@id, '.DisplayCustomerName_I')]");
        private static readonly By MobileNumInput = By.XPath("//input[contains(@id, '.MobileNum_I')]");

        // ── Constructor ─────────────────────────────────────────────────────
        public HeaderHandlers(IWebDriver driver, WaitHelper wait)
            : base(driver, wait) { }

        // ── PUBLIC ENTRY ────────────────────────────────────────────────────
        public void Fill(SalesInvoiceHeaderDM header)
        {
            FillInvoiceDate(header.InvoiceDate);

            Lookup("Customer", header.Customer);
            Lookup("Currency", header.Currency);
            Lookup("PriceList", header.PriceList);
            Lookup("Warehouse", header.Warehouse);
            Lookup("Salesman", header.Salesman);
            Lookup("PaymentMethod", header.PaymentMethod);
            Lookup("PaymentTerm", header.PaymentTerm);

            Type(ReferenceNumInput, header.ReferenceNum);
            Type(CustomerPONumInput, header.CustomerPONum);
            Type(DisplayNameInput, header.DisplayName);
            Type(MobileNumInput, header.MobileNum);

            WaitForLoader();
        }

        // ── 🔥 GENERIC LOOKUP METHOD ────────────────────────────────────────
        private void Lookup(string fieldKey, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var (dropdown, input, nextPage) = BuildLookupLocators(fieldKey);

            OpenDropdown(dropdown);

            //Type(input, value);
            //WaitForLoader();

            SelectOption(LookupText, nextPage, value);
        }

        // ── 🔥 DYNAMIC LOCATOR BUILDER ──────────────────────────────────────
        private (By dropdown, By input, By nextPage) BuildLookupLocators(string fieldKey)
        {
            if (!FieldMap.ContainsKey(fieldKey))
                throw new Exception($"Field '{fieldKey}' not mapped.");

            string field = FieldMap[fieldKey];

            // Detect module prefix dynamically (SalesInvoice)
            var anyInput = Wait.UntilVisible(By.XPath("//input[contains(@id,'Lookup_I')]"));
            string id = anyInput.GetAttribute("id");

            if (string.IsNullOrWhiteSpace(id) || !id.Contains('.'))
                throw new Exception($"Invalid ID format: {id}");

            string modulePrefix = id.Split('.')[0];

            string baseId = $"{modulePrefix}.{field}";

            var dropdown = By.Id($"{baseId}Lookup_B-1");
            var input = By.Id($"{baseId}Lookup_I");
            var nextPage = By.Id($"{baseId}_NextPage");

            return (dropdown, input, nextPage);
        }

        // ── NON-LOOKUP FIELDS ──────────────────────────────────────────────

        private void FillInvoiceDate(string? date)
        {
            if (string.IsNullOrWhiteSpace(date)) return;

            OpenDropdown(TxnDateDropdown);
            ClearAndType(TxnDateInput, date);
        }
    }
}