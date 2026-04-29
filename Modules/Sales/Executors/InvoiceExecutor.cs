using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Engine;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels.Invoice;
using Enfinity.ERP.Automation.Modules.Sales.Handlers;
using Enfinity.ERP.Automation.Modules.Sales.Validators;
using OpenQA.Selenium;

namespace Enfinity.ERP.Automation.Modules.Sales.Executors;

public class InvoiceExecutor : BaseExecutor<InvoiceDM>
{
    // ── Handlers ───────────────────────────────────────────────────────────
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

    // ── Network ────────────────────────────────────────────────────────────
    private readonly NetworkHelper _networkHelper;

    private const string EditInvoiceRoute = "sales/invoice/edit/{0}";

    public InvoiceExecutor(IWebDriver driver, WaitHelper wait, ReportHelper report)
        : base(driver, wait, report)
    {
        _headerHandler = new HeaderHandlers(driver, wait);
        _linesHandler = new LineHandler(driver, wait);
        _chargesHandler = new ChargesHandler(driver, wait);
        _paymentsHandler = new PaymentHandler(driver, wait);
        _othersHandler = new OtherHandler(driver, wait);
        _expectationHandler = new ExpectationHandler(driver, wait);

        _headerValidator = new HeaderValidator(driver, wait, report, _expectationHandler);
        _linesValidator = new LinesValidator(driver, wait, report, _expectationHandler);
        _totalsValidator = new TotalsValidator(driver, wait, report, _expectationHandler);
        _messageValidator = new MessageValidator(driver, wait, report, _expectationHandler);

        _networkHelper = new NetworkHelper(driver);
    }

    // ── Entry ──────────────────────────────────────────────────────────────

    public override void Execute(InvoiceDM data)
    {
        Report.Info($"── Sales Invoice Executor: {data.ScenarioType} ──");
        Report.Info($"Test: {data.TestDescription}");

        switch (data.ScenarioType?.ToUpperInvariant())
        {
            case "CREATE":
                ExecuteCreate(data);
                break;

            case "APPROVAL":
                ExecuteApproval(data);
                break;

            default:
                throw new ArgumentException($"Unknown ScenarioType: {data.ScenarioType}");
        }
    }

    // ── CREATE ─────────────────────────────────────────────────────────────

    //private void ExecuteCreate(InvoiceDM data)
    //{
    //    ExecuteStep("Navigate to New Sales Invoice", () =>
    //    {
    //        NavigateToModule("Sales");
    //        NavigateToListing("Invoice");
    //        OpenFormMode("New");
    //    });

    //    ExecuteStep("Fill Header", () =>
    //    {
    //        _headerHandler.Fill(data.Header);
    //        Save();
    //        ValidateAfterSave(data);
    //    });

    //    // 🔥 Dynamic Sections (Lines, Charges, Payments)
    //    var sections = new List<(string Name, Func<bool> ShouldRun, Action Action)>
    //    {
    //        ("Lines",
    //            () => data.Lines?.Any() == true,
    //            () => _linesHandler.Fill(data.Lines)
    //        ),

    //        ("Charges",
    //            () => data.Charges?.Items?.Any() == true,
    //            () => _chargesHandler.Fill(data.Charges)
    //        ),

    //        ("Payments",
    //            () => data.Payments?.Entries?.Any() == true,
    //            () => _paymentsHandler.Fill(data.Payments)
    //        )
    //    };

    //    ExecuteStep("Fill Dynamic Sections", () =>
    //    {
    //        foreach (var section in sections)
    //        {
    //            if (!section.ShouldRun())
    //                continue;

    //            ExecuteStep($"Fill {section.Name}", () =>
    //            {
    //                section.Action();
    //                Save();
    //            });
    //        }
    //    });

    //    ExecuteStep("Fill Others", () =>
    //    {
    //        _othersHandler.Fill(data.Others);
    //        Save(); // API fires here
    //    });

    //    // 🔥 Start API capture BEFORE final save
    //    ExecuteStep("Start totals API capture", () =>
    //    {
    //        _networkHelper.Clear();
    //        _networkHelper.StartCapture("/SalesInvoice/GetTxnSubtotals");
    //    });

