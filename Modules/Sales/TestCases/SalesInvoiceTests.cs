using Enfinity.ERP.Automation.Core.Base;
using Enfinity.ERP.Automation.Core.Utilities;
using Enfinity.ERP.Automation.Modules.Sales.Builders;
using Enfinity.ERP.Automation.Modules.Sales.Executors;

namespace Enfinity.ERP.Automation.Modules.Sales.TestCases;

[TestFixture]
[Category("Sales")]
[Category("SalesInvoice")]
public class SalesInvoiceTests : BaseTest
{
    // ── Executor — initialized per test in SetUp ───────────────────────────
    private SalesInvoiceExecutor _executor = null!;

    // ── JSON folder paths ──────────────────────────────────────────────────
    private const string CreateFolder = "Modules/Sales/Data/SalesInvoice/Create";
    private const string ApprovalFolder = "Modules/Sales/Data/SalesInvoice/Approve";
    private const string NegativeFolder = "Modules/Sales/Data/SalesInvoice/Negative";
    private const string EditFolder = "Modules/Sales/Data/SalesInvoice/Edit";
    private const string ValidationFolder = "Modules/Sales/Data/SalesInvoice/Validation";

    // ── SetUp ─────────────────────────────────────────────────────

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _executor = new SalesInvoiceExecutor(Driver, Wait, Report);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CREATE SCENARIOS
    // ══════════════════════════════════════════════════════════════════════

    [Test, Order(1)]
    [TestCaseSource(nameof(CreateScenarios))]
    [Category("Create")]
    public void SalesInvoice_Create_Multiline_ValidateTotal_Base(string jsonPath)
    {
        var data = SalesInvoiceBuilder.FromJson(jsonPath).Build();

        Report.Info($"Scenario: {data.TestDescription}");

        _executor.Execute(data);
    }

    // ══════════════════════════════════════════════════════════════════════
    // APPROVAL SCENARIOS
    // ══════════════════════════════════════════════════════════════════════

    [Test, Order(2)]
    [TestCaseSource(nameof(ApprovalScenarios))]
    [Category("Approval")]
    public void SalesInvoice_Approval_MultiLine_ValidateTotal_Base(string jsonPath)
    {
        var data = SalesInvoiceBuilder
            .FromJson(jsonPath)
            .WithApproval()
            .Build();

        Report.Info($"Scenario: {data.TestDescription}");

        _executor.Execute(data);
    }

    // ══════════════════════════════════════════════════════════════════════
    // NEGATIVE SCENARIOS
    // ══════════════════════════════════════════════════════════════════════

    //[Test]
    //[TestCaseSource(nameof(NegativeScenarios))]
    //[Category("Negative")]
    //public void SalesInvoice_Negative_ValidationError_Json(string jsonPath)
    //{
    //    var data = SalesInvoiceBuilder
    //        .FromJson(jsonPath)
    //        .AsScenario("Negative")
    //        .Build();

    //    Report.Info($"Scenario: {data.TestDescription}");
    //    Report.Info($"Expected Error: {data.Expected?.ErrorMessage}");

    //    _executor.Execute(data);
    //}

    // ══════════════════════════════════════════════════════════════════════
    // EDIT SCENARIOS
    // ══════════════════════════════════════════════════════════════════════

    //[Test]
    //[TestCaseSource(nameof(EditScenarios))]
    //[Category("Edit")]
    //public void SalesInvoice_Edit_Update_Json(string jsonPath)
    //{
    //    var data = SalesInvoiceBuilder
    //        .FromJson(jsonPath)
    //        .AsScenario("Edit")
    //        .Build();

    //    Report.Info($"Scenario: {data.TestDescription}");
    //    Report.Info($"Document: {data.DocumentNo}");

    //    _executor.Execute(data);
    //}

    // ══════════════════════════════════════════════════════════════════════
    // VALIDATION SCENARIOS
    // ══════════════════════════════════════════════════════════════════════

    //[Test]
    //[TestCaseSource(nameof(ValidationScenarios))]
    //[Category("Validation")]
    //public void SalesInvoice_Validation_ExpectedValues_Json(string jsonPath)
    //{
    //    var data = SalesInvoiceBuilder
    //        .FromJson(jsonPath)
    //        .AsScenario("Validation")
    //        .Build();

    //    Report.Info($"Scenario: {data.TestDescription}");
    //    Report.Info($"Document: {data.DocumentNo}");

    //    _executor.Execute(data);
    //}

