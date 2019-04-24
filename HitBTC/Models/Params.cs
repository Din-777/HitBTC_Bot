using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HitBTC.Models
{
	public class ParamsTicker
	{
		[JsonProperty("symbol")]
		public string Symbol;		
	}
	
	public class ParamsAuth
	{
		[JsonProperty("algo")]
		public string Algo { get; internal set; }

		[JsonProperty("pKey")]
		public string PKey { get; internal set; }

		[JsonProperty("sKey")]
		public string Skey { get; internal set; }
	}

	public class ParamsBalance
	{
	}

}
