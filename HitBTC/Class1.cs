using System;

using System.Text;
using System.Net;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;
using System.Collections.Generic;
using RestSharp;
using RestSharp.Authenticators;

namespace HitBTC
{
	static public class Period
	{
		static public string M1 { get { return "M1"; } }
		static public string M3 { get { return "M3"; } }
		static public string M5 { get { return "M5"; } }
		static public string M15 { get { return "M15"; } }
		static public string M30 { get { return "M30"; } }
		static public string H1 { get { return "H1"; } }
		static public string H4 { get { return "H4"; } }
		static public string D1 { get { return "D1"; } }
		static public string D7 { get { return "D7"; } }
		static public string M { get { return "1M"; } }
	}

	static public class Pair
	{
		static public string BCHBTC { get { return "BCHBTC"; } }
		static public string BCCBTC { get { return "BCCBTC"; } }
		static public string ETHBTC { get { return "ETHBTC"; } }
		static public string LTCBTC { get { return "LTCBTC"; } }
		static public string XRPBTC { get { return "XRPBTC"; } }
		static public string BTGBTC { get { return "BTGBTC"; } }
		static public string XMRBTC { get { return "XMRBTC"; } }
		static public string NXTBTC { get { return "NXTBTC"; } }
		static public string BCNBTC { get { return "BCNBTC"; } }
		static public string EMCBTC { get { return "EMCBTC"; } }
		static public string DASHBTC { get { return "DASHBTC"; } }
		static public string DOGEBTC { get { return "DOGEBTC"; } }

		static public string TNTUSD { get { return "TNTUSD"; } }


		static public string BTCUSD { get { return "BTCUSD"; } }
		static public string BCCUSD { get { return "BCCUSD"; } }
		static public string BCHUSD { get { return "BCHUSD"; } }
		static public string EOSUSD { get { return "EOSUSD"; } }
		static public string ETHUSD { get { return "ETHUSD"; } }
		static public string LTCUSD { get { return "LTCUSD"; } }
		static public string EMCUSD { get { return "EMCUSDT"; } }
		static public string XRPUSD { get { return "XRPUSDT"; } }
		static public string BTGUSD { get { return "BTGUSD"; } }
		static public string XMRUSD { get { return "XMRUSD"; } }
		static public string NXTUSD { get { return "NXTUSD"; } }
		static public string NEOUSD { get { return "NEOUSD"; } }
		static public string BCNUSD { get { return "BCNUSD"; } }
		static public string ZECUSD { get { return "ZECUSD"; } }
		static public string EBTCUSD { get { return "EBTCNEWUSD"; } }
		static public string DASHUSD { get { return "DASHUSD"; } }
		static public string DOGEUSD { get { return "DOGEUSD"; } }

	}

	public class Candle
	{
		/* timestamp":"2017-12-07T05:00:00.000Z",
        "open":"0.030331",
        "close":"0.029836",
        "min":"0.029772",
        "max":"0.030353",
        "volume":"662.744",
        "volumeQuote":"19.907778902 */

		public DateTime timestamp { get; set; }
		public float open { get; set; }
		public float close { get; set; }
		public float min { get; set; }
		public float max { get; set; }
		public string volume { get; set; }
		public string volumeQuote { get; set; }
	}

	public struct CandleScale
	{
		public DateTime timestamp { get; set; }
		public float open { get; set; }
		public float close { get; set; }
		public float min { get; set; }
		public float max { get; set; }
		public string volume { get; set; }
		public string volumeQuote { get; set; }

		public float shadowUp { get; set; }
		public float shadowLo { get; set; }
		public float body { get; set; }
		public float shadow { get; set; }
		public float color { get; set; }
		public double percent { get; set; }
	}

	public class Scaling
	{
		public float min { get; set; }
		public float max { get; set; }

		public Candle[] candles { get; }
		public CandleScale[] candleScale { get; }

		private float d;
		public float k { get; }

		public Scaling(string candlesSours)
		{
			candles = JsonConvert.DeserializeObject<Candle[]>(candlesSours);

			candleScale = new CandleScale[candles.Length];

			min = candles[0].min;
			max = candles[0].max;

			foreach (Candle c in candles)
			{
				if (c.max > max) max = c.max;
				if (c.min < min) min = c.min;
			}

			d = max - min;
			k = 1.0f / d;

			for (int i = 0; i < candles.Length; i++)
			{
				if (i == 0)
				{
					candleScale[i].percent = 0.0;
				}
				else
				{
					candleScale[i].percent = candles[i].close / candles[i - 1].close;
				}

				candleScale[i].min = (candles[i].min - min) * k;
				candleScale[i].max = (candles[i].max - min) * k;
				candleScale[i].open = (candles[i].open - min) * k;
				candleScale[i].close = (candles[i].close - min) * k;


				float bd = candles[i].max - candles[i].min;
				float bk = 1.0f / bd;

				float scalMax = (candles[i].max - candles[i].min) * k;
				float scalMin = (candles[i].min - candles[i].min) * k;
				float scalOpen = (candles[i].open - candles[i].min) * k;
				float scalClose = (candles[i].close - candles[i].min) * k;

				candleScale[i].body = scalClose - scalOpen;
				candleScale[i].color = candleScale[i].body > 0.0f ? candleScale[i].color = 1.0f : candleScale[i].color = -1.0f;
				candleScale[i].shadowUp = candleScale[i].body > 0.0f ? scalMax - scalClose : scalMin - scalClose;
				candleScale[i].shadowLo = candleScale[i].body > 0.0f ? scalOpen : scalClose;
				candleScale[i].shadow = candleScale[i].shadowUp + candleScale[i].shadowLo;

				candleScale[i].timestamp = candles[i].timestamp;

			}
		}
	}

	public class Ticker
	{
		public float ask { get; set; }
		public float bid { get; set; }
		public float last { get; set; }
		public float open { get; set; }
		public float low { get; set; }
		public float high { get; set; }
		public float volume { get; set; }
		public float volumeQuote { get; set; }
		public DateTime timestamp { get; set; }
		public string symbol { get; set; }
	}

	public class Symbols
	{
		public string id { get; set; }                  // String  Symbol identifier.In the future, the description will simply use the symbol
		public string baseCurrency { get; set; }        // String
		public string quoteCurrency { get; set; }       // String
		public float quantityIncrement { get; set; }    // Number
		public float tickSize { get; set; }             // Number
		public float takeLiquidityRate { get; set; }    // Number Default fee rate
		public float provideLiquidityRate { get; set; } // Number  Default fee rate for market making trades
		public string feeCurrency { get; set; }         // String
	}

	public class Trade
	{
		public int id { get; set; }
		public float price { get; set; }
		public float quantity { get; set; }
		public string side { get; set; }
		public DateTime timestamp { get; set; }
	}


	public class Orderbook
	{
		/*	"ask": 
			[
				{
					"price": "0.046002",
					"size": "0.088"	
				},
				{
					"price": "0.046800",
					"size": "0.200"
				}
			],
			"bid": 
			[
				{
					"price": "0.046001",
					"size": "0.005"
				},
				{
					 "price": "0.046000",
					"size": "0.200"
				}
			]	  */
	}

	public class Balance
	{
		[JsonProperty("currency")]
		public string Currency { get; set; }    // String  Currency code

		[JsonProperty("available")]
		public float Available { get; set; }    // Number  Amount available for trading or transfer to main account

		[JsonProperty("reserved")]
		public decimal Reserved { get; set; }     // Number  Amount reserved for active orders or incomplete transfers to main account
	}

	public class HBTC
	{
		public string respons { get; set; }

		public Candle[] candles { get; private set; }
		public CandleScale[] candleScale { get; private set; }

		public List<Balance> Balance { get; private set; }


		public string Request(out List<Balance> Balance)
		{
			string key = "";
			string secret = "";

			string url = "https://api.hitbtc.com/api/2/";

			var client = new RestClient(url);
			client.Authenticator = new HttpBasicAuthenticator(key, secret);

			var request = new RestRequest("trading/balance", Method.GET);

			// execute the request
			IRestResponse response = client.Execute(request);
			var content = response.Content;

			Balance = JsonConvert.DeserializeObject<List<Balance>>(content);

			return content;
		}

		public string Request(out Trade[] trades, string pair, int limit, int from = 0)
		{
			string url = "https://api.hitbtc.com/api/2/public/trades/" + pair + "?sort=DESC";
			url += "&from=" + from + "&limit=" + limit;

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			// Get the stream associated with the response.
			Stream receiveStream = response.GetResponseStream();

			// Pipes the stream to a higher level stream reader with the required encoding format. 
			StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

			respons = readStream.ReadToEnd();

			response.Close();
			readStream.Close();

			trades = JsonConvert.DeserializeObject<Trade[]>(respons);

			return respons;
		}

		public string Request(out Ticker ticker, string pair)
		{
			string url = "https://api.hitbtc.com/api/2/public/ticker/" + pair;

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			// Get the stream associated with the response.
			Stream receiveStream = response.GetResponseStream();

			// Pipes the stream to a higher level stream reader with the required encoding format. 
			StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

			respons = readStream.ReadToEnd();

			response.Close();
			readStream.Close();

			ticker = JsonConvert.DeserializeObject<Ticker>(respons);

			return respons;
		}

		public string Request(out Ticker[] tickers)
		{
			string url = "https://api.hitbtc.com/api/2/public/ticker/";

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			// Get the stream associated with the response.
			Stream receiveStream = response.GetResponseStream();

			// Pipes the stream to a higher level stream reader with the required encoding format. 
			StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

			respons = readStream.ReadToEnd();

			response.Close();
			readStream.Close();

			tickers = JsonConvert.DeserializeObject<Ticker[]>(respons);

			return respons;
		}

		public string Request(out Symbols[] tickers)
		{
			string url = "https://api.hitbtc.com/api/2/public/symbol/";

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			// Get the stream associated with the response.
			Stream receiveStream = response.GetResponseStream();

			// Pipes the stream to a higher level stream reader with the required encoding format. 
			StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

			respons = readStream.ReadToEnd();

			response.Close();
			readStream.Close();

			tickers = JsonConvert.DeserializeObject<Symbols[]>(respons);

			return respons;
		}

		public string Request(string pair, string period, int limit)
		{
			string url = "https://api.hitbtc.com/api/2/public/candles/" + pair;

			url += "?limit=" + limit; url += "&period=" + period;

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			//Console.WriteLine("Content length is {0}", response.ContentLength);
			//Console.WriteLine("Content type is {0}", response.ContentType);

			// Get the stream associated with the response.
			Stream receiveStream = response.GetResponseStream();

			// Pipes the stream to a higher level stream reader with the required encoding format. 
			StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

			//Console.WriteLine("Response stream received.");
			//Console.WriteLine(readStream.ReadToEnd());
			respons = readStream.ReadToEnd();

			response.Close();
			readStream.Close();

			Scaling scaling = new Scaling(respons);

			candles = scaling.candles;
			candleScale = scaling.candleScale;

			return respons;
		}


	}
}
