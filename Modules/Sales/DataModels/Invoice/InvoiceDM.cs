using Enfinity.ERP.Automation.Core.DataModels.Shared;

namespace Enfinity.ERP.Automation.Modules.Sales.DataModels.Invoice;

/// <summary>
/// Complete data model for a Sales Invoice document.
/// Maps 1:1 with the Sales Invoice JSON test data files.
/// 
/// Sections:
///   Header   → Customer, Date, Currency, Location, SalesPerson, etc.
///   Lines    → List of line items with Item, Qty, Price, Tax, Discount
///   Charges  → Additional charges: Freight, Packing, etc.
///   Payments → Advance / payment entries linked to this invoice
///   Others   → Remarks, Internal Notes, Attachments info
///   Expected → Totals and status assertions used by Validators
/// </summary>
public class InvoiceDM : BaseDocumentDM
{
    public PreferenceDM? AppPreference { get; set; }
    public TxnParameterDM? TxnParameter { get; set; }
    public InvoiceHeaderDM Header { get; set; } = new();
    public List<InvoiceLineDM> Lines { get; set; } = new();
    public InvoiceChargesDM Charges { get; set; } = new();
    public InvoicePaymentsDM Payments { get; set; } = new();
    public InvoiceOthersDM Others { get; set; } = new();
}