namespace Enfinity.ERP.Automation.Modules.Sales.DataModels;

/// <summary>
/// Header section fields of the Sales Invoice form.
/// All fields map directly to the form fields on the ERP screen.
/// Null values are skipped during form filling.
/// </summary>
public class SalesInvoiceHeaderDM
{
    // ── Customer ───────────────────────────────────────────────────────────

    /// <summary>Customer name or code — typed into autocomplete field.</summary>
    public string? Customer { get; set; }

    // ── Dates ──────────────────────────────────────────────────────────────

    /// <summary>Invoice date. Format: dd-MM-yyyy</summary>
    public string? InvoiceDate { get; set; }

    /// <summary>Payment due date. Format: dd-MM-yyyy</summary>
    public string? DueDate { get; set; }

    // ── Financial ─────────────────────────────────────────────────────────

    /// <summary>Currency code. Example: USD, INR, AED</summary>
    public string? Currency { get; set; }

    /// <summary>Price list to apply. Example: "Standard Selling"</summary>
    public string? PriceList { get; set; }

    /// <summary>Payment terms. Example: "Net 30", "Immediate"</summary>
    public string? PaymentTerms { get; set; }

    // ── Location ──────────────────────────────────────────────────────────

    /// <summary>Warehouse or location the invoice is raised from.</summary>
    public string? Location { get; set; }

    // ── Sales Info ────────────────────────────────────────────────────────

    /// <summary>Sales person assigned to this invoice.</summary>
    public string? SalesPerson { get; set; }

    /// <summary>External reference number (e.g. customer PO number).</summary>
    public string? ReferenceNo { get; set; }
}