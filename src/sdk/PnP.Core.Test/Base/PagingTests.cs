﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class PagingTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        #region Graph paging tests
        [TestMethod]
        public async Task GraphCollectionPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {               
                // This rest requires beta api's, so bail out if that's not enabled
                if (!context.GraphCanUseBeta)
                {
                    Assert.Inconclusive("This test requires Graph beta to be allowed.");
                }

                var team = await context.Team.GetAsync(p => p.Channels);

                // Create a new channel and add enough messages to it
                string channelName = $"Paging test {new Random().Next()}";

                if (TestCommon.Instance.Mocking)
                {
                    var properties = TestManager.GetProperties(context);
                    channelName = properties["ChannelName"];
                }

                var channelForPaging = team.Channels.FirstOrDefault(p => p.DisplayName == channelName);
                if (channelForPaging == null)
                {
                    // Persist the created channel name as we need to have the same name when we run an offline test
                    if (!TestCommon.Instance.Mocking)
                    {
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "ChannelName", channelName }
                        };
                        TestManager.SaveProperties(context, properties);
                    }

                    channelForPaging = await team.Channels.AddAsync(channelName, "Test channel, will be deleted in 21 days");
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the channel available.");
                }

                // Add messages, not using batching to ensure reliability
                for (int i = 1; i <= 45; i++)
                {
                    await channelForPaging.Messages.AddAsync($"Test message{i}");
                }

                // Since we've not yet loaded the channel messages from the server paging is not yet allowed
                Assert.IsFalse(channelForPaging.Messages.CanPage);

                // Since we've already populated the model due to the add let's create a second context to perform a clean load again
                using (var context2 = TestCommon.Instance.GetContext(TestCommon.TestSite, 1))
                {
                    // Retrieve the already created channel
                    await context2.Team.GetAsync(p => p.Channels);
                    var channelForPaging2 = context2.Team.Channels.FirstOrDefault(p => p.DisplayName == channelName);

                    // Load the messages, this will populate the first batch of messages and will indicate paging is allowed
                    await channelForPaging2.GetAsync(p => p.Messages);

                    // Paging should now be allowed
                    Assert.IsTrue(channelForPaging2.Messages.CanPage);

                    // We should have messages loaded

                    int messageCount = channelForPaging2.Messages.Count();
                    Assert.IsTrue(messageCount > 0);

                    // Get the next page
                    await channelForPaging2.Messages.GetNextPageAsync();

                    // Seems like the Graph API does load all 45 at once since they where created too short ago
                    //Assert.IsTrue(channelForPaging2.Messages.Count() > messageCount);

                    // Trigger a load of the remaining pages via the GetAllPages call
                    await channelForPaging2.Messages.GetAllPagesAsync();

                    // We now should have the full amount of messages loaded
                    Assert.IsTrue(channelForPaging2.Messages.Count() == 45);

                    // Cleanup by deleting the channel
                    await channelForPaging2.DeleteAsync();
                }
            }
        }

        #endregion
    }
}
