using Microsoft.VisualStudio.TestTools.UnitTesting;
using SingleResponsibilityPrinciple;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SingleResponsibilityPrinciple.Tests
{
    [TestClass()]
    public class TradeProcessorTests
    {
        private int CountDbRecords()
        {
            // The connection string uses |DataDirectory| 
            //    and assumes the tradedatabase.mdf file is stored in 
            //    SingleResponsibilityPrinciple\bin\Debug 
            using (var connection = new System.Data.SqlClient.SqlConnection(@"Server=tcp:cis3115-server.database.windows.net,1433;Initial Catalog=CIS3115;Persist Security Info=False;User ID=cis3115;Password=Saints4SQL;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string myScalarQuery = "SELECT COUNT(*) FROM trade";
                SqlCommand myCommand = new SqlCommand(myScalarQuery, connection);
                int count = (int)myCommand.ExecuteScalar();
                connection.Close();
                return count;
            }
        }
        [TestMethod()]
        public void TestNormalFile()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleResponsibilityPrincipleTests.goodtrades.txt");
            var tradeProcessor = new TradeProcessor();

            //Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);

            //Assert
            int countAfter = CountDbRecords();
            Assert.AreEqual(countBefore + 4, countAfter);
        }

        [TestMethod()]
        public void TestFileOneTrade()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleResponsibilityPrincipleTests.goodtradessingletrade.txt");
            var tradeProcessor = new TradeProcessor();

            //Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);

            //Assert
            int countAfter = CountDbRecords();
            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod()]
        public void TestFileNoTrades()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleResponsibilityPrincipleTests.notrades.txt");
            var tradeProcessor = new TradeProcessor();

            //Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);

            //Assert
            int countAfter = CountDbRecords();
            Assert.AreEqual(countBefore, countAfter);
        }

        [TestMethod()]
        public void TestFileNegativePrice()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleResponsibilityPrincipleTests.badtradesnegativeprice.txt");
            var tradeProcessor = new TradeProcessor();

            //Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);

            //Assert
            int countAfter = CountDbRecords();
            Assert.AreEqual(countBefore, countAfter);
        }

        [TestMethod()]
        public void TestFileInvalidInteger()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleResponsibilityPrincipleTests.badtradesinvalidinteger.txt");
            var tradeProcessor = new TradeProcessor();

            //Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);

            //Assert
            int countAfter = CountDbRecords();
            Assert.AreEqual(countBefore, countAfter);
        }

        [TestMethod()]
        public void TestFileLineMatch()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleResponsibilityPrincipleTests.goodtradeslinematch.txt");
            var tradeProcessor = new TradeProcessor();
            int i = 0;
            bool isMatched = false;

            //Act
            IEnumerable<string> theLines = tradeProcessor.ReadTradeData(tradeStream);
            foreach (var item in theLines)
            {
                if(i == 2)
                {
                        isMatched = item.Equals("USDEUR,1500,1.14");
                }
                i++;
            }

            //Assert
            if (!isMatched) Assert.Fail();
        }

        [TestMethod()]
        public void TestFileLineNoMatch()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleResponsibilityPrincipleTests.goodtradeslinematch.txt");
            var tradeProcessor = new TradeProcessor();
            int i = 0;
            bool isMatched = false;

            //Act
            IEnumerable<string> theLines = tradeProcessor.ReadTradeData(tradeStream);
            foreach (var item in theLines)
            {
                if(i == 3)
                {
                        isMatched = item.Equals("USDCAD,69000,1.4");
                }
                i++;
            }

            //Assert
            if (isMatched) Assert.Fail();
        }
    }
}