using OpenQA.Selenium;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers;

/// <summary>
/// Handles all UI interactions for the Sales Invoice PAYMENTS section.
///
/// Payments section allows linking advance payments or receipts
/// against this invoice to reduce the balance due.
///
/// ASP.NET Razor pattern:
///   Each payment entry is a separate form row.
///   Payments[0].PaymentMode, Payments[0].Amount, Payments[0].ReferenceNo etc.
///
/// HOW TO UPDATE:
///   Inspect the Payments section in your ERP HTML.
///   Update GetPaymentLocator() with the actual id/name pattern.
///   Update NavigateToPaymentsSection() if it is inside a tab.
/// </summary>
public class PaymentsHandler : BaseHandler
{
    // ── Section-level locators ─────────────────────────────────────────────

    private static readonly By AddPaymentButton = By.XPath(
        "//button[normalize-space()='Add Payment'] | " +
        "//button[normalize-space()='Add']         | " +
        "//a[normalize-space()='Add Payment']      | " +
        "//button[contains(@id,'addPayment')]"
    );

    // ── Constructor ────────────────────────────────────────────────────────
    public PaymentsHandler(IWebDriver driver, WaitHelper wait)
        : base(driver, wait) { }

    // ── Public entry point ─────────────────────────────────────────────────

    /// <summary>
    /// Fill all payment entries from the data model.
    /// Skips gracefully if Payments section is empty.
    /// </summary>
    public void Fill(SalesInvoicePaymentsDM payments)
    {
        if (payments?.Entries == null || payments.Entries.Count == 0) return;

        NavigateToPaymentsSection();

        for (int i = 0; i < payments.Entries.Count; i++)
        {
            AddNewPayment(i);
            FillPayment(i, payments.Entries[i]);
            WaitForLoader();
        }
    }

    // ── Private methods ────────────────────────────────────────────────────

    /// <summary>
    /// Navigate to the Payments tab if the form uses tabbed sections.
    /// TODO: Remove body if Payments section is always visible.
    /// </summary>
    private void NavigateToPaymentsSection()
    {
        By paymentsTab = By.XPath(
            "//a[normalize-space()='Payments']   | " +
            "//li[normalize-space()='Payments']  | " +
            "//button[normalize-space()='Payments']"
        );

        try
        {
            Wait.UntilClickable(paymentsTab, timeoutSeconds: 3).Click();
            WaitForLoader();
        }
        catch
        {
            // Payments section already visible
        }
    }

    /// <summary>Click Add Payment to create a new payment row.</summary>
    private void AddNewPayment(int paymentIndex)
    {
        if (paymentIndex == 0)
        {
            bool firstRowExists = IsVisible(GetPaymentLocator(0, "Amount"));
            if (firstRowExists) return;
        }

        Click(AddPaymentButton);

        Wait.Until(driver =>
            driver.FindElements(GetPaymentLocator(paymentIndex, "Amount")).Count > 0,
            timeoutSeconds: 8
        );
    }

    /// <summary>Fill all fields for a single payment entry row.</summary>
    private void FillPayment(int index, PaymentEntryDM payment)
    {
        FillPaymentMode(index, payment.PaymentMode);
        FillPaymentAmount(index, payment.AmountFC);
        FillPaymentReference(index, payment.ReferenceNo);
        FillPaymentDate(index, payment.PaymentDate);
        FillPaymentAccount(index, payment.Account);
    }

    /// <summary>
    /// Select payment mode from dropdown.
    /// Examples: "Cash", "Bank Transfer", "Cheque"
    /// </summary>
    private void FillPaymentMode(int index, string? paymentMode)
    {
        if (string.IsNullOrWhiteSpace(paymentMode)) return;

        By locator = GetPaymentLocator(index, "PaymentModeId");

        try
        {
            SelectByText(locator, paymentMode);
            WaitForLoader(); // Mode change may show/hide Account field
        }
        catch
        {
            Type(GetPaymentLocator(index, "PaymentMode"), paymentMode);
        }
    }

    /// <summary>Enter the payment amount.</summary>
    private void FillPaymentAmount(int index, decimal amount)
    {
        if (amount <= 0) return;

        By locator = GetPaymentLocator(index, "Amount");
        IWebElement el = Wait.UntilVisible(locator);
        ScrollIntoView(el);

        el.SendKeys(Keys.Control + "a");
        el.SendKeys(amount.ToString("F2"));
        el.SendKeys(Keys.Tab);
    }

    /// <summary>Enter the payment reference / transaction ID.</summary>
    private void FillPaymentReference(int index, string? referenceNo)
    {
        if (string.IsNullOrWhiteSpace(referenceNo)) return;
        Type(GetPaymentLocator(index, "ReferenceNo"), referenceNo);
    }

    /// <summary>Set the payment date.</summary>
    private void FillPaymentDate(int index, string? date)
    {
        if (string.IsNullOrWhiteSpace(date)) return;

        By locator = GetPaymentLocator(index, "PaymentDate");
        IWebElement el = Wait.UntilVisible(locator);
        el.Clear();
        el.SendKeys(date);
        el.SendKeys(Keys.Tab);
    }

    /// <summary>
    /// Select the bank/cash account for this payment.
    /// May be autocomplete or <select> depending on your ERP.
    /// TODO: Update to autocomplete pattern if needed.
    /// </summary>
    private void FillPaymentAccount(int index, string? account)
    {
        if (string.IsNullOrWhiteSpace(account)) return;

        By locator = GetPaymentLocator(index, "AccountId");

        try
        {
            SelectByText(locator, account);
        }
        catch
        {
            // Try autocomplete pattern as fallback
            By inputLocator = GetPaymentLocator(index, "Account_input");
            By dropdownItems = By.CssSelector(".account-dropdown li");

            IWebElement input = Wait.UntilVisible(inputLocator);
            input.Clear();
            input.SendKeys(account);
            Wait.UntilVisible(dropdownItems, timeoutSeconds: 5);
            Click(Driver.FindElements(dropdownItems).First());
        }
    }

    // ── Locator builder ────────────────────────────────────────────────────

    /// <summary>
    /// Builds the locator for a field in a specific payment row.
    /// TODO: Verify this pattern against your ERP's HTML.
    /// </summary>
    private static By GetPaymentLocator(int index, string fieldName)
    {
        return By.Id($"Payments_{index}__{fieldName}");
    }
}