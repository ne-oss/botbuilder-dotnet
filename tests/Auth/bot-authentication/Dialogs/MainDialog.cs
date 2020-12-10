﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using AuthenticationBot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class MainDialog : LogoutDialog
    {
#pragma warning disable SA1401 // Fields should be private
        protected readonly ILogger Logger;
#pragma warning restore SA1401 // Fields should be private

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            Logger = logger;

            var settings = new Test_SignInPromptSettings
            {
                ConnectionName = ConnectionName,
                Text = "Please Sign In",
                Title = "Sign In",
                Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
            };

            //AddDialog(new OAuthPrompt(
            //    nameof(OAuthPrompt),
            //    new OAuthPromptSettings
            //    {
            //        ConnectionName = ConnectionName,
            //        Text = "Please Sign In",
            //        Title = "Sign In",
            //        Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
            //    }));

            AddDialog(new Test_SignInPrompt(settings));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(
                new WaterfallDialog(
                    nameof(WaterfallDialog),
#pragma warning disable SA1118 // Parameter should not span multiple lines
                    new WaterfallStep[]
                    {
                        PromptStepAsync,
                        LoginStepAsync,
                        DisplayTokenPhase1Async,
                        DisplayTokenPhase2Async,
                    }));
#pragma warning restore SA1118 // Parameter should not span multiple lines

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(Test_SignInPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("You are now logged in."), cancellationToken);
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to view your token?") }, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);

            var result = (bool)stepContext.Result;
            if (result)
            {
                // Call the prompt again because we need the token. The reasons for this are:
                // 1. If the user is already logged in we do not need to store the token locally in the bot and worry
                // about refreshing it. We can always just call the prompt again to get the token.
                // 2. We never know how long it will take a user to respond. By the time the
                // user responds the token may have expired. The user would then be prompted to login again.
                //
                // There is no reason to store the token locally in the bot because we can always just call
                // the OAuth prompt to get the token or get a new token if needed.
                return await stepContext.BeginDialogAsync(nameof(Test_SignInPrompt), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is your token {tokenResponse.Token}"), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
