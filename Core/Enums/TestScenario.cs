using System;
using System.Collections.Generic;
using System.Text;

namespace Enfinity.ERP.Automation.Core.Enums
{
    /// <summary>
    /// Test scenario type — maps directly to the JSON subfolder name.
    /// Create → Data/Create/, Negative → Data/Negative/, etc.
    /// </summary>
    public enum TestScenario
    {
        Create,
        Edit,
        Negative,
        Approval,
        Validation,
        Delete
    }
}
