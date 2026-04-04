using Enfinity.ERP.Automation.Core.Utilities;
using OpenQA.Selenium;

namespace Enfinity.ERP.Automation.Core.Base;

/// <summary>
/// Abstract base class for ALL Executor classes.
/// 
/// An Executor orchestrates the complete end-to-end flow for one document type.
/// It coordinates Handlers in the correct sequence: Navigate → Fill → Save → Approve → Validate.
/// 
/// Each module executor (SalesInvoiceExecutor, PurchaseOrderExecutor, etc.)
/// inherits this and overrides Execute() with its specific flow.
/// 
/// Usage:
///   public class SalesInvoiceExecutor : BaseExecutor&lt;SalesInvoiceDM&gt;
///   {
///       public override void Execute(SalesInvoiceDM data) { ... }
///   }
/// </summary>
public abstract class BaseExecutor<TDataModel> where TDataModel : class
{
    // ── Core dependencies ──────────────────────────────────────────────────
    protected readonly IWebDriver Driver;
    protected readonly WaitHelper Wait;
    protected readonly ConfigReader Config = ConfigReader.Instance;
    protected readonly ReportHelper Report;

    // ── Constructor ────────────────────────────────────────────────────────
    protected BaseExecutor(IWebDriver driver, WaitHelper wait, ReportHelper report)
    {
        Driver = driver;
        Wait = wait;
        Report = report;
    }

    // ── Abstract contract — every executor MUST implement this ─────────────

    /// <summary>
    /// Execute the full document creation / edit / action flow.
    /// Called directly by TestCase methods.
    /// </summary>
    public abstract void Execute(TDataModel data);

    // ── Shared navigation methods ──────────────────────────────────────────

    /// <summary>
    /// Navigate to a full URL path from the base URL.
    /// Example: Navigate("sales/invoices/new")
    /// </summary>
    protected void Navigate(string route)
    {
        string url = $"{Config.BaseUrl.TrimEnd('/')}/{route.TrimStart('/')}";
        Driver.Navigate().GoToUrl(url);
        WaitForPageLoad();
        Report.Info($"Navigated to: {url}");
    }

    protected void NavigateToEntity(string moduleName, string entityName)
    {
        By moduleButton = By.Id("AppModuleButton");

        By moduleLocator = By.XPath($"//span[normalize-space()='{moduleName}']");

        By entityLocator = By.XPath($"//a[@title='{entityName}']");

        Wait.UntilClickable(moduleButton, 5).Click();
        WaitForLoader();

        Wait.UntilClickable(moduleLocator, 5).Click();
        WaitForLoader();

        Wait.UntilClickable(entityLocator, 5).Click();
        WaitForLoader();
    }

    protected void OpenFormInCreateMode()
    {
        By formMode = By.XPath($"//li[@title='New']");

        Wait.UntilClickable(formMode, 5).Click();
        WaitForLoader();
    }

    /// <summary>
    /// Click a menu item by its visible text to navigate within ERP.
    /// Example: NavigateViaMenu("Sales", "Invoices", "New Invoice")
    /// </summary>
    protected void NavigateViaMenu(params string[] menuPath)
    {
        foreach (string menuItem in menuPath)
        {
            By locator = By.XPath($"//a[normalize-space()='{menuItem}'] | //span[normalize-space()='{menuItem}']");
            IWebElement element = Wait.UntilClickable(locator);
            element.Click();
            Thread.Sleep(300); // Brief pause for menu animation
        }
        WaitForPageLoad();
        Report.Info($"Navigated via menu: {string.Join(" → ", menuPath)}");
    }

    // ── Shared document actions ────────────────────────────────────────────

    /// <summary>
    /// Click the Save button and wait for the success confirmation.
    /// Override if your ERP uses a different save button locator.
    /// </summary>
    protected virtual void Save()
    {
        Report.Info("Saving document...");

        By saveButton = By.XPath(
            "//button[normalize-space()='Save'] | " +
            "//button[normalize-space()='Save & Close'] | " +
            "//input[@value='Save']"
        );

        Wait.UntilClickable(saveButton).Click();
        WaitForLoader();
        WaitForSuccessToast();

        Report.Info("Document saved successfully.");
    }

