using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Recaptcha2
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ValidateRecaptcha2Attribute : ActionFilterAttribute
    {
        public string ErrorMessage { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var gRecaptchaResponseVariableName = @"g-recaptcha-response";
            var gRecaptchaResponse = filterContext.HttpContext.Request[gRecaptchaResponseVariableName];
            var gRecaptchaSecret = ConfigurationManager.AppSettings["Recaptcha2Secret"];
            var gRecaptchaRemoteIp = filterContext.HttpContext.Request.UserHostAddress;

            var verifier = new Recaptcha2Verifier(gRecaptchaResponse, gRecaptchaSecret, gRecaptchaRemoteIp);
            var verifyResponse = Task.Run(async () => await verifier.VerifyAsync()).Result;

            if (verifyResponse.Success) return;

            if (ErrorMessage == null) ErrorMessage = verifyResponse.ErrorCodes[0];
            filterContext.Controller.ViewData.ModelState.AddModelError(gRecaptchaResponseVariableName, ErrorMessage);
        }
    }

    class Recaptcha2Verifier
    {
        private readonly string gRecaptchaUrl = @" https://www.google.com/recaptcha/api/siteverify";
        private readonly string gRecaptchaResponse;
        private readonly string gRecaptchaSecret;
        private readonly string gRecaptchaRemoteIp;

        public Recaptcha2Verifier(string gRecaptchaResponse, string gRecaptchaSecret, string gRecaptchaRemoteIp)
        {
            this.gRecaptchaResponse = gRecaptchaResponse;
            this.gRecaptchaSecret = gRecaptchaSecret;
            this.gRecaptchaRemoteIp = gRecaptchaRemoteIp;
        }

        public async Task<Recaptcha2VerifyResponse> VerifyAsync()
        {
            var result = new Recaptcha2VerifyResponse();
            string responseString;

            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    {"secret", gRecaptchaSecret},
                    {"response", gRecaptchaResponse},
                    {"remoteip", gRecaptchaRemoteIp},
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(gRecaptchaUrl, content);
                responseString = await response.Content.ReadAsStringAsync();
            }

            if (string.IsNullOrWhiteSpace(responseString)) return result;

            result = JsonConvert.DeserializeObject<Recaptcha2VerifyResponse>(responseString);

            return result;
        }
    }

    class Recaptcha2VerifyResponse
    {
        [JsonProperty("success")]
        private bool? _success = null;

        public bool Success
        {
            get { return _success == true; }
        }

        [JsonProperty("error-codes")]
        private string[] _errorCodes = null;

        public string[] ErrorCodes
        {
            get { return _errorCodes ?? new string[0]; }
        }
    }

    public static class Recaptcha2HtmlHelperExtension
    {
        public static MvcHtmlString Recaptcha2(this HtmlHelper html, string name = "g-recaptcha-response")
        {
            var gRecaptchaScript = "<script src=\"https://www.google.com/recaptcha/api.js\" async defer></script>";
            var gRecaptchaSiteKey = ConfigurationManager.AppSettings["Recaptcha2SiteKey"];
            var gRecaptchaWidget = "<div class=\"g-recaptcha\" data-sitekey=\"" + gRecaptchaSiteKey + "\"></div>";
            return new MvcHtmlString(gRecaptchaScript + gRecaptchaWidget);
        }
    }
}