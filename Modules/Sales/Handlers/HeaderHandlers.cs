using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;
using OpenQA.Selenium;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers
{
    /// <summary>
    /// Handles all UI interactions for the Sales Invoice HEADER section.
    /// Optimized with reusable lookup handling.
    /// </summary>
    public class HeaderHandlers : BaseHandler
    {
        // ── Common Lookup Locators ──────────────────────────────────────────
        private static readonly By LookupText = By.XPath("//div[@class='lookup-text']");
        private static readonly By NextPage = By.XPath("//div[contains(@id, 'NextPage')]");

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

        // ── Reference No ────────────────────────────────────────────────────
        private static readonly By ReferenceNoInput = By.XPath("//input[contains(@id, '.ReferenceNum_I')]");

        // ── Constructor ─────────────────────────────────────────────────────
        public HeaderHandlers(IWebDriver driver, WaitHelper wait)
            : base(driver, wait) { }

        // ── Public Entry Point ──────────────────────────────────────────────
        public void Fill(SalesInvoiceHeaderDM header)
        {
            FillInvoiceDate(header.InvoiceDate);
            FillCustomer(header.Customer);
            FillReferenceNo(header.ReferenceNo);
            FillCurrency(header.Currency);
            //FillPriceList(header.PriceList);
            FillWarehouse(header.Warehouse);
            FillSalesman(header.Salesman);

            WaitForLoader();
        }

        // ── Generic Lookup Handler (🔥 Core Optimization) ───────────────────
        private void SelectFromLookup(By dropdown, By input, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
                
            OpenDropdown(dropdown);
            //ClearAndType(input, value);
            Type(input, value);
            SelectOption(LookupText, NextPage, value);
        }

        // ── Field Methods ───────────────────────────────────────────────────

        private void FillInvoiceDate(string? date)
        {
            if (string.IsNullOrWhiteSpace(date)) return;

            OpenDropdown(TxnDateDropdown);
            //Type(TxnDateInput, date);
            ClearAndType(TxnDateInput, date);
        }

        private void FillCustomer(string? customer)
        {
            SelectFromLookup(CustomerDropdown, CustomerInput, customer);
        }

        private void FillReferenceNo(string? referenceNo)
        {
            if (string.IsNullOrWhiteSpace(referenceNo)) return;

            //ClearAndType(ReferenceNoInput, referenceNo);
            Type(ReferenceNoInput, referenceNo);
        }

        private void FillCurrency(string? currency)
        {
            SelectFromLookup(CurrencyDropdown, CurrencyInput, currency);
        }

        private void FillPriceList(string? priceList)
        {
            SelectFromLookup(PriceListDropdown, PriceListInput, priceList);
        }

        private void FillWarehouse(string? location)
        {
             SelectFromLookup(WarehouseDropdown, WarehouseInput, location);
        }

        private void FillSalesman(string? salesPerson)
        {
            SelectFromLookup(SalesmanDropdown, SalesmanInput, salesPerson);
        }
    }
}