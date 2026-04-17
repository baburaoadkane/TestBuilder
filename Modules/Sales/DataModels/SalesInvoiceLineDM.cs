namespace Enfinity.ERP.Automation.Modules.Sales.DataModels;

/// <summary>
/// Represents one line item row in the Sales Invoice Lines grid.
/// Each entry in the Lines array maps to one row in the ERP grid.
/// </summary>
public class SalesInvoiceLineDM
{
    // ── Item ──────────────────────────────────────────────────────────────

    /// <summary>Item Barcode — scanned or typed into barcode field./// </summary>
    public string? Barcode { get; set; }
    /// <summary>Item code — typed into item search field.</summary>
    public string? Item { get; set; }

    /// <summary>Item name — used for verification / fallback search.</summary>
    public string? ItemName { get; set; }

    public string? Description { get; set; }
    public string? Warehouse { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }

    // ── Quantity ──────────────────────────────────────────────────────────

    /// <summary>Unit of measure. Example: Nos, Kg, Box, Ltr</summary>
    public string? UOM { get; set; }

    /// <summary>Quantity to invoice.</summary>
    public decimal Quantity { get; set; }

    // ── Pricing ───────────────────────────────────────────────────────────

    /// <summary>Unit price per UOM.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Discount percentage on this line. Example: 10 for 10%</summary>
    public decimal DiscountInPercent { get; set; }

    public string? DiscountValue { get; set; }

    // ── Tax ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Tax type / tax code applied on this line.
    /// Supports multiple tax types — maps to the ERP's tax dropdown.
    /// Example: "GST 18%", "VAT 5%", "Tax Exempt"
    /// </summary>
    public string? TaxType { get; set; }

    /// <summary>Tax rate percentage. Example: 18 for 18%</summary>
    public decimal TaxPercent { get; set; }

    // ── Additional Fields ─────────────────────────────────────────────────
    public string? BonusQty { get; set; }
    public string? Remarks { get; set; }
    // ── Computed (Expected) ───────────────────────────────────────────────

    /// <summary>
    /// Expected line total amount after discount and before tax.
    /// Used by LinesValidator to assert the computed value shown in ERP.
    /// Formula: (Quantity × UnitPrice) × (1 - DiscountPercent / 100)
    /// </summary>
    public decimal ExpectedLineTotal { get; set; }
}