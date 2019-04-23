using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HitBTC.Models
{
	public class Params
	{
		[JsonProperty("symbol")]
		public string Symbol;
	}
}
