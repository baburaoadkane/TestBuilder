using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.Net.NetworkInformation;

namespace Enfinity.ERP.Automation.Core.Utilities;

/// <summary>
/// Wrapper around ExtentReports for structured HTML test reporting.
/// 
/// Usage:
///   ReportHelper.InitializeReport(...)   // Call once in [OneTimeSetUp]
///   var report = new ReportHelper(testName);
///   report.Info("Step done");
///   report.Pass("Assertion passed");
///   report.Fail("Assertion failed", screenshotPath);
///   ReportHelper.FlushReport();          // Call once in [OneTimeTearDown]
/// </summary>
public class ReportHelper
{
    // ── Static shared report instance ──────────────────────────────────────
    private static ExtentReports? _extentReports;
    private static readonly object _lock = new();

    // ── Per-test node ──────────────────────────────────────────────────────
    private readonly ExtentTest _test;

    // ── Initialize report — call ONCE before all tests ─────────────────────
    public static void InitializeReport(
        string outputPath,
        string reportTitle,
        string environment,
        string browser)
    {
        lock (_lock)
        {
            if (_extentReports != null) return;

            Directory.CreateDirectory(outputPath);

            string reportPath = Path.Combine(
                outputPath,
                $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"
            );

            var htmlReporter = new ExtentSparkReporter(reportPath);
            htmlReporter.Config.DocumentTitle = reportTitle;
            htmlReporter.Config.ReportName = $"{reportTitle} — Automation Report";
            htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Dark;

            _extentReports = new ExtentReports();
            _extentReports.AttachReporter(htmlReporter);
            _extentReports.AddSystemInfo("Environment", environment);
            _extentReports.AddSystemInfo("Browser", browser);
            _extentReports.AddSystemInfo("OS", Environment.OSVersion.ToString());
            _extentReports.AddSystemInfo("Machine", Environment.MachineName);
        }
    }

    // ── Constructor — creates a test node for one test ─────────────────────
    public ReportHelper(string testName)
    {
        if (_extentReports == null)
            throw new InvalidOperationException(
                "[ReportHelper] Report not initialized. Call InitializeReport() first.");

        _test = _extentReports.CreateTest(testName);
    }

    // ── Logging methods ────────────────────────────────────────────────────

    /// <summary>Log an informational step.</summary>
    public void Info(string message) =>
        _test.Log(Status.Info, message);

    /// <summary>Log a passing assertion.</summary>
    public void Pass(string message) =>
        _test.Log(Status.Pass, message);

    /// <summary>Log a failed assertion, with optional screenshot.</summary>
    public void Fail(string message, string? screenshotPath = null)
    {
        if (!string.IsNullOrEmpty(screenshotPath) && File.Exists(screenshotPath))
            _test.Fail(message, MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build());
        else
            _test.Log(Status.Fail, message);
    }

    /// <summary>Log a skipped test.</summary>
    public void Skip(string message) =>
        _test.Log(Status.Skip, message);

    /// <summary>Log a warning.</summary>
    public void Warning(string message) =>
        _test.Log(Status.Warning, message);

    /// <summary>Attach a screenshot without marking as pass/fail.</summary>
    public void AttachScreenshot(string screenshotPath)
    {
        if (File.Exists(screenshotPath))
            _test.AddScreenCaptureFromPath(screenshotPath);
    }

    // ── Flush — call ONCE after all tests ─────────────────────────────────
    public static void FlushReport()
    {
        lock (_lock)
        {
            _extentReports?.Flush();
        }
    }
}