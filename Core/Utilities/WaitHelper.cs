using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Enfinity.ERP.Automation.Core.Utilities;

/// <summary>
/// Centralized explicit wait helper.
/// 
/// All Handlers and Validators use this — never raw Thread.Sleep.
/// Every wait method has a configurable timeout with a sensible default.
/// 
/// Usage:
///   var wait = new WaitHelper(driver, 30);
///   IWebElement el = wait.UntilVisible(By.Id("customer"));
///   wait.UntilInvisible(By.CssSelector(".spinner"));
/// </summary>
public class WaitHelper
{
    private readonly IWebDriver _driver;
    private readonly int _defaultTimeout;

    public WaitHelper(IWebDriver driver, int defaultTimeoutSeconds)
    {
        _driver = driver;
        _defaultTimeout = defaultTimeoutSeconds;
    }

    public async Task WaitForSeconds(int seconds)
    {
        await Task.Delay(seconds * 1000);
    }

    // ── Element presence ───────────────────────────────────────────────────

    /// <summary>Wait until element is visible in the DOM and on screen.</summary>
    public IWebElement UntilVisible(By locator, int? timeoutSeconds = null)
    {
        return GetWait(timeoutSeconds).Until(driver =>
        {
            try
            {
                var el = driver.FindElement(locator);
                return el.Displayed ? el : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });
    }

    /// <summary>Wait until element exists in DOM (may not be visible).</summary>
    public IWebElement UntilPresent(By locator, int? timeoutSeconds = null)
    {
        return GetWait(timeoutSeconds).Until(driver =>
        {
            try
            {
                return driver.FindElement(locator);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });
    }

    /// <summary>Wait until element is invisible or removed from DOM.</summary>
    public void UntilInvisible(By locator, int? timeoutSeconds = null)
    {
        GetWait(timeoutSeconds).Until(driver =>
        {
            try
            {
                var elements = driver.FindElements(locator);
                if (elements.Count == 0) return true;
                foreach (var el in elements)
                {
                    if (el.Displayed) return false;
                }
                return true;
            }
            catch (StaleElementReferenceException)
            {
                return true;
            }
        });
    }

    // ── Clickability ───────────────────────────────────────────────────────

    /// <summary>Wait until element is visible AND enabled (ready to click).</summary>
    public IWebElement UntilClickable(By locator, int? timeoutSeconds = null)
    {
        return GetWait(timeoutSeconds).Until(driver =>
        {
            try
            {
                var el = driver.FindElement(locator);
                return (el.Displayed && el.Enabled) ? el : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });
    }

    /// <summary>Wait until a located element is clickable.</summary>
    public IWebElement UntilElementClickable(IWebElement element, int? timeoutSeconds = null)
    {
        return GetWait(timeoutSeconds).Until(driver =>
        {
            try
            {
                return (element.Displayed && element.Enabled) ? element : null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        });
    }

    // ── Text conditions ────────────────────────────────────────────────────

    /// <summary>Wait until element contains specific text.</summary>
    public void UntilTextPresent(By locator, string text, int? timeoutSeconds = null)
    {
        GetWait(timeoutSeconds).Until(driver =>
        {
            try
            {
                var el = driver.FindElement(locator);
                return !string.IsNullOrEmpty(el.Text) && el.Text.Contains(text);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        });
    }

    /// <summary>Wait until an input field has a non-empty value.</summary>
    public void UntilValuePresent(By locator, string value, int? timeoutSeconds = null)
    {
        GetWait(timeoutSeconds).Until(driver =>
        {
            try
            {
                var el = driver.FindElement(locator);
                var val = el.GetAttribute("value");
                return !string.IsNullOrEmpty(val) && val.Contains(value);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        });
    }

    // ── URL conditions ─────────────────────────────────────────────────────

    /// <summary>Wait until URL contains a specific substring.</summary>
    public void UntilUrlContains(string urlFragment, int? timeoutSeconds = null)
    {
        GetWait(timeoutSeconds).Until(driver => driver.Url != null && driver.Url.Contains(urlFragment));
    }

    /// <summary>Wait until URL matches exactly.</summary>
    public void UntilUrlIs(string url, int? timeoutSeconds = null)
    {
        GetWait(timeoutSeconds).Until(driver => driver.Url == url);
    }

    // ── Page load ──────────────────────────────────────────────────────────

    /// <summary>Wait until document.readyState === 'complete'.</summary>
    public void UntilPageLoaded(int? timeoutSeconds = null)
    {
        GetWait(timeoutSeconds).Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState")
                ?.ToString() == "complete"
        );
    }

    // ── Multiple elements ──────────────────────────────────────────────────

    /// <summary>Wait until at least one element matching locator is present.</summary>
    public IReadOnlyList<IWebElement> UntilAllVisible(By locator, int? timeoutSeconds = null)
    {
        return GetWait(timeoutSeconds).Until(driver =>
        {
            var elements = driver.FindElements(locator);
            var visible = elements.Where(e => e.Displayed).ToList();
            return visible.Count > 0 ? (IReadOnlyList<IWebElement>)visible : null;
        });
    }

    // ── Custom condition ───────────────────────────────────────────────────

    /// <summary>
    /// Wait for any custom condition expressed as a Func.
    /// Example: wait.Until(driver => driver.FindElements(locator).Count > 3)
    /// </summary>
    public TResult Until<TResult>(Func<IWebDriver, TResult> condition, int? timeoutSeconds = null)
    {
        return GetWait(timeoutSeconds).Until(condition);
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private WebDriverWait GetWait(int? timeoutSeconds)
    {
        int timeout = timeoutSeconds ?? _defaultTimeout;

        return new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout))
        {
            PollingInterval = TimeSpan.FromMilliseconds(300),
            Message = $"Element not found within {timeout} seconds."
        };
    }
}