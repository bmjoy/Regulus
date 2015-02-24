﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
namespace VGame.Project.FishHunter.UnitTest
{

    
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestVerifySuccess()
        {
            string id = "12345678";
            string pw = "0000";
            var stroage = NSubstitute.Substitute.For<VGame.Project.FishHunter.IStorage>();
            stroage.FindAccount(id).Returns(new Regulus.Remoting.Value<VGame.Project.FishHunter.Data.Account>(new VGame.Project.FishHunter.Data.Account { Name = id, Password = pw }));

            bool returnValue = false;
            bool eventValue = false;
            var stage = new VGame.Project.FishHunter.Verify(stroage);
            stage.DoneEvent += (account) => { eventValue = true; };

            VGame.Project.FishHunter.IVerify verify = stage;
            var val = verify.Login("12345678" , "0000");
            val.OnValue += (result) => { returnValue = result; };
            

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual(true , eventValue );

        }

        [TestMethod]

        public void TestVerifyFail()
        {
            string id = "12345678";
            string pw = "0000";
            var stroage = NSubstitute.Substitute.For<VGame.Project.FishHunter.IStorage>();
            stroage.FindAccount(id).Returns(new Regulus.Remoting.Value<VGame.Project.FishHunter.Data.Account>(new VGame.Project.FishHunter.Data.Account { Name = id, Password = pw }));

            bool returnValue = false;
            bool eventValue = false;
            var stage = new VGame.Project.FishHunter.Verify(stroage);
            stage.DoneEvent += (account) => { eventValue = true; };

            VGame.Project.FishHunter.IVerify verify = stage;
            var val = verify.Login("12345678", "011111001");
            val.OnValue += (result) => { returnValue = result; };


            Assert.AreEqual(false, returnValue);
            Assert.AreEqual(false, eventValue);

        }

        
        
    }
}