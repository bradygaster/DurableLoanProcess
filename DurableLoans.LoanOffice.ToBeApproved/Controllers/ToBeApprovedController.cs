using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DurableLoans.LoanOffice.ToBeApproved
{
    [Route("[controller]")]
    public class ToBeApprovedController : Controller
    {
        public ToBeApprovedController(ILogger<ToBeApprovedController> logger,
            DataLayer dataLayer)
        {
            Logger = logger;
            DataLayer = dataLayer;
        }

        public ILogger<ToBeApprovedController> Logger { get; }
        public DataLayer DataLayer { get; }

        [HttpGet]
        public IEnumerable<LoanApplicationResultRecord> Get()
        {
            Logger.LogInformation("Getting all of the to-be-approved loans");
            return DataLayer.GetInbox();
        }

        [HttpPost]
        public async Task<bool> Post([FromBody] LoanApprovalRequest request)
        {
            Logger.LogInformation($"Deleting loan {request.Id}");
            var success = await DataLayer.DeleteRecord(request.Id);
            if(success)
            {
                Logger.LogInformation($"Deleted loan {request.Id}");
            }

            return success;
        }
    }

    public class LoanApprovalRequest
    {
        public string Id { get; set; }
        public bool IsApproved { get; set; }
    }
}
