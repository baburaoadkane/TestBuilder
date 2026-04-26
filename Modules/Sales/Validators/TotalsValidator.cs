using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.DataModels.Shared;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;
using Enfinity.ERP.Automation.Modules.Sales.Handlers;
using OpenQA.Selenium;

namespace Enfinity.ERP.Automation.Modules.Sales.Validators;

/// <summary>
/// Validates the financial TOTALS section of a saved Sales Invoice.
///
/// Validates every amount in the invoice summary:
///   SubTotal, TotalDiscount, TotalTax, TotalCharges,
///   GrandTotal, AmountPaid, BalanceDue
///
/// Uses ExpectationHandler.ReadTotals() to get actual values from the UI.
/// Compares against ExpectedTotalsDM from the JSON file.
/// Tolerance: ±0.01 to handle ERP rounding differences.
/// </summary>
public class TotalsValidator : BaseValidator
{
    private readonly ExpectationHandler _expectation;

    public TotalsValidator(
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report,
        ExpectationHandler expectation)
        : base(driver, wait, report)
    {
        _expectation = expectation;
    }

    // ── Public validation methods ──────────────────────────────────────────

    /// <summary>
    /// Validate all totals against the expected values from the JSON file.
    /// Skips fields where the expected value is 0 and not critical.
    /// </summary>
    public void ValidateTotals(ExpectedResultDM? expected)
    {
        if (expected?.Totals == null)
        {
            Report.Warning("Expected.Totals not defined in JSON — skipping totals validation.");
            return;
        }

        Report.Info("── Validating Financial Totals ──");

        // Read all actual totals from the UI in one call
        var actuals = _expectation.ReadTotals();

        ValidateSingleTotal(actuals, "SubTotal", expected.Totals.SubTotal);
        ValidateSingleTotal(actuals, "TotalDiscount", expected.Totals.TotalDiscount);
        ValidateSingleTotal(actuals, "TotalTax", expected.Totals.TotalTax);
        ValidateSingleTotal(actuals, "TotalCharges", expected.Totals.TotalCharges);
        ValidateSingleTotal(actuals, "GrandTotal", expected.Totals.GrandTotal);
        ValidateSingleTotal(actuals, "AmountPaid", expected.Totals.AmountPaid);
        ValidateSingleTotal(actuals, "BalanceDue", expected.Totals.BalanceDue);
    }    

    /// <summary>
    /// Validate only the GrandTotal — quick assertion for simple scenarios.
    /// </summary>
    public void ValidateGrandTotal(decimal expectedGrandTotal)
    {
        Report.Info($"Validating Grand Total: Expected = {expectedGrandTotal}");
        var actuals = _expectation.ReadTotals();

        if (actuals.TryGetValue("GrandTotal", out decimal actual))
            AssertAmountEqual(expectedGrandTotal, actual, "Grand Total");
        else
            Report.Warning("Could not read GrandTotal from UI.");
    }

    /// <summary>
    /// Validate only the BalanceDue — useful after partial payment scenarios.
    /// </summary>
    public void ValidateBalanceDue(decimal expectedBalance)
    {
        Report.Info($"Validating Balance Due: Expected = {expectedBalance}");
        var actuals = _expectation.ReadTotals();

        if (actuals.TryGetValue("BalanceDue", out decimal actual))
            AssertAmountEqual(expectedBalance, actual, "Balance Due");
        else
            Report.Warning("Could not read BalanceDue from UI.");
    }

    // ── Private helper ─────────────────────────────────────────────────────

    /// <summary>
    /// Validate a single total field by key name.
    /// Reads from the actuals dictionary and compares with expected value.
    /// </summary>
    private void ValidateSingleTotal(
        Dictionary<string, decimal> actuals,
        string key,
        decimal expected)
    {
        if (!actuals.TryGetValue(key, out decimal actual))
        {
            Report.Warning($"⚠ {key}: Could not read from UI — locator may need updating.");
            return;
        }

        AssertAmountEqual(expected, actual, key);
    }

    public void ValidateFromApi(InvoiceTotalsResponse actual, ExpectedResultDM? expected)
    {
        if (expected == null)
        {
            Report.Warning("Expected result is null — skipping API totals validation.");
            return;
        }

        Assert.AreEqual(expected.Totals.SubTotal, actual.GrossValue, "SubTotal mismatch");
        Assert.AreEqual(expected.Totals.TotalDiscount, actual.DiscountValue, "Discount mismatch");
        Assert.AreEqual(expected.Totals.GrandTotal, actual.NetValue, "GrandTotal mismatch");
    }
}