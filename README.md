# Enfinity.ERP.Automation

Scalable, modular, data-driven ERP test automation framework.
Built with Selenium WebDriver + NUnit + ExtentReports on .NET 8.

---

## Project Structure
```
Enfinity.ERP.Automation/
├── Core/                     ← Shared infrastructure (all modules use this)
│   ├── Base/                 ← BaseTest, BaseHandler, BaseExecutor, BaseValidator
│   ├── DataModels/Shared/    ← BaseDocumentDM, ExpectedResultDM, ExpectedTotalsDM
│   ├── Utilities/            ← DriverFactory, WaitHelper, JsonLoader, ReportHelper
│   └── Enums/                ← BrowserType, DocumentStatus, ERPModule, TestScenario
│
├── Modules/
│   └── Sales/
│       ├── DataModels/       ← SalesInvoiceDM, SalesOrderDM, SalesReturnDM
│       ├── Json/             ← Test data JSON files per document + scenario type
│       ├── Handlers/         ← HeaderHandler, LinesHandler, ChargesHandler, ...
│       ├── Executors/        ← SalesInvoiceExecutor
│       ├── Validators/       ← HeaderValidator, LinesValidator, TotalsValidator, ...
│       ├── Builders/         ← SalesInvoiceBuilder (fluent API)
│       └── TestCases/        ← SalesInvoiceTests, SalesOrderTests, SalesReturnTests
│
├── Config/
│   ├── appsettings.json
│   ├── appsettings.test.json
│   └── appsettings.staging.json
│
└── runsettings/
    └── test.runsettings
```

---

## Running Tests
```bash
# Run all tests
dotnet test

# Run only Smoke tests
dotnet test --filter "Category=Smoke"

# Run only Sales module
dotnet test --filter "Category=Sales"

# Run only Create scenarios
dotnet test --filter "Category=Create"

# Run only Sales Invoice Create scenarios
dotnet test --filter "Category=SalesInvoice&Category=Create"

# Run against Staging environment
ERP_ENV=Staging dotnet test

# Run headless (CI/CD)
dotnet test --settings runsettings/test.runsettings
```

---

## Adding a New Test Scenario

1. Create a JSON file in the correct folder:
   `Modules/Sales/Json/SalesInvoice/Create/SI_Create_YourScenario.json`

2. Run the tests — your new scenario is picked up automatically.
   **No code changes required.**

---

## Adding a New ERP Module (e.g. Purchase)

1. Create `Modules/Purchase/` with the same 7 subfolders
2. Add `PurchaseOrderDM.cs` extending `BaseDocumentDM`
3. Add `PurchaseOrderHeaderDM.cs`, `PurchaseOrderLineDM.cs` etc.
4. Create `Handlers/` extending `BaseHandler`
5. Create `PurchaseOrderExecutor.cs` extending `BaseExecutor<PurchaseOrderDM>`
6. Create `Validators/` extending `BaseValidator`
7. Create `PurchaseOrderBuilder.cs`
8. Create `PurchaseOrderTests.cs` extending `BaseTest`

The Core framework never changes — only new module files are added.

---

## Locator Update Checklist

Before running tests, update these locators to match your ERP's HTML:

| File | Locator | What to check |
|------|---------|---------------|
| `HeaderHandler.cs` | `CustomerInput` | Customer autocomplete input ID |
| `HeaderHandler.cs` | `InvoiceDateInput` | Invoice date field ID |
| `LinesHandler.cs` | `GetLineLocator()` | Lines indexed field ID pattern |
| `LinesHandler.cs` | `AddLineButton` | Add line button text or ID |
| `ChargesHandler.cs` | `GetChargeLocator()` | Charges indexed field ID pattern |
| `PaymentsHandler.cs` | `GetPaymentLocator()` | Payments indexed field ID pattern |
| `ExpectationHandler.cs` | `GrandTotalAmount` | Grand total display element ID |
| `ExpectationHandler.cs` | `SuccessToast` | Success notification CSS class |
| `LoginHelper.cs` | `UsernameField` | Login page username field ID |
| `BaseExecutor.cs` | `NewInvoiceRoute` | Sales Invoice new page URL route |

---

## Tech Stack

| Tool | Version | Purpose |
|------|---------|---------|
| .NET | 8.0 | Runtime |
| NUnit | 4.1 | Test framework |
| Selenium WebDriver | 4.21 | Browser automation |
| ExtentReports | 5.0 | HTML test reporting |
| System.Text.Json | 8.0 | JSON deserialization |