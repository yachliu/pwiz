using AutoQC;
using AutoQC.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoQCTest
{
    [TestClass]
    public class AutoQcLoaderTest: AbstractAutoQcFunctionalTest
    {
        [TestMethod]
        public void CreateConfigsTest()
        {
            TestFilesZip = @"AutoQCTest\AutoQCLoaderTest.zip";
            RunFunctionalTest();
        }

        protected override void DoTest()
        {
            var mainForm = Program.MainWindow;
            Assert.AreEqual(0, mainForm.ConfigCount());

            var newConfigForm = ShowDialog<AutoQcConfigForm>(() => mainForm.ShowNewConfigForm());

            RunUI(() =>
            {
                PopulateNewConfigForm(newConfigForm);
                newConfigForm.Save();
            });
            WaitForClosedForm(newConfigForm);
            Assert.AreEqual(1, mainForm.ConfigCount());
            
            newConfigForm = ShowDialog<AutoQcConfigForm>(() => Program.MainWindow.ShowNewConfigForm());
            RunUI(() =>
            {
                PopulateNewConfigForm(newConfigForm);
            });

            RunDlg<AlertDlg>(() => newConfigForm.Save(),
                dlg =>
                {
                    Assert.AreEqual(Resources.AutoQcConfigForm_ValidateConfigName_A_configuration_with_this_name_already_exists_,
                        dlg.Message);
                    dlg.OkDialog();
                });

            RunUI(() => { newConfigForm.CancelButton.PerformClick(); });
            WaitForClosedForm(newConfigForm);
        }

        private void PopulateNewConfigForm(AutoQcConfigForm newConfigForm)
        {
            newConfigForm.ConfigName = @"This is a test config";
            newConfigForm.SkylineFilePath = TestFilesDir.GetTestPath(@"QEP_2015_0424_RJ.sky");
            newConfigForm.FolderToWatch = TestFilesDir.FullPath;
        }
    }
}