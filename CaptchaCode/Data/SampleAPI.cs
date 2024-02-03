using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CaptchaCode.Data
{
    public class SampleAPI
    {
        private IHttpClientFactory HttpClientFactory { get; }

        private IOptionsMonitor<reCAPTCHAVerificationOptions> reCAPTCHAVerificationOptions { get; }

        public SampleAPI(IHttpClientFactory httpClientFactory, IOptionsMonitor<reCAPTCHAVerificationOptions> reCAPTCHAVerificationOptions)
        {
            HttpClientFactory = httpClientFactory;
            this.reCAPTCHAVerificationOptions = reCAPTCHAVerificationOptions;
        }

        public async Task<(bool Success, string[] ErrorCodes)> Post(string reCAPTCHAResponse)
        {
            var url = "https://www.google.com/recaptcha/api/siteverify";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"secret", reCAPTCHAVerificationOptions.CurrentValue.Secret},
                {"response", reCAPTCHAResponse}
            });

            var httpClient = HttpClientFactory.CreateClient();
            var response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            
            //reCAPTCHAVerificationResponse
            //= await response.Content.ReadAsStringAsync<reCAPTCHAVerificationResponse>();
            var readTask = response.Content.ReadAsStringAsync();
            readTask.Wait();
            var result = readTask.Result;
            var verificationResponse = JsonConvert.DeserializeObject<reCAPTCHAVerificationResponse>(result);
            if (verificationResponse.Success) return (Success: true, ErrorCodes: new string[0]);

            return (
                Success: false,
                ErrorCodes: verificationResponse.ErrorCodes.Select(err => err.Replace('-', ' ')).ToArray());
        }
    }
}
