using Enfinity.ERP.Automation.Core.DataModels.Shared;
using Enfinity.ERP.Automation.Modules.Sales.DataModels.Invoice;

namespace Enfinity.ERP.Automation.Modules.Sales.DataModels.Return;

/// <summary>
/// Data model for a Sales Return document.
/// Always linked back to the original Sales Invoice.
/// </summary>
public class ReturnDM : BaseDocumentDM
{
    public SalesReturnHeaderDM Header { get; set; } = new();
    public List<InvoiceLineDM> Lines { get; set; } = new();
    public InvoiceOthersDM Others { get; set; } = new();
}

public class SalesReturnHeaderDM
{
    /// <summary>Original Sales Invoice number being returned against.</summary>
    public string? OriginalInvoiceNo { get; set; }

    public string? Customer { get; set; }
    public string? ReturnDate { get; set; }
    public string? Currency { get; set; }
    public string? Location { get; set; }

    /// <summary>Reason for return. Example: "Damaged Goods", "Wrong Item"</summary>
    public string? ReturnReason { get; set; }
}