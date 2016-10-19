using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Utilities;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Web.IntegrationTests.ServiceAgents
{
    [TestClass]
    public class ContractServiceAgentTest
    {
        private IHttpApiClient _client;
        private Mock<ILoggingService> _loggingService;
        private const string DefUserName = "user@user.com";
        private const string DefUserPassword = "123_Qwe";

        [TestInitialize]
        public void Intialize()
        {
            _loggingService = new Mock<ILoggingService>();

            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            _client = new HttpApiClient(baseUrl);
        }

        [TestMethod]
        public void TestCreateContractForNotAutorizedUser()
        {
            IContractServiceAgent contractServiceAgent = new ContractServiceAgent(_client, _loggingService.Object);
            var result = contractServiceAgent.CreateContract().GetAwaiter().GetResult();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Item2);
            Assert.AreEqual(result.Item2.Count, 1);
            Assert.AreEqual(result.Item2.First().Type, AlertType.Error);
        }

        [TestMethod]
        public async Task TestCreateContractForAutorizedUser()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            var authResult = await securityServiceAgent.Authenicate(DefUserName, DefUserPassword);
            securityServiceAgent.SetAuthorizationHeader(authResult.Item1);

            IContractServiceAgent contractServiceAgent = new ContractServiceAgent(_client, _loggingService.Object);
            var result = await contractServiceAgent.CreateContract();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Item1);
            Assert.IsNotNull(result.Item2);
            Assert.AreEqual(result.Item2.Count, 0);
        }

        [TestMethod]
        public async Task TestGetContract()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            var authResult = await securityServiceAgent.Authenicate(DefUserName, DefUserPassword);
            securityServiceAgent.SetAuthorizationHeader(authResult.Item1);

            // Create a contract
            IContractServiceAgent contractServiceAgent = new ContractServiceAgent(_client, _loggingService.Object);
            var contractResult = await contractServiceAgent.CreateContract();
            Assert.IsNotNull(contractResult);
            Assert.IsNotNull(contractResult.Item1);

            // Try to get contract with same Id
            contractResult = await contractServiceAgent.GetContract(contractResult.Item1.Id);
            Assert.IsNotNull(contractResult);
            Assert.IsNotNull(contractResult.Item1);

            // Try to get lists of contracts
            var contracts = await contractServiceAgent.GetContracts();
            Assert.IsNotNull(contracts);
            Assert.IsTrue(contracts.Count > 0);
        }

        [TestMethod]
        public async Task TestUpdateContract()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            var authResult = await securityServiceAgent.Authenicate(DefUserName, DefUserPassword);
            securityServiceAgent.SetAuthorizationHeader(authResult.Item1);

            // Create a contract
            IContractServiceAgent contractServiceAgent = new ContractServiceAgent(_client, _loggingService.Object);
            var contractResult = await contractServiceAgent.CreateContract();
            Assert.IsNotNull(contractResult);
            Assert.IsNotNull(contractResult.Item1);
            var contract = contractResult.Item1;
            var contractData = new ContractDataDTO()
            { 
                Id = contract.Id,
                PrimaryCustomer =
                    new CustomerDTO()
                    {
                        FirstName = "FirstName",
                        LastName = "LastName",
                        DateOfBirth = DateTime.Now
                    }
            };
            var updateAlerts = await contractServiceAgent.UpdateContractData(contractData);
            Assert.IsNotNull(updateAlerts);
            Assert.IsTrue(updateAlerts.All(a => a.Type != AlertType.Error));
            var updatedContractResult = await contractServiceAgent.GetContract(contract.Id);
            Assert.IsNotNull(updatedContractResult);
            Assert.IsNotNull(updatedContractResult.Item1);
            var updatedContract = updatedContractResult.Item1;
            Assert.AreEqual(updatedContract.ContractState, ContractState.CustomerInfoInputted);
            Assert.IsNotNull(updatedContract.PrimaryCustomer);
        }

        [TestMethod]
        public async Task TestInitiateCreditCheck()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            var authResult = await securityServiceAgent.Authenicate(DefUserName, DefUserPassword);
            securityServiceAgent.SetAuthorizationHeader(authResult.Item1);

            // Create a contract
            IContractServiceAgent contractServiceAgent = new ContractServiceAgent(_client, _loggingService.Object);
            var contractResult = await contractServiceAgent.CreateContract();
            Assert.IsNotNull(contractResult);
            Assert.IsNotNull(contractResult.Item1);
            var checkAlerts = await contractServiceAgent.InitiateCreditCheck(contractResult.Item1.Id);
            // should be an error result, as we can't perfom credit check with contract without client data
            Assert.IsNotNull(checkAlerts);
            Assert.IsTrue(checkAlerts.First().Type == AlertType.Error);
        }

        [TestMethod]
        public async Task TestGetCreditCheckResult()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            var authResult = await securityServiceAgent.Authenicate(DefUserName, DefUserPassword);
            securityServiceAgent.SetAuthorizationHeader(authResult.Item1);

            // Create a contract
            IContractServiceAgent contractServiceAgent = new ContractServiceAgent(_client, _loggingService.Object);
            var contractResult = await contractServiceAgent.CreateContract();
            Assert.IsNotNull(contractResult);
            Assert.IsNotNull(contractResult.Item1);
            var checkAlerts = await contractServiceAgent.GetCreditCheckResult(contractResult.Item1.Id);
            // should be an error result, as we didn't start credit check before
            Assert.IsNotNull(checkAlerts);
            Assert.IsNotNull(checkAlerts.Item2);
            Assert.IsTrue(checkAlerts.Item2.First().Type == AlertType.Error);
        }

        [TestMethod]
        public async Task TestAddRemoveContractComment()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            var authResult = await securityServiceAgent.Authenicate(DefUserName, DefUserPassword);
            securityServiceAgent.SetAuthorizationHeader(authResult.Item1);

            // Create a contract
            // There must be no comments for it at this point in order to do testing
            IContractServiceAgent contractServiceAgent = new ContractServiceAgent(_client, _loggingService.Object);
            var contractResult = await contractServiceAgent.CreateContract();

            //Adding comments
            var comment1 = new CommentDTO();
            comment1.Text = "Comment number 1";
            comment1.ContractId = contractResult.Item1.Id;
            var addAlerts = await contractServiceAgent.AddComment(comment1);
            Assert.IsNotNull(addAlerts);
            Assert.IsTrue(addAlerts.All(a => a.Type != AlertType.Error));
            var comments = (await contractServiceAgent.GetContract(contractResult.Item1.Id)).Item1.Comments;
            var updatedComment = comments.First();
            var comment2 = new CommentDTO();
            comment2.Text = "Comment number 2";
            comment2.CommentId = updatedComment.Id;
            addAlerts = await contractServiceAgent.AddComment(comment2);
            Assert.IsNotNull(addAlerts);
            Assert.IsTrue(addAlerts.All(a => a.Type != AlertType.Error));

            //Removing comments with replies
            comments = (await contractServiceAgent.GetContract(contractResult.Item1.Id)).Item1.Comments;
            var removeAlerts = await contractServiceAgent.RemoveComment(comments.First());
            Assert.IsNotNull(removeAlerts);
            Assert.IsTrue(removeAlerts.Any(x => x.Header == ErrorConstants.CommentUpdateFailed));
            //Removing comments without replies 
            removeAlerts = await contractServiceAgent.RemoveComment(comments.First().Replies.First());
            Assert.IsNotNull(removeAlerts);
            Assert.IsTrue(removeAlerts.All(a => a.Type != AlertType.Error));
        }
    }
}
