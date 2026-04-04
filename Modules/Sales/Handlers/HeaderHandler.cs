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
public class HeaderHandler : BaseHandler
{
    // ── Locators — UPDATE THESE to match your ERP's actual HTML ───────────

    // Customer autocomplete    
    private static readonly By CustomerDropdown = By.XPath("//td[contains(@id, '.CustomerIdLookup_B-1')]");
    private static readonly By CustomerInput = By.XPath("//input[contains(@id, '.CustomerIdLookup_I')]");
    private static readonly By LookupText = By.XPath("//div[@class='lookup-text']");
    private static readonly By NextPage = By.XPath("//div[contains(@id, 'NextPage')]");
    // Dates
    private static readonly By TxnDateDropdown = By.XPath("//td[contains(@id, '.TxnDate_B-1')]");
    private static readonly By TxnDateInput = By.XPath("//input[contains(@id, '.TxnDate_I')]");

    // Financial dropdowns
    private static readonly By CurrencyDropdown = By.Id("Header_CurrencyId");
    private static readonly By PriceListDropdown = By.Id("Header_PriceListId");
    private static readonly By PriceListInput = By.Id("Header_PriceListId");
    private static readonly By PaymentTermsDropdown = By.Id("Header_PaymentTermsId");

    // Location — TODO: verify if autocomplete or <select> in your ERP
    private static readonly By LocationInput = By.XPath("//input[contains(@id, '.WarehouseIdLookup_I')]");
    private static readonly By LocationDropdown = By.XPath("//td[contains(@id, '.WarehouseIdLookup_B-1')]");

    // Sales Person — TODO: verify if autocomplete or <select> in your ERP
    private static readonly By SalesPersonInput = By.Id("Header_SalesPersonId_input");
    private static readonly By SalesPersonDropdown = By.CssSelector(".salesperson-dropdown li");

    // Reference No — plain text input
    private static readonly By ReferenceNoInput = By.Id("Header_ReferenceNo");

    // ── Constructor ────────────────────────────────────────────────────────
    public HeaderHandler(IWebDriver driver, WaitHelper wait)
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
        //FillReferenceNo(header.ReferenceNo);
        //FillCurrency(header.Currency);
        //FillPriceList(header.PriceList);
        FillLocation(header.Location);
        //FillSalesPerson(header.SalesPerson);
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
        Type(ReferenceNoInput, referenceNo);
    }    

    /// <summary>Select currency from native ASP.NET <select> dropdown.</summary>
    private void FillCurrency(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency)) return;
        SelectByText(CurrencyDropdown, currency);
        WaitForLoader(); // Currency change may reload price list
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
    private void FillLocation(string? location)
    {
        if (string.IsNullOrWhiteSpace(location)) return;

        OpenDropdown(LocationDropdown);
        ClearAndType(LocationInput, location);
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
    private void FillSalesPerson(string? salesPerson)
    {
        if (string.IsNullOrWhiteSpace(salesPerson)) return;

        OpenDropdown(SalesPersonDropdown);
        ClearAndType(SalesPersonInput, salesPerson);
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