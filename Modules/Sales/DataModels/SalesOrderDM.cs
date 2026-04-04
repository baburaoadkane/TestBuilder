using Enfinity.ERP.Automation.Core.DataModels.Shared;

namespace Enfinity.ERP.Automation.Modules.Sales.DataModels;

/// <summary>
/// Data model for a Sales Order document.
/// Shares the same section structure as Sales Invoice.
/// Header fields differ slightly (Delivery Date instead of Due Date).
/// </summary>
public class SalesOrderDM : BaseDocumentDM
{
    public SalesOrderHeaderDM Header { get; set; } = new();
    public List<SalesInvoiceLineDM> Lines { get; set; } = new();
    public SalesInvoiceChargesDM Charges { get; set; } = new();
    public SalesInvoiceOthersDM Others { get; set; } = new();
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