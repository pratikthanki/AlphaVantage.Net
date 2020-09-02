using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AlphaVantage.Net.Core.Parsing;
using AlphaVantage.Net.Stocks.TimeSeries;

namespace AlphaVantage.Net.Stocks.Parsing
{
    internal class TimeSeriesParserBase
    {
        protected List<StockDataPoint> GetDataPoints(JsonDocument jsonDocument, bool isAdjusted)
        {
            var result = new List<StockDataPoint>();

            var dataPointsJsonElement = jsonDocument.RootElement.EnumerateObject().Last().Value;

            foreach (var dataPointJson in dataPointsJsonElement.EnumerateObject())
            {
                var dataPoint = isAdjusted ? new StockAdjustedDataPoint() : new StockDataPoint();
                dataPoint.Time = dataPointJson.Name.ParseToDateTime();

                var dataPointFieldsJson = dataPointJson.Value;
                EnrichDataPointFields(dataPoint, dataPointFieldsJson);

                result.Add(dataPoint);
            }

            return result;
        }

        private static void EnrichDataPointFields(StockDataPoint dataPoint, JsonElement dataPointFieldsJson)
        {
            foreach (var fieldJson in dataPointFieldsJson.EnumerateObject())
            {
                if (ParsingDelegates.ContainsKey(fieldJson.Name) == false) continue;

                ParsingDelegates[fieldJson.Name].Invoke(dataPoint, fieldJson.Value.GetString());
            }
        }

        private static readonly Dictionary<string, Action<StockDataPoint, string>> ParsingDelegates =
            new Dictionary<string, Action<StockDataPoint, string>>()
            {
                {"open", (dataPoint, strValue) => { dataPoint.OpeningPrice = strValue.ParseToDecimal(); }},
                {"high", (dataPoint, strValue) => { dataPoint.HighestPrice = strValue.ParseToDecimal(); }},
                {"low", (dataPoint, strValue) => { dataPoint.LowestPrice = strValue.ParseToDecimal(); }},
                {"close", (dataPoint, strValue) => { dataPoint.ClosingPrice = strValue.ParseToDecimal(); }},
                {"volume", (dataPoint, strValue) => { dataPoint.Volume = strValue.ParseToLong(); }},
                {
                    "adjusted close",
                    (dataPoint, strValue) =>
                    {
                        ((StockAdjustedDataPoint) dataPoint).AdjustedClosingPrice = strValue.ParseToDecimal();
                    }
                },
                {
                    "dividend amount",
                    (dataPoint, strValue) =>
                    {
                        ((StockAdjustedDataPoint) dataPoint).DividendAmount = strValue.ParseToDecimal();
                    }
                },
                {
                    "split coefficient",
                    (dataPoint, strValue) =>
                    {
                        ((StockAdjustedDataPoint) dataPoint).SplitCoefficient = strValue.ParseToDecimal();
                    }
                },
            };
    }
}