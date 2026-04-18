using OpenQA.Selenium;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers;

/// <summary>
/// Handles all UI interactions for the Sales Invoice OTHERS / REMARKS section.
///
/// Contains free-text fields:
///   - Remarks (customer-facing, printed on invoice)
///   - Internal Notes (staff only)
///   - Terms and Conditions
///
/// ASP.NET Razor pattern:
///   Plain textarea or input fields.
///   IDs follow: Others_Remarks, Others_InternalNotes, Others_TermsAndConditions
///
/// HOW TO UPDATE:
///   Inspect the Others/Remarks section in your ERP.
///   Replace By.Id values with actual field IDs from your ERP's HTML.
/// </summary>
public class OthersHandler : BaseHandler
{
    // ── Locators — UPDATE to match your ERP's actual HTML ─────────────────

    private static readonly By RemarksTextarea = By.Id("Others_Remarks");
    private static readonly By InternalNotesTextarea = By.Id("Others_InternalNotes");
    private static readonly By TermsAndConditionsTextarea = By.Id("Others_TermsAndConditions");

    // ── Constructor ────────────────────────────────────────────────────────
    public OthersHandler(IWebDriver driver, WaitHelper wait)
        : base(driver, wait) { }

    // ── Public entry point ─────────────────────────────────────────────────

    /// <summary>
    /// Fill all Others/Remarks fields from the data model.
    /// Each field is skipped if null or empty.
    /// </summary>
    public void Fill(SalesInvoiceOthersDM others)
    {
        if (others == null) return;

        NavigateToOthersSection();

        FillRemarks(others.Remarks);
        FillInternalNotes(others.InternalNotes);
        FillTermsAndConditions(others.TermsAndConditions);
    }

    // ── Private methods ────────────────────────────────────────────────────

    /// <summary>
    /// Navigate to the Others/Remarks tab if the form is tabbed.
    /// TODO: Remove body if Others section is always visible.
    /// </summary>
    private void NavigateToOthersSection()
    {
        By othersTab = By.XPath(
            "//a[normalize-space()='Others']      | " +
            "//a[normalize-space()='Remarks']     | " +
            "//li[normalize-space()='Others']     | " +
            "//button[normalize-space()='Others']"
        );

        try
        {
            Wait.UntilClickable(othersTab, timeoutSeconds: 3).Click();
            WaitForLoader();
        }
        catch
        {
            // Section already visible — no tab click needed
        }
    }

    /// <summary>
    /// Fill the customer-facing Remarks field.
    /// This text typically appears printed on the invoice PDF.
    /// </summary>
    private void FillRemarks(string? remarks)
    {
        if (string.IsNullOrWhiteSpace(remarks)) return;        
    }

    /// <summary>
    /// Fill the Internal Notes field — visible to staff only, not printed.
    /// </summary>
    private void FillInternalNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return;

        IWebElement el = Wait.UntilVisible(InternalNotesTextarea);
        ScrollIntoView(el);
        el.Clear();
        el.SendKeys(notes);
    }

    /// <summary>
    /// Fill the Terms and Conditions textarea.
    /// May be pre-populated from a default template — clear before typing.
    /// </summary>
    private void FillTermsAndConditions(string? terms)
    {
        if (string.IsNullOrWhiteSpace(terms)) return;

        IWebElement el = Wait.UntilVisible(TermsAndConditionsTextarea);
        ScrollIntoView(el);
        el.Clear();
        el.SendKeys(terms);
    }
}