using System.Collections.Generic;
using System.Threading.Tasks;
using AlphaVantage.Net.Common;
using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Common.Size;
using AlphaVantage.Net.Stocks.Parsing;
using AlphaVantage.Net.Stocks.TimeSeries;

namespace AlphaVantage.Net.Stocks.Client
{
    public static class StocksClientExtensions
    {
        private static readonly GlobalQuoteParser GlobalQuoteParser = new GlobalQuoteParser();
        private static readonly SearchResultParser SearchResultParser = new SearchResultParser();
        
        
        /// <summary>
        /// Return stocks time series for requested symbol 
        /// </summary>
        /// <param name="stocksClient"></param>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="size"></param>
        /// <param name="isAdjusted"></param>
        /// <returns></returns>
        public static async Task<StockTimeSeries> GetTimeSeriesAsync(this StocksClient stocksClient, 
            string symbol, 
            Interval interval,
            OutputSize size = OutputSize.Compact,
            bool isAdjusted = false)
        {
            var parser = new StocksTimeSeriesParser(interval, isAdjusted);
            
            var query = new Dictionary<string, string>()
            {
                {ApiConstants.SymbolQueryVar, symbol},
                {ApiConstants.OutputSizeQueryVar, size.ConvertToString()}
            };

            var function = interval.ConvertToApiFunction(isAdjusted);
            if (function == ApiFunction.TIME_SERIES_INTRADAY)
            {
                query.Add(ApiConstants.IntervalQueryVar, interval.ConvertToString());
            }
            
            return await stocksClient.RequestApiAsync(parser, function, query);
        }

        /// <summary>
        /// Returns the price and volume information for a symbol
        /// </summary>
        /// <param name="stocksClient"></param>
        /// <param name="symbol"></param>
        /// <remarks>
        /// function=GLOBAL_QUOTE
        /// </remarks>
        public static async Task<GlobalQuote?> GetGlobalQuoteAsync(this StocksClient stocksClient, string symbol)
        {
            var query = new Dictionary<string, string>(){{ApiConstants.SymbolQueryVar, symbol}};
            return await stocksClient.RequestApiAsync(GlobalQuoteParser, ApiFunction.GLOBAL_QUOTE, query);
        }
        
        /// <summary>
        /// Returns the best-matching symbols and market information based on keywords
        /// </summary>
        /// <param name="stocksClient"></param>
        /// <param name="keyWords"></param>
        /// <remarks>
        /// function=SYMBOL_SEARCH
        /// </remarks>
        public static async Task<ICollection<SymbolSearchMatch>> SearchSymbolAsync(this StocksClient stocksClient, string keyWords)
        {
            var query = new Dictionary<string, string>(){{"keywords", keyWords}};
            return await stocksClient.RequestApiAsync(SearchResultParser, ApiFunction.SYMBOL_SEARCH, query);
        }
    }
}