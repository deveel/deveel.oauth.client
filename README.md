# Deveel OAuth2 Client Library

A helper .NET library for authenticating client applications through one of the OAuth2 flows

# Why?

As silly as it seems, I had issues finding a generic library that I could use to obtain access tokens from _Identity Providers_, so that I could authenticate _.NET Core_ client applications to remote RESTful services.

I took a peek at the official **[OAuth](https://oauth.net)** reference website, in particular to the _[OAuth Libraries for .NET](https://oauth.net/code/dotnet/)_ page, finding the ones listed there pretty obsolete and discontinued.

Google and StackOverflow didn't help my quest, leading me to service-specific implementations

# What?

I ended up developing this little library in few minutes, trying to respect the standards provided by the [OAuth2 specifications](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-par), to be able to receive a token from one of the providers ([Auth0](https://auth0.com)) that is securing one of my services.

The intention of this artifact is not to be complicated, advanced or rich of functions, but rather to be helpful to those, like me, are looking for something generic and small: as such I deciced to publish it under [Apache Public License 2.0)(https://www.apache.org/licenses/LICENSE-2.0)