    // ══════════════════════════════════════════════════════════════════════
    // SMOKE TESTS — programmatic, no JSON file needed
    // ══════════════════════════════════════════════════════════════════════

    //[Test]
    //[Category("Create")]
    //[Category("Smoke")]
    //public void SalesInvoice_Create_SingleLine_SmokeTest()
    //{
    //    var data = SalesInvoiceBuilder
    //        .New()
    //        .WithCustomer("C0002 | Minnah Elamin")
    //        .WithWarehouse("Grand Prime House")
    //        .WithReferenceNum("Smoke Test")
    //        .AddLine(
    //            barcode: "",
    //            item: "I0001 | Screen Protectors"
    //        )
    //        .AsScenario("Create")
    //        .Build();

    //    _executor.Execute(data);
    //}

    //[Test]
    //[Category("Approval")]
    //[Category("Smoke")]
    //public void SalesInvoice_Approval_FullFlow_SmokeTest()
    //{
    //    var data = SalesInvoiceBuilder
    //        .New()
    //        .WithCustomer("C0002 | Minnah Elamin")
    //        .WithWarehouse("Grand Prime House")
    //        .WithReferenceNum("Smoke Test With Approval")
    //        .AddLine(
    //            barcode: "",
    //            item: "I0001 | Screen Protectors"
    //        )
    //        .WithApproval()
    //        .Build();

    //    _executor.Execute(data);
    //}

    //[Test]
    //[Category("Negative")]
    //[Category("Smoke")]
    //public void SalesInvoice_Negative_MissingCustomer_SmokeTest()
    //{
    //    var data = SalesInvoiceBuilder
    //        .New()
    //        .AsScenario("Negative")
    //        .Build();

    //    data.Expected = new Core.DataModels.Shared.ExpectedResultDM
    //    {
    //        ValidationMessage = "Currency is required."
    //    };

    //    _executor.Execute(data);
    //}

    //[Test]
    //[Category("Negative")]
    //[Category("Smoke")]
    //public void SalesInvoice_Negative_MissingWarehouse_SmokeTest()
    //{
    //    var data = SalesInvoiceBuilder
    //        .New()
    //        .WithCustomer("C0002 | Minnah Elamin")
    //        .AsScenario("Negative")
    //        .Build();

    //    data.Expected = new Core.DataModels.Shared.ExpectedResultDM
    //    {
    //        ValidationMessage = "Warehouse is required."
    //    };

    //    _executor.Execute(data);
    //}

    // ══════════════════════════════════════════════════════════════════════
    // TEST CASE SOURCES
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Returns all JSON file paths from the Create folder.</summary>
    private static IEnumerable<TestCaseData> CreateScenarios()
        => BuildTestCases(CreateFolder);

    /// <summary>Returns all JSON file paths from the Approval folder.</summary>
    private static IEnumerable<TestCaseData> ApprovalScenarios()
        => BuildTestCases(ApprovalFolder);

    /// <summary>Returns all JSON file paths from the Negative folder.</summary>
    private static IEnumerable<TestCaseData> NegativeScenarios()
        => BuildTestCases(NegativeFolder);

    /// <summary>Returns all JSON file paths from the Edit folder.</summary>
    private static IEnumerable<TestCaseData> EditScenarios()
        => BuildTestCases(EditFolder);

    /// <summary>Returns all JSON file paths from the Validation folder.</summary>
    private static IEnumerable<TestCaseData> ValidationScenarios()
        => BuildTestCases(ValidationFolder);

    /// <summary>
    /// Discovers all JSON files in the given folder relative to the
    /// test output directory (AppContext.BaseDirectory).
    ///
    /// Called at DISCOVERY TIME by NUnit — must never throw,
    /// must never use TestContext (it is null during discovery).
    /// Returns empty silently if the folder has no files yet.
    /// </summary>
    /// 
    private static IEnumerable<TestCaseData> BuildTestCases(string folderPath)
    {
        IEnumerable<string> files;

        try
        {
            files = JsonLoader.GetAllFiles(folderPath);
        }
        catch
        {
            // Folder doesn't exist yet — return empty so suite doesn't fail
            yield break;
        }

        foreach (string filePath in files)
        {
            string testName = Path.GetFileNameWithoutExtension(filePath);

            yield return new TestCaseData(filePath)
                .SetName(testName)         // Shows filename as test name in report
                .SetDescription(testName); // Shows in NUnit test explorer
        }
    }
}