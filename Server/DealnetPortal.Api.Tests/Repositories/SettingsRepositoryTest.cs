using System;
using System.IO;
using DealnetPortal.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using DealnetPortal.DataAccess.Repositories;

namespace DealnetPortal.Api.Tests.Repositories
{
    [TestClass]
    public class SettingsRepositoryTest : BaseRepositoryTest
    {
        protected ISettingsRepository _settingsRepository;
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory",
                Path.Combine(context.TestDeploymentDir, string.Empty));
        }

        [TestInitialize]
        public void Initialize()
        {
            InitializeTestDatabase();
            InitUserSettings();
            _settingsRepository = new SettingsRepository(_databaseFactory);
        }

        private void InitUserSettings()
        {
            var setting1 = new SettingKey() {Key = "Setting1"};
            var setting2 = new SettingKey() { Key = "Setting2" };
            var keysList = new List<SettingKey> { setting1, setting2 };
            _databaseFactory.Get().SettingKeys.AddRange(keysList);
            _unitOfWork.Save();

            var userSettings = _databaseFactory.Get().UserSettings.Add(new UserSettings()
            {
                SettingValues = new List<SettingValue>()
                {
                    new SettingValue() { Key = setting1, Value = "Value1"},
                    new SettingValue() { Key = setting2, Value = "Value2"},
                }
            });
            _user.Settings = userSettings;
            _unitOfWork.Save();
        }

        [TestMethod]
        public void TestGetUserSettings()
        {
            var userSettings = _settingsRepository.GetUserSettings(_user.Id);
            Assert.IsNotNull(userSettings);
            Assert.IsNotNull(userSettings.SettingValues);
            Assert.AreEqual(userSettings.SettingValues.Count, 2);
        }
    }
}