    //    ExecuteStep("Open View Mode", () =>
    //    {
    //        ClickOnForm("View");
    //    });

    //    ExecuteStep("Validate After View", () =>
    //    {
    //        ValidateAfterView(data);
    //    });
    //}


    private void ExecuteCreate(InvoiceDM data)
    {
        ExecuteStep("Navigate to New Sales Invoice", () =>
        {
            NavigateToModule("Sales");
            NavigateToListing("Invoice");
            OpenFormMode("New");
        });

        ExecuteStep("Fill Header", () =>
        {
            _headerHandler.Fill(data.Header);
            Save();
            ValidateAfterSave(data);
        });

        // ── Execute Sections ──────────────────────────────────────────────
        var sections = new List<SectionDefinition<InvoiceDM>>
        {
            new()
            {
                Name = "Lines",
                ShouldRun = d => d.Lines?.Any() == true,
                Action = d => _linesHandler.Fill(d.Lines)
            },
            new()
            {
                Name = "Charges",
                ShouldRun = d => d.Charges?.Items?.Any() == true,
                Action = d => _chargesHandler.Fill(d.Charges)
            },
            new()
            {
                Name = "Payments",
                ShouldRun = d => d.Payments?.Entries?.Any() == true,
                Action = d => _paymentsHandler.Fill(d.Payments)
            },
            new()
            {
                Name = "Others",
                ShouldRun = d => d.Others != null,
                Action = d => _othersHandler.Fill(d.Others),
                RequiresSave = true
            }
        };

        var engine = new SectionEngine<InvoiceDM>(
            sections,
            Save,
            Report
        );

        ExecuteStep("Execute All Sections", () =>
        {
            engine.Execute(data);
        });

        ExecuteStep("Start totals API capture", () =>
        {
            _networkHelper.Clear();
            _networkHelper.StartCapture("/SalesInvoice/GetTxnSubtotals");
        });

        ExecuteStep("Open View Mode", () =>
        {
            ClickOnForm("View");
        });

        ExecuteStep("Validate After View", () =>
        {
            ValidateAfterView(data);
        });
    }

    // ── APPROVAL ───────────────────────────────────────────────────────────

    private void ExecuteApproval(InvoiceDM data)
    {
        ExecuteCreate(data);

        ExecuteStep("Approve Document", () =>
        {
            ClickOnForm("Approve");
            Wait.WaitForSeconds(3);
        });

        ExecuteStep("Validate After Approve", () =>
        {
            ValidateAfterApprove(data);
        });
    }

    // ── VALIDATIONS ────────────────────────────────────────────────────────

    private void ValidateAfterSave(InvoiceDM data)
    {
        if (data.Expected == null)
        {
            Report.Warning("No Expected values defined — skipping validation.");
            return;
        }

        _messageValidator.ValidateSuccessMessage(data.Expected);
        _headerValidator.ValidateDocumentNumberGenerated();
    }

    private void ValidateAfterView(InvoiceDM data)
    {
        if (data.Expected == null)
        {
            Report.Warning("No Expected values defined — skipping validation.");
            return;
        }

        _linesValidator.ValidateLineTotals(data.Lines);

        var totals = _networkHelper.GetResponse<TotalsResponseDM>();

        _totalsValidator.ValidateTotalsFromApi(data.Expected, totals);
    }

    private void ValidateAfterApprove(InvoiceDM data)
    {
        if (data.Expected == null)
        {
            Report.Warning("No Expected values defined — skipping validation.");
            return;
        }

        _headerValidator.ValidateDocumentStatus(data.Expected);
        _headerValidator.ValidateDocumentPaymentStatus(data.Expected);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private void ExecuteStep(string stepName, Action action)
    {
        try
        {
            Report.Info($"Step: {stepName}");
            action();
        }
        catch (Exception ex)
        {
            Report.Fail($"Failed at step: {stepName} | {ex.Message}");
            throw;
        }
    }

    private void Save()
    {
        ClickOnForm("Save");
        WaitForLoader();
    }
}
