// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using AzureCognitiveSearch.PowerSkills.Common;
using AzureCognitiveSearch.PowerSkills.Text.BingEntitySearch;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests.BingEntitySearchTests
{
    [TestClass]
    public class BingEntitySearchTests
    {
        const string licenseNotice = "The license notice";
        const string imageSuffix = ":image";
        const string descriptionSuffix = ":description";
        const string urlPrefix = "https://encyclopedia/";
        const string searchUrlPrefix = "https://www.bing.com/entityexplore?q=";
        const string idSuffix = ":id";

        const string person = "Some Person";
        const string location = "Some Location";
        const string organization = "Some Organization";
        static readonly string[] entities = new[] { person, location, organization };

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            WebApiSkillHelpers.TestMode = true;
            WebApiSkillHelpers.TestWww = req =>
            {
                string query = req.RequestUri.ParseQueryString()["q"];
                string entityType =
                    query == person ? "Person" :
                    query == location ? "Location" :
                    query == organization ? "Organization" :
                    "Unknown";

                return req.RespondRequestWith(new
                {
                    entities = new
                    {
                        value = new object[] {
                            new
                            {
                                entityPresentationInfo = new
                                {
                                    entityTypeHints = new [] { entityType }
                                },
                                contractualRules = new object[] {
                                    new
                                    {
                                        _type = "ContractualRules/LicenseAttribution",
                                        licenseNotice
                                    },
                                    new
                                    {
                                        _type = "ContractualRules/LinkAttribution",
                                        targetPropertyName = "description",
                                        url = urlPrefix + query
                                    }
                                },
                                image = new
                                {
                                    thumbnailUrl = query + imageSuffix
                                },
                                description = query + descriptionSuffix,
                                name = query,
                                url = urlPrefix + query,
                                bingId = query + idSuffix,
                                webSearchUrl = searchUrlPrefix + query
                            }
                        }
                    }
                });
            };
        }

        [TestMethod]
        public async Task GetAllEntityData()
        {
            foreach (string entity in entities)
            {
                var foundEntities = await Helpers.QuerySkill(
                BingEntitySearch.RunEntitySearch,
                new { Name = entity },
                "entities") as List<BingEntity>;

                Assert.AreEqual(1, foundEntities.Count);
                BingEntity foundEntity = foundEntities[0];
                Assert.AreEqual(entity + idSuffix, foundEntity.BingId);
                Assert.AreEqual(entity + descriptionSuffix, foundEntity.Description);
                Assert.AreEqual(entity, foundEntity.Name);
                Assert.AreEqual(urlPrefix + entity, foundEntity.Url);
                Assert.AreEqual(searchUrlPrefix + entity, foundEntity.WebSearchUrl);
                Assert.AreEqual(entity + imageSuffix, foundEntity.Image.ThumbnailUrl);
                Assert.AreEqual(2, foundEntity.ContractualRules.Length);
            }
        }

        [TestMethod]
        public async Task GetTopMetaData()
        {
            foreach (string entity in entities)
            {
                Dictionary<string, object> topMetadata =
                (await Helpers.QueryFunction(
                    Helpers.BuildPayload(new { Name = entity }),
                    BingEntitySearch.RunEntitySearch)
                ).Values[0].Data;

                Assert.AreEqual(entity, topMetadata["name"]);
                Assert.AreEqual(entity + descriptionSuffix, topMetadata["description"]);
                Assert.AreEqual(entity + imageSuffix, topMetadata["imageUrl"]);
                Assert.AreEqual(urlPrefix + entity, topMetadata["url"]);
                Assert.AreEqual(licenseNotice, topMetadata["licenseAttribution"]);
            }
        }

        [TestMethod]
        public async Task UnknownEntityTypeYieldsNoTopMetadata()
        {
            Dictionary<string, object> topMetadata =
            (await Helpers.QueryFunction(
                Helpers.BuildPayload(new { Name = "Not a known entity" }),
                BingEntitySearch.RunEntitySearch)
            ).Values[0].Data;

            Assert.AreEqual(1, topMetadata.Count);
        }
    }
}
