namespace Enfinity.ERP.Automation.Modules.Sales.DataModels;

/// <summary>
/// Others / Remarks section of the Sales Invoice.
/// Contains free-text notes, internal remarks, and terms.
/// </summary>
public class SalesInvoiceOthersDM
{
    /// <summary>Customer-facing remarks printed on the invoice.</summary>
    public string? Remarks { get; set; }

    /// <summary>Internal notes visible only to staff.</summary>
    public string? InternalNotes { get; set; }

    /// <summary>Terms and conditions text for this invoice.</summary>
    public string? TermsAndConditions { get; set; }
}