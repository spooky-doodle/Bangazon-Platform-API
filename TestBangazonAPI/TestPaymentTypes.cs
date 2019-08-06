﻿using BangazonAPI.Controllers;
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

        [Fact]
        public async Task Test_Update_Existing_PaymentType()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                 * ARRANGE
                 */

                PaymentType testPaymentType = new PaymentType()
                {
                    Id = 5,
                    Name = "CitiBank",
                    AcctNumber = 12346,
                    CustomerId = 2
                };

                var jsonTestPaymentType = JsonConvert.SerializeObject(testPaymentType);

                /*
                    ACT
                */
                var response = await client.PutAsync(
                    "/api/PaymentTypes/5",
                    new StringContent(jsonTestPaymentType, Encoding.UTF8, "application/json")
                    );

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*   
                 *   GET   
                */

                var getPaymentType = await client.GetAsync($"/api/PaymentTypes/5");
                getPaymentType.EnsureSuccessStatusCode();

                string getResponse = await getPaymentType.Content.ReadAsStringAsync();
                PaymentType updatedPaymentType = JsonConvert.DeserializeObject<PaymentType>(getResponse);

                Assert.Equal(HttpStatusCode.OK, getPaymentType.StatusCode);
                Assert.Equal(5, updatedPaymentType.Id);
                Assert.Equal(testPaymentType.Name, updatedPaymentType.Name);
                Assert.Equal(testPaymentType.AcctNumber, updatedPaymentType.AcctNumber);
            }
        }
    }
}