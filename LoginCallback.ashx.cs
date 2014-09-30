
using Auth0.AspNet;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Services;
using System.Linq;
using System.Web;
namespace ASP
{
    public class LoginCallback : IHttpHandler
    {
        private readonly Auth0.Client client = new Auth0.Client(
                                ConfigurationManager.AppSettings["auth0:ClientId"],
                                ConfigurationManager.AppSettings["auth0:ClientSecret"],
                                ConfigurationManager.AppSettings["auth0:Domain"]);

        public void ProcessRequest(HttpContext context)
        {
            var code = context.Request.QueryString["code"];
            var url = context.Request.Url.ToString();
            //start
            RestRequest restRequest = new RestRequest("/oauth/token", Method.POST);
            restRequest.AddHeader("accept", "application/json");
            restRequest.AddParameter("client_id", (object)this.clientID, ParameterType.GetOrPost);
            restRequest.AddParameter("client_secret", (object)this.clientSecret, ParameterType.GetOrPost);
            restRequest.AddParameter("code", (object)code, ParameterType.GetOrPost);
            restRequest.AddParameter("grant_type", (object)"authorization_code", ParameterType.GetOrPost);
            restRequest.AddParameter("redirect_uri", (object)redirectUri, ParameterType.GetOrPost);
            Dictionary<string, string> data = this.client.Execute<Dictionary<string, string>>((IRestRequest)restRequest).Data;
            if (data.ContainsKey("error") || data.ContainsKey("error_description"))
                throw new OAuthException(data["error_description"], data["error"]);
            return new TokenResult()
            {
                AccessToken = data["access_token"],
                IdToken = data.ContainsKey("id_token") ? data["id_token"] : string.Empty
            };



            //end
            var token = client.ExchangeAuthorizationCodePerAccessToken(code, url);
            var profile = client.GetUserInfo(token);

            var user = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name", profile.Name),
                new KeyValuePair<string, object>("email", profile.Email),
                new KeyValuePair<string, object>("family_name", profile.FamilyName),
                new KeyValuePair<string, object>("gender", profile.Gender),
                new KeyValuePair<string, object>("given_name", profile.GivenName),
                new KeyValuePair<string, object>("nickname", profile.Nickname),
                new KeyValuePair<string, object>("picture", profile.Picture),
                new KeyValuePair<string, object>("user_id", profile.UserId),
                new KeyValuePair<string, object>("id_token", token.IdToken),
                new KeyValuePair<string, object>("access_token", token.AccessToken),
                new KeyValuePair<string, object>("connection", profile.Identities.First().Connection),
                new KeyValuePair<string, object>("provider", profile.Identities.First().Provider)

            };

            // NOTE: Uncomment the following code in order to include claims from associated identities
            //profile.Identities.ToList().ForEach(i =>
            //{
            //    user.Add(new KeyValuePair<string, object>(i.Connection + ".access_token", i.AccessToken));
            //    user.Add(new KeyValuePair<string, object>(i.Connection + ".provider", i.Provider));
            //    user.Add(new KeyValuePair<string, object>(i.Connection + ".user_id", i.UserId));
            //});

            // NOTE: uncomment this if you send roles
            // user.Add(new KeyValuePair<string, object>(ClaimTypes.Role, profile.ExtraProperties["roles"]));

            // NOTE: this will set a cookie with all the user claims that will be converted 
            //       to a ClaimsPrincipal for each request using the SessionAuthenticationModule HttpModule. 
            //       You can choose your own mechanism to keep the user authenticated (FormsAuthentication, Session, etc.)
            FederatedAuthentication.SessionAuthenticationModule.CreateSessionCookie(user);

            if (context.Request.QueryString["state"] != null && context.Request.QueryString["state"].StartsWith("ru="))
            {
                var state = HttpUtility.ParseQueryString(context.Request.QueryString["state"]);
                context.Response.Redirect(state["ru"], true);
            }

            context.Response.Redirect("/");
        }

        public bool IsReusable { get { return false; } }
    }
}