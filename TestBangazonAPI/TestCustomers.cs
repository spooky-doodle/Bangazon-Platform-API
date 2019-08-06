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
    public class TestCustomers
    {
        [Fact]
        public async Task Test_Get_All_Customers()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/customers");


                string responseBody = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(customers.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_All_Customers_Include_Products()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE  
                    TODO:  Create a new product and assign to a customer.
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/customers?_include=products");


                string responseBody = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(customers.Count > 0);
                Assert.NotNull(customers[1].Products[0]);

            }
        }

        [Fact]
        public async Task Test_Get_All_Customers_Include_Payments()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE  
                    TODO:  Create a new product and assign to a customer.
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/customers?_include=products");


                string responseBody = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(customers.Count > 0);
                Assert.NotNull(customers[1].PaymentType);

            }
        }

        [Fact]
        public async Task Test_Get_One_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/customers/1");


                string responseBody = await response.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<Customer>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(customer.FirstName != null);
                Assert.True(customer.Id == 1);
            }
        }
        [Fact]
        public async Task Test_Get_One_Customer_Nonexistant()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/customers/99999999");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }
        [Fact]
        public async Task Test_Create_New_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                Customer newCustomer = new Customer()
                {
                    FirstName = "Bob",
                    LastName = "Barker"
                };

                var jsonCustomer = JsonConvert.SerializeObject(newCustomer);

                /*
                    ACT
                */
                var response = await client.PostAsync(
                    "/api/customers",
                    new StringContent(jsonCustomer, Encoding.UTF8, "application/json")
                    );


                string responseBody = await response.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<Customer>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.True(customer.Id != 0);
                Assert.Equal(customer.FirstName, newCustomer.FirstName);
                Assert.Equal(customer.LastName, newCustomer.LastName);


            }
        }

        [Fact]
        public async Task Test_Update_Existing_Customer()
        {
            int testId = 2;
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                Customer testCustomer = new Customer()
                {
                    Id = testId,
                    FirstName = "Jason",
                    LastName = "Server"
                };

                var jsonCustomer = JsonConvert.SerializeObject(testCustomer);

                /*
                    ACT
                */
                var response = await client.PutAsync(
                    $"/api/customers/{testId}",
                    new StringContent(jsonCustomer, Encoding.UTF8, "application/json")
                    );



                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*   
                 *   GET   
                */

                var getCustomer = await client.GetAsync($"/api/customers/{testId}");
                getCustomer.EnsureSuccessStatusCode();

                string getResponse = await getCustomer.Content.ReadAsStringAsync();
                Customer updatedCustomer = JsonConvert.DeserializeObject<Customer>(getResponse);

                Assert.Equal(HttpStatusCode.OK, getCustomer.StatusCode);
                Assert.Equal(testId, updatedCustomer.Id);
                Assert.Equal(testCustomer.FirstName, updatedCustomer.FirstName);
                Assert.Equal(testCustomer.LastName, updatedCustomer.LastName);


            }
        }

        [Fact]
        public async Task Test_Update_Nonexisting_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                Customer testCustomer = new Customer()
                {
                    FirstName = "Billy",
                    LastName = "Blanks"
                };

                var jsonCustomer = JsonConvert.SerializeObject(testCustomer);

                /*
                    ACT
                */
                var response = await client.PutAsync(
                    "/api/customers/9999999",
                    new StringContent(jsonCustomer, Encoding.UTF8, "application/json")
                    );




                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


            }
        }

        //[Fact]
        //public async Task Test_Delete_Existing_Customer()
        //{


        //    using (var client = new APIClientProvider().Client)
        //    {
        //        /*
        //            ARRANGE
        //        */
        //        Customer newCustomer = new Customer()
        //        {
        //            FirstName = "Adam",
        //            LastName = "Driver"
        //        };

        //        var jsonCustomer = JsonConvert.SerializeObject(newCustomer);


        //        var response = await client.PostAsync(
        //            "/api/customers",
        //            new StringContent(jsonCustomer, Encoding.UTF8, "application/json")
        //            );


        //        string responseBody = await response.Content.ReadAsStringAsync();
        //        var customer = JsonConvert.DeserializeObject<Customer>(responseBody);


        //        /*
        //            ACT
        //        */
        //        var deleteResponse = await client.DeleteAsync($"/api/customers/{customer.Id}");


        //        /*
        //            ASSERT
        //        */
        //        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
         




        //    }
        //}


        //[Fact]
        //public async Task Test_Delete_Nonexisting_Customer()
        //{


        //    using (var client = new APIClientProvider().Client)
        //    {
        //        /*
        //            ARRANGE
        //        */
              
        //        /*
        //            ACT
        //        */
        //        var deleteResponse = await client.DeleteAsync($"/api/customers/135123233");


        //        /*
        //            ASSERT
        //        */
        //        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);





        //    }
        //}

    }
}
