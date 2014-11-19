﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace APIWrapper.AuthenticationManager {

    public class Authentication {

        //OAuth URI Endpoints
        private static readonly string RequestTokenURI = "http://www.tumblr.com/oauth/request_token";
        private static readonly string AuthorizationURI = "http://www.tumblr.com/oauth/authorize";
        private static readonly string AccessTokenURI = "http://www.tumblr.com/oauth/access_token";

        //XAuth URI Endpoints
        private static readonly string SecureAccessTokenURI = "https://www.tumblr.com/oauth/access_token";

        //Keys
        public static readonly string ConsumerKey = "BUHsuO5U9DF42uJtc8QTZlOmnUaJmBJGuU1efURxeklbdiLn9L";
        public static readonly string ConsumerSecretKey = "olOu3aRBCdqCuMFm8fmzNjMAWmICADSIuXWTnVSFng1ZcLU1cV";
        
        #region OTHER KEYS
        //["API_KEY"] = "4HL5w2Uht9FwOQZbsmUFFB8lrXUW2D4krfG24BvcF4Sijj3Bgd";
        //["API_SECRET_KEY"] = "kIXPU2gyaZ9EYWpuNX5FbCc9jQmhVuD40Ca7trnSr7PkDdYxLE";
        //Tumblrs API KEYS
        //["API_KEY"] = "BUHsuO5U9DF42uJtc8QTZlOmnUaJmBJGuU1efURxeklbdiLn9L";
        //["API_SECRET_KEY"] = "olOu3aRBCdqCuMFm8fmzNjMAWmICADSIuXWTnVSFng1ZcLU1cV";
        //["API_KEY"] = "4MjLMcdDEDvsyAeM50KBVOLgJ1wN02Rs6AbOjy2Np9X0LA5npB";
        //["API_SECRET_KEY"] = "1lkQGtH9N1dXeUXuDity3PZ78Cs9zqwqWXlwLNVzf5iDu3j2lT";
        #endregion

        //Authenticated Tokens
        public static Dictionary<string, string> AuthenticatedTokens { get; private set; }
        public static Dictionary<string, string> AuthenticatedSecretTokens { get; private set; }
        public static string Token {
            get {
                if (LocalSettings.Values["Token"] != null)
                    return LocalSettings.Values["Token"].ToString();
                return "";
            }
            set {
                LocalSettings.Values["Token"] = value;
            }
        }
        public static string TokenSecret {
            get {
                if (LocalSettings.Values["TokenSecret"] != null)
                    return LocalSettings.Values["TokenSecret"].ToString();
                return "";
            }
            set {
                LocalSettings.Values["TokenSecret"] = value;
            }
        }
        public static string TokenVerifier {
            get {
                if (LocalSettings.Values["TokenVerifier"] != null)
                    return LocalSettings.Values["TokenVerifier"].ToString();
                return "";
            }
            set {
                LocalSettings.Values["TokenVerifier"] = value;
            }
        }

        //Current account
        public static string SelectedAccount {
            get {
                return LocalSettings.Values["SelectedAccount"].ToString();
            }
            set {
                LocalSettings.Values["SelectedAccount"] = value;
            }
        }

        //Authentication Utilities
        public static AuthenticationUtilities Utils = new AuthenticationUtilities();

        //Local Application Storage Container
        public static ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        //Authenticate
        public Authentication() {
            GetAuthenticatedTokens();
        }

        //Retreive locally stored tokens
        private static void GetAuthenticatedTokens() {
            try {
                AuthenticatedTokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(LocalSettings.Values["AccountTokens"].ToString());
                AuthenticatedSecretTokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(LocalSettings.Values["AccountSecretTokens"].ToString());
            } catch {
                Debug.WriteLine("[Authentication]: Cannot load from storage; Assigning default values.");
                AuthenticatedTokens = new Dictionary<string, string>();
                AuthenticatedSecretTokens = new Dictionary<string, string>();
            }
        }

        //Save tokens to storage
        public static void SetAuthenticatedTokens() {
            try {
                LocalSettings.Values["AccountTokens"] = JsonConvert.SerializeObject(AuthenticatedTokens);
                LocalSettings.Values["AccountSecretTokens"] = JsonConvert.SerializeObject(AuthenticatedSecretTokens);
                Debug.WriteLine("[Authentication]: Saved to storage.");
            } catch {
                Debug.WriteLine("[Authentication]: Cannot save to storage; Assigning default values.");
            }
        }

        //Request Token
        public static async Task<string> RequestAccessToken(string userName, string password) {
            if (NetworkInterface.GetIsNetworkAvailable()) {
                var nonce = Utils.GetNonce();
                var timeStamp = Utils.GetTimeStamp();

                var signatureParameters = new SortedDictionary<string, string> {
                { "oauth_consumer_key", ConsumerKey},
                {"oauth_nonce", nonce},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_timestamp", timeStamp},
                {"oauth_version", "1.0"},
                {"x_auth_mode", "client_auth"},
                {"x_auth_password", Utils.UrlEncode(Uri.EscapeDataString(password))},
                {"x_auth_username", Uri.EscapeDataString(userName)}
            }.Select(kv => kv.Key + "=" + kv.Value).Aggregate((i, j) => i + "&" + j);

                var signatureString = "POST&" + Uri.EscapeDataString(SecureAccessTokenURI) + "&" + Uri.EscapeDataString(signatureParameters);
                var signature = Utils.GenerateSignature(signatureString, ConsumerSecretKey);

                var response = await Utils.PostAuthenticationData(SecureAccessTokenURI, signatureParameters + "&oauth_signature=" + Uri.EscapeDataString(signature));

                Debug.WriteLine("Access Token Response: " + response);

                //Parse response data
                if (!string.IsNullOrEmpty(response) && response.Contains("oauth_token") && response.Contains("oauth_token_secret")) {
                    var tokens = response.Split('&');
                    var accessToken = tokens[0].Split('=');
                    var accessTokenSecret = tokens[1].Split('=');

                    //Add token to token dictionary
                    AuthenticatedTokens.Add(userName, accessToken[1]);
                    if (AuthenticatedTokens.Count == 1) {
                        Token = accessToken[1];
                        SelectedAccount = userName;
                    }
                    Debug.WriteLine("Access Token: " + Token);

                    //Add secret token to secret token dictionary
                    AuthenticatedSecretTokens.Add(userName, accessTokenSecret[1]);
                    if (AuthenticatedSecretTokens.Count == 1) {
                        TokenSecret = accessTokenSecret[1];
                    }
                    Debug.WriteLine("OAuth Access Token: " + TokenSecret);

                    SetAuthenticatedTokens();
                    return "OK";
                } else if (response == "Invalid xAuth credentials.") {
                    return "We were unable to sign you in. Please check your username/password.";
                }
            } else {
                return "No Network";
            }

            return null;
        }


    }
}