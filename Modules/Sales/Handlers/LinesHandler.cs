using OpenQA.Selenium;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers;

/// <summary>
/// Handles all UI interactions for the Sales Invoice LINES section.
///
/// ASP.NET Razor specifics:
///   Each line is a SEPARATE FORM ROW — meaning the ERP renders
///   individual input fields per row using indexed names:
///   Lines[0].ItemCode, Lines[0].Quantity, Lines[1].ItemCode etc.
///
///   Flow per line:
///     1. Click "Add Line" / "Add Item" button
///     2. Wait for new row to render
///     3. Fill each field in that row using the row index
///     4. Repeat for all lines in the data model
///
/// HOW TO UPDATE LOCATORS:
///   Inspect the Lines section in your ERP.
///   Check the name="" or id="" pattern for line fields.
///   Common ASP.NET patterns:
///     name="Lines[0].ItemCode"  → By.Name("Lines[0].ItemCode")
///     id="Lines_0__ItemCode"    → By.Id("Lines_0__ItemCode")
///   Update GetLineLocator() below accordingly.
/// </summary>
public class LinesHandler : BaseHandler
{
    // ── Section-level locators ─────────────────────────────────────────────
    private static readonly By LookupText = By.XPath("//div[@class='lookup-text']");
    private static readonly By NextPage = By.XPath("//img[@alt='Next']");
    private static readonly By DeleteLineButton = By.XPath("//div[@class='dx-button-content' and .//span[text()='Delete Line']]");

    /// <summary>Button to add a new line row. TODO: verify exact button text/id.</summary>
    private static readonly By AddLineButton = By.XPath("//div[@class='dx-button-content' and .//span[text()='New Line']]");

    private static readonly By BarcodeDropdown = By.XPath("//td[contains(@id, '_ItemBarcodeId_B-1')]");
    private static readonly By ItemDropdown = By.XPath("//td[contains(@id, '_ItemId_B-1')]");

    // ── Constructor ────────────────────────────────────────────────────────
    public LinesHandler(IWebDriver driver, WaitHelper wait)
        : base(driver, wait) { }

    // ── Public entry point ─────────────────────────────────────────────────

    /// <summary>
    /// Fill all line item rows from the data model.
    /// Adds a new row for each line and fills all fields.
    /// </summary>
    public void Fill(List<SalesInvoiceLineDM> lines)
    {
        if (lines == null || lines.Count == 0) return;

        DeleteLine();

        for (int i = 0; i < lines.Count; i++)
        {            
            AddNewLine();
            FillLine(lines[i]);
            WaitForLoader();
        }
    }

    // ── Private methods ────────────────────────────────────────────────────
    private void DeleteLine()
    {
        Click(DeleteLineButton);
        Wait.WaitForSeconds(1);
    }

    /// <summary>
    /// Click "Add Line" to create a new empty row.
    /// For the first line, the row may already exist — handle gracefully.
    /// </summary>
    private void AddNewLine()
    {        
        Click(AddLineButton);
        Wait.WaitForSeconds(1);        
    }

    /// <summary>Fill all fields for a single line row at the given index.</summary>
    private void FillLine(SalesInvoiceLineDM line)
    {
        FillItem(line.Barcode, line.Item);
        FillQuantity(line.Quantity);
        FillUOM(line.UOM);
        FillUnitPrice(line.UnitPrice);
        FillDiscountPercent(line.DiscountInPercent);
        FillTaxType(line.TaxType);
    }

    private void SelectFromLookup(By dropdown, By input, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        OpenDropdown(dropdown);

        //Type(input, value);
        //WaitForLoader();

        //var nextPage = GetNextPageLocator(input);
        var nextPage = NextPage;

        SelectOption(LookupText, nextPage, value);
    }

    private By GetNextPageLocator(By inputLocator)
    {
        var element = Wait.UntilVisible(inputLocator);
        string id = element.GetAttribute("id");

        // Example:
        // SalesInvoice.CustomerIdLookup_I

        // Step 1: Split by '.'
        var parts = id.Split('.');

        if (parts.Length < 2)
            throw new Exception($"Invalid id format: {id}");

        string modulePrefix = parts[0]; // SalesInvoice
        string fieldPart = parts[1];    // CustomerIdLookup_I

        // Step 2: Remove suffix "_I"
        fieldPart = fieldPart.Replace("_I", "");

        // Step 3: Remove "Lookup"
        string fieldName = fieldPart.Replace("Lookup", ""); // CustomerId

        // Final:
        // SalesInvoice.CustomerId_NextPage

        string nextPageId = $"{modulePrefix}.{fieldName}_NextPage";

        return By.Id(nextPageId);
    }

