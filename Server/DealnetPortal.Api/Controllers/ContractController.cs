using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Controllers
{
    public class ContractController : BaseApiController
    {
        private IContractRepository ContractRepository { get; set; }

        public ContractController(ILoggingService loggingService, IContractRepository contractRepository)
            : base(loggingService)
        {
            ContractRepository = contractRepository;
        }

        //Get: api/Contract/
        [HttpGet]
        public IHttpActionResult GetContracts()
        {
            throw new NotImplementedException();
        }

        //Get: api/Contract/{contract}
        [HttpGet]
        public IHttpActionResult GetContract(int contractId)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        public IHttpActionResult CreateContract()
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        public IHttpActionResult UpdateContract(ContractDTO contractData)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public IHttpActionResult GetContractData(int contractId)
        {
            throw new NotImplementedException();
        }

    }
}
