using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal abstract class MobileServicePKCEAuthentication : MobileServiceAuthentication
    {
        /// <summary>
        /// The <see cref="MobileServiceClient"/> used by this authentication session.
        /// </summary>
        private readonly MobileServiceClient client;

        protected Uri LoginUri { get; private set; }

        protected Uri CallbackUri { get; private set; }
        
        protected string CodeVerifier { get; private set; }

        protected MobileServicePKCEAuthentication(MobileServiceClient client, string provider, string uriScheme, IDictionary<string, string> parameters)
            : base(client, provider, parameters)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            if (string.IsNullOrWhiteSpace(uriScheme))
            {
                throw new ArgumentException("uriScheme");
            }

            this.client = client;
            this.CodeVerifier = GetCodeVerifier();
            this.CallbackUri = new Uri(MobileServiceUrlBuilder.CombileSchemeAndPath(uriScheme, "easyauth.callback"));

            var path = MobileServiceUrlBuilder.CombinePaths(LoginAsyncUriFragment, this.ProviderName);
            if (!string.IsNullOrEmpty(this.Client.LoginUriPrefix))
            {
                path = MobileServiceUrlBuilder.CombinePaths(this.Client.LoginUriPrefix, this.ProviderName);
            }
            var loginParameters = parameters != null ? new Dictionary<string, string>(parameters) : new Dictionary<string, string>();
            loginParameters.Add("post_login_redirect_url", this.CallbackUri.AbsoluteUri);
            loginParameters.Add("code_challenge", GetSha256Hash(this.CodeVerifier));
            loginParameters.Add("code_challenge_method", "S256");
            loginParameters.Add("session_mode", "token");
            var loginQueryString = MobileServiceUrlBuilder.GetQueryString(loginParameters, false);
            var loginPathAndQuery = MobileServiceUrlBuilder.CombinePathAndQuery(path, loginQueryString);
            
            this.LoginUri = new Uri(this.Client.MobileAppUri, loginPathAndQuery);
            if (this.Client.AlternateLoginHost != null)
            {
                this.LoginUri = new Uri(this.Client.AlternateLoginHost, loginPathAndQuery);
            }
        }

        /// <summary>
        /// Login via OAuth 2.0 PKCE protocol.
        /// </summary>
        /// <returns></returns>
        protected sealed override async Task<string> LoginAsyncOverride()
        {
            // Show platform-specific login ui and care about handling authorization_code from callback via deep linking.
            var authorizationCode = await this.GetAuthorizationCodeAsync();

            // Send authorization_code and code_verifier via HTTPS request to complete the PKCE flow.
            var path = MobileServiceUrlBuilder.CombinePaths(LoginAsyncUriFragment, ProviderName);
            if (!string.IsNullOrEmpty(client.LoginUriPrefix))
            {
                path = MobileServiceUrlBuilder.CombinePaths(client.LoginUriPrefix, ProviderName);
            }
            path = MobileServiceUrlBuilder.CombinePaths(path, "token");
            var tokenParameters = Parameters != null ? new Dictionary<string, string>(Parameters) : new Dictionary<string, string>();
            tokenParameters.Add("authorization_code", authorizationCode);
            tokenParameters.Add("code_verifier", CodeVerifier);
            var queryString = MobileServiceUrlBuilder.GetQueryString(tokenParameters);
            var pathAndQuery = MobileServiceUrlBuilder.CombinePathAndQuery(path, queryString);
            var httpClient = client.AlternateLoginHost == null ? client.HttpClient : client.AlternateAuthHttpClient;
            return await httpClient.RequestWithoutHandlersAsync(HttpMethod.Get, pathAndQuery, null);
        }

        protected abstract Task<string> GetAuthorizationCodeAsync();

        private static string GetCodeVerifier()
        {
            byte[] randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// SHA-256 hashing followed by Base64 encoding of the string input.
        /// </summary>
        /// <param name="data">Input data</param>
        /// <returns>Base64 encoded SHA-256 hash</returns>
        private static string GetSha256Hash(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            byte[] hash;
            using (var sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(bytes);
            }
            return Convert.ToBase64String(hash);
        }
    }
}
