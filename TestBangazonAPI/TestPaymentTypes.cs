using BangazonAPI.Controllers;
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
    public class TestPaymentTypes
    {
        [Fact]
        public async Task Test_Get_All_PaymentTypes()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                 * ARRANGE
                 */

                /*
                * ACT
                */
                var response = await client.GetAsync("/api/PaymentTypes");

                string responseBody = await response.Content.ReadAsStringAsync();
                var paymentTypes = JsonConvert.DeserializeObject<List<PaymentType>>(responseBody);

                /*
                * ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(paymentTypes.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Specific_PaymentType()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                 * ARRANGE
                 */

                /*
                * ACT
                */
                var response = await client.GetAsync("/api/PaymentTypes/1");

                string responseBody = await response.Content.ReadAsStringAsync();
                var paymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                /*
                * ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(paymentType.Name != null);
                Assert.Equal(1, paymentType.Id);
            }
        }

        [Fact]
        public async Task Test_Add_PaymentType()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                 * ARRANGE
                 */
                var newPaymentType = new PaymentType()
                {
                    Name = "Visa",
                    AcctNumber = 12345,
                    CustomerId = 2
                };
                var jsonNewPaymentType = JsonConvert.SerializeObject(newPaymentType);

                /*
                * ACT
                */
                var response = await client.PostAsync("/api/PaymentTypes",
                    new StringContent(jsonNewPaymentType, 
                    Encoding.UTF8, "application/json"));

                string responseBody = await response.Content.ReadAsStringAsync();
                var paymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                /*
                * ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.True(paymentType.Id != 0);
                Assert.Equal(newPaymentType.Name, paymentType.Name);
                Assert.Equal(newPaymentType.AcctNumber, paymentType.AcctNumber);
                Assert.Equal(newPaymentType.CustomerId, paymentType.CustomerId);
            }
        }
    }
}
