using System;
using DurableLoans.DomainModel;

namespace DurableLoans.LoanOffice.InboxProcessor
{
    public class LoanApplicationResultRecord
    {
        internal const string DEFAULT_PARTITION_KEY = "default";

        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }
        public string PartitionKey { get; set; } = DEFAULT_PARTITION_KEY;
        public LoanApplicationResult LoanApplication { get; set; }
    }
}
