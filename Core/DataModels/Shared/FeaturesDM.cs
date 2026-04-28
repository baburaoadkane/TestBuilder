using System;
using System.Collections.Generic;
using System.Text;

namespace Enfinity.ERP.Automation.Core.DataModels.Shared
{
    public abstract class FeaturesDM
    {
        public bool IsPricelistEnabled { get; set; }

        public bool IsChargesEnabled { get; set; }
    }
}
