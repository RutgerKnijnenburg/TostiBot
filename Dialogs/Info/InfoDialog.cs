using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using BasicBot;
using BasicBot.Dialogs.Info;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.BotBuilderSamples
{
    public class InfoDialog : ComponentDialog
    {
        public IStatePropertyAccessor<InfoState> InfoAccessor { get; }

        private const string infoDialog = "infoDialog";
        private string historyText = "The sandwich's first recorded appearance on a Paris café menu was in 1910. Its earliest mention in literature appears to be in volume two of Proust's In Search of Lost Time in 1918. A ham and cheese sandwich snack, very similar to the croque-monsieur though not containing any béchamel or egg, is called a tosti in the Netherlands, and toast (pronounced tost) in Italy and Greece.";
        private string recipeText = "A croque monsieur is traditionally made with boiled ham between slices of brioche-like pain de mie topped with grated cheese and slightly salted and peppered, which is baked in an oven or fried in a frying pan. The dish can also be made with normal butter bread,[clarification needed] with a soft crust. Instead of the butter bread, the bread may optionally be browned by grilling before being dipped in beaten egg. Traditionally, Emmental or Gruyère is used, or sometimes Comté cheese as well. Some brasseries also add Béchamel sauce.";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetTypesDialog"/> class.
        /// </summary>
        public InfoDialog(IStatePropertyAccessor<InfoState> infoStateAccessor, ILoggerFactory loggerFactory)
            : base(nameof(InfoDialog))
        {
            InfoAccessor = infoStateAccessor ?? throw new ArgumentNullException(nameof(infoStateAccessor));
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    CheckTypeInput,
            };
            AddDialog(new WaterfallDialog(infoDialog, waterfallSteps));
        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var typeState = await InfoAccessor.GetAsync(stepContext.Context, () => null);
            if (typeState == null)
            {
                var infoStateOptions = stepContext.Options as InfoState;
                if (infoStateOptions != null)
                {
                    await InfoAccessor.SetAsync(stepContext.Context, infoStateOptions);
                }
                else
                {
                    await InfoAccessor.SetAsync(stepContext.Context, new InfoState());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> CheckTypeInput(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var context = stepContext.Context;
            var typeState = await InfoAccessor.GetAsync(context);
            switch (typeState.CurrentEntity)
            {
                case "History":
                    await context.SendActivityAsync($"Here's what Wikipedia says:");
                    await context.SendActivityAsync(historyText);
                    break;
                case "Recipe":
                    await context.SendActivityAsync($"Here's what Wikipedia tells me:");
                    await context.SendActivityAsync(recipeText);
                    break;
                default:
                    await ShowList(context, cancellationToken);
                    break;
            }
            return await stepContext.EndDialogAsync();
        }

        private async Task<ResourceResponse> ShowList(ITurnContext context, CancellationToken cancellationToken)
        {
            Activity replyToConversation = context.Activity.CreateReply("Should go to conversation");
            AdaptiveCard card = new AdaptiveCard();

            // Add text to the card.
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Tosti information",
                Size = AdaptiveTextSize.Large,
                Weight = AdaptiveTextWeight.Bolder,
            });

            // Add text to the card.
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "What would you like to know?",
            });

            // Add buttons to the card.
            card.Actions.Add(new AdaptiveSubmitAction()
            {
                Data = "What is the history of the tosti",
                Title = "History",
            });

            card.Actions.Add(new AdaptiveSubmitAction()
            {
                Data = "What is the recipe for a tosti",
                Title = "Recipe",
            });

            // Create the attachment.
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            replyToConversation.Attachments.Add(attachment);
            return await context.SendActivityAsync(replyToConversation, cancellationToken: cancellationToken);
        }
    }
}
