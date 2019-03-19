// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class TokenResponse
    {
        /// <summary>
        /// Initializes a new instance of the TokenResponse class.
        /// </summary>
        public TokenResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TokenResponse class.
        /// </summary>
        public TokenResponse(string channelId = default(string), string connectionName = default(string), string token = default(string), string expiration = default(string))
        {
            ChannelId = channelId;
            ConnectionName = connectionName;
            Token = token;
            Expiration = expiration;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "channelId")]
        public string ChannelId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "expiration")]
        public string Expiration { get; set; }

    }
}