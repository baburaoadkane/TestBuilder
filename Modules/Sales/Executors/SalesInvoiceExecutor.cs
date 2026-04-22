using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Enums;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;
using Enfinity.ERP.Automation.Modules.Sales.Handlers;
using Enfinity.ERP.Automation.Modules.Sales.Validators;
using OpenQA.Selenium;

namespace Enfinity.ERP.Automation.Modules.Sales.Executors;

/// <summary>
/// Orchestrates the complete end-to-end flow for a Sales Invoice.
///
/// Execution order:
///   1. Navigate  → Go to New Sales Invoice page
///   2. Fill      → Header → Lines → Charges → Payments → Others
///   3. Save      → Click Save, wait for success
///   4. Submit    → (if RequiresApproval) Click Submit, wait for confirmation
///   5. Approve   → (if RequiresApproval) Click Approve, wait for confirmation
///   6. Validate  → Assert status, totals, messages against Expected values
///
/// Scenario branching:
///   ScenarioType = Create    → Fill + Save + Validate
///   ScenarioType = Approval  → Fill + Save + Submit + Approve + Validate
///   ScenarioType = Negative  → Fill + Save + Assert error message
///   ScenarioType = Edit      → Navigate to existing doc + Fill + Save + Validate
///   ScenarioType = Validation→ Navigate to existing doc + Validate only
/// </summary>
public class SalesInvoiceExecutor : BaseExecutor<SalesInvoiceDM>
{
    // ── Handlers ───────────────────────────────────────────────────────────
    //private readonly HeaderHandler _headerHandler;
    private readonly HeaderHandlers _headerHandler;
    private readonly LineHandler _linesHandler;
    private readonly ChargesHandler _chargesHandler;
    private readonly PaymentsHandler _paymentsHandler;
    private readonly OthersHandler _othersHandler;
    private readonly ExpectationHandler _expectationHandler;

    // ── Validators ─────────────────────────────────────────────────────────
    private readonly HeaderValidator _headerValidator;
    private readonly LinesValidator _linesValidator;
    private readonly TotalsValidator _totalsValidator;
    private readonly MessageValidator _messageValidator;

    // ── Navigation route ───────────────────────────────────────────────────
    // TODO: Update this to match your ERP's actual Sales Invoice URL route
    private const string NewInvoiceRoute = "sales/invoice/new";
    private const string EditInvoiceRoute = "sales/invoice/edit/{0}"; // {0} = DocumentNo

    // ── Constructor ────────────────────────────────────────────────────────
    public SalesInvoiceExecutor(IWebDriver driver, WaitHelper wait, ReportHelper report)
        : base(driver, wait, report)
    {
        // Initialize all handlers
        //_headerHandler = new HeaderHandler(driver, wait);
        _headerHandler = new HeaderHandlers(driver, wait);
        _linesHandler = new LineHandler(driver, wait);
        _chargesHandler = new ChargesHandler(driver, wait);
        _paymentsHandler = new PaymentsHandler(driver, wait);
        _othersHandler = new OthersHandler(driver, wait);
        _expectationHandler = new ExpectationHandler(driver, wait);

        // Initialize all validators
        _headerValidator = new HeaderValidator(driver, wait, report, _expectationHandler);
        _linesValidator = new LinesValidator(driver, wait, report, _expectationHandler);
        _totalsValidator = new TotalsValidator(driver, wait, report, _expectationHandler);
        _messageValidator = new MessageValidator(driver, wait, report, _expectationHandler);
    }

    // ── Main entry point ───────────────────────────────────────────────────

    /// <summary>
    /// Execute the full Sales Invoice flow based on the ScenarioType in data.
    /// Called directly by test cases — one method handles all scenario branches.
    /// </summary>
    public override void Execute(SalesInvoiceDM data)
    {
        Report.Info($"── Sales Invoice Executor: {data.ScenarioType} ──");
        Report.Info($"Test: {data.TestDescription}");

        string scenarioType = data.ScenarioType?.ToUpperInvariant() ?? "CREATE";

        switch (scenarioType)
        {
            case "CREATE":
                NavigateToModule("Sales");
                NavigateToListing("Invoice");
                ExecuteCreate(data);
                break;

            case "APPROVAL":
                ExecuteApproval(data);
                break;

            case "NEGATIVE":
                ExecuteNegative(data);
                break;

            case "EDIT":
                ExecuteEdit(data);
                break;

            case "VALIDATION":
                ExecuteValidation(data);
                break;

            default:
                throw new ArgumentException(
                    $"[SalesInvoiceExecutor] Unknown ScenarioType: '{data.ScenarioType}'. " +
                    $"Valid values: Create, Approval, Negative, Edit, Validation.");
        }
    }

    // ── Scenario implementations ───────────────────────────────────────────

    /// <summary>
    /// CREATE scenario: Navigate → Fill all sections → Save → Validate totals + status.
    /// </summary>
    private void ExecuteCreate(SalesInvoiceDM data)
    {
        Report.Info("Step 1: Navigate to New Sales Invoice");        
        OpenFormMode("New");

        Report.Info("Step 2: Fill Header");
        _headerHandler.Fill(data.Header);
        ClickOnForm("Save");

        Report.Info("Step 3: Fill Lines");
        _linesHandler.Fill(data.Lines);

        Report.Info("Step 4: Fill Charges");
        _chargesHandler.Fill(data.Charges);

        Report.Info("Step 5: Fill Payments");
        _paymentsHandler.Fill(data.Payments);

        Report.Info("Step 6: Fill Others / Remarks");
        _othersHandler.Fill(data.Others);

        Report.Info("Step 7: Save document");
        //ClickOnForm("Save");
        ClickOnForm("View");

        Report.Info("Step 8: Validate");
        ValidateAfterSave(data);
    }

    /// <summary>
    /// APPROVAL scenario: Full create flow + Submit + Approve.
    /// Runs the complete 3-step Save → Submit → Approve workflow.
    /// </summary>
    private void ExecuteApproval(SalesInvoiceDM data)
    {
        // Step 1–7: Same as Create
        ExecuteCreate(data);

        // Step 8: Submit for approval
        Report.Info("Step 8: Submit document for approval");
        Submit();
        _messageValidator.ValidateSuccessMessage(data.Expected);

        // Step 9: Approve
        Report.Info("Step 9: Approve document");
        Approve();
        _messageValidator.ValidateSuccessMessage(data.Expected);

        // Step 10: Validate final approved state
        Report.Info("Step 10: Validate approved state");
        _headerValidator.ValidateStatus(data.Expected);
        _totalsValidator.ValidateTotals(data.Expected);
    }

    /// <summary>
    /// NEGATIVE scenario: Fill form → attempt Save → assert error message appears.
    /// Does NOT assert totals — only validates the error.
    /// </summary>
    private void ExecuteNegative(SalesInvoiceDM data)
    {
        Report.Info("Step 1: Navigate to New Sales Invoice");
        Navigate(NewInvoiceRoute);

        Report.Info("Step 2: Fill form with invalid/incomplete data");
        _headerHandler.Fill(data.Header);

        if (data.Lines?.Count > 0)
            _linesHandler.Fill(data.Lines);

        Report.Info("Step 3: Attempt to Save (expecting validation error)");
        TryClickSave(); // Clicks save but does NOT wait for success toast

        Report.Info("Step 4: Validate error message");
        _messageValidator.ValidateErrorMessage(data.Expected);
    }

    /// <summary>
    /// EDIT scenario: Navigate to existing document → update fields → Save → Validate.
    /// Requires DocumentNo to be set in the data model.
    /// </summary>
    private void ExecuteEdit(SalesInvoiceDM data)
    {
        if (string.IsNullOrWhiteSpace(data.DocumentNo))
            throw new InvalidOperationException(
                "[SalesInvoiceExecutor] Edit scenario requires DocumentNo in the JSON file.");

        Report.Info($"Step 1: Navigate to existing invoice: {data.DocumentNo}");
        Navigate(string.Format(EditInvoiceRoute, data.DocumentNo));

        Report.Info("Step 2: Update Header fields");
        _headerHandler.Fill(data.Header);

        Report.Info("Step 3: Update Lines");
        _linesHandler.Fill(data.Lines);

        Report.Info("Step 4: Update Charges");
        _chargesHandler.Fill(data.Charges);

        Report.Info("Step 5: Update Payments");
        _paymentsHandler.Fill(data.Payments);

        Report.Info("Step 6: Update Others");
        _othersHandler.Fill(data.Others);

        Report.Info("Step 7: Save updated document");
        ClickOnForm("Save");

        Report.Info("Step 8: Validate updated values");
        ValidateAfterSave(data);
    }

    /// <summary>
    /// VALIDATION scenario: Navigate to existing document → validate only (no form filling).
    /// Used to assert the state of a document created by a previous test or manually.
    /// </summary>
    private void ExecuteValidation(SalesInvoiceDM data)
    {
        if (string.IsNullOrWhiteSpace(data.DocumentNo))
            throw new InvalidOperationException(
                "[SalesInvoiceExecutor] Validation scenario requires DocumentNo in the JSON file.");

        Report.Info($"Step 1: Navigate to existing invoice: {data.DocumentNo}");
        Navigate(string.Format(EditInvoiceRoute, data.DocumentNo));

        Report.Info("Step 2: Validate all sections");
        ValidateAfterSave(data);
    }

    // ── Shared validation ──────────────────────────────────────────────────

    /// <summary>
    /// Run all validators after a successful Save.
    /// Called by Create, Edit, and Validation scenarios.
    /// </summary>
    private void ValidateAfterSave(SalesInvoiceDM data)
    {
        if (data.Expected == null)
        {
            Report.Warning("No Expected values defined in JSON — skipping validation.");
            return;
        }

        _messageValidator.ValidateSuccessMessage(data.Expected);
        _headerValidator.ValidateStatus(data.Expected);
        _totalsValidator.ValidateTotals(data.Expected);
        _linesValidator.ValidateLineTotals(data.Lines);
    }

    // ── Override base Save for SalesInvoice-specific locator ──────────────

    /// <summary>
    /// Click Save and wait for the success confirmation.
    /// TODO: Update the save button locator to match your ERP.
    /// </summary>
    //protected override void Save()
    //{
    //    By saveButton = By.XPath("//span[contains(@class, 'dx-vam') and text()='Save']");

    //    Report.Info("Clicking Save...");
    //    Wait.UntilClickable(saveButton).Click();
    //    WaitForLoader();
    //    WaitForPageLoad();
    //}

    /// <summary>
    /// Click Save without waiting for success — used in Negative scenarios.
    /// </summary>
    private void TryClickSave()
    {
        By saveButton = By.XPath(
            "//button[normalize-space()='Save']       | " +
            "//button[normalize-space()='Save Draft'] | " +
            "//input[@value='Save']"
        );

        try
        {
            Wait.UntilClickable(saveButton, timeoutSeconds: 5).Click();
            WaitForLoader(timeoutSeconds: 3);
        }
        catch
        {
            // Expected — validation error may prevent save button response
        }
    }

    /// <summary>Wait for loader with a configurable timeout override.</summary>
    private void WaitForLoader(int timeoutSeconds = 30)
    {
        By loader = By.CssSelector(".loading, .loader, .spinner, .overlay");
        try { Wait.UntilInvisible(loader, timeoutSeconds: timeoutSeconds); }
        catch { /* Loader may not appear */ }
    }
}