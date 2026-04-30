using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels.Invoice;
using OpenQA.Selenium;

namespace Enfinity.ERP.Automation.Archieve
{
    /// <summary>
    /// Handles all UI interactions for the Sales Invoice HEADER section.
    /// Optimized with reusable lookup handling.
    /// </summary>
    public class HeaderHandler : BaseHandler
    {
        // ── Common Lookup Locators ──────────────────────────────────────────
        private static readonly By LookupText = By.XPath("//div[@class='lookup-text']");

        // ── Date ────────────────────────────────────────────────────────────
        private static readonly By TxnDateDropdown = By.XPath("//td[contains(@id, '.TxnDate_B-1')]");
        private static readonly By TxnDateInput = By.XPath("//input[contains(@id, '.TxnDate_I')]");

        // ── Customer ────────────────────────────────────────────────────────
        private static readonly By CustomerDropdown = By.XPath("//td[contains(@id, '.CustomerIdLookup_B-1')]");
        private static readonly By CustomerInput = By.XPath("//input[contains(@id, '.CustomerIdLookup_I')]");

        // ── Currency ────────────────────────────────────────────────────────
        private static readonly By CurrencyDropdown = By.XPath("//td[contains(@id, '.CurrencyIdLookup_B-1')]");
        private static readonly By CurrencyInput = By.XPath("//input[contains(@id, '.CurrencyIdLookup_I')]");

        // ── Price List ──────────────────────────────────────────────────────
        private static readonly By PriceListDropdown = By.XPath("//td[contains(@id, '.PriceListIdLookup_B-1')]");
        private static readonly By PriceListInput = By.XPath("//input[contains(@id, '.PriceListIdLookup_I')]");

        // ── Warehouse ───────────────────────────────────────────────────────
        private static readonly By WarehouseDropdown = By.XPath("//td[contains(@id, '.WarehouseIdLookup_B-1')]");
        private static readonly By WarehouseInput = By.XPath("//input[contains(@id, '.WarehouseIdLookup_I')]");

        // ── Salesman ────────────────────────────────────────────────────────
        private static readonly By SalesmanDropdown = By.XPath("//td[contains(@id, '.SalesmanIdLookup_B-1')]");
        private static readonly By SalesmanInput = By.XPath("//input[contains(@id, '.SalesmanIdLookup_I')]");

        // ── Payment Method ────────────────────────────────────────────────────────
        private static readonly By PaymentMethodDropdown = By.XPath("//td[contains(@id, '.PaymentMethodIdLookup_B-1')]");
        private static readonly By PaymentMethodInput = By.XPath("//input[contains(@id, '.PaymentMethodIdLookup_I')]");

        // ── Payment Term ────────────────────────────────────────────────────────
        private static readonly By PaymentTermDropdown = By.XPath("//td[contains(@id, '.PaymentTermIdLookup_B-1')]");
        private static readonly By PaymentTermInput = By.XPath("//input[contains(@id, '.PaymentTermIdLookup_I')]");

        // ── Reference No ────────────────────────────────────────────────────
        private static readonly By ReferenceNumInput = By.XPath("//input[contains(@id, '.ReferenceNum_I')]");
        private static readonly By CustomerPONumInput = By.XPath("//input[contains(@id, '.CustomerPONum_I')]");
        private static readonly By DisplayNameInput = By.XPath("//input[contains(@id, '.DisplayCustomerName_I')]");
        private static readonly By MobileNumInput = By.XPath("//input[contains(@id, '.MobileNum_I')]");

        // ── Constructor ─────────────────────────────────────────────────────
        public HeaderHandler(IWebDriver driver, WaitHelper wait, ReportHelper report)
            : base(driver, wait, report) { }

        // ── Public Entry Point ──────────────────────────────────────────────
        public void Fill(InvoiceHeaderDM header)
        {
            FillInvoiceDate(header.InvoiceDate);
            FillCustomer(header.Customer);
            FillCurrency(header.Currency);
            FillPriceList(header.PriceList);
            FillWarehouse(header.Warehouse);
            FillSalesman(header.Salesman);
            FillReferenceNum(header.ReferenceNum);
            FillCustomerPONum(header.CustomerPONum);
            FillDisplayName(header.DisplayName);
            FillMobileNum(header.MobileNum);
            FillPaymentMethod(header.PaymentMethod);
            FillPaymentTerm(header.PaymentTerm);
            WaitForLoader();
        }

        // ── Generic Lookup Handler (🔥 Core Optimization) ───────────────────
        private void SelectFromLookup(By dropdown, By input, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            OpenDropdown(dropdown);

            Type(input, value);
            WaitForLoader();

            var nextPage = GetNextPageLocator(input);

            SelectOption(LookupText, nextPage, value);
        }

        private By GetNextPageLocator(By inputLocator)
        {
            var element = Wait.UntilVisible(inputLocator);
            string id = element.GetAttribute("id");

            // Example:
            // SalesInvoice.CustomerIdLookup_I

            // Step 1: Split by '.'
            var parts = id.Split('.');

            if (parts.Length < 2)
                throw new Exception($"Invalid id format: {id}");

            string modulePrefix = parts[0]; // SalesInvoice
            string fieldPart = parts[1];    // CustomerIdLookup_I

            // Step 2: Remove suffix "_I"
            fieldPart = fieldPart.Replace("_I", "");

            // Step 3: Remove "Lookup"
            string fieldName = fieldPart.Replace("Lookup", ""); // CustomerId

            // Final:
            // SalesInvoice.CustomerId_NextPage

            string nextPageId = $"{modulePrefix}.{fieldName}_NextPage";

            return By.Id(nextPageId);
        }

        // ── Field Methods ───────────────────────────────────────────────────

        private void FillInvoiceDate(string? date)
        {
            if (string.IsNullOrWhiteSpace(date)) return;

            OpenDropdown(TxnDateDropdown);
            ClearAndType(TxnDateInput, date);
        }

        private void FillCustomer(string? customer)
        {
            SelectFromLookup(CustomerDropdown, CustomerInput, customer);
        }

        private void FillReferenceNum(string? referenceNo)
        {
            if (string.IsNullOrWhiteSpace(referenceNo)) return;

            Type(ReferenceNumInput, referenceNo);
        }

        private void FillCustomerPONum(string? customerPONo)
        {
            if (string.IsNullOrWhiteSpace(customerPONo)) return;

            Type(CustomerPONumInput, customerPONo);
        }

        private void FillDisplayName(string? displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return;

            Type(DisplayNameInput, displayName);
        }

        private void FillMobileNum(string? mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber)) return;

            Type(MobileNumInput, mobileNumber);
        }

        private void FillCurrency(string? currency)
        {
            SelectFromLookup(CurrencyDropdown, CurrencyInput, currency);
        }

        private void FillPriceList(string? priceList)
        {
            SelectFromLookup(PriceListDropdown, PriceListInput, priceList);
        }

        private void FillWarehouse(string? warehouse)
        {
            SelectFromLookup(WarehouseDropdown, WarehouseInput, warehouse);
        }

        private void FillSalesman(string? salesPerson)
        {
            SelectFromLookup(SalesmanDropdown, SalesmanInput, salesPerson);
        }

        private void FillPaymentMethod(string? paymentMethod)
        {
            SelectFromLookup(PaymentMethodDropdown, PaymentMethodInput, paymentMethod);
        }

        private void FillPaymentTerm(string? paymentTerm)
        {
            SelectFromLookup(PaymentTermDropdown, PaymentTermInput, paymentTerm);
        }
    }
}