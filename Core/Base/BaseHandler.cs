using Enfinity.ERP.Automation.Core.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using static System.Net.Mime.MediaTypeNames;

namespace Enfinity.ERP.Automation.Core.Base;

/// <summary>
/// Base class for ALL Handler classes (HeaderHandler, LinesHandler, etc.)
/// 
/// Responsibilities:
///   - Wraps all common Selenium interactions (Click, Type, Select, etc.)
///   - Uses WaitHelper before every action — no raw Thread.Sleep
///   - Built-in retry logic for flaky elements
///   - Scroll into view before interacting
///   - Clear + Type pattern for input fields
/// 
/// Usage:
///   public class HeaderHandler : BaseHandler
///   {
///       public HeaderHandler(IWebDriver driver, WaitHelper wait)
///           : base(driver, wait) { }
///   }
/// </summary>
public abstract class BaseHandler
{
    // ── Core dependencies ──────────────────────────────────────────────────
    protected readonly IWebDriver Driver;
    protected readonly WaitHelper Wait;
    protected readonly ConfigReader Config = ConfigReader.Instance;

    private readonly Actions _actions;

    // ── Constructor ────────────────────────────────────────────────────────
    protected BaseHandler(IWebDriver driver, WaitHelper wait)
    {
        Driver = driver;
        Wait = wait;
        _actions = new Actions(driver);
    }

    // ── Click Actions ──────────────────────────────────────────────────────

    /// <summary>Wait for element to be clickable, then click.</summary>
    protected void Click(By locator)
    {
        IWebElement element = Wait.UntilClickable(locator, 3);
        ScrollIntoView(element);
        element.Click();
    }

    /// <summary>Click directly on a located element.</summary>
    protected void Click(IWebElement element)
    {
        ScrollIntoView(element);
        Wait.UntilElementClickable(element);
        element.Click();
    }

    /// <summary>
    /// JavaScript click — use when standard click is intercepted.
    /// Useful for hidden elements or elements behind overlays.
    /// </summary>
    protected void JSClick(By locator)
    {
        IWebElement element = Wait.UntilVisible(locator);
        ScrollIntoView(element);
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", element);
    }

    /// <summary>Double-click an element (e.g. to open inline edit in grids).</summary>
    protected void DoubleClick(By locator)
    {
        IWebElement element = Wait.UntilClickable(locator);
        ScrollIntoView(element);
        _actions.DoubleClick(element).Perform();
    }

    // ── Input Actions ──────────────────────────────────────────────────────

    /// <summary>
    /// Clear existing text and type new value.
    /// Skips if value is null or empty.
    /// </summary>
    protected void Type(By locator, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        IWebElement element = Wait.UntilVisible(locator);
        ScrollIntoView(element);
        element.Clear();
        element.SendKeys(value);
        Thread.Sleep(600);
    }

    /// <summary>Type into an already-located element.</summary>
    protected void Type(IWebElement element, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        ScrollIntoView(element);
        element.Clear();
        element.SendKeys(value);
        Thread.Sleep(600);
    }
    protected void ClearAndType(By locator, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        IWebElement element = Driver.FindElement(locator);
        element.SendKeys(Keys.Control + "a");
        element.SendKeys(Keys.Delete);
        element.SendKeys(value);
        Thread.Sleep(1000);
    }
    /// <summary>
    /// Type and press Enter — used for search/autocomplete fields.
    /// Types the value then waits briefly for the dropdown to appear.
    /// </summary>
    protected void TypeAndSelect(By locator, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        Type(locator, value);
        Thread.Sleep(600); // Brief wait for autocomplete dropdown
        SendKey(locator, Keys.ArrowDown);
        SendKey(locator, Keys.Enter);
    }

    /// <summary>Send a specific key to an element (Enter, Tab, Escape, etc.)</summary>
    protected void SendKey(By locator, string key)
    {
        IWebElement element = Wait.UntilVisible(locator);
        element.SendKeys(key);
    }

    /// <summary>Clear an input field completely.</summary>
    protected void Clear(By locator)
    {
        IWebElement element = Wait.UntilVisible(locator);
        element.Clear();
    }

    // ── Date Actions ───────────────────────────────────────────────────────

    /// <summary>
    /// Set a date field value using JavaScript to bypass datepicker popups.
    /// Format: "dd-MM-yyyy" or whatever your ERP expects.
    /// </summary>
    protected void SetDate(By locator, string date)
    {
        if (string.IsNullOrWhiteSpace(date)) return;

        IWebElement element = Wait.UntilVisible(locator);
        ScrollIntoView(element);
        ((IJavaScriptExecutor)Driver)
            .ExecuteScript("arguments[0].value = arguments[1];", element, date);

        // Trigger change event so ERP form reacts
        ((IJavaScriptExecutor)Driver)
            .ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", element);
    }

    // ── Checkbox / Radio Actions ───────────────────────────────────────────

    /// <summary>Check a checkbox if it is not already checked.</summary>
    protected void Check(By locator)
    {
        IWebElement element = Wait.UntilVisible(locator);
        if (!element.Selected) element.Click();
    }

    /// <summary>Uncheck a checkbox if it is currently checked.</summary>
    protected void Uncheck(By locator)
    {
        IWebElement element = Wait.UntilVisible(locator);
        if (element.Selected) element.Click();
    }

    // ── Read / Get Actions ─────────────────────────────────────────────────

    /// <summary>Get the visible text of an element.</summary>
    protected string GetText(By locator)
    {
        IWebElement element = Wait.UntilVisible(locator);
        return element.Text.Trim();
    }

    /// <summary>Get the value attribute of an input field.</summary>
    protected string GetValue(By locator)
    {
        IWebElement element = Wait.UntilVisible(locator);
        return element.GetAttribute("value")?.Trim() ?? string.Empty;
    }

    /// <summary>Get any HTML attribute of an element.</summary>
    protected string GetAttribute(By locator, string attribute)
    {
        IWebElement element = Wait.UntilVisible(locator);
        return element.GetAttribute(attribute)?.Trim() ?? string.Empty;
    }

    /// <summary>Check if an element is currently visible on the page.</summary>
    protected bool IsVisible(By locator)
    {
        try
        {
            return Driver.FindElement(locator).Displayed;
        }
        catch (NoSuchElementException) { return false; }
        catch (StaleElementReferenceException) { return false; }
    }

    /// <summary>Check if an element is enabled (not disabled/readonly).</summary>
    protected bool IsEnabled(By locator)
    {
        try
        {
            return Driver.FindElement(locator).Enabled;
        }
        catch { return false; }
    }

    // ── Scroll Actions ─────────────────────────────────────────────────────

    /// <summary>Scroll element into the visible viewport.</summary>
    protected void ScrollIntoView(IWebElement element)
    {
        ((IJavaScriptExecutor)Driver)
            .ExecuteScript("arguments[0].scrollIntoView({block:'center'});", element);
    }

    /// <summary>Scroll to top of page.</summary>
    protected void ScrollToTop()
    {
        ((IJavaScriptExecutor)Driver)
            .ExecuteScript("window.scrollTo(0, 0);");
    }

    /// <summary>Scroll to bottom of page.</summary>
    protected void ScrollToBottom()
    {
        ((IJavaScriptExecutor)Driver)
            .ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
    }

    // ── Wait + Retry helpers ───────────────────────────────────────────────

    /// <summary>
    /// Retry an action up to N times with a delay between attempts.
    /// Swallows exceptions until the last attempt.
    /// </summary>
    protected void RetryAction(Action action, int maxAttempts = 0, int delayMs = 0)
    {
        int attempts = maxAttempts > 0 ? maxAttempts : Config.MaxRetryAttempts;
        int delay = delayMs > 0 ? delayMs : Config.RetryDelayMs;

        for (int i = 1; i <= attempts; i++)
        {
            try
            {
                action();
                return; // Success — exit
            }
            catch when (i < attempts)
            {
                Thread.Sleep(delay);
            }
        }
    }

    /// <summary>
    /// Wait for a loading spinner / overlay to disappear.
    /// Override the locator in derived classes for your ERP's specific spinner.
    /// </summary>
    protected virtual void WaitForLoader(By? loaderLocator = null)
    {
        loaderLocator ??= By.CssSelector(".loading-overlay, .spinner, [data-loading='true']");

        try
        {
            Wait.UntilInvisible(loaderLocator);
        }
        catch
        {
            // Loader may not appear at all — that's fine
        }
    }



    // ── Dropdown Actions ───────────────────────────────────────────────────

    protected void OpenDropdown(By locator)
    {
        IWebElement element = Driver.FindElement(locator);
        Click(element);
    }
    protected void SelectOption(By lookupText, By nextPage, string? optionText)
    {
        while (true)
        {
            var options = Driver.FindElements(lookupText);
            Thread.Sleep(1000);

            foreach (var option in options)
            {
                string actualValue = option.Text.Trim();
                if (!string.IsNullOrEmpty(optionText) && actualValue.Contains(optionText, StringComparison.OrdinalIgnoreCase))
                {
                    option.Click();
                    Thread.Sleep(2000);
                    return;
                }
            }
            Thread.Sleep(1000);
            var nextButton = Driver.FindElement(nextPage);
            Click(nextButton);
            Thread.Sleep(2000);
        }
    }
    /// <summary>Select from a standard HTML &lt;select&gt; by visible text.</summary>
    protected void SelectByText(By locator, string text)
    {
        IWebElement element = Wait.UntilVisible(locator);
        new SelectElement(element).SelectByText(text);
    }

    /// <summary>Select from a standard HTML &lt;select&gt; by value attribute.</summary>
    protected void SelectByValue(By locator, string value)
    {
        IWebElement element = Wait.UntilVisible(locator);
        new SelectElement(element).SelectByValue(value);
    }

    /// <summary>
    /// Select from a custom ERP dropdown (non-native select).
    /// Clicks the dropdown trigger, waits for options, then clicks the matching option.
    /// </summary>
    protected void SelectFromCustomDropdown(By triggerLocator, By optionLocator, string optionText)
    {
        Click(triggerLocator);
        Wait.UntilVisible(optionLocator);

        var options = Driver.FindElements(optionLocator);
        var match = options.FirstOrDefault(o =>
            o.Text.Trim().Equals(optionText, StringComparison.OrdinalIgnoreCase));

        if (match == null)
            throw new NoSuchElementException(
                $"[Dropdown] Option '{optionText}' not found in dropdown.");

        Click(match);
    }
}