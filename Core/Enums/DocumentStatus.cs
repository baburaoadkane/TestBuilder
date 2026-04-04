using System;
using System.Collections.Generic;
using System.Text;

namespace Enfinity.ERP.Automation.Core.Enums
{
    /// <summary>
    /// ERP document lifecycle statuses.
    /// Used by validators to assert the correct workflow state.
    /// </summary>
    public enum DocumentStatus
    {
        Draft,
        PendingApproval,
        Approved,
        Rejected,
        Cancelled,
        Closed,
        Posted
    }
}
