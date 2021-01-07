﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Moq;
using Xunit;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Tests.RecognizerTelemetryUtils;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [CollectionDefinition("Dialogs.Adaptive.Recognizers")]
    public class CrossTrainedRecognizerSetTests : IClassFixture<ResourceExplorerFixture>
    {
        private static readonly Lazy<CrossTrainedRecognizerSet> Recognizers = new Lazy<CrossTrainedRecognizerSet>(() =>
        {
            return new CrossTrainedRecognizerSet()
            {
                Recognizers = new List<Recognizer>()
                {
                    new RegexRecognizer()
                    {
                        Id = "x",
                        Intents = new List<IntentPattern>()
                        {
                            new IntentPattern("DeferToRecognizer_y", CrossTrainText),
                            new IntentPattern("x", "x")
                        }
                    },
                    new RegexRecognizer()
                    {
                        Id = "y",
                        Intents = new List<IntentPattern>()
                        {
                            new IntentPattern("y", CrossTrainText),
                            new IntentPattern("y", "y")
                        }
                    },
                    new RegexRecognizer()
                    {
                        Id = "z",
                        Intents = new List<IntentPattern>()
                        {
                            new IntentPattern("z", CrossTrainText),
                            new IntentPattern("z", "z")
                        }
                    },
                }
            };
        });

        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public CrossTrainedRecognizerSetTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(CrossTrainedRecognizerSetTests));
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_DoubleDefer()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_CircleDefer()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_DoubleIntent()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_NoneWithIntent()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_AllNone()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_NoneIntentWithEntities()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }
        
        [Fact]
        public async Task CrossTrainedRecognizerSetTests_Empty()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_Telemetry_LogsPii_WhenTrue()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizers = Recognizers.Value;
            recognizers.TelemetryClient = telemetryClient.Object;
            recognizers.LogPersonalInformation = true;

            await RecognizeIntentAndValidateTelemetry(CrossTrainText, recognizers, telemetryClient, callCount: 1);
            await RecognizeIntentAndValidateTelemetry("x", recognizers, telemetryClient, callCount: 2);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_Telemetry_DoesNotLogPii_WhenFalse()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizers = Recognizers.Value;
            recognizers.TelemetryClient = telemetryClient.Object;
            recognizers.LogPersonalInformation = false;

            await RecognizeIntentAndValidateTelemetry(CrossTrainText, recognizers, telemetryClient, callCount: 1);
            await RecognizeIntentAndValidateTelemetry("x", recognizers, telemetryClient, callCount: 2);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_LogPii_IsFalseByDefault()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizers = Recognizers.Value;
            recognizers.TelemetryClient = telemetryClient.Object;
            var dc = TestUtils.CreateContext(CrossTrainText);

            var (logPersonalInformation, _) = recognizers.LogPersonalInformation.TryGetValue(dc.State);
            Assert.False(logPersonalInformation);

            var result = await recognizers.RecognizeAsync(dc, dc.Context.Activity, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Single(result.Intents);
        }
    }
}
