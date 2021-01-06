﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Tests.ColorAndCodeUtils;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [CollectionDefinition("Dialogs.Adaptive.Recognizers")]
    public class RecognizerSetTests : IClassFixture<ResourceExplorerFixture>
    {
        private static readonly Lazy<RecognizerSet> Recognizers = new Lazy<RecognizerSet>(() =>
        {
            return new RecognizerSet()
            {
                Recognizers = new List<Recognizer>()
                {
                    new RegexRecognizer()
                    {
                        Id = "CodeRecognizer",
                        Intents = new List<IntentPattern>()
                        {
                            new IntentPattern("codeIntent", "(?<code>[a-z][0-9])"),
                        },
                        Entities = new EntityRecognizerSet()
                        {
                            new AgeEntityRecognizer(),
                            new NumberEntityRecognizer(),
                            new PercentageEntityRecognizer(),
                            new PhoneNumberEntityRecognizer(),
                            new TemperatureEntityRecognizer()
                        }
                    },
                    new RegexRecognizer()
                    {
                        Id = "ColorRecognizer",
                        Intents = new List<IntentPattern>()
                        {
                            new IntentPattern("colorIntent", "(?i)(color|colour)")
                        },
                        Entities = new EntityRecognizerSet()
                        {
                            new UrlEntityRecognizer(),
                            new RegexEntityRecognizer() { Name = "color", Pattern = "(?i)(red|green|blue|purple|orange|violet|white|black)" },
                            new RegexEntityRecognizer() { Name = "backgroundColor", Pattern = "(?i)(back|background)" },
                            new RegexEntityRecognizer() { Name = "foregroundColor", Pattern = "(?i)(foreground|front) {color}" }
                        }
                    }
                }
            };
        });

        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public RecognizerSetTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(RecognizerSetTests));
        }

        [Fact]
        public async Task RecognizerSetTests_Merge()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task RecognizerSetTests_None()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task RecognizerSetTests_Merge_LogsTelemetry_WhenLogPiiTrue()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizers = Recognizers.Value;
            recognizers.TelemetryClient = telemetryClient.Object;
            recognizers.LogPersonalInformation = true;

            await RecognizeIntentAndValidateTelemetry(CodeIntentText, recognizers, telemetryClient, callCount: 1);
            await RecognizeIntentAndValidateTelemetry(ColorIntentText, recognizers, telemetryClient, callCount: 2);

            // Test custom activity
            await RecognizeIntentAndValidateTelemetry_WithCustomActivity(CodeIntentText, recognizers, telemetryClient, callCount: 3);
            await RecognizeIntentAndValidateTelemetry_WithCustomActivity(ColorIntentText, recognizers, telemetryClient, callCount: 4);
        }
        
        [Fact]
        public async Task RecognizerSetTests_Merge_LogsTelemetry_WhenLogPiiFalse()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizers = Recognizers.Value;
            recognizers.TelemetryClient = telemetryClient.Object;
            recognizers.LogPersonalInformation = false;

            await RecognizeIntentAndValidateTelemetry(CodeIntentText, recognizers, telemetryClient, callCount: 1);
            await RecognizeIntentAndValidateTelemetry(ColorIntentText, recognizers, telemetryClient, callCount: 2);

            // Test custom activity
            await RecognizeIntentAndValidateTelemetry_WithCustomActivity(CodeIntentText, recognizers, telemetryClient, callCount: 3);
            await RecognizeIntentAndValidateTelemetry_WithCustomActivity(ColorIntentText, recognizers, telemetryClient, callCount: 4);
        }

        [Fact]
        public async Task RecognizerSetTests_LogPii_FalseByDefault()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = Recognizers.Value;
            recognizer.TelemetryClient = telemetryClient.Object;
            var dc = TestUtils.CreateContext("Salutations!");
            var (logPersonalInformation, _) = recognizer.LogPersonalInformation.TryGetObject(dc.State);

            Assert.Equal(false, logPersonalInformation);

            // Test with DC
            await RecognizeIntentAndValidateTelemetry(CodeIntentText, recognizer, telemetryClient, callCount: 1);
            await RecognizeIntentAndValidateTelemetry(ColorIntentText, recognizer, telemetryClient, callCount: 2);

            // Test custom activity
            await RecognizeIntentAndValidateTelemetry_WithCustomActivity(CodeIntentText, recognizer, telemetryClient, callCount: 3);
            await RecognizeIntentAndValidateTelemetry_WithCustomActivity(ColorIntentText, recognizer, telemetryClient, callCount: 4);
        }
    }
}
