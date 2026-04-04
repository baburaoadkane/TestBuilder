namespace Enfinity.ERP.Automation.Modules.Sales.DataModels;

/// <summary>
/// Represents one line item row in the Sales Invoice Lines grid.
/// Each entry in the Lines array maps to one row in the ERP grid.
/// </summary>
public class SalesInvoiceLineDM
{
    // ── Item ──────────────────────────────────────────────────────────────

    /// <summary>Item code — typed into item search field.</summary>
    public string? ItemCode { get; set; }

    /// <summary>Item name — used for verification / fallback search.</summary>
    public string? ItemName { get; set; }

    // ── Quantity ──────────────────────────────────────────────────────────

    /// <summary>Quantity to invoice.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Unit of measure. Example: Nos, Kg, Box, Ltr</summary>
    public string? UOM { get; set; }

    // ── Pricing ───────────────────────────────────────────────────────────

    /// <summary>Unit price per UOM.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Discount percentage on this line. Example: 10 for 10%</summary>
    public decimal DiscountPercent { get; set; }

    // ── Tax ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Tax type / tax code applied on this line.
    /// Supports multiple tax types — maps to the ERP's tax dropdown.
    /// Example: "GST 18%", "VAT 5%", "Tax Exempt"
    /// </summary>
    public string? TaxType { get; set; }

    /// <summary>Tax rate percentage. Example: 18 for 18%</summary>
    public decimal TaxPercent { get; set; }

    // ── Computed (Expected) ───────────────────────────────────────────────

    /// <summary>
    /// Expected line total amount after discount and before tax.
    /// Used by LinesValidator to assert the computed value shown in ERP.
    /// Formula: (Quantity × UnitPrice) × (1 - DiscountPercent / 100)
    /// </summary>
    public decimal ExpectedLineTotal { get; set; }
}