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
            }
        };
}