    /// <summary>
    /// Submit the document for approval workflow.
    /// Override with your ERP's specific submit/approve button.
    /// </summary>
    protected virtual void Submit()
    {
        Report.Info("Submitting document for approval...");

        By submitButton = By.XPath(
            "//button[normalize-space()='Submit'] | " +
            "//button[normalize-space()='Send for Approval']"
        );

        Wait.UntilClickable(submitButton).Click();
        WaitForLoader();
        WaitForSuccessToast();

        Report.Info("Document submitted for approval.");
    }

    /// <summary>
    /// Approve the document (called from an approver context).
    /// Override with your ERP's specific approve button.
    /// </summary>
    protected virtual void Approve()
    {
        Report.Info("Approving document...");

        By approveButton = By.XPath(
            "//button[normalize-space()='Approve'] | " +
            "//button[normalize-space()='Approve Document']"
        );

        Wait.UntilClickable(approveButton).Click();
        WaitForLoader();
        WaitForSuccessToast();

        Report.Info("Document approved successfully.");
    }

    /// <summary>
    /// Cancel the document.
    /// Override with your ERP's specific cancel button and confirmation dialog.
    /// </summary>
    protected virtual void Cancel()
    {
        Report.Info("Cancelling document...");

        By cancelButton = By.XPath("//button[normalize-space()='Cancel']");
        Wait.UntilClickable(cancelButton).Click();

        // Handle confirmation dialog if it appears
        ConfirmDialog();
        WaitForLoader();

        Report.Info("Document cancelled.");
    }

    /// <summary>
    /// Delete the document.
    /// Override with your ERP's specific delete button and confirmation dialog.
    /// </summary>
    protected virtual void Delete()
    {
        Report.Info("Deleting document...");

        By deleteButton = By.XPath("//button[normalize-space()='Delete']");
        Wait.UntilClickable(deleteButton).Click();

        ConfirmDialog();
        WaitForLoader();

        Report.Info("Document deleted.");
    }

    // ── Shared UI helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Handle a Yes/Confirm/OK dialog button.
    /// Override for ERP-specific confirmation dialogs.
    /// </summary>
    protected virtual void ConfirmDialog()
    {
        By confirmBtn = By.XPath(
            "//button[normalize-space()='Yes'] | " +
            "//button[normalize-space()='Confirm'] | " +
            "//button[normalize-space()='OK']"
        );

        try
        {
            Wait.UntilClickable(confirmBtn, timeoutSeconds: 5).Click();
            Report.Info("Confirmation dialog accepted.");
        }
        catch
        {
            // Dialog may not appear — that's fine
        }
    }

    /// <summary>
    /// Wait for a success toast/notification to appear and disappear.
    /// Override the locator for your ERP's specific toast element.
    /// </summary>
    protected virtual void WaitForSuccessToast()
    {
        By toast = By.CssSelector(
            ".toast-success, .alert-success, [class*='success'], [class*='notification']"
        );

        try
        {
            Wait.UntilVisible(toast, timeoutSeconds: 10);
            Report.Info("Success notification received.");
        }
        catch
        {
            // Toast may be too fast to catch — not a failure
        }
    }

    /// <summary>
    /// Wait for any loading overlay/spinner to disappear.
    /// Override the locator for your ERP's specific loader element.
    /// </summary>
    protected virtual void WaitForLoader()
    {
        By loader = By.CssSelector(
            ".loading, .loader, .spinner, [data-loading='true'], .overlay"
        );

        try { Wait.UntilInvisible(loader, timeoutSeconds: 30); }
        catch { /* Loader may not appear — continue */ }
    }

    /// <summary>
    /// Wait for the page to fully load (document.readyState = 'complete').
    /// </summary>
    protected void WaitForPageLoad()
    {
        Wait.UntilPageLoaded();
    }

    /// <summary>
    /// Get the current document number from the page after saving.
    /// Override with your ERP's specific document number field locator.
    /// </summary>
    protected virtual string GetDocumentNumber()
    {
        By docNumLocator = By.CssSelector(
            "[data-field='docno'], .document-number, #documentNumber"
        );

        try
        {
            return Wait.UntilVisible(docNumLocator).Text.Trim();
        }
        catch
        {
            return string.Empty;
        }
    }
}