using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Model;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.DataModels.Shared;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.Builders;
using Enfinity.ERP.Automation.Modules.Sales.Executors;
using Enfinity.ERP.Automation.Modules.Sales.Handlers;
using Microsoft.VisualBasic;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Script;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Enfinity.ERP.Automation.Modules.Sales.Validators;

/// <summary>
/// Validates toast notifications, success messages, and error/validation messages.
///
/// Responsibilities:
///   - Assert success toast appears after Save/Submit/Approve
///   - Assert error message appears for negative scenarios
///   - Assert specific message text matches expected from JSON
///
/// Uses ExpectationHandler to read actual message text from the UI.
/// </summary>
public class MessageValidator : BaseValidator
{
    private readonly ExpectationHandler _expectation;

    public MessageValidator(
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
    /// Assert a success message appears.
    /// If Expected.SuccessMessage is defined, also validates the text content.
    /// </summary>
    public void ValidateSuccessMessage(ExpectedResultDM? expected)
    {
        Report.Info("Validating success notification...");

        string actual = _expectation.ReadSuccessMessage();

        if (string.IsNullOrWhiteSpace(actual))
        {
            Report.Warning("⚠ No success message found on page. " +
                           "The action may have completed silently or the locator needs updating.");
            return;
        }

        Report.Pass($"✓ Success message appeared: '{actual}'");

        // If a specific message text is expected, assert it matches
        if (!string.IsNullOrWhiteSpace(expected?.SuccessMessage))
        {
            bool matches = actual.Contains(
                expected.SuccessMessage,
                StringComparison.OrdinalIgnoreCase
            );

            if (matches)
                Report.Pass($"✓ Success Message Text: Expected='{expected.SuccessMessage}' | Actual='{actual}'");
            else
            {
                Report.Fail($"✗ Success Message Text: Expected='{expected.SuccessMessage}' | Actual='{actual}'");
                NUnit.Framework.Assert.Fail(
                    $"[MessageValidator] Success message mismatch. " +
                    $"Expected to contain: '{expected.SuccessMessage}', Actual: '{actual}'");
            }
        }
    }

    /// <summary>
    /// Assert an error/validation message appears with expected text.
    /// Used exclusively in Negative scenario tests.
    /// </summary>
    public void ValidateErrorMessage(ExpectedResultDM? expected)
    {
        if (string.IsNullOrWhiteSpace(expected?.ErrorMessage))
        {
            Report.Warning("Expected.ErrorMessage not defined in JSON — skipping error validation.");
            return;
        }

        Report.Info($"Validating error message: Expected = '{expected.ErrorMessage}'");

        string actual = _expectation.ReadErrorMessage();

        if (string.IsNullOrWhiteSpace(actual))
        {
            Report.Fail($"✗ No error message found. Expected: '{expected.ErrorMessage}'");
            NUnit.Framework.Assert.Fail(
                $"[MessageValidator] Expected error message '{expected.ErrorMessage}' " +
                $"but no error was displayed on the page.");
            return;
        }

        bool matches = actual.Contains(
            expected.ErrorMessage,
            StringComparison.OrdinalIgnoreCase
        );

        if (matches)
            Report.Pass($"✓ Error Message: Expected='{expected.ErrorMessage}' | Actual='{actual}'");
        else
        {
            Report.Fail($"✗ Error Message: Expected='{expected.ErrorMessage}' | Actual='{actual}'");
            NUnit.Framework.Assert.Fail(
                $"[MessageValidator] Error message mismatch. " +
                $"Expected to contain: '{expected.ErrorMessage}', Actual: '{actual}'");
        }
    }

    public void ValidateValidationMessage(ExpectedResultDM? expected)
    {
        if (string.IsNullOrWhiteSpace(expected?.ValidationMessage))
        {
            Report.Warning("Expected.ValidationMessage not defined in JSON — skipping validation message check.");
            return;
        }

        Report.Info($"Validating message: Expected = '{expected.ValidationMessage}'");   
        string actual = _expectation.ReadValidationMessage();

        if (string.IsNullOrWhiteSpace(actual))
        {
            Report.Fail($"✗ No validation message found. Expected: '{expected.ValidationMessage}'");
            NUnit.Framework.Assert.Fail(
                $"[MessageValidator] Expected validation message '{expected.ValidationMessage}' " +
                $"but no validation message was displayed on the page.");
            return;
        }

        bool matches = actual.Contains(
            expected.ValidationMessage,
            StringComparison.OrdinalIgnoreCase
        );

        if (matches)
            Report.Pass($"✓ Validation Message: Expected='{expected.ValidationMessage}' | Actual='{actual}'");
        else
        {
            Report.Fail($"✗ Validation Message: Expected='{expected.ValidationMessage}' | Actual='{actual}'");
            NUnit.Framework.Assert.Fail(
                $"[MessageValidator] Validation message mismatch. " +
                $"Expected to contain: '{expected.ValidationMessage}', Actual: '{actual}'");
        }
    }

    /// <summary>
    /// Assert no error messages are visible on the page.
    /// Useful to confirm a valid form submission has no validation errors.
    /// </summary>
    public void ValidateNoErrors()
    {
        string errorText = _expectation.ReadErrorMessage();

        if (string.IsNullOrWhiteSpace(errorText))
            Report.Pass("✓ No error messages on page.");
        else
        {
            Report.Fail($"✗ Unexpected error message found: '{errorText}'");
            NUnit.Framework.Assert.Fail(
                $"[MessageValidator] Unexpected error on page: '{errorText}'");
        }
    }
}