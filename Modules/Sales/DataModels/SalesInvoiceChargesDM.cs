namespace Enfinity.ERP.Automation.Modules.Sales.DataModels;

/// <summary>
/// Additional charges applied at the invoice level.
/// Example: Freight, Packing, Insurance, Handling.
/// Each charge has a type, amount, tax type, and whether it's taxable.
/// </summary>
public class SalesInvoiceChargesDM
{
    /// <summary>List of individual charge entries.</summary>
    public List<ChargeDM> Items { get; set; } = new();
}

public class ChargeDM
{
    /// <summary>Charge type / description. Example: "Freight", "Packing Charges"</summary>
    public string? ChargeType { get; set; }

    /// <summary>Charge amount.</summary>
    public decimal Amount { get; set; }

    /// <summary>Tax type applied on this charge. Example: "GST 18%"</summary>
    public string? TaxType { get; set; }

    /// <summary>Whether this charge is taxable.</summary>
    public bool IsTaxable { get; set; }
}