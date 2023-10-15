using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new("auctionApp", "Auction app full access")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new()
            {
                ClientId = "postman",
                ClientName = "postman",
                AllowedScopes = { "openid", "profile", "auctionApp" },
                RedirectUris =
                {
                    "https://www.getpostman.com/oauth2/callback"
                }, //redirect anywhere because since it's for postman, it wouldn't redirect us anywhere
                ClientSecrets = new[]
                {
                    new Secret("NotASecret".Sha256())
                    // this is the secret we're going to use in postman to send up that objects that we need (the client's secrets)
                    // that what we're going to send up to request our token
                },
                AllowedGrantTypes = { GrantType.ResourceOwnerPassword }
            },

            new()
            {
                ClientId = "nextApp",
                ClientName = "nextApp",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials, // this means that our client is going to
                // be able to securely talk internally from inside our network to identity server inside our network, and
                // be issued with the access token without the actual browser involvement, so we don't send the secret to 
                // our browser or client's browser
                RequirePkce =
                    false, // for this app is just gonna be a browser based version of our client application. Now, if
                // we were making a mobile app, then we wouldn't have or would not be able to use nextApp for that, we would
                // need to use something else because next app is just for browser based web application
                // if we were developing a mobile app, then we couldn't use "CodeAndClientCredentials", we would only use 
                // code and we would have to use a different authentication flow called Pkce because we would not be able to
                // store a secret in a Reactive Native App
                RedirectUris = { "http://localhost:3000/api/auth/callback/id-server" },
                AllowOfflineAccess = true, // this is so that we can enable refresh token functionality
                AllowedScopes = {"openid", "profile", "auctionApp"},
                AccessTokenLifetime = 3600*24*30
            }
        };
}