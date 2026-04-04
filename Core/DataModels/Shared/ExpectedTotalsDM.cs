namespace Enfinity.ERP.Automation.Core.DataModels.Shared;

/// <summary>
/// Expected financial totals used by TotalsValidator.
/// Values are compared against what the ERP displays after save.
/// Tolerance of ±0.01 applied during decimal comparison.
/// </summary>
public class ExpectedTotalsDM
{
    /// <summary>Sum of all line amounts before any charges or tax.</summary>
    public decimal SubTotal { get; set; }

    /// <summary>Total discount amount applied across all lines.</summary>
    public decimal TotalDiscount { get; set; }

    /// <summary>Total tax amount computed across all lines.</summary>
    public decimal TotalTax { get; set; }

    /// <summary>Total additional charges (freight, packing, etc.)</summary>
    public decimal TotalCharges { get; set; }

    /// <summary>Final payable amount: SubTotal - Discount + Tax + Charges.</summary>
    public decimal GrandTotal { get; set; }

    /// <summary>Amount already paid (from Payments section).</summary>
    public decimal AmountPaid { get; set; }

    /// <summary>Balance due: GrandTotal - AmountPaid.</summary>
    public decimal BalanceDue { get; set; }
}