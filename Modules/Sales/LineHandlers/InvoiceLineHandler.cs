using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels.Invoice;
using OpenQA.Selenium;

namespace Enfinity.ERP.Automation.Modules.Sales.LineHandlers;

public class InvoiceLineHandler : BaseLineHandler<InvoiceLineDM>
{
    public InvoiceLineHandler(IWebDriver driver, WaitHelper wait, ReportHelper report) 
        : base(driver, wait, report) { }

    protected override Dictionary<string, FieldConfig> FieldMap => new()
    {
        ["Barcode"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_ItemBarcodeId_B-1')]"),
            ColumnIndex = 1
        },
        ["Item"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_ItemId_B-1')]"),
            ColumnIndex = 2
        },
        ["Description"] = new()
        {
            ColumnIndex = 3
        },
        ["Size"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_ItemSizeId_B-1')]"),
            ColumnIndex = 4
        },
        ["Color"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_ItemColorId_B-1')]"),
            ColumnIndex = 5
        },
        ["Warehouse"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_WarehouseId_B-1')]"),
            ColumnIndex = 6
        },
        ["Quantity"] = new()
        {
            ColumnIndex = 7
        },
        ["Unit"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_SalesUnitId_B-1')]"),
            ColumnIndex = 8
        },
        ["Price"] = new()
        {
            ColumnIndex = 9
        },
        ["LineAmount"] = new()
        {
            ColumnIndex = 10
        }
    };

    // ── Public Entry ──────────────────────────────────────────────────────
    public void Fill(List<InvoiceLineDM> lines)
    {
        if (lines == null || lines.Count == 0) return;

        DeleteExistingLine();

        foreach (var line in lines)
        {
            AddNewLine();
            FillLine(line);
            WaitForLoader();
        }
    }

    // ── Core Line Fill ────────────────────────────────────────────────────
    private void FillLine(InvoiceLineDM line)
    {
        if (!string.IsNullOrWhiteSpace(line.Barcode))
            Lookup("Barcode", line.Barcode);
        else
            LookupCell("Item", line.Item);

        SetCell("Description", line.Description);
        LookupCell("Color", line.Color);
        LookupCell("Size", line.Size);
        LookupCell("Warehouse", line.Warehouse);

        SetCell("Quantity", line.Quantity);
        SetCell("UnitPrice", line.UnitPrice);
        SetCell("GrossAmount", line.GrossAmount);
        SetCell("BonusQty", line.BonusQty);

        if (!string.IsNullOrWhiteSpace(line.UOM) ||
            !string.IsNullOrWhiteSpace(line.Remarks) ||
            line.DiscountInPercent > 0 ||
            line.DiscountValue > 0)
        {
            ClickToShowExtraFields();
        }

        LookupCell("UOM", line.UOM);
        SetCell("DiscountPercent", line.DiscountInPercent);
        SetCell("DiscountValue", line.DiscountValue);
        SetCell("Remarks", line.Remarks);
    }    
}

