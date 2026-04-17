using OpenQA.Selenium;
using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.DataModels;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers;

/// <summary>
/// Handles all UI interactions for the Sales Invoice CHARGES section.
///
/// Charges are additional costs applied at document level:
/// Freight, Packing, Insurance, Handling charges etc.
///
/// ASP.NET Razor pattern:
///   Each charge is a separate form row — same indexed pattern as Lines.
///   Charges[0].ChargeType, Charges[0].Amount, Charges[0].TaxTypeId
///
/// HOW TO UPDATE:
///   Inspect the Charges section in your ERP.
///   Update GetChargeLocator() with the actual id/name pattern.
///   Update the AddChargeButton locator with the actual button.
/// </summary>
public class ChargesHandler : BaseHandler
{
    // ── Section-level locators ─────────────────────────────────────────────

    private static readonly By AddChargeButton = By.XPath(
        "//button[normalize-space()='Add Charge'] | " +
        "//button[normalize-space()='Add']        | " +
        "//a[normalize-space()='Add Charge']      | " +
        "//button[contains(@id,'addCharge')]"
    );

    // ── Constructor ────────────────────────────────────────────────────────
    public ChargesHandler(IWebDriver driver, WaitHelper wait)
        : base(driver, wait) { }

    // ── Public entry point ─────────────────────────────────────────────────

    /// <summary>
    /// Fill all charge rows from the data model.
    /// Skips gracefully if Charges section is empty.
    /// </summary>
    public void Fill(SalesInvoiceChargesDM charges)
    {
        if (charges?.Items == null || charges.Items.Count == 0) return;

        // Navigate to Charges tab/section if it's a separate tab
        NavigateToChargesSection();

        for (int i = 0; i < charges.Items.Count; i++)
        {
            AddNewCharge(i);
            FillCharge(i, charges.Items[i]);
            WaitForLoader();
        }
    }

    // ── Private methods ────────────────────────────────────────────────────

    /// <summary>
    /// Click the Charges tab if the form uses tabs for sections.
    /// TODO: If Charges is always visible (not tabbed), remove this method body.
    /// </summary>
    private void NavigateToChargesSection()
    {
        By chargesTab = By.XPath(
            "//a[normalize-space()='Charges']   | " +
            "//li[normalize-space()='Charges']  | " +
            "//button[normalize-space()='Charges']"
        );

        try
        {
            Wait.UntilClickable(chargesTab, timeoutSeconds: 3).Click();
            WaitForLoader();
        }
        catch
        {
            // Charges section already visible — no tab navigation needed
        }
    }

    /// <summary>Click Add Charge button to create a new charge row.</summary>
    private void AddNewCharge(int chargeIndex)
    {
        if (chargeIndex == 0)
        {
            bool firstRowExists = IsVisible(GetChargeLocator(0, "ChargeType"));
            if (firstRowExists) return;
        }

        Click(AddChargeButton);

        Wait.Until(driver =>
            driver.FindElements(GetChargeLocator(chargeIndex, "Amount")).Count > 0,
            timeoutSeconds: 8
        );
    }

    /// <summary>Fill all fields for a single charge row.</summary>
    private void FillCharge(int index, ChargeDM charge)
    {
        FillChargeType(index, charge.ChargeType);
        FillChargeAmount(index, charge.AmountFC);
        FillChargeTaxType(index, charge.TaxType);
        FillChargeTaxable(index, charge.IsTaxable);
    }

    /// <summary>
    /// Select charge type from dropdown.
    /// Examples: "Freight", "Packing Charges", "Insurance"
    /// TODO: May be a <select> or autocomplete depending on your ERP.
    /// </summary>
    private void FillChargeType(int index, string? chargeType)
    {
        if (string.IsNullOrWhiteSpace(chargeType)) return;

        By locator = GetChargeLocator(index, "ChargeTypeId");

        try
        {
            SelectByText(locator, chargeType);
        }
        catch
        {
            // Try as text input if not a <select>
            Type(GetChargeLocator(index, "ChargeType"), chargeType);
        }
    }

    /// <summary>Enter the charge amount.</summary>
    private void FillChargeAmount(int index, decimal amount)
    {
        if (amount <= 0) return;

        By locator = GetChargeLocator(index, "Amount");
        IWebElement el = Wait.UntilVisible(locator);
        ScrollIntoView(el);

        el.SendKeys(Keys.Control + "a");
        el.SendKeys(amount.ToString("F2"));
        el.SendKeys(Keys.Tab);
    }

    /// <summary>Select tax type applied on this charge.</summary>
    private void FillChargeTaxType(int index, string? taxType)
    {
        if (string.IsNullOrWhiteSpace(taxType)) return;

        By locator = GetChargeLocator(index, "TaxTypeId");
        try
        {
            SelectByText(locator, taxType);
            WaitForLoader();
        }
        catch
        {
            // Tax type may be optional or read-only
        }
    }

    /// <summary>Check/uncheck the IsTaxable checkbox for this charge.</summary>
    private void FillChargeTaxable(int index, bool isTaxable)
    {
        By locator = GetChargeLocator(index, "IsTaxable");

        try
        {
            if (isTaxable)
                Check(locator);
            else
                Uncheck(locator);
        }
        catch
        {
            // Checkbox may not exist if tax is controlled by TaxType selection
        }
    }

    // ── Locator builder ────────────────────────────────────────────────────

    /// <summary>
    /// Builds the locator for a specific field in a specific charge row.
    /// TODO: Verify this pattern matches your ERP's actual rendered HTML.
    /// </summary>
    private static By GetChargeLocator(int index, string fieldName)
    {
        return By.Id($"Charges_{index}__{fieldName}");
    }
}