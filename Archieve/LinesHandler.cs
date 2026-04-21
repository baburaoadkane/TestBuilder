using OpenQA.Selenium;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers;

public class LinesHandler : BaseHandler
{
    // ── Section-level locators ─────────────────────────────────────────────
    private static readonly By LookupText = By.XPath("//div[@class='lookup-text']");
    private static readonly By NextPage = By.XPath("//img[@alt='Next']");

    private static readonly By DeleteLineButton = By.XPath("//div[@class='dx-button-content' and .//span[text()='Delete Line']]");
    private static readonly By AddLineButton = By.XPath("//div[@class='dx-button-content' and .//span[text()='New Line']]");

    private static readonly By BarcodeDropdown = By.XPath("//td[contains(@id, '_ItemBarcodeId_B-1')]");
    private static readonly By ItemDropdown = By.XPath("//td[contains(@id, '_ItemId_B-1')]");
    private static readonly By ItemDescriptionInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[3]");

    private static readonly By SizeInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[4]");
    private static readonly By SizeDropdown = By.XPath("//td[contains(@id, '_ItemSizeId_B-1')]");

    private static readonly By ColorInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[5]");
    private static readonly By ColorDropdown = By.XPath("//td[contains(@id, '_ItemColorId_B-1')]");

    private static readonly By WarehouseInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[6]");
    private static readonly By WarehouseDropdown = By.XPath("//td[contains(@id, '_WarehouseId_B-1')]");

    private static readonly By QuantityInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[8]");

    private static readonly By UnitpriceInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[9]");

    private static readonly By GrossAmountInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[12]");

    private static readonly By BonusQtyInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[13]");

    private static readonly By ExtraFieldIcon = By.XPath("//img[contains(@id, '_DXCBtn-1Img')]");

    private static readonly By UOMInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[29]");
    private static readonly By UOMDropdown = By.XPath("//td[contains(@id, '_UnitOfMeasureId_B-1')]");

    private static readonly By DiscountInPercentInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[30]");
    private static readonly By DiscountValueInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[31]");
    private static readonly By RemarksInput = By.XPath("(//div[@class='dxgBCTC dx-ellipsis'])[32]");

    // ── Constructor ────────────────────────────────────────────────────────
    public LinesHandler(IWebDriver driver, WaitHelper wait)
        : base(driver, wait) { }

    // ── Public entry point ─────────────────────────────────────────────────

    public void Fill(List<SalesInvoiceLineDM> lines)
    {
        if (lines == null || lines.Count == 0) return;

        DeleteLine();

        foreach (var line in lines)
        {
            AddNewLine();
            FillLine(line);
            WaitForLoader();
        }
    }

    // ── Private methods ────────────────────────────────────────────────────
    private void DeleteLine()
    {
        if (IsVisible(DeleteLineButton))
        {
            Click(DeleteLineButton);
            Wait.WaitForSeconds(1);
        }
    }
    private void AddNewLine()
    {
        Click(AddLineButton);
        Wait.WaitForSeconds(1);
    }

    private void FillLine(SalesInvoiceLineDM line)
    {
        FillBarcode(line.Barcode);
        FillItem(line.Item);
        FillItemDescription(line.Description);
        FillColor(line.Color);
        FillSize(line.Size);
        FillWarehouse(line.Warehouse);
        FillQuantity(line.Quantity);
        FillUnitPrice(line.UnitPrice);
        FillGrossAmount(line.GrossAmount);
        FillBonusQty(line.BonusQty);
        ClickOnExtraField();
        FillUOM(line.UOM);
        FillDiscountInPercent(line.DiscountInPercent);
        FillDiscountValue(line.DiscountValue);
        FillRemarks(line.Remarks);
    }

    private void SelectFromLookup(By dropdown, By nextPage, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        OpenDropdown(dropdown);

        SelectOption(LookupText, nextPage, value);
    }
    private void FillBarcode(string? barCode)
    {
        if (string.IsNullOrWhiteSpace(barCode)) return;

        SelectFromLookup(BarcodeDropdown, NextPage, barCode);
    }
    private void FillItem(string? item)
    {
        if (string.IsNullOrEmpty(item)) return;

        PressEnter();
        SelectFromLookup(ItemDropdown, NextPage, item);
    }
    private void FillItemDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description)) return;

        SetClipboardValue(ItemDescriptionInput, description);
    }
    private void FillColor(string? option)
    {
        if (string.IsNullOrEmpty(option)) return;
        Click(ColorInput);
        SelectFromLookup(ColorDropdown, NextPage, option);
        Wait.WaitForSeconds(1);
    }
    private void FillSize(string? option)
    {
        if (string.IsNullOrEmpty(option)) return;
        Click(SizeInput);
        SelectFromLookup(SizeDropdown, NextPage, option);
        Wait.WaitForSeconds(1);
    }
    private void FillWarehouse(string? option)
    {
        if (string.IsNullOrEmpty(option)) return;
        Click(WarehouseInput);
        SelectFromLookup(WarehouseDropdown, NextPage, option);
        Wait.WaitForSeconds(1);
    }
    private void FillQuantity(decimal quantity)
    {
        if (quantity <= 0) return;

        SetClipboardValue(QuantityInput, quantity.ToString("G29"));
    }
    private void FillUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0) return;

        SetClipboardValue(UnitpriceInput, unitPrice.ToString("G29"));
    }
    private void FillGrossAmount(decimal unitPrice)
    {
        if (unitPrice <= 0) return;

        SetClipboardValue(GrossAmountInput, unitPrice.ToString("G29"));
    }
    private void FillBonusQty(decimal unitPrice)
    {
        if (unitPrice <= 0) return;

        SetClipboardValue(BonusQtyInput, unitPrice.ToString("G29"));
    }
    private void ClickOnExtraField()
    {
        Click(ExtraFieldIcon);
    }
    private void FillUOM(string? uom)
    {
        if (string.IsNullOrWhiteSpace(uom)) return;
        Click(UOMInput);
        SelectFromLookup(UOMDropdown, NextPage, uom);
        Wait.WaitForSeconds(1);
    }
    private void FillDiscountInPercent(decimal discountInPercent)
    {
        if (discountInPercent <= 0) return;
        SetClipboardValue(DiscountInPercentInput, discountInPercent.ToString("G29"));
    }
    private void FillDiscountValue(decimal discountValue)
    {
        if (discountValue <= 0) return;
        SetClipboardValue(DiscountValueInput, discountValue.ToString("G29"));
    }
    private void FillRemarks(string? remarks)
    {
        if (string.IsNullOrWhiteSpace(remarks)) return;

        SetClipboardValue(RemarksInput, remarks);
    }
}