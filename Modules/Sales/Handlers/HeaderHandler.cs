using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;
using OpenQA.Selenium;
using System.Xml.Linq;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers;

/// <summary>
/// Handles all UI interactions for the Sales Invoice HEADER section.
///
/// ASP.NET Razor specifics applied:
///   - Input IDs follow ASP.NET naming: Header_Customer, Header_InvoiceDate etc.
///   - Customer field is an autocomplete — type, wait for dropdown, select first match
///   - Date fields use the HTML date input or masked text input
///   - Dropdowns (Currency, PaymentTerms, PriceList) are native <select> elements
///   - Location and SalesPerson may be autocomplete or <select> — marked with TODO
///
/// HOW TO UPDATE LOCATORS:
///   Open Chrome DevTools (F12) on your Sales Invoice form.
///   Inspect each field and replace the By.Id / By.Name values below
///   with the actual id="" or name="" attribute from your ERP's HTML.
/// </summary>
public class HeaderHandlerOld : BaseHandler
{
    // ── Locators — UPDATE THESE to match your ERP's actual HTML ───────────

    private static readonly By LookupText = By.XPath("//div[@class='lookup-text']");
    private static readonly By NextPage = By.XPath("//div[contains(@id, 'NextPage')]");

    // Dates
    private static readonly By TxnDateDropdown = By.XPath("//td[contains(@id, '.TxnDate_B-1')]");
    private static readonly By TxnDateInput = By.XPath("//input[contains(@id, '.TxnDate_I')]");

    // Customer autocomplete    
    private static readonly By CustomerDropdown = By.XPath("//td[contains(@id, '.CustomerIdLookup_B-1')]");
    private static readonly By CustomerInput = By.XPath("//input[contains(@id, '.CustomerIdLookup_I')]");

    // Financial dropdowns
    private static readonly By CurrencyDropdown = By.XPath("//td[contains(@id, '.CurrencyIdLookup_B-1')]");
    private static readonly By CurrencyInput = By.XPath("//input[contains(@id, '.CurrencyIdLookup_I')]");

    private static readonly By PriceListDropdown = By.XPath("//td[contains(@id, '.PriceListIdLookup_B-1')]");
    private static readonly By PriceListInput = By.XPath("//input[contains(@id, '.PriceListIdLookup_I')]");

    // Location — TODO: verify if autocomplete or <select> in your ERP    
    private static readonly By WarehouseDropdown = By.XPath("//td[contains(@id, '.WarehouseIdLookup_B-1')]");
    private static readonly By WarehouseInput = By.XPath("//input[contains(@id, '.WarehouseIdLookup_I')]");

    // Sales Person — TODO: verify if autocomplete or <select> in your ERP    
    private static readonly By SalesmanDropdown = By.XPath("//td[contains(@id, '.SalesmanIdLookup_B-1')]");
    private static readonly By SalesmanInput = By.XPath("//input[contains(@id, '.SalesmanIdLookup_I')]");

    // Reference No — plain text input
    private static readonly By ReferenceNoInput = By.XPath("//input[contains(@id, '.ReferenceNum_I')]");

    // ── Constructor ────────────────────────────────────────────────────────
    public HeaderHandlerOld(IWebDriver driver, WaitHelper wait)
        : base(driver, wait) { }

    // ── Public entry point ─────────────────────────────────────────────────

    /// <summary>
    /// Fill all header fields from the data model.
    /// Skips any field whose value is null or empty.
    /// </summary>
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

    // ── Individual field methods ───────────────────────────────────────────

    /// <summary>Set Invoice Date. Handles both HTML date input and masked text input.</summary>
    private void FillInvoiceDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date)) return;

        OpenDropdown(TxnDateDropdown);
        ClearAndType(TxnDateInput, date);

        // Try standard input first; fallback to JS date set for datepicker widgets
        //RetryAction(() =>
        //{
        //    IWebElement el = Wait.UntilVisible(TxnDateInput);
        //    el.Clear();
        //    el.SendKeys(date);
        //    el.SendKeys(Keys.Tab); // Trigger change event
        //});
    }

    /// <summary>
    /// Type customer name into the autocomplete field and select first match.
    /// Pattern: Type partial name → wait for dropdown → click first result.
    /// </summary>  
    private void FillCustomer(string? customer)
    {
        if (string.IsNullOrWhiteSpace(customer)) return;

        OpenDropdown(CustomerDropdown);
        ClearAndType(CustomerInput, customer);
        SelectOption(LookupText, NextPage, customer);

        //IWebElement customerDropdown = Wait.UntilVisible(CustomerDropdown);
        //Click(customerDropdown);

        // Clear and type into the autocomplete input
        //IWebElement input = Wait.UntilVisible(CustomerInput);
        //input.SendKeys(Keys.Control + "a");
        //input.SendKeys(Keys.Delete);
        //input.SendKeys(customer);
        //Thread.Sleep(500);
    }

    /// <summary>Fill Reference No — plain text input.</summary>
    private void FillReferenceNo(string? referenceNo)
    {
        if (string.IsNullOrWhiteSpace(referenceNo)) return;
        //Type(ReferenceNoInput, referenceNo);
        ClearAndType(ReferenceNoInput, referenceNo);
    }

    /// <summary>Select currency from native ASP.NET <select> dropdown.</summary>
    private void FillCurrency(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency)) return;

        OpenDropdown(CurrencyDropdown);
        ClearAndType(CurrencyInput, currency);
        SelectOption(LookupText, NextPage, currency);
    }

    /// <summary>Select price list from native <select> dropdown.</summary>
    private void FillPriceList(string? priceList)
    {
        if (string.IsNullOrWhiteSpace(priceList)) return;

        OpenDropdown(PriceListDropdown);
        ClearAndType(PriceListInput, priceList);
        SelectOption(LookupText, NextPage, priceList);

        //SelectByText(PriceListDropdown, priceList);
    }

    /// <summary>
    /// Fill Location — autocomplete pattern same as Customer.
    /// TODO: If Location is a plain <select>, replace with SelectByText().
    /// </summary>
    private void FillWarehouse(string? location)
    {
        if (string.IsNullOrWhiteSpace(location)) return;

        OpenDropdown(WarehouseDropdown);
        ClearAndType(WarehouseInput, location);
        SelectOption(LookupText, NextPage, location);

        //var locationDropdown = Driver.FindElement(LocationDropdown);
        //Click(locationDropdown);

        //IWebElement input = Wait.UntilVisible(LocationInput);
        //input.SendKeys(Keys.Control + "a");
        //input.SendKeys(Keys.Delete);
        //input.SendKeys(location);
        //Thread.Sleep(800);

        //SelectOption(LookupText, NextPage, location);
    }

    /// <summary>
    /// Fill Sales Person — autocomplete pattern.
    /// TODO: If SalesPerson is a plain <select>, replace with SelectByText().
    /// </summary>
    private void FillSalesman(string? salesPerson)
    {
        if (string.IsNullOrWhiteSpace(salesPerson)) return;

        OpenDropdown(SalesmanDropdown);
        ClearAndType(SalesmanInput, salesPerson);
        SelectOption(LookupText, NextPage, salesPerson);

        //IWebElement input = Wait.UntilVisible(SalesPersonInput);
        //ScrollIntoView(input);
        //input.Clear();
        //input.SendKeys(salesPerson);

        //Wait.UntilVisible(SalesPersonDropdown, timeoutSeconds: 8);
        //var options = Driver.FindElements(SalesPersonDropdown);
        //var match = options.FirstOrDefault(o =>
        //    o.Text.Trim().Contains(salesPerson, StringComparison.OrdinalIgnoreCase));

        //Click(match ?? options.First());
    }
}