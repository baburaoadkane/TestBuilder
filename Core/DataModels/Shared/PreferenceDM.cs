using System;
using System.Collections.Generic;
using System.Text;

namespace Enfinity.ERP.Automation.Core.DataModels.Shared
{
    public abstract class PreferenceDM
    {
        // ── Accounting Related ────────────────────────────────────
        public bool IsAllowMultiCurrencyInTxn { get; set; }
        public bool IsFinancialSegmentsEnabled { get; set; }

        // ── Bank Related ──────────────────────────────────────────
        public bool IsBankChargesEnabled { get; set; }
        public bool IsChequeBookEnabled { get; set; }
        public bool IsPDCChequeEnabled { get; set; }

        // ── General ───────────────────────────────────────────────
        public bool IsPricelistEnabled { get; set; }
        public bool IsChargesEnabled { get; set; }
    }
}
