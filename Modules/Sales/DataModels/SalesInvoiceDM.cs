using Enfinity.ERP.Automation.Core.DataModels.Shared;

namespace Enfinity.ERP.Automation.Modules.Sales.DataModels;

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
public class SalesInvoiceDM : BaseDocumentDM
{
    public SalesInvoiceHeaderDM Header { get; set; } = new();
    public List<SalesInvoiceLineDM> Lines { get; set; } = new();
    public SalesInvoiceChargesDM Charges { get; set; } = new();
    public SalesInvoicePaymentsDM Payments { get; set; } = new();
    public SalesInvoiceOthersDM Others { get; set; } = new();
}