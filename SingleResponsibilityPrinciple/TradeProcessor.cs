﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleResponsibilityPrinciple
{
    public class TradeProcessor
    {
        // User story 1 implementation:
        bool shouldProcess = true;
        public IEnumerable<string> ReadTradeData(Stream stream)
        {
            var tradeData = new List<string>();
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    tradeData.Add(line);
                }
            }
            return tradeData;
        }

        private IEnumerable<TradeRecord> ParseTrades(IEnumerable<string> tradeData)
        {
            var trades = new List<TradeRecord>();
            var lineCount = 1;
            foreach (var line in tradeData)
            {
                var fields = line.Split(new char[] { ',' });

                if (!ValidateTradeData(fields, lineCount))
                {
                    // User story 1 implementation:
                    shouldProcess = false;
                    continue;
                }

                var trade = MapTradeDataToTradeRecord(fields);

                trades.Add(trade);

                lineCount++;
            }

            return trades;
        }

        private bool ValidateTradeData(string[] fields, int currentLine)
        {
            if (fields.Length != 3)
            {
                LogMessage("WARN: Line {0} malformed. Only {1} field(s) found.", currentLine, fields.Length);
                return false;
            }

            if (fields[0].Length != 6)
            {
                LogMessage("WARN: Trade currencies on line {0} malformed: '{1}'", currentLine, fields[0]);
                return false;
            }

            int tradeAmount;
            if (!int.TryParse(fields[1], out tradeAmount))
            {
                LogMessage("WARN: Trade amount on line {0} not a valid integer: '{1}'", currentLine, fields[1]);
                return false;
            }

            decimal tradePrice;
            if (!decimal.TryParse(fields[2], out tradePrice))
            {
                LogMessage("WARN: Trade price on line {0} not a valid decimal: '{1}'", currentLine, fields[2]);
                return false;
            }

            // User story 2 implementation:
            if(decimal.Parse(fields[2]) < 0)
            {
                LogMessage("WARN: Trade price on line {0} is negative: '{1}'", currentLine, fields[2]);
                return false;
            }

            return true;
        }

        private void LogMessage(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        private TradeRecord MapTradeDataToTradeRecord(string[] fields)
        {
            float LotSize = 100000f;
            var sourceCurrencyCode = fields[0].Substring(0, 3);
            var destinationCurrencyCode = fields[0].Substring(3, 3);
            var tradeAmount = int.Parse(fields[1]);
            var tradePrice = decimal.Parse(fields[2]);

            var trade = new TradeRecord
            {
                SourceCurrency = sourceCurrencyCode,
                DestinationCurrency = destinationCurrencyCode,
                Lots = tradeAmount / LotSize,
                Price = tradePrice
            };

            return trade;
        }

        private void StoreTrades(IEnumerable<TradeRecord> trades)
        {
            LogMessage("INFO: Connecting to database");
            // The connection string uses |DataDirectory| 
            //    and assumes the tradedatabase.mdf file is stored in 
            //    SingleResponsibilityPrinciple\bin\Debug 
            //using (var connection = new System.Data.SqlClient.SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\tradedatabase.mdf;Integrated Security=True;Connect Timeout=30;"))
            using (var connection = new System.Data.SqlClient.SqlConnection(@"Server=tcp:cis3115-server.database.windows.net,1433;Initial Catalog=CIS3115;Persist Security Info=False;User ID=cis3115;Password=Saints4SQL;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"))
            // Template for connection string from database connection file
            //    The @ sign allows for back slashes
            //    Watch for double quotes which must be escaped using "" 
            //    Watch for extra spaces after C: and avoid paths with - hyphens -
            // using (var connection = new System.Data.SqlClient.SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\tgibbons\source\repos\cis-3285-asg-8-tgibbons-css\tradedatabase.mdf"";Integrated Security=True;Connect Timeout=30;"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var trade in trades)
                    {
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = "dbo.insert_trade";
                        command.Parameters.AddWithValue("@sourceCurrency", trade.SourceCurrency);
                        command.Parameters.AddWithValue("@destinationCurrency", trade.DestinationCurrency);
                        command.Parameters.AddWithValue("@lots", trade.Lots);
                        command.Parameters.AddWithValue("@price", trade.Price);

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                connection.Close();
            }

            LogMessage("INFO: {0} trades processed", trades.Count());
        }

        public void ProcessTrades(Stream stream)
        {
            var lines = ReadTradeData(stream);
            var trades = ParseTrades(lines);
            // User story 1 implementation:
            // User story 2 implementation:
            if (shouldProcess)
            {
                StoreTrades(trades);
            }
        }

    }
}
