using Newtonsoft.Json;

namespace HitBTC.Models
{
    public class AddressModel
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }
    }
}
