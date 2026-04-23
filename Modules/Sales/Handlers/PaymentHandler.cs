using OpenQA.Selenium;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers;

public class PaymentHandler : BaseHandler
{
    // ── Section-level locators ─────────────────────────────────────────────
    private static readonly By LookupText = By.XPath("//div[contains(@class,'lookup-text')]");
    private static readonly By AddPaymentButton = By.Id("SalesInvoicePaymentNewButton");
    private static readonly By NextButton = By.XPath("//a[contains(@class,'dxp-button')]//img[@alt='Next']");
    // ── Constructor ────────────────────────────────────────────────────────
    public PaymentHandler(IWebDriver driver, WaitHelper wait) : base(driver, wait) { }

    private class FieldConfig
    {
        public By? Dropdown { get; set; }
        public int? ColumnIndex { get; set; }
    }

    private static readonly Dictionary<string, FieldConfig> FieldMap = new()
    {
        ["PaymentMethod"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_PaymentMethodId_B-1')]"),
            ColumnIndex = 1
        },        
        ["Currency"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_CurrencyId_B-1')]"),
            ColumnIndex = 2
        },
        ["CardNum"] = new()
        {
            ColumnIndex = 3
        },
        ["AmountFC"] = new()
        {
            ColumnIndex = 4
        },
        ["Remarks"] = new()
        {
            ColumnIndex = 5
        }
    };

    // ── Public entry point ─────────────────────────────────────────────────

    /// <summary>
    /// Fill all charge rows from the data model.
    /// Skips gracefully if Charges section is empty.
    /// </summary>
    public void Fill(SalesInvoicePaymentsDM payments)
    {
        if (payments?.Entries == null || payments.Entries.Count == 0) return;

        NavigateToPaymentSection();

        foreach (var payment in payments.Entries)
        {
            AddNewPayment();
            FillPayment(payment);
            WaitForLoader();
        }
    }

    /// <summary>Fill all fields for a single charge row.</summary>
    private void FillPayment(PaymentEntryDM payment)
    {
        Lookup("Charge", payment.PaymentMode);
        LookupCell("Account", payment.Account);
        SetCell("AmountFC", payment.AmountFC);
        SetCell("Remarks", payment.Remarks);
    }

    // ── 🔥 Lookup inside Grid Cell ────────────────────────────────────────
    private void Lookup(string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var dropdown = GetDropdown(field);

        OpenDropdown(dropdown);
        WaitForLoader();

        SelectOption(LookupText, NextButton, value);
    }

    // ── 🔥 Lookup inside Grid Cell ────────────────────────────────────────
    private void LookupCell(string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var cell = GetCell(field);
        Click(cell);

        var dropdown = GetDropdown(field);

        OpenDropdown(dropdown);
        WaitForLoader();

        SelectOption(LookupText, NextButton, value);
    }

    private By GetDropdown(string field)
    {
        if (!FieldMap.ContainsKey(field) || FieldMap[field].Dropdown == null)
            throw new Exception($"Dropdown not defined for field: {field}");

        return FieldMap[field].Dropdown!;
    }

    private int GetColIndex(string field)
    {
        if (!FieldMap.ContainsKey(field) || FieldMap[field].ColumnIndex == null)
            throw new Exception($"Column index not defined for field: {field}");

        return FieldMap[field].ColumnIndex!.Value;
    }

    // ── 🔥 Cell Locator ───────────────────────────────────────────────────
    private By GetCell(string field)
    {
        int colIndex = GetColIndex(field);
        return By.XPath($"(//div[@class='dxgBCTC dx-ellipsis'])[{colIndex}]");
    }

    // ── Private methods ────────────────────────────────────────────────────

    /// <summary>
    /// Click the Charges tab if the form uses tabs for sections.
    /// TODO: If Charges is always visible (not tabbed), remove this method body.
    /// </summary>
    private void NavigateToPaymentSection()
    {
        By chargesTab = By.XPath("//td[contains(@id, 'PaymentMethods_HC')]");

        try
        {
            Wait.UntilClickable(chargesTab, timeoutSeconds: 3).Click();
            WaitForLoader();
        }
        catch
        {
            // Charges section already visible — no tab navigation needed
        }
    }

    /// <summary>Click Add Charge button to create a new charge row.</summary>
    private void AddNewPayment()
    {
        Wait.UntilClickable(AddPaymentButton).Click();
        //Click(AddChargeButton);
        WaitForLoader();
    }

    // ── Set Cell Value ────────────────────────────────────────────────────
    private void SetCell(string field, object? value)
    {
        if (value == null || !IsValidValue(value)) return;

        string finalValue = value switch
        {
            decimal d => d.ToString("G29"),
            double d => d.ToString("G29"),
            _ => value.ToString()
        };

        var cell = GetCell(field);
        SetClipboardValue(cell, finalValue);
    }

    // ── Validation ────────────────────────────────────────────────────────
    private bool IsValidValue(object value)
    {
        return value switch
        {
            string s => !string.IsNullOrWhiteSpace(s),
            decimal d => d > 0,
            int i => i > 0,
            double d => d > 0,
            _ => true
        };
    }
}