// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Demonstrates the following concepts:
    /// - Use a subclass of ComponentDialog to implement a multi-turn conversation
    /// - Use a Waterflow dialog to model multi-turn conversation flow
    /// - Use custom prompts to validate user input
    /// - Store conversation and user state.
    /// </summary>
    public class CheckTostiDialog : ComponentDialog
    {
        static HttpClient client = new HttpClient();
        private const string checkTostiDialog = "checkTostiDialog";
        /// <summary>
        /// Initializes a new instance of the <see cref="GreetingDialog"/> class.
        /// </summary>
        /// <param name="botServices">Connected services used in processing.</param>
        /// <param name="botState">The <see cref="UserState"/> for storing properties at user-scope.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> that enables logging and tracing.</param>
        public CheckTostiDialog(ILoggerFactory loggerFactory)
            : base(nameof(CheckTostiDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                CheckUserImageForTosti,
            };
            AddDialog(new WaterfallDialog(checkTostiDialog, waterfallSteps));
        }

        // Helper function to greet user with information in GreetingState.
        private async Task<DialogTurnResult> CheckUserImageForTosti(WaterfallStepContext stepContext,CancellationToken token)
        {
            var context = stepContext.Context;
            var prediction = await CheckIfImageIsTosti(stepContext);
            if(prediction.TagName == "tosti")
            {
                await context.SendActivityAsync($"That looks like a delicious tosti!");
            }
            else if(prediction.TagName == "pizza")
            {
                await context.SendActivityAsync($"I could identify a pizza when I see one... That my friend, is a pizza.");
            }
            else
            {
                await context.SendActivityAsync($"That doesn't seems like a tosti to me...");
            }

            return await stepContext.EndDialogAsync();
        }

        private async Task<PredictionModel> CheckIfImageIsTosti(DialogContext context)
        {
            CustomVisionPredictionClient endpoint = new CustomVisionPredictionClient()
            {
                ApiKey = "d646f6731d0b4697885148ca74ccb5a6",
                Endpoint = "https://westeurope.api.cognitive.microsoft.com"
            };

            var attachment = context.Context.Activity.Attachments.First();
            using (HttpClient httpClient = new HttpClient())
            {
                var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;

                var content = await responseMessage.Content.ReadAsStreamAsync();
                if (content != null)
                {
                        var result = endpoint.ClassifyImage(Guid.Parse("36ea8b59-240e-4596-a9d5-b0b043e302cb"), "Tosti Recognizer2", content);
                        foreach (var c in result.Predictions)
                        {
                            Console.WriteLine($"\t{c.TagName}: {c.Probability:P1}");
                        }

                        return result.Predictions[0];

                }
                else
                {
                    return null;
                }
            }
        }
    }
}
