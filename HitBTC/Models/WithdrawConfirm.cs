using Newtonsoft.Json;

namespace HitBTC.Models
{
    public class WithdrawConfirm
    {
        [JsonProperty("result")]
        public bool Result { set; get; }
    }
}
