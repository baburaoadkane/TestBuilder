using OpenQA.Selenium;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers;

public class LinesHandlers : BaseHandler
{
    // ── Common ────────────────────────────────────────────────────────────
    //private static readonly By LookupText = By.XPath("//div[@class='lookup-text']");
    private static readonly By LookupText = By.XPath("//div[contains(@class,'lookup-text')]");
    private static readonly By DeleteLineButton = By.XPath("//div[@class='dx-button-content' and .//span[text()='Delete Line']]");
    private static readonly By AddLineButton = By.XPath("//div[@class='dx-button-content' and .//span[text()='New Line']]");
    //private static readonly By NextPageButton = By.XPath("//img[@alt='Next']");
    private static readonly By NextPageButton = By.XPath("//a[contains(@class,'dxp-button')]//img[@alt='Next']");
    private static readonly By ExtraFieldButton = By.XPath("//img[contains(@id, '_DXCBtn-1Img')]");

    // ── Constructor ───────────────────────────────────────────────────────
    public LinesHandlers(IWebDriver driver, WaitHelper wait)
        : base(driver, wait) { }

    // ── Public Entry ──────────────────────────────────────────────────────
    public void Fill(List<SalesInvoiceLineDM> lines)
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
    private void FillLine(SalesInvoiceLineDM line)
    {
        if (!string.IsNullOrWhiteSpace(line.Barcode))
        {
            Lookup("Barcode", line.Barcode);
        }
        else
        {
            LookupCell("Item", line.Item);
        }

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
            line.DiscountValue > 0
            )
        {
            ClickToShowExtraFields();
        }

        LookupCell("UOM", line.UOM);
        SetCell("DiscountPercent", line.DiscountInPercent);
        SetCell("DiscountValue", line.DiscountValue);
        SetCell("Remarks", line.Remarks);
    }

    // ── 🔥 Generic Lookup (Dropdown based) ────────────────────────────────
    private void Lookup(string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var dropdown = GetDropdown(field);

        OpenDropdown(dropdown);

        var nextPage = NextPageLocator();

        SelectOption(LookupText, nextPage, value);
    }

    // ── 🔥 Lookup inside Grid Cell ────────────────────────────────────────
    private void LookupCell(string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var input = GetCell(field);
        Click(input);

        var dropdown = GetDropdown(field);
        OpenDropdown(dropdown);

        var nextPage = NextPageLocator();

        SelectOption(LookupText, nextPage, value);
    }

    // ── 🔥 Field Mapping ──────────────────────────────────────────────────
    private By GetDropdown(string field) => field switch
    {
        "Barcode" => By.XPath("//td[contains(@id, '_ItemBarcodeId_B-1')]"),
        "Item" => By.XPath("//td[contains(@id, '_ItemId_B-1')]"),
        "Color" => By.XPath("//td[contains(@id, '_ItemColorId_B-1')]"),
        "Size" => By.XPath("//td[contains(@id, '_ItemSizeId_B-1')]"),
        "Warehouse" => By.XPath("//td[contains(@id, '_WarehouseId_B-1')]"),
        "UOM" => By.XPath("//td[contains(@id, '_UnitOfMeasureId_B-1')]"),
        _ => throw new Exception($"Dropdown mapping not found for {field}")
    };

    private By GetCell(string field) => field switch
    {
        "Item" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[2]"),
        "Description" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[3]"),
        "Size" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[4]"),
        "Color" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[5]"),
        "Warehouse" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[6]"),
        "Quantity" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[8]"),
        "UnitPrice" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[9]"),
        "GrossAmount" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[12]"),
        "BonusQty" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[13]"),
        "UOM" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[29]"),
        "DiscountPercent" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[30]"),
        "DiscountValue" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[31]"),
        "Remarks" => By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[32]"),
        _ => throw new Exception($"Cell mapping not found for {field}")
    };

    // ── 🔥 Next Page Locator ──────────────────────────────────────    
    private By NextPageLocator()
    {
        return NextPageButton;
    }

    // ── Line Actions ──────────────────────────────────────────────────────
    private void DeleteExistingLine()
    {
        if (IsVisible(DeleteLineButton))
        {
            Click(DeleteLineButton);
            //Wait.WaitForSeconds(1);
            WaitForLoader();
        }
    }
    private void AddNewLine()
    {
        Click(AddLineButton);
        //Wait.WaitForSeconds(1);
        WaitForLoader();
    }

    private void ClickToShowExtraFields()
    {
        Click(ExtraFieldButton);
        //Wait.WaitForSeconds(1);
        WaitForLoader();
    }
    // ── Reusable Validation ──────────────────────────────────────────────────────
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

    private void SetCell(string field, object? value)
    {
        if (value == null || !IsValidValue(value)) return;

        string finalValue = value switch
        {
            decimal d => d.ToString("G29"),
            double d => d.ToString("G29"),
            _ => value.ToString()
        };

        SetClipboardValue(GetCell(field), finalValue);
        WaitForLoader();
    }
}