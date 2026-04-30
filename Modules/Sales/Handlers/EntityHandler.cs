using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enfinity.ERP.Automation.Modules.Sales.Handlers
{
    public class EntityHandler : BaseHandler
    {
        public EntityHandler(IWebDriver driver, WaitHelper wait, ReportHelper report)
        : base(driver, wait, report) { }


        private void NavigateToEntity(string moduleName, string entityName)
        {
            By moduleButton = By.Id("AppModuleButton");

            By moduleLocator = By.XPath(
                $"//a[normalize-space()='{moduleName}'] | " +
                $"//li[normalize-space()='{moduleName}'] | " +
                $"//button[normalize-space()='{moduleName}']"
            );

            By entityLocator = By.XPath(
                $"//a[normalize-space()='{entityName}'] | " +
                $"//li[normalize-space()='{entityName}']"
            );

            try
            {
                // Open module menu
                Wait.UntilClickable(moduleButton, 5).Click();
                WaitForLoader();

                // Click module (e.g., Sales)
                Wait.UntilClickable(moduleLocator, 5).Click();
                WaitForLoader();

                // Click entity (e.g., Invoice)
                Wait.UntilClickable(entityLocator, 5).Click();
                WaitForLoader();
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new Exception(
                    $"Navigation failed for Module: {moduleName}, Entity: {entityName}", ex);
            }
        }
    }
}
