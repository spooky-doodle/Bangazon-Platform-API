using System;
using System.Net;
using Newtonsoft.Json;
using Xunit;
using BangazonAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace TestBangazonAPI
{
    public class TestProducts
    {
        [Fact]
        public async Task Test_Get_All_Products()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/products");


                string responseBody = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<Product>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(products.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_One_Product()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/products/1");


                string responseBody = await response.Content.ReadAsStringAsync();
                var product = JsonConvert.DeserializeObject<Product>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(product.Title != null);
                Assert.True(product.Id == 1);
            }
        }
        [Fact]
        public async Task Test_Create_New_Product()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                Product newProduct = new Product()
                {
                    Title = "Test Product",
                    ProductTypeId = 1,
                    CustomerId = 1,
                    Price = (decimal)21.25,
                    Description = "Test Description",
                    Quantity = 1
                };

                var jsonProduct = JsonConvert.SerializeObject(newProduct);

                /*
                    ACT
                */
                var response = await client.PostAsync(
                    "/api/products",
                    new StringContent(jsonProduct, Encoding.UTF8, "application/json")
                    );


                string responseBody = await response.Content.ReadAsStringAsync();
                var product = JsonConvert.DeserializeObject<Product>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.True(product.Id != 0);
                Assert.Equal(product.Title, newProduct.Title);
                Assert.Equal(product.Description, newProduct.Description);


            }
        }

        [Fact]
        public async Task Test_Update_Existing_Product()
        {
            int testId = 1;
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                Product testProduct = new Product()
                {
                    Title = "Test Product",
                    ProductTypeId = 1,
                    CustomerId = 1,
                    Price = (decimal)21.25,
                    Description = "Test Description",
                    Quantity = 1
                };

                var jsonProduct = JsonConvert.SerializeObject(testProduct);

                /*
                    ACT
                */
                var response = await client.PutAsync(
                    $"/api/products/{testId}",
                    new StringContent(jsonProduct, Encoding.UTF8, "application/json")
                    );



                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*   
                 *   GET   
                */

                var getProduct = await client.GetAsync($"/api/products/{testId}");
                getProduct.EnsureSuccessStatusCode();

                string getResponse = await getProduct.Content.ReadAsStringAsync();
                Product updatedProduct = JsonConvert.DeserializeObject<Product>(getResponse);

                Assert.Equal(HttpStatusCode.OK, getProduct.StatusCode);
                Assert.Equal(testId, updatedProduct.Id);
                Assert.Equal(testProduct.Title, updatedProduct.Title);
                Assert.Equal(testProduct.Description, updatedProduct.Description);


            }
        }

        [Fact]
        public async Task Test_Update_Nonexisting_Product()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                Product testProduct = new Product()
                {
                    Title = "Test Product 2",
                    ProductTypeId = 6,
                    CustomerId = 6,
                    Price = (decimal)21.25,
                    Description = "Test Description 2",
                    Quantity = 1
                };

                var jsonProduct = JsonConvert.SerializeObject(testProduct);

                /*
                    ACT
                */
                var response = await client.PutAsync(
                    "/api/products/9999999",
                    new StringContent(jsonProduct, Encoding.UTF8, "application/json")
                    );




                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


            }
        }

        [Fact]
        public async Task Test_Delete_Existing_Product()
        {


            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                Product newProduct = new Product()
                {
                    Title = "Test Product",
                    ProductTypeId = 1,
                    CustomerId = 1,
                    Price = (decimal)21.25,
                    Description = "Test Description",
                    Quantity = 1
                };

                var jsonProduct = JsonConvert.SerializeObject(newProduct);


                var response = await client.PostAsync(
                    "/api/products",
                    new StringContent(jsonProduct, Encoding.UTF8, "application/json")
                    );


                string responseBody = await response.Content.ReadAsStringAsync();
                var product = JsonConvert.DeserializeObject<Product>(responseBody);


                /*
                    ACT
                */
                var deleteResponse = await client.DeleteAsync($"/api/products/{product.Id}");


                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);





            }
        }


        [Fact]
        public async Task Test_Delete_Nonexisting_Product()
        {


            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                /*
                    ACT
                */
                var deleteResponse = await client.DeleteAsync($"/api/products/135123233");


                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);





            }
        }
    }
}
