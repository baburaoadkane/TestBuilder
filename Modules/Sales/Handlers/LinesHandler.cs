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

    private static readonly By DeleteLineButton = By.XPath("//div[@class='dx-button-content' and .//span[text()='Delete Line']]");

    /// <summary>Button to add a new line row. TODO: verify exact button text/id.</summary>
    private static readonly By AddLineButton = By.XPath("//div[@class='dx-button-content' and .//span[text()='New Line']]");

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

        for (int i = 0; i < lines.Count; i++)
        {
            DeleteLine();
            AddNewLine(i);
            FillLine(i, lines[i]);
            WaitForLoader(); // Wait for line total to recalculate after each row
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
    private void AddNewLine(int lineIndex)
    {
        // First line row often pre-exists in ASP.NET forms
        // Only click Add Line for subsequent rows
        //if (lineIndex == 0)
        //{
        //    bool firstRowExists = IsVisible(GetLineLocator(0, "ItemCode"));
        //    if (firstRowExists) return;
        //}

        Click(AddLineButton);
        Wait.WaitForSeconds(1);
        // Wait for the new row's item field to be visible
        //Wait.Until(driver =>
        //{
        //    var elements = driver.FindElements(GetLineLocator(lineIndex, "ItemCode"));
        //    return elements.Count > 0 && elements[0].Displayed;
        //}, timeoutSeconds: 10);
    }

    /// <summary>Fill all fields for a single line row at the given index.</summary>
    private void FillLine(int index, SalesInvoiceLineDM line)
    {
        FillItemCode(index, line.ItemCode);
        FillQuantity(index, line.Quantity);
        FillUOM(index, line.UOM);
        FillUnitPrice(index, line.UnitPrice);
        FillDiscountPercent(index, line.DiscountPercent);
        FillTaxType(index, line.TaxType);
    }

    /// <summary>
    /// Type item code into the autocomplete field and select the match.
    /// After selection, ERP auto-populates ItemName, UOM, UnitPrice.
    /// </summary>
    private void FillItemCode(int index, string? itemCode)
    {
        if (string.IsNullOrWhiteSpace(itemCode)) return;

        By inputLocator = GetLineLocator(index, "ItemCode");
        IWebElement input = Wait.UntilVisible(inputLocator);
        ScrollIntoView(input);
        input.Clear();
        input.SendKeys(itemCode);

        // Wait for item autocomplete dropdown
        By itemDropdown = By.CssSelector(
            ".item-autocomplete li, ul.item-dropdown li, .item-search-results li"
        );

        Wait.UntilVisible(itemDropdown, timeoutSeconds: 8);
        var options = Driver.FindElements(itemDropdown);
        var match = options.FirstOrDefault(o =>
            o.Text.Trim().Contains(itemCode, StringComparison.OrdinalIgnoreCase));

        Click(match ?? options.First());
        WaitForLoader(); // Item selection auto-fills price, UOM, tax
    }

    /// <summary>Set quantity for a line. Clears existing value first.</summary>
    private void FillQuantity(int index, decimal quantity)
    {
        if (quantity <= 0) return;

        By locator = GetLineLocator(index, "Quantity");
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
    private void FillUOM(int index, string? uom)
    {
        if (string.IsNullOrWhiteSpace(uom)) return;

        By locator = GetLineLocator(index, "UOMId");

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
    private void FillUnitPrice(int index, decimal unitPrice)
    {
        if (unitPrice <= 0) return;

        By locator = GetLineLocator(index, "UnitPrice");
        IWebElement el = Wait.UntilVisible(locator);
        ScrollIntoView(el);

        el.SendKeys(Keys.Control + "a");
        el.SendKeys(unitPrice.ToString("F2"));
        el.SendKeys(Keys.Tab);
    }

    /// <summary>Set discount percentage for the line.</summary>
    private void FillDiscountPercent(int index, decimal discountPercent)
    {
        if (discountPercent <= 0) return;

        By locator = GetLineLocator(index, "DiscountPercent");
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
    private void FillTaxType(int index, string? taxType)
    {
        if (string.IsNullOrWhiteSpace(taxType)) return;

        By locator = GetLineLocator(index, "TaxTypeId");

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
    private static By GetLineLocator(int index, string fieldName)
    {
        // TODO: Verify this ID pattern against your ERP's actual HTML
        return By.Id($"Lines_{index}__{fieldName}");
    }
}