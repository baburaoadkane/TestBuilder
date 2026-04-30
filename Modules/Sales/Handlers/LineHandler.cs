using OpenQA.Selenium;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels.Invoice;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers;

public class LineHandler : BaseHandler
{
    // ── Common ────────────────────────────────────────────────────────────
    private static readonly By LookupText = By.XPath("//div[contains(@class,'lookup-text')]");
    private static readonly By DeleteLineButton = By.XPath("//div[@class='dx-button-content' and .//span[text()='Delete Line']]");
    private static readonly By AddLineButton = By.Id("SalesInvoiceLineNewButton");
    private static readonly By NextButton = By.XPath("//a[contains(@class,'dxp-button')]//img[@alt='Next']");
    private static readonly By ExtraFieldButton = By.XPath("//img[contains(@id, '_DXCBtn-1Img')]");

    // ── Constructor ───────────────────────────────────────────────────────
    public LineHandler(IWebDriver driver, WaitHelper wait, ReportHelper report) : base(driver, wait, report) { }

    // ── 🔥 Field Configuration Model ──────────────────────────────────────
    private class FieldConfig
    {
        public By? Dropdown { get; set; }
        public int? ColumnIndex { get; set; }
    }

    // ── 🔥 Central Field Mapping (Single Source of Truth) ──────────────────
    private static readonly Dictionary<string, FieldConfig> FieldMap = new()
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
            ColumnIndex = 8 
        },
        ["UnitPrice"] = new() 
        { 
            ColumnIndex = 9 
        },
        ["GrossAmount"] = new() 
        { 
            ColumnIndex = 12 
        },
        ["BonusQty"] = new() 
        {
            ColumnIndex = 13
        },
        ["UOM"] = new() 
        { 
            Dropdown = By.XPath("//td[contains(@id, '_UnitOfMeasureId_B-1')]"),
            ColumnIndex = 29 
        },
        ["DiscountPercent"] = new() 
        { 
            ColumnIndex = 30 
        },
        ["DiscountValue"] = new() 
        { 
            ColumnIndex = 31
        },
        ["Remarks"] = new() 
        { 
            ColumnIndex = 32 
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

    // ── 🔥 Generic Lookup ────────────────────────────────────────────────
    private void Lookup(string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var dropdown = GetDropdown(field);

        OpenDropdown(dropdown);
        WaitForLoader();

        SelectOption(LookupText, NextButton, value);
    }

    // ── 🔥 Lookup inside Grid Cell ────────────────────────────────────────
    private void LookupCell(string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var cell = GetCell(field);
        Click(cell);

        var dropdown = GetDropdown(field);

        OpenDropdown(dropdown);
        WaitForLoader();

        SelectOption(LookupText, NextButton, value);
    }

    // ── 🔥 Mapping Accessors ──────────────────────────────────────────────
    private By GetDropdown(string field)
    {
        if (!FieldMap.ContainsKey(field) || FieldMap[field].Dropdown == null)
            throw new Exception($"Dropdown not defined for field: {field}");

        return FieldMap[field].Dropdown!;
    }

    private int GetColIndex(string field)
    {
        if (!FieldMap.ContainsKey(field) || FieldMap[field].ColumnIndex == null)
            throw new Exception($"Column index not defined for field: {field}");

        return FieldMap[field].ColumnIndex!.Value;
    }

    // ── 🔥 Cell Locator ───────────────────────────────────────────────────
    private By GetCell(string field)
    {
        int colIndex = GetColIndex(field);
        return By.XPath($"(//div[@class='dxgBCTC dx-ellipsis'])[{colIndex}]");
    }

    // ── Line Actions ──────────────────────────────────────────────────────
    private void DeleteExistingLine()
    {
        if (IsVisible(DeleteLineButton))
        {
            Click(DeleteLineButton);
            WaitForLoader();
        }
    }

    private void AddNewLine()
    {
        Click(AddLineButton);
        WaitForLoader();
    }

    private void ClickToShowExtraFields()
    {
        Click(ExtraFieldButton);
        WaitForLoader();
    }

    // ── Set Cell Value ────────────────────────────────────────────────────
    private void SetCell(string field, object? value)
    {
        if (value == null || !IsValidValue(value)) return;

        string finalValue = value switch
        {
            decimal d => d.ToString("G29"),
            double d => d.ToString("G29"),
            _ => value.ToString()
        };

        var cell = GetCell(field);
        SetClipboardValue(cell, finalValue);
    }

    // ── Validation ────────────────────────────────────────────────────────
    private bool IsValidValue(object value)
    {
        return value switch
        {
            string s => !string.IsNullOrWhiteSpace(s),
            decimal d => d > 0,
            int i => i > 0,
            double d => d > 0,
            _ => true
        };
    }
}