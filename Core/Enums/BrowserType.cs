using System;
using System.Collections.Generic;
using System.Text;

namespace Enfinity.ERP.Automation.Core.Enums
{
    /// <summary>
    /// Supported browser types for WebDriver creation.
    /// Value is read from appsettings.json → Browser.Type
    /// </summary>
    public enum BrowserType
    {
        Chrome,
        Firefox,
        Edge
    }
}
