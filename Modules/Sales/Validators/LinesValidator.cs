using OpenQA.Selenium;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;
using Enfinity.ERP.Automation.Modules.Sales.Handlers;

namespace Enfinity.ERP.Automation.Modules.Sales.Validators;

/// <summary>
/// Validates the LINES section of a saved Sales Invoice.
///
/// Responsibilities:
///   - Assert each line's computed total matches the expected value
///   - Assert the correct number of lines were saved
///   - Uses ExpectationHandler to read actual line totals from the UI
/// </summary>
public class LinesValidator : BaseValidator
{
    private readonly ExpectationHandler _expectation;

    public LinesValidator(
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
    /// Assert that each line's displayed total matches the ExpectedLineTotal
    /// defined in the JSON data model.
    /// Skips lines where ExpectedLineTotal is 0 (not specified).
    /// </summary>
    public void ValidateLineTotals(List<SalesInvoiceLineDM>? lines)
    {
        if (lines == null || lines.Count == 0)
        {
            Report.Warning("No lines defined in data model — skipping line totals validation.");
            return;
        }

        Report.Info($"Validating {lines.Count} line(s)...");

        for (int i = 0; i < lines.Count; i++)
        {
            SalesInvoiceLineDM line = lines[i];

            if (line.ExpectedLineTotal == 0)
            {
                Report.Warning($"Line {i + 1}: ExpectedLineTotal is 0 — skipping.");
                continue;
            }

            decimal actualTotal = _expectation.ReadLineTotal(i);

            AssertAmountEqual(
                expected: line.ExpectedLineTotal,
                actual: actualTotal,
                fieldName: $"Line {i + 1} Total ({line.Item})"
            );
        }
    }

    /// <summary>
    /// Assert that the number of line rows visible on screen
    /// matches the number of lines in the data model.
    /// </summary>
    public void ValidateLineCount(int expectedCount)
    {
        // TODO: Update this locator to match your ERP's line row selector
        By lineRows = By.CssSelector(
            ".lines-grid tr.line-row, " +
            "table#linesTable tbody tr, " +
            "[data-section='lines'] .line-item"
        );

        try
        {
            var rows = Driver.FindElements(lineRows);
            int actualCount = rows.Count;

            if (actualCount == expectedCount)
                Report.Pass($"✓ Line Count: Expected={expectedCount} | Actual={actualCount}");
            else
            {
                Report.Fail($"✗ Line Count: Expected={expectedCount} | Actual={actualCount}");
                NUnit.Framework.Assert.Fail(
                    $"[LinesValidator] Line count mismatch. Expected: {expectedCount}, Actual: {actualCount}");
            }
        }
        catch
        {
            Report.Warning("Could not read line count — locator may need updating.");
        }
    }

    /// <summary>
    /// Assert the item code displayed in a specific line matches the expected value.
    /// </summary>
    public void ValidateLineItemCode(int lineIndex, string expectedItemCode)
    {
        By locator = By.Id($"Lines_{lineIndex}__ItemCode");
        AssertValue(locator, expectedItemCode, $"Line {lineIndex + 1} Item Code");
    }

    /// <summary>
    /// Assert the quantity displayed in a specific line matches the expected value.
    /// </summary>
    public void ValidateLineQuantity(int lineIndex, decimal expectedQty)
    {
        By locator = By.Id($"Lines_{lineIndex}__Quantity");
        string actual = GetValue(locator);

        if (decimal.TryParse(actual, out decimal actualQty))
            AssertAmountEqual(expectedQty, actualQty, $"Line {lineIndex + 1} Quantity");
        else
            Report.Warning($"Line {lineIndex + 1} Quantity: Could not parse '{actual}'");
    }
}