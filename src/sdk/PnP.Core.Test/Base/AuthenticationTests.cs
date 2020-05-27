﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using PnP.Core.Model;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on Authentication specifics
    /// </summary>
    [TestClass]
    public class AuthenticationTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext testContext)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            // TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task TestGraphAccessToken()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var accessToken = await context.AuthenticationProvider.GetAccessTokenAsync(
                    PnPConstants.MicrosoftGraphBaseUri, 
                    new string[] { "Group.ReadWrite.All" }).ConfigureAwait(true);

                Assert.IsNotNull(accessToken);
            }
        }

        [TestMethod]
        public async Task TestSPOAccessToken()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var accessToken = await context.AuthenticationProvider.GetAccessTokenAsync(
                    context.Uri, null).ConfigureAwait(true);

                Assert.IsNotNull(accessToken);
            }
        }
    }
}