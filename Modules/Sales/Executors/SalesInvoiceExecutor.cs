using Enfinity.ERP.Automation.Archieve;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Enums;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;
using Enfinity.ERP.Automation.Modules.Sales.Handlers;
using Enfinity.ERP.Automation.Modules.Sales.Validators;
using OpenQA.Selenium;

namespace Enfinity.ERP.Automation.Modules.Sales.Executors;

public class SalesInvoiceExecutor : BaseExecutor<SalesInvoiceDM>
{
    // ── Handlers ───────────────────────────────────────────────────────────
    //private readonly HeaderHandler _headerHandler;
    private readonly HeaderHandlers _headerHandler;
    private readonly LineHandler _linesHandler;
    private readonly ChargesHandler _chargesHandler;
    private readonly PaymentHandler _paymentsHandler;
    private readonly OtherHandler _othersHandler;
    private readonly ExpectationHandler _expectationHandler;

    // ── Validators ─────────────────────────────────────────────────────────
    private readonly HeaderValidator _headerValidator;
    private readonly LinesValidator _linesValidator;
    private readonly TotalsValidator _totalsValidator;
    private readonly MessageValidator _messageValidator;
    private readonly NetworkHelper _networkHelper;

    // ── Navigation route ───────────────────────────────────────────────────
    // TODO: Update this to match your ERP's actual Sales Invoice URL route
    private const string EditInvoiceRoute = "sales/invoice/edit/{0}";

    // ── Constructor ────────────────────────────────────────────────────────
    public SalesInvoiceExecutor(IWebDriver driver, WaitHelper wait, ReportHelper report)
        : base(driver, wait, report)
    {
        // Initialize all handlers
        _headerHandler = new HeaderHandlers(driver, wait);
        _linesHandler = new LineHandler(driver, wait);
        _chargesHandler = new ChargesHandler(driver, wait);
        _paymentsHandler = new PaymentHandler(driver, wait);
        _othersHandler = new OtherHandler(driver, wait);
        _expectationHandler = new ExpectationHandler(driver, wait);

        // Initialize all validators
        _headerValidator = new HeaderValidator(driver, wait, report, _expectationHandler);
        _linesValidator = new LinesValidator(driver, wait, report, _expectationHandler);
        _totalsValidator = new TotalsValidator(driver, wait, report, _expectationHandler);
        _messageValidator = new MessageValidator(driver, wait, report, _expectationHandler);
        _networkHelper = new NetworkHelper(driver);
    }

    // ── Entry point ───────────────────────────────────────────────────

    public override void Execute(SalesInvoiceDM data)
    {
        Report.Info($"── Sales Invoice Executor: {data.ScenarioType} ──");
        Report.Info($"Test: {data.TestDescription}");

        string scenarioType = data.ScenarioType?.ToUpperInvariant() ?? "CREATE";

        switch (scenarioType)
        {
            case "CREATE":
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

    private void ExecuteCreate(SalesInvoiceDM data)
    {
        Report.Info("Step 1: Navigate to New Sales Invoice");
        NavigateToModule("Sales");
        NavigateToListing("Invoice");
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

        Report.Info("Step 6: Fill Others");
        _othersHandler.Fill(data.Others);

        Report.Info("Step 7: Save document");
        _networkHelper.Clear();
        ClickOnForm("View");

        Report.Info("Start capturing totals API");        
        _networkHelper.StartCapture("/GetTxnSubtotals");

        Report.Info("Step 8: Validate");
        ValidateAfterSave(data);
    }

    private void ExecuteApproval(SalesInvoiceDM data)
    {
        // Step 1–7: Same as Create
        ExecuteCreate(data);

        // Step 8: Submit for approval
        //Report.Info("Step 8: Submit document for approval");
        //Submit();
        //_messageValidator.ValidateSuccessMessage(data.Expected);

        // Step 9: Approve
        Report.Info("Step 9: Approve document");
        ClickOnForm("Approve");
        //_messageValidator.ValidateSuccessMessage(data.Expected);

        // Step 10: Validate final approved state
        //Report.Info("Step 10: Validate approved state");
        //_headerValidator.ValidateStatus(data.Expected);
        //_totalsValidator.ValidateTotals(data.Expected);
    }

    private void ExecuteNegative(SalesInvoiceDM data)
    {
        Report.Info("Step 1: Navigate to New Sales Invoice");
        NavigateToModule("Sales");
        NavigateToListing("Invoice");
        OpenFormMode("New");

        Report.Info("Step 2: Fill form with invalid/incomplete data");
        _headerHandler.Fill(data.Header);

        Report.Info("Step 3: Attempt to Save (expecting validation error)");
        ClickOnForm("Save");

        Report.Info("Step 4: Validate validation message");
        _messageValidator.ValidateValidationMessage(data.Expected);
    }

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

    private void ValidateAfterSave(SalesInvoiceDM data)
    {
        if (data.Expected == null)
        {
            Report.Warning("No Expected values defined in JSON — skipping validation.");
            return;
        }

        //_messageValidator.ValidateSuccessMessage(data.Expected);
        _headerValidator.ValidateStatus(data.Expected);
        //_linesValidator.ValidateLineTotals(data.Lines);
        //_totalsValidator.ValidateTotals(data.Expected);

        var totals = _networkHelper.GetResponse<InvoiceTotalsResponse>();

        _totalsValidator.ValidateFromApi(totals, data.Expected);
    }    
}