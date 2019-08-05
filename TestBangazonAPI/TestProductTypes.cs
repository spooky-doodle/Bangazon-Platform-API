using BangazonAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestBangazonAPI
{
    public class TestProductTypes
    {
        [Fact]
        public async Task Test_Get_All_ProductTypes()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/productTypes");


                string responseBody = await response.Content.ReadAsStringAsync();
                var productTypes = JsonConvert.DeserializeObject<List<ProductType>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(productTypes.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_ProductType()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/productTypes/1");


                string responseBody = await response.Content.ReadAsStringAsync();
                var productType = JsonConvert.DeserializeObject<ProductType>(responseBody);

                /*
                 * ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(1, productType.Id);
                Assert.Equal("Food", productType.Name);
                Assert.NotNull(productType);


            }
        }

        [Fact]
        public async Task Test_Create_And_Delete_ProductType()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                ProductType laser = new ProductType
                {
                    Name = "Laser"
                };

                var LaserAsJSON = JsonConvert.SerializeObject(laser);


                /*
                    ACT
                */

                var response = await client.PostAsync(
                    "/api/productTypes",
                    new StringContent(LaserAsJSON, Encoding.UTF8, "application/json")
                );

                string responseBody = await response.Content.ReadAsStringAsync();
                var NewLaser = JsonConvert.DeserializeObject<ProductType>(responseBody);

                /*
                    ASSERT
                */

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(laser.Name, NewLaser.Name);

                /*
                    ACT
                */
                var deleteResponse = await client.DeleteAsync($"/api/productTypes/{NewLaser.Id}");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Modify_ProductType()
        {
            string NewName = "Food";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT
                */
                ProductType modifiedGrocery = new ProductType
                {
                    Name = NewName
                };

                var modifiedGroceryAsJSON = JsonConvert.SerializeObject(modifiedGrocery);

                var response = await client.PutAsync(
                    "/api/productTypes/1",
                    new StringContent(modifiedGroceryAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                   GET section
                */
                var getGrocery = await client.GetAsync("/api/productTypes/1");
                getGrocery.EnsureSuccessStatusCode();

                string getGroceryBody = await getGrocery.Content.ReadAsStringAsync();
                ProductType newGrocery = JsonConvert.DeserializeObject<ProductType>(getGroceryBody);

                /*
                   ASSERT
                */

                Assert.Equal(HttpStatusCode.OK, getGrocery.StatusCode);
                Assert.Equal(NewName, newGrocery.Name);
            }
        }
    }
}
