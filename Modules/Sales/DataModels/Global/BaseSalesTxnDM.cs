using Enfinity.ERP.Automation.Core.DataModels.Shared;

namespace Enfinity.ERP.Automation.Modules.Sales.DataModels.Global;

public class BaseSalesTxnDM : BaseDocumentDM
{
    public PreferenceDM? AppPreference { get; set; }
    public TxnParameterDM? TxnParameter { get; set; }
    public SalesTxnHeaderDM Header { get; set; } = new();
    public SalesTxnDiscountDM Discount { get; set; } = new();
    public List<SalesTxnLineDM> Lines { get; set; } = new();
    public SalesTxnChargesDM Charges { get; set; } = new();
    public SalesTxnPaymentsDM Payments { get; set; } = new();
    public SalesTxnGeneralDM General { get; set; } = new();
    public SalesTxnOthersDM Others { get; set; } = new();
}
