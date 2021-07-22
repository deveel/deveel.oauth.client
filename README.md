# Deveel OAuth2 Client Library

A helper .NET library for authenticating client applications through one of the OAuth2 flows

# Why?

As silly as it seems, I had issues finding a generic library that I could use to obtain access tokens from _Identity Providers_, so that I could authenticate _.NET Core_ client applications to remote RESTful services.

I took a peek at the official **[OAuth](https://oauth.net)** reference website, in particular to the _[OAuth Libraries for .NET](https://oauth.net/code/dotnet/)_ page, finding the ones listed there pretty obsolete and discontinued.

Google and StackOverflow didn't help my quest, leading me to service-specific implementations

# What?

I ended up developing this little library in few minutes, trying to respect the standards provided by the [OAuth2 specifications](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-par), to be able to receive a token from one of the providers ([Auth0](https://auth0.com)) that is securing one of my services.

The intention of this artifact is not to be complicated, advanced or rich of functions, but rather to be helpful to those, like me, are looking for something generic and small: as such I deciced to publish it under [Apache Public License 2.0)(https://www.apache.org/licenses/LICENSE-2.0)

The core library is developed as a **[.NET Standard 2.1](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md)** artifact and can be downloaded from NuGet

# How?

## Installation

## Usage

### OAuth2 Client Credentials Flow

When an application tries to access resources provided by a service (the _resource server_), the process of granting is not interactive, like in the case of the _Authorization Code_ flow: this means that the credentials of the machine are sent alltogether in one request.

``` csharp
public static async Task Main(string[] args) {
  var options = new OAuthClientOptions {
    TokenUrl = "https://secure.example.com/oauth/token"
  };
  
  const string clientId = "<client id>";
  const string clientSecret = "<client super-secret word>";
  
  OAuthAccessToken token;
  
  using(var client = new OAuthAuthenticationClient(options)) {
    token = await client.RequestAccessTokenAsync(new OAuthClientCredentials(clientId, clientSecret) {
      Scopes = new[]{ "read:user" }
    });
  }
  
  Console.Out.WriteLine("Obtained a new Access Token");
  Console.Out.WriteLine($"Token Type: {token.TokenType}");
  Console.Out.WriteLine($"Access Token: {token.AccessToken}");
  Console.Out.WriteLine($"Expires At: {token.ExpiresAt}");
}
```

### OAuth2 Refresh Token

### OAuth2 Authorization Code Flow

The _Authorization Code_ flow is an interactive process that ensures the credentials of the consumer of a client application (a website or a mobile application) are not known to the application itself: this allows the user to own the resources provided by the remote service, sharing with the application a temporary code to let it to access it.

This process consists of two phases:

1. Obtaining the URL where to redirect the user of the application
2. Use the code returned by the authentication server to the user to obtain an access token

``` csharp
var options = new OAuthClientOptions {
  AuthorizationUrl = "https://secure.example.com/authorize",
  TokenUrl = "https://secure.example.com/token"
};

var client = new OAuthAuthenticationClient(options);


// Phase 1 - Obtain the Redirect URL

const string clientId = "<client id>";
const string clientSecret = "<client super-secret word>";
  
var redirectUri = new Uri("https://example.com/authenticate"); 

var authUrl = client.GetRedirectUri(new OAuthAuthorizationCodeInfo(clientId, redirectUri) {
  State = "ye36223",
  Scopes = new [] { "photos" }
});
  
...
  
// Phase 2 - Obtain the access token

var code = "<code returned by the server to the user>";
var accessToken = client.RequestAccessToken(new OAuthAuthorizationCode(clientId, clientSecret, code) {
   RedirectUri = redirectUri
});

```


### Microsoft Rest Service Credentials

The library provides an implementation of the _Microsoft Rest Client Runtime_ `ServiceClientCredentials` ([see this](https://docs.microsoft.com/en-us/dotnet/api/microsoft.rest.serviceclientcredentials?view=azure-dotnet)) class, ready to be used in clients generated through the _[AuthoRest](https://github.com/azure/autorest)_ clients

# Contribute

_Do you feel something is missing and you want to improve this effort?_

Feel free to open a Pull Request and I will review it and eventually approve it :)

# Contact

For further information and contact requests, feel free to drop me a message at _antonello at deveel dot com_