    /// <summary>
    /// Type item code into the autocomplete field and select the match.
    /// After selection, ERP auto-populates ItemName, UOM, UnitPrice.
    /// </summary>
    private void FillItem(string? barCode, string? item)
    {
        if (string.IsNullOrEmpty(barCode) || string.IsNullOrEmpty(item)) return;

        if (!string.IsNullOrWhiteSpace(barCode))
        {
            SelectFromLookup(BarcodeDropdown, NextPage, barCode);
        }
        else
        {
            PressEnter();
            SelectFromLookup(ItemDropdown, NextPage, item);
            Wait.WaitForSeconds(1);
        }        
    }

    /// <summary>Set quantity for a line. Clears existing value first.</summary>
    private void FillQuantity(decimal quantity)
    {
        if (quantity <= 0) return;

        By locator = GetLineLocator("Quantity");
        IWebElement el = Wait.UntilVisible(locator);
        ScrollIntoView(el);

        // Triple-click to select all existing text, then type new value
        el.SendKeys(Keys.Control + "a");
        el.SendKeys(quantity.ToString("G29"));
        el.SendKeys(Keys.Tab); // Trigger line total recalculation
    }

    /// <summary>
    /// Set UOM — may be a <select> or read-only (auto-filled by item selection).
    /// TODO: If UOM is read-only after item selection, remove this method call.
    /// </summary>
    private void FillUOM(string? uom)
    {
        if (string.IsNullOrWhiteSpace(uom)) return;

        By locator = GetLineLocator("UOMId");

        // Check if it's a dropdown or read-only field
        try
        {
            SelectByText(locator, uom);
        }
        catch
        {
            // Field may be read-only after item auto-fill — skip silently
        }
    }

    /// <summary>
    /// Set Unit Price. May be auto-filled by price list — override if needed.
    /// TODO: If UnitPrice is locked after item selection, remove this method call.
    /// </summary>
    private void FillUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0) return;

        By locator = GetLineLocator("UnitPrice");
        IWebElement el = Wait.UntilVisible(locator);
        ScrollIntoView(el);

        el.SendKeys(Keys.Control + "a");
        el.SendKeys(unitPrice.ToString("F2"));
        el.SendKeys(Keys.Tab);
    }

    /// <summary>Set discount percentage for the line.</summary>
    private void FillDiscountPercent(decimal discountPercent)
    {
        if (discountPercent <= 0) return;

        By locator = GetLineLocator("DiscountPercent");
        IWebElement el = Wait.UntilVisible(locator);
        ScrollIntoView(el);

        el.SendKeys(Keys.Control + "a");
        el.SendKeys(discountPercent.ToString("G29"));
        el.SendKeys(Keys.Tab);
    }

    /// <summary>
    /// Select tax type from the line-level tax dropdown.
    /// Supports multiple tax types — selects by visible text.
    /// </summary>
    private void FillTaxType(string? taxType)
    {
        if (string.IsNullOrWhiteSpace(taxType)) return;

        By locator = GetLineLocator("TaxTypeId");

        try
        {
            SelectByText(locator, taxType);
            WaitForLoader(); // Tax selection triggers line total recalculation
        }
        catch
        {
            // Tax may be auto-applied from item master — skip if field not editable
        }
    }

    // ── Locator builder ────────────────────────────────────────────────────

    /// <summary>
    /// Builds the locator for a specific field in a specific line row.
    ///
    /// ASP.NET Razor renders indexed fields as:
    ///   id="Lines_0__ItemCode"   (underscores, double underscore)
    ///   name="Lines[0].ItemCode" (brackets and dot)
    ///
    /// TODO: Inspect your ERP's HTML and confirm which pattern is used.
    ///       Update the format string below to match exactly.
    ///
    /// Common patterns:
    ///   By.Id($"Lines_{index}__{{fieldName}}")        → Lines_0__ItemCode
    ///   By.Name($"Lines[{{index}}].{{fieldName}}")    → Lines[0].ItemCode
    ///   By.CssSelector($"tr:nth-child({{rowNum}}) input[data-field='{{fieldName}}']")
    /// </summary>
    private static By GetLineLocator(string fieldName)
    {
        // TODO: Verify this ID pattern against your ERP's actual HTML
        return By.Id($"Lines_{fieldName}");
    }
}