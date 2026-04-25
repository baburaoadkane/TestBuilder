using OpenQA.Selenium;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers;

/// <summary>
/// Reads actual values from the Sales Invoice UI for validation purposes.
///
/// This handler does NOT write anything — it only READS.
/// It exposes the actual displayed values so Validators can
/// compare them against the expected values from the JSON.
///
/// Used by:
///   TotalsValidator  → ReadTotals()
///   HeaderValidator  → ReadHeaderValues()
///   MessageValidator → ReadToastMessage()
///
/// HOW TO UPDATE:
///   Inspect the totals/summary section in your ERP.
///   Replace all By locators with the actual IDs/classes
///   from your ERP's rendered HTML.
/// </summary>
public class ExpectationHandler : BaseHandler
{
    // ── Document status ────────────────────────────────────────────────────

    /// <summary>Status badge/label showing Draft, Submitted, Approved etc.</summary>
    private static readonly By DocumentStatus = By.CssSelector(".TxnWorkflowStatus");

    // ── Totals section ─────────────────────────────────────────────────────

    //private static readonly By SubTotalAmount = By.Id("Summary_SubTotal");
    private static readonly By SubTotalAmount = By.XPath("//input[contains(@id, '.TxnTotalGrossValue_I')]");
    private static readonly By TotalDiscountAmt = By.XPath("//input[contains(@id, '.DiscountValue_I')]");
    private static readonly By GrandTotalAmount = By.XPath("//input[contains(@id, '.TotalNetValueLC_I')]");
    //private static readonly By AmountPaidAmount = By.XPath("//input[contains(@id, '.AmountPaid_I')]");
    //private static readonly By BalanceDueAmount = By.XPath("//input[contains(@id, '.BalanceDue_I')]");

    // ── Toast / notification ───────────────────────────────────────────────

    private static readonly By SuccessToast = By.CssSelector(
        ".toast-success, .alert-success, [class*='success-message'], .notification-success"
    );

    private static readonly By ErrorToast = By.CssSelector(
        ".toast-error, .alert-danger, [class*='error-message'], .validation-summary-errors"
    );

    private static readonly By ValidationMessage = By.Id("ValidationSummary");
    // ── Document number ────────────────────────────────────────────────────

    private static readonly By DocumentNoField = By.CssSelector(
        "#documentNo, [data-field='DocumentNo'], .document-number, #Header_DocumentNo"
    );

    // ── Constructor ────────────────────────────────────────────────────────
    public ExpectationHandler(IWebDriver driver, WaitHelper wait)
        : base(driver, wait) { }

    // ── Read methods ───────────────────────────────────────────────────────

    /// <summary>Read the current document status from the UI.</summary>
    public string ReadDocumentStatus()
        => GetText(DocumentStatus);

    /// <summary>Read the generated document number after save.</summary>
    public string ReadDocumentNumber()
        => GetText(DocumentNoField);

    /// <summary>Read the success toast message text.</summary>
    public string ReadSuccessMessage()
    {
        try
        {
            Wait.UntilVisible(SuccessToast, timeoutSeconds: 5);
            return GetText(SuccessToast);
        }
        catch { return string.Empty; }
    }

    /// <summary>Read the error/validation message text.</summary>
    public string ReadErrorMessage()
    {
        try
        {
            Wait.UntilVisible(ErrorToast, timeoutSeconds: 5);
            return GetText(ErrorToast);
        }
        catch { return string.Empty; }
    }

    public string ReadValidationMessage()
    {
        try
        {
            Wait.UntilVisible(ValidationMessage, timeoutSeconds: 5);
            return GetText(ValidationMessage);
        }
        catch { return string.Empty; }
    }

    /// <summary>
    /// Read all financial totals from the invoice summary section.
    /// Returns a dictionary keyed by field name for easy validator access.
    /// </summary>
    public Dictionary<string, decimal> ReadTotals()
    {
        return new Dictionary<string, decimal>
        {
            ["SubTotal"] = ParseAmount(GetText(SubTotalAmount)),
            ["TotalDiscount"] = ParseAmount(GetText(TotalDiscountAmt)),
            ["GrandTotal"] = ParseAmount(GetText(GrandTotalAmount))
        };
    }

    /// <summary>
    /// Read a specific line's displayed total amount at the given row index.
    /// TODO: Update locator pattern to match your ERP's line total field.
    /// </summary>
    public decimal ReadLineTotal(int lineIndex)
    {
        By locator = By.XPath($"//tr[contains(@id, '_DXDataRow{lineIndex}')]//td[@class='grid-cell dx-wrap dxgv dx-ellipsis dx-ar'][3]");       
        string raw = GetText(locator);
        return ParseAmount(raw);
    }

    // ── Private helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Parse a displayed amount string to decimal.
    /// Handles commas, currency symbols, and empty strings.
    /// Examples: "1,234.56" → 1234.56 | "$2,360.00" → 2360.00 | "" → 0
    /// </summary>
    private static decimal ParseAmount(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 0m;

        string cleaned = raw
            .Replace(",", "")
            .Replace("$", "")
            .Replace("₹", "")
            .Replace("€", "")
            .Replace("£", "")
            .Trim();

        return decimal.TryParse(cleaned, out decimal result) ? result : 0m;
    }
}