﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector.Authentication
{
    public sealed class EnterpriseChannelValidation
    {
        /// <summary>
        /// TO BOT FROM ENTERPRISE CHANNEL: Token validation parameters when connecting to a bot.
        /// </summary>
        public static readonly TokenValidationParameters ToBotFromEnterpriseChannelTokenValidationParameters =
            new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { AuthenticationConstants.ToBotFromChannelTokenIssuer },

                // Audience validation takes place in JwtTokenExtractor
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
            };

        /// <summary>
        /// Validate the incoming Auth Header as a token sent from a Bot Framework Channel Service.
        /// </summary>
        /// <param name="authHeader">The raw HTTP header in the format: "Bearer [longString]".</param>
        /// <param name="credentials">The user defined set of valid credentials, such as the AppId.</param>
        /// <param name="channelProvider">The user defined configuration for the channel.</param>
        /// <param name="serviceUrl">The service url from the request.</param>
        /// <param name="httpClient">Authentication of tokens requires calling out to validate Endorsements and related documents. The
        /// HttpClient is used for making those calls. Those calls generally require TLS connections, which are expensive to
        /// setup and teardown, so a shared HttpClient is recommended.</param>
        /// <param name="channelId">The ID of the channel to validate.</param>
        /// <returns>ClaimsIdentity.</returns>
        public static async Task<ClaimsIdentity> AuthenticateChannelToken(string authHeader, ICredentialProvider credentials, IChannelProvider channelProvider, string serviceUrl, HttpClient httpClient, string channelId, AuthenticationConfiguration authConfig = null)
        {
            var channelService = await channelProvider.GetChannelServiceAsync().ConfigureAwait(false);

            var tokenExtractor = new JwtTokenExtractor(
                httpClient,
                ToBotFromEnterpriseChannelTokenValidationParameters,
                string.Format(AuthenticationConstants.ToBotFromEnterpriseChannelOpenIdMetadataUrlFormat, channelService),
                AuthenticationConstants.AllowedSigningAlgorithms);

            var identity = await tokenExtractor.GetIdentityAsync(authHeader, channelId, authConfig?.RequiredEndorsements).ConfigureAwait(false);

            await ValidateIdentity(identity, credentials, serviceUrl).ConfigureAwait(false);

            return identity;
        }

        public static async Task ValidateIdentity(ClaimsIdentity identity, ICredentialProvider credentials, string serviceUrl)
        {
            if (identity == null)
            {
                // No valid identity. Not Authorized.
                throw new UnauthorizedAccessException();
            }

            if (!identity.IsAuthenticated)
            {
                // The token is in some way invalid. Not Authorized.
                throw new UnauthorizedAccessException();
            }

            // Now check that the AppID in the claimset matches
            // what we're looking for. Note that in a multi-tenant bot, this value
            // comes from developer code that may be reaching out to a service, hence the
            // Async validation.

            // Look for the "aud" claim, but only if issued from the Bot Framework
            Claim audienceClaim = identity.Claims.FirstOrDefault(
                c => c.Issuer == AuthenticationConstants.ToBotFromChannelTokenIssuer && c.Type == AuthenticationConstants.AudienceClaim);

            if (audienceClaim == null)
            {
                // The relevant audience Claim MUST be present. Not Authorized.
                throw new UnauthorizedAccessException();
            }

            // The AppId from the claim in the token must match the AppId specified by the developer.
            // In this case, the token is destined for the app, so we find the app ID in the audience claim.
            string appIdFromClaim = audienceClaim.Value;
            if (string.IsNullOrWhiteSpace(appIdFromClaim))
            {
                // Claim is present, but doesn't have a value. Not Authorized.
                throw new UnauthorizedAccessException();
            }

            if (!await credentials.IsValidAppIdAsync(appIdFromClaim))
            {
                // The AppId is not valid. Not Authorized.
                throw new UnauthorizedAccessException($"Invalid AppId passed on token: {appIdFromClaim}");
            }

            if (serviceUrl != null)
            {
                var serviceUrlClaim = identity.Claims.FirstOrDefault(claim => claim.Type == AuthenticationConstants.ServiceUrlClaim)?.Value;
                if (string.IsNullOrWhiteSpace(serviceUrlClaim))
                {
                    // Claim must be present. Not Authorized.
                    throw new UnauthorizedAccessException();
                }

                if (!string.Equals(serviceUrlClaim, serviceUrl))
                {
                    // Claim must match. Not Authorized.
                    throw new UnauthorizedAccessException();
                }
            }
        }
    }
}
