namespace Enfinity.ERP.Automation.Core.DataModels.Shared;

/// <summary>
/// Expected results section — present in every test data JSON file.
/// Validators use these values to assert correctness after document actions.
/// </summary>
public class ExpectedResultDM
{
    /// <summary>Expected document status after the flow completes.</summary>
    /// <example>Draft, Submitted, Approved, Cancelled</example>
    public string? Status { get; set; }

    /// <summary>Expected success message text to appear in toast/notification.</summary>
    public string? SuccessMessage { get; set; }

    /// <summary>Expected error/validation message for negative test cases.</summary>
    public string? ErrorMessage { get; set; }

    public string? ValidationMessage { get; set; } = null;

    /// <summary>Expected totals — validated by TotalsValidator.</summary>
    public ExpectedTotalsDM? Totals { get; set; }
}