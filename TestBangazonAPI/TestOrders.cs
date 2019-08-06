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
    public class TestOrders
    {
        [Fact]
        public async Task Test_Get_All_Orders()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/orders");


                string responseBody = await response.Content.ReadAsStringAsync();
                var orders = JsonConvert.DeserializeObject<List<Order>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(orders.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Order()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/orders/1");


                string responseBody = await response.Content.ReadAsStringAsync();
                var order = JsonConvert.DeserializeObject<Order>(responseBody);

                /*
                 * ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(1, order.Id);
                Assert.Equal(2, order.CustomerId);
                Assert.Equal(1, order.PaymentTypeId);
                Assert.NotNull(order);


            }
        }

        [Fact]
        public async Task Test_Create_And_Delete_Order()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                Order order = new Order
                {
                    CustomerId = 2,
                    PaymentTypeId = 2
                };

                var OrderAsJSON = JsonConvert.SerializeObject(order);


                /*
                    ACT
                */

                var response = await client.PostAsync(
                    "/api/orders",
                    new StringContent(OrderAsJSON, Encoding.UTF8, "application/json")
                );

                string responseBody = await response.Content.ReadAsStringAsync();
                var NewOrder = JsonConvert.DeserializeObject<Order>(responseBody);

                /*
                    ASSERT
                */

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(order.CustomerId, NewOrder.CustomerId);
                Assert.Equal(order.PaymentTypeId, NewOrder.PaymentTypeId);

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
    }

}
