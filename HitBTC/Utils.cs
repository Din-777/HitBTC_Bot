using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitBTC
{
	public class Utils
	{
		public static string GenerateId() => Guid.NewGuid().ToString()
									.Replace("=",  "")
									.Replace("+",  "")
									.Replace(@"\", "")
									.Replace(@"/", "")
									.Replace("-",  "");

	}
}
