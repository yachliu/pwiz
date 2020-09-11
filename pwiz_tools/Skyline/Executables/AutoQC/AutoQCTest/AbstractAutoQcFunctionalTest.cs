using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AutoQC;
using AutoQC.Properties;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pwiz.Common.Controls;
using pwiz.Common.SystemUtil;
using pwiz.Skyline.Util;
using pwiz.SkylineTestUtil;

namespace AutoQCTest
{
    /// <summary>
    /// All functional tests MUST derive from this base class.
    /// </summary>
    public abstract class AbstractAutoQcFunctionalTest : AbstractFunctionalTest
    {
        private const int SLEEP_INTERVAL = 100;
        public new const int WAIT_TIME = 60 * 1000;    // 60 seconds
        
        private bool _testCompleted;

        public static MainForm MainWindow => Program.MainWindow;

        /// <summary>
        /// Starts up AutoQCLoader, and runs the <see cref="AbstractFunctionalTest.DoTest"/> test method.
        /// </summary>
        protected void RunFunctionalTest()
        {
            bool firstTry = true;
            // Be prepared to re-run test in the event that a previously downloaded data file is damaged or stale
            for (; ; )
            {
                try
                {
                    RunFunctionalTestOrThrow();
                }
                catch (Exception x)
                {
                    Program.AddTestException(x);
                }

                // Delete unzipped test files.
                if (TestFilesDirs != null)
                {
                    foreach (TestFilesDir dir in TestFilesDirs)
                    {
                        try
                        {
                            dir?.Dispose();
                        }
                        catch (Exception x)
                        {
                            Program.AddTestException(x);
                            FileStreamManager.Default.CloseAllStreams();
                        }
                    }
                }

                if (firstTry && Program.TestExceptions.Count > 0 && RetryDataDownloads)
                {
                    try
                    {
                        if (FreshenTestDataDownloads())
                        {
                            firstTry = false;
                            Program.TestExceptions.Clear();
                            continue;
                        }
                    }
                    catch (Exception xx)
                    {
                        Program.AddTestException(xx); // Some trouble with data download, make a note of it
                    }
                }


                if (Program.TestExceptions.Count > 0)
                {
                    //Log<AbstractFunctionalTest>.Exception(@"Functional test exception", Program.TestExceptions[0]);
                    const string errorSeparator = "------------------------------------------------------";
                    Assert.Fail("{0}{1}{2}{3}",
                        Environment.NewLine + Environment.NewLine,
                        errorSeparator + Environment.NewLine,
                        Program.TestExceptions[0],
                        Environment.NewLine + errorSeparator + Environment.NewLine);
                }
                break;
            }

            if (!_testCompleted)
            {
                //Log<AbstractFunctionalTest>.Fail(@"Functional test did not complete");
                Assert.Fail("Functional test did not complete");
            }
        }

        protected void RunFunctionalTestOrThrow()
        {
            Program.FunctionalTest = true;
            Program.TestExceptions = new List<Exception>();
            LocalizationHelper.InitThread();

            // Unzip test files.
            if (TestFilesZipPaths != null)
            {
                TestFilesDirs = new TestFilesDir[TestFilesZipPaths.Length];
                for (int i = 0; i < TestFilesZipPaths.Length; i++)
                {
                    TestFilesDirs[i] = new TestFilesDir(TestContext, TestFilesZipPaths[i], TestDirectoryName,
                        TestFilesPersistent, IsExtractHere(i));
                }
            }

            // Run test in new thread (Skyline on main thread).
            // Program.Init();
            InitializeAutoQcSettings();

            var threadTest = new Thread(WaitForMainWindow) { Name = @"Functional test thread" };
            LocalizationHelper.InitThread(threadTest);
            threadTest.Start();
            Program.Main();
            threadTest.Join();

            // Were all windows disposed?
            FormEx.CheckAllFormsDisposed();
            CommonFormEx.CheckAllFormsDisposed();
        }

        /// <summary>
        /// Reset the settings for the application before starting a test.
        /// Tests can override this method if they have have any settings that need to
        /// be set before the test's DoTest method gets called.
        /// </summary>
        protected void InitializeAutoQcSettings()
        {
            Settings.Default.Reset();
        }

        private static int GetWaitCycles(int millis = WAIT_TIME)
        {
            int waitCycles = millis / SLEEP_INTERVAL;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // When debugger is attached, some vendor readers are S-L-O-W!
                waitCycles *= 10;
            }

            // Wait a little longer for debug build. (This may also imply code coverage testing, slower yet)
            if (ExtensionTestContext.IsDebugMode)
            {
                waitCycles = waitCycles * 4;
            }

            return waitCycles;
        }

        protected new static TDlg ShowDialog<TDlg>([InstantHandle] Action act, int millis = -1) where TDlg : Form
        {
            var existingDialog = FindOpenForm<TDlg>();
            if (existingDialog != null)
            {
                var alertDlg = existingDialog as AlertDlg;
                if (alertDlg == null)
                    AssertEx.IsNull(existingDialog, typeof(TDlg) + " is already open");
                else
                    Assert.Fail(typeof(TDlg) + " is already open with the message: " + alertDlg.Message);
            }

            AppBeginInvoke(act);
            TDlg dlg;
            if (millis == -1)
                dlg = WaitForOpenForm<TDlg>();
            else
                dlg = WaitForOpenForm<TDlg>(millis);
            Assert.IsNotNull(dlg);

            return dlg;
        }

        public new static TDlg WaitForOpenForm<TDlg>(int millis = WAIT_TIME) where TDlg : Form
        {
            var result = TryWaitForOpenForm<TDlg>(millis);
            if (result == null)
            {
                int waitCycles = GetWaitCycles(millis);
                Assert.Fail(@"Timeout {0} seconds exceeded in WaitForOpenForm({1}). Open forms: {2}", waitCycles * SLEEP_INTERVAL / 1000, typeof(TDlg).Name, GetOpenFormsString());
            }
            return result;
        }

        private static string GetOpenFormsString()
        {
            var result = string.Join(", ", OpenForms.Select(form => string.Format("{0} ({1})", form.GetType().Name, GetTextForForm(form))));
            // Without line numbers, this isn't terribly useful.  Disable for now.
            // result += GetAllThreadsStackTraces();
            return result;
        }

        private static string GetTextForForm(Control form)
        {
            var result = form.Text;
            var threadExceptionDialog = form as ThreadExceptionDialog;
            if (threadExceptionDialog != null)
            {
                // Locate the details text box, return the contents - much more informative than the dialog title
                result = threadExceptionDialog.Controls.Cast<Control>()
                    .Where(control => control is TextBox)
                    .Aggregate(result, (current, control) => current + ": " + GetExceptionText(control));
            }

            FormEx formEx = form as FormEx;
            if (formEx != null)
            {
                String detailedMessage = formEx.DetailedMessage;
                if (detailedMessage != null)
                {
                    result = detailedMessage;
                }
            }
            return result;
        }

        private static string GetExceptionText(Control control)
        {
            string text = control.Text;
            int assembliesIndex = text.IndexOf("************** Loaded Assemblies **************", StringComparison.Ordinal);
            if (assembliesIndex != -1)
                text = pwiz.Skyline.Util.Extensions.TextUtil.LineSeparate(text.Substring(0, assembliesIndex).Trim(), "------------- End ThreadExceptionDialog Stack -------------");
            return text;
        }

        public new static TDlg TryWaitForOpenForm<TDlg>(int millis = WAIT_TIME, Func<bool> stopCondition = null) where TDlg : Form
        {
            int waitCycles = GetWaitCycles(millis);
            for (int i = 0; i < waitCycles; i++)
            {
                Assert.IsFalse(Program.TestExceptions.Any(), "Exception while running test");

                var tForm = FindOpenForm<TDlg>();
                if (tForm != null)
                {
                    return tForm;
                }

                if (stopCondition != null && stopCondition())
                    break;

                Thread.Sleep(SLEEP_INTERVAL);
            }
            return null;
        }


        public new static void WaitForClosedForm(Form formClose)
        {
            int waitCycles = GetWaitCycles();
            for (int i = 0; i < waitCycles; i++)
            {
                Assert.IsFalse(Program.TestExceptions.Any(), "Exception while running test");

                bool isOpen = true;
                AppInvoke(() => isOpen = IsFormOpen(formClose));
                if (!isOpen)
                    return;
                Thread.Sleep(SLEEP_INTERVAL);
            }

            Assert.Fail(@"Timeout {0} seconds exceeded in WaitForClosedForm. Open forms: {1}", waitCycles * SLEEP_INTERVAL / 1000, GetOpenFormsString());
        }

        protected static void RunDlg<TDlg>(Action show, [InstantHandle] Action<TDlg> act = null, bool pause = false, int millis = -1) where TDlg : Form
        {
            RunDlg(show, false, act, pause, millis);
        }

        protected static void RunDlg<TDlg>(Action show, bool waitForDocument, Action<TDlg> act = null, bool pause = false, int millis = -1) where TDlg : Form
        {
            TDlg dlg = ShowDialog<TDlg>(show, millis);
            // if (pause)
            //     PauseTest();
            RunUI(() =>
            {
                if (act != null)
                    act(dlg);
                else
                    dlg.CancelButton.PerformClick();
            });
            WaitForClosedForm(dlg);
        }

        private void WaitForMainWindow()
        {
            try
            {
                int waitCycles = GetWaitCycles();
                for (int i = 0; i < waitCycles; i++)
                {
                    if (Program.MainWindow != null && Program.MainWindow.IsHandleCreated)
                        break;

                    Thread.Sleep(SLEEP_INTERVAL);
                }
                
                Assert.IsTrue(Program.MainWindow != null && Program.MainWindow.IsHandleCreated,
                    @"Timeout {0} seconds exceeded in WaitForSkyline", waitCycles * SLEEP_INTERVAL / 1000);
                
                RunTest();
            }
            catch (Exception x)
            {
                // Save exception for reporting from main thread.
                Program.AddTestException(x);
            }

            EndTest();

            Settings.Default.Reset();
        }

        private void RunTest()
        {
            // Use internal clipboard for testing so that we don't collide with other processes
            // using the clipboard during a test run.
            ClipboardEx.UseInternalClipboard();
            ClipboardEx.Clear();

            var doClipboardCheck = TestContext.Properties.Contains(@"ClipboardCheck");
            string clipboardCheckText = doClipboardCheck ? (string)TestContext.Properties[@"ClipboardCheck"] : String.Empty;
            if (doClipboardCheck)
            {
                RunUI(() => Clipboard.SetText(clipboardCheckText));
            }

            DoTest();
            if (doClipboardCheck)
            {
                RunUI(() => Assert.AreEqual(clipboardCheckText, Clipboard.GetText()));
            }
        }

        private void EndTest()
        {
            var appWindow = Program.MainWindow;
            if (appWindow == null || appWindow.IsDisposed || !IsFormOpen(appWindow))
            {
                return;
            }

            try
            {
                // TODO: Release all resources 
                // WaitForCondition(1000, () => !FileStreamManager.Default.HasPooledStreams, string.Empty, false);
                // if (FileStreamManager.Default.HasPooledStreams)
                // {
                //     // Just write to console to provide more information. This should cause a failure
                //     // trying to remove the test directory, which will provide a path to the problem file
                //     Console.WriteLine(TextUtil.LineSeparate("Streams left open:", string.Empty,
                //         FileStreamManager.Default.ReportPooledStreams()));
                // }

                if (Program.TestExceptions.Count == 0)
                {
                    WaitForConditionUI(5000, () => OpenForms.Count() == 1);
                }
            }
            catch (Exception x)
            {
                // An exception occurred outside RunTest
                Program.AddTestException(x);
            }

            CloseOpenForms(typeof(MainForm));

            _testCompleted = true;

            try
            {
                // Clear the clipboard to avoid the appearance of a memory leak.
                ClipboardEx.Release();
                // Occasionally this causes an InvalidOperationException during stress testing.
                RunUI(MainWindow.Close);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (System.ComponentModel.InvalidAsynchronousStateException)
            {
                // This gets thrown a lot during nightly tests under Windows 10
            }
            catch (InvalidOperationException)
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        public new static bool WaitForConditionUI(int millis, Func<bool> func, Func<string> timeoutMessage = null, bool failOnTimeout = true, bool throwOnProgramException = true)
        {
            int waitCycles = GetWaitCycles(millis);
            for (int i = 0; i < waitCycles; i++)
            {
                if (throwOnProgramException)
                    Assert.IsFalse(Program.TestExceptions.Any(), "Exception while running test");

                bool isCondition = false;
                Program.MainWindow.Invoke(new Action(() => isCondition = func()));
                if (isCondition)
                    return true;
                Thread.Sleep(SLEEP_INTERVAL);

                // Assistance in chasing down intermittent timeout problems
                if (i == waitCycles - 1 && LaunchDebuggerOnWaitForConditionTimeout)
                {
                    System.Diagnostics.Debugger.Launch(); // Try again, under the debugger
                    System.Diagnostics.Debugger.Break();
                    i = 0; // For debugging ease - stay in loop
                }
            }
            if (failOnTimeout)
            {
                var msg = string.Empty;
                if (timeoutMessage != null)
                    RunUI(() => msg = " (" + timeoutMessage() + ")");

                AssertEx.Fail(@"Timeout {0} seconds exceeded in WaitForConditionUI{1}. Open forms: {2}", waitCycles * SLEEP_INTERVAL / 1000, msg, GetOpenFormsString());
            }
            return false;
        }

        private static IEnumerable<Form> OpenForms
        {
            get
            {
                return FormUtil.OpenForms;
            }
        }

        private void CloseOpenForms(Type exceptType)
        {
            // Actually throwing an exception can cause an infinite loop in MSTest
            var openForms = OpenForms.Where(form => form.GetType() != exceptType).ToList();
            Program.TestExceptions.AddRange(
                from form in openForms
                select new AssertFailedException(
                    String.Format(@"Form of type {0} left open at end of test", form.GetType())));
            while (openForms.Count > 0)
                CloseOpenForm(openForms.First(), openForms);
        }

        private void CloseOpenForm(Form formToClose, List<Form> openForms)
        {
            openForms.Remove(formToClose);
            // Close any owned forms, since they may be pushing message loops that would keep this form
            // from closing.
            foreach (var ownedForm in formToClose.OwnedForms)
            {
                CloseOpenForm(ownedForm, openForms);
            }

            var messageDlg = formToClose as AlertDlg;
            // ReSharper disable LocalizableElement
            if (messageDlg == null)
                Console.WriteLine("\n\nClosing open form of type {0}\n", formToClose.GetType()); // Not L10N
            else
                Console.WriteLine("\n\nClosing open MessageDlg: {0}\n", TextUtil.LineSeparate(messageDlg.Message, messageDlg.DetailMessage)); // Not L10N
            // ReSharper restore LocalizableElement

            RunUI(() =>
            {
                try
                {
                    formToClose.Close();
                }
                catch
                {
                    // Ignore exceptions
                }
            });
        }

        protected new static void RunUI([InstantHandle] Action act)
        {
            AppInvoke(() =>
            {
                try
                {
                    act();
                }
                catch (Exception e)
                {
                    Assert.Fail(e.ToString());
                }
            });
        }

        private static void AppInvoke(Action act)
        {
            MainWindow?.Invoke(act);
        }

        private static void AppBeginInvoke(Action act)
        {
            MainWindow?.BeginInvoke(act);
        }
    }
}