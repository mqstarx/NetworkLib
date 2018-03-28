using NetworkLib;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace UnitTests
{
    [TestClass]
    public class TestMethods
    {
        [TestMethod]
        public void CalculatePacketLenTest()
        {
            int test_len = 4343;
            TcpModuleMiddleLevel tcp_mid = new TcpModuleMiddleLevel();
           // Assert.AreEqual(test_len, tcp_mid.CalculatePacketLen(tcp_mid.GetPacketHeader(test_len)));
        }
    }
}
