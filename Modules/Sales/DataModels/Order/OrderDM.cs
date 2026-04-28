using Enfinity.ERP.Automation.Core.DataModels.Shared;
using Enfinity.ERP.Automation.Modules.Sales.DataModels.Invoice;

namespace Enfinity.ERP.Automation.Modules.Sales.DataModels.Order;

/// <summary>
/// Data model for a Sales Order document.
/// Shares the same section structure as Sales Invoice.
/// Header fields differ slightly (Delivery Date instead of Due Date).
/// </summary>
public class OrderDM : BaseDocumentDM
{
    public SalesOrderHeaderDM Header { get; set; } = new();
    public List<InvoiceLineDM> Lines { get; set; } = new();
    public InvoiceChargesDM Charges { get; set; } = new();
    public InvoiceOthersDM Others { get; set; } = new();
}

public class SalesOrderHeaderDM
{
    public string? Customer { get; set; }
    public string? OrderDate { get; set; }

    /// <summary>Requested/promised delivery date.</summary>
    public string? DeliveryDate { get; set; }

    public string? Currency { get; set; }
    public string? PriceList { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Location { get; set; }
    public string? SalesPerson { get; set; }
    public string? ReferenceNo { get; set; }
}