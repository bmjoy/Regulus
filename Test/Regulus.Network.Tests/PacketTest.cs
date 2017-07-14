﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Regulus.Network.RUDP;

namespace Regulus.Network.Tests
{
    [TestClass]
    public class PacketTest
    {
        [TestMethod]
        public void Seq()
        {
            
            var buffer = new SegmentPackage(Config.PackageSize);
            buffer.SetSeq(0x1234);
            var seq = buffer.GetSeq();
            Assert.AreEqual((ushort)0x1234, seq);
        }

        [TestMethod]
        public void Ack()
        {
            var buffer = new SegmentPackage(Config.PackageSize);
            buffer.SetAck(0x1234);
            var value = buffer.GetAck();
            Assert.AreEqual((ushort)0x1234, value);
        }

        [TestMethod]
        public void AckFields()
        {
            var buffer = new SegmentPackage(Config.PackageSize);
            buffer.SetAckFields(0x12345678u);
            var value = buffer.GetAckFields();
            Assert.AreEqual((uint)0x12345678, value);
        }

        [TestMethod]
        public void Operation()
        {
            var buffer = new SegmentPackage(Config.PackageSize);
            buffer.SetOperation(0x12);
            var value = buffer.GetOperation();
            Assert.AreEqual((byte)0x12, value);
        }
        


        [TestMethod]
        public void Payload()
        {
            var buffer = new SegmentPackage(Config.PackageSize);
            var payloadSize = buffer.GetPayloadBufferSize();
            var payloadSource = new byte[payloadSize];
            var payloadReaded = new byte[payloadSize];

            _BuildPayloadData(payloadSource);
            buffer.WritePayload(payloadSource ,0 , payloadSource.Length);

            var ok = buffer.CheckPayload();
            Assert.IsTrue(ok);
            var payloadLength = buffer.GetPayloadLength();
            Assert.AreEqual((ushort)payloadSize , payloadLength);

            
            var result = buffer.ReadPayload(payloadReaded , 0);
            Assert.IsTrue(result);
            for (int i = 0; i < payloadLength; ++i)
            {                
                Assert.AreEqual((byte) i, payloadReaded[i]);
            }            
        }

        private void _BuildPayloadData(byte[] buffer)
        {
            for(int i = 0 ; i < buffer.Length ; ++i)
            {
                buffer[i] = (byte)i;
            }
        }
    }
}
