using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace HitBTC.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Side
    {
        [EnumMember(Value = "buy")]
        Buy,
        [EnumMember(Value = "sell")]
        Sell
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Sort
    {
        [EnumMember(Value = "DESC")]
        Desc,
        [EnumMember(Value = "ASC")]
        Asc
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Period
    {
        [EnumMember(Value = "M1")]
        M1,
        [EnumMember(Value = "M3")]
        M3,
        [EnumMember(Value = "M5")]
        M5,
        [EnumMember(Value = "M15")]
        M15,
        [EnumMember(Value = "M30")]
        M30,
        [EnumMember(Value = "H1")]
        H1,
        [EnumMember(Value = "H4")]
        H4,
        [EnumMember(Value = "D1")]
        D1,
        [EnumMember(Value = "D7")]
        D7,
        [EnumMember(Value = "1M")]
        Month1
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum RequestMethod
    {
        [EnumMember(Value = "getCurrency")]
        GetCurrency,
        [EnumMember(Value = "getCurrencies")]
        GetCurrencies,
        [EnumMember(Value = "getSymbol")]
        GetSymbol,
        [EnumMember(Value = "getSymbols")]
        GetSymbols,
        [EnumMember(Value = "subscribeTicker")]
        SubscribeTicker,
        [EnumMember(Value = "unsubscribeTicker")]
        UnsubscribeTicker,
        [EnumMember(Value = "subscribeOrderbook")]
        SubscribeOrderbook,
        [EnumMember(Value = "unsubscribeOrderbook")]
        UnsubscribeOrderbook,
        [EnumMember(Value = "subscribeTrades")]
        SubscribeTrades,
        [EnumMember(Value = "unsubscribeTrades")]
        UnsubscribeTrades,
        [EnumMember(Value = "getTrades")]
        GetTrades,
        [EnumMember(Value = "subscribeCandles")]
        SubscribeCandles,
        [EnumMember(Value = "unsubscribeCandles")]
        UnsubscribeCandles,
        [EnumMember(Value = "login")]
        Login,
        [EnumMember(Value = "subscribeReports")]
        SubscribeReports,
        [EnumMember(Value = "unsubscribeReports")]
        UnsubscribeReports,
        [EnumMember(Value = "newOrder")]
        NewOrder,
        [EnumMember(Value = "cancelOrder")]
        CancelOrder,
        [EnumMember(Value = "cancelReplaceOrder")]
        CancelReplaceOrder,
        [EnumMember(Value = "getOrders")]
        GetOrders,
        [EnumMember(Value = "getTradingBalance")]
        GetTradingBalance
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum NotificationMethod
    {
        [EnumMember(Value = "ticker")]
        Ticker,
        [EnumMember(Value = "snapshotOrderbook")]
        SnapshotOrderBook,
        [EnumMember(Value = "updateOrderbook")]
        UpdateOrderBook,
        [EnumMember(Value = "snapshotTrades")]
        SnapshotTrades,
        [EnumMember(Value = "updateTrades")]
        UpdateTrades,
        [EnumMember(Value = "snapshotCandles")]
        SnapshotCandles,
        [EnumMember(Value = "updateCandles")]
        UpdateCandles,
        [EnumMember(Value = "activeOrders")]
        ActiveOrders,
        [EnumMember(Value = "report")]
        Report,
    }

    public enum ConnectionState
    {
        PrepareToConnect,
        Connecting,
        Connected,
        Failed,
        Disconnecting,
        Disconnected
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderStatus
    {
        [EnumMember(Value = "new")]
        New,
        [EnumMember(Value = "suspended")]
        Suspended,
        [EnumMember(Value = "partiallyFilled")]
        PartiallyFilled,
        [EnumMember(Value = "filled")]
        Filled,
        [EnumMember(Value = "canceled")]
        Canceled,
        [EnumMember(Value = "expired")]
        Expired
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderType
    {
        [EnumMember(Value = "limit")]
        Limit,
        [EnumMember(Value = "market")]
        Market,
        [EnumMember(Value = "stopLimit")]
        StopLimit,
        [EnumMember(Value = "stopMarket")]
        StopMarket
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimeInForce
    {
        [EnumMember(Value = "GTC")]
        GTC,
        [EnumMember(Value = "IOC")]
        IOC,
        [EnumMember(Value = "FOK")]
        FOK,
        [EnumMember(Value = "Day")]
        Day,
        [EnumMember(Value = "GTD")]
        GTD
    }

    internal enum By
    {
        [EnumMember(Value = "timestamp")]
        Timestamp,
        [EnumMember(Value = "id")]
        Id
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransactionStatus
    {
        [EnumMember(Value = "pending")]
        Pending,
        [EnumMember(Value = "failed")]
        Failed,
        [EnumMember(Value = "success")]
        Success
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransactionType
    {
        [EnumMember(Value = "payout")]
        Payout,
        [EnumMember(Value = "payin")]
        Payin,
        [EnumMember(Value = "deposit")]
        Deposit,
        [EnumMember(Value = "withdraw")]
        Withdraw,
        [EnumMember(Value = "bankToExchange")]
        BankToExchange,
        [EnumMember(Value = "exchangeToBank")]
        ExchangeToBank
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum tLoginAlgo
    {
        [EnumMember(Value = "BASIC")]
        Basic,
        [EnumMember(Value = "HS256")]
        SHA256
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReportType
    {
        [EnumMember(Value = "status")]
        Status,
        [EnumMember(Value = "new")]
        New,
        [EnumMember(Value = "canceled")]
        Canceled,
        [EnumMember(Value = "expired")]
        Expired,
        [EnumMember(Value = "suspended")]
        Suspended,
        [EnumMember(Value = "trade")]
        Trade,
        [EnumMember(Value = "replaced")]
        Replaced
    }
}