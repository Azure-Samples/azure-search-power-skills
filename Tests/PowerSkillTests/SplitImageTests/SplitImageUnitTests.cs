// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using AzureCognitiveSearch.PowerSkills.Text.BingEntitySearch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace AzureCognitiveSearch.PowerSkills.Tests.SplitImageTests
{
    [TestClass]
    public class SplitImageUnitTests
    {
        [TestMethod]
        public void CanCombineNormalUriWithNormalSasToken()
        {
            // This is typical case when user supplies blob metadata_storage_path and metadata_storage_sas_token
            var imageUri = @"http://www.myblob.com";
            var sasToken = @"?sp=rl&st=2020-01-17T23:22:10Z&se=2020-01-31T23:22:00Z&sv=2019-02-02&sr=b&sig=wcXZOnZGjJssuJYHgjSeSXIrEs6FVckj6lgTtB1VpVc%3D";

            var result = Vision.SplitImage.SplitImage.CombineSasTokenWithUri(imageUri, sasToken);
            Assert.AreEqual(
                "http://www.myblob.com:80/?sp=rl&st=2020-01-17T23%3a22%3a10Z&se=2020-01-31T23%3a22%3a00Z&sv=2019-02-02&sr=b&sig=wcXZOnZGjJssuJYHgjSeSXIrEs6FVckj6lgTtB1VpVc%3d"
                , result);
        }

        [TestMethod]
        public void CanCombineLessWellFormedUriAndSas()
        {
            var imageUri = @"www.myblob.com";
            var sasToken = @"?sp=rl&st=2020-01-17T23:22:10Z&se=2020-01-31T23:22:00Z&sv=2019-02-02&sr=b&sig=wcXZOnZGjJssuJYHgjSeSXIrEs6FVckj6lgTtB1VpVc%3D";

            var result = Vision.SplitImage.SplitImage.CombineSasTokenWithUri(imageUri, sasToken);
            Assert.AreEqual(
                "http://www.myblob.com:80/?sp=rl&st=2020-01-17T23%3a22%3a10Z&se=2020-01-31T23%3a22%3a00Z&sv=2019-02-02&sr=b&sig=wcXZOnZGjJssuJYHgjSeSXIrEs6FVckj6lgTtB1VpVc%3d"
                , result);

            sasToken = @"sp=rl&st=2020-01-17T23:22:10Z&se=2020-01-31T23:22:00Z&sv=2019-02-02&sr=b&sig=wcXZOnZGjJssuJYHgjSeSXIrEs6FVckj6lgTtB1VpVc%3D";

            result = Vision.SplitImage.SplitImage.CombineSasTokenWithUri(imageUri, sasToken);
            Assert.AreEqual(
                "http://www.myblob.com:80/?sp=rl&st=2020-01-17T23%3a22%3a10Z&se=2020-01-31T23%3a22%3a00Z&sv=2019-02-02&sr=b&sig=wcXZOnZGjJssuJYHgjSeSXIrEs6FVckj6lgTtB1VpVc%3d"
                , result);

            sasToken = @"sp=rl&st=2020-01-17T23:22:10Z";

            result = Vision.SplitImage.SplitImage.CombineSasTokenWithUri(imageUri, sasToken);
            Assert.AreEqual(
                "http://www.myblob.com:80/?sp=rl&st=2020-01-17T23%3a22%3a10Z"
                , result);

            imageUri = @"https://www.myblob.com";
            sasToken = @"?sp=rl&st=2020-01-17T23:22:10Z&se=2020-01-31T23:22:00Z&sv=2019-02-02&sr=b&sig=wcXZOnZGjJssuJYHgjSeSXIrEs6FVckj6lgTtB1VpVc%3D";

            result = Vision.SplitImage.SplitImage.CombineSasTokenWithUri(imageUri, sasToken);
            Assert.AreEqual(
                "https://www.myblob.com:443/?sp=rl&st=2020-01-17T23%3a22%3a10Z&se=2020-01-31T23%3a22%3a00Z&sv=2019-02-02&sr=b&sig=wcXZOnZGjJssuJYHgjSeSXIrEs6FVckj6lgTtB1VpVc%3d"
                , result);

            imageUri = @"ftp://www.myblob.net";

            result = Vision.SplitImage.SplitImage.CombineSasTokenWithUri(imageUri, sasToken);
            Assert.AreEqual(
                "ftp://www.myblob.net:21/?sp=rl&st=2020-01-17T23%3a22%3a10Z&se=2020-01-31T23%3a22%3a00Z&sv=2019-02-02&sr=b&sig=wcXZOnZGjJssuJYHgjSeSXIrEs6FVckj6lgTtB1VpVc%3d"
                , result);


            imageUri = @"http://www.myblob.net:7777";

            result = Vision.SplitImage.SplitImage.CombineSasTokenWithUri(imageUri, sasToken);
            Assert.AreEqual(
                "http://www.myblob.net:7777/?sp=rl&st=2020-01-17T23%3a22%3a10Z&se=2020-01-31T23%3a22%3a00Z&sv=2019-02-02&sr=b&sig=wcXZOnZGjJssuJYHgjSeSXIrEs6FVckj6lgTtB1VpVc%3d"
                , result);

            imageUri = @"http://www.myblob.net";
            sasToken = null; // no sas needed

            result = Vision.SplitImage.SplitImage.CombineSasTokenWithUri(imageUri, sasToken);
            Assert.AreEqual(
                "http://www.myblob.net:80/"
                , result);
        }
    }
}
