using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using BasicBot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.BotBuilderSamples
{
    public class GetTypesDialog : ComponentDialog
    {
        public IStatePropertyAccessor<TypeState> GetTypeAccessor { get; }

        private const string TypesDialog = "typesDialog";
        private string sasUrl = ConfigurationManager.AppSettings["sasUrl"];
        private Storage _storage;

        private Dictionary<string, string> meatTypes = new Dictionary<string, string>()
        {
            { "Salami",  "/salami.jpg" },
            { "Chicken filet", "/turkey.jpg" },
            { "Turkey filet", "/turkey.jpg" },
            { "Ham", "/ham.jpg" },
        };

        private Dictionary<string, string> cheeseTypes = new Dictionary<string, string>()
        {
            { "Cheddar", ConfigurationManager.AppSettings["sasUrl"] + "/cheddar.jpg" },
            { "Gouda cheese", ConfigurationManager.AppSettings["sasUrl"] + "/gouda.jpg" },
            { "Young cheese", ConfigurationManager.AppSettings["sasUrl"] + "/youngcheese.png" },
            { "Old cheese", ConfigurationManager.AppSettings["sasUrl"] + "/oldcheese.jpg" },
        };

        private Dictionary<string, string> breadTypes = new Dictionary<string, string>()
        {
            { "White bread", ConfigurationManager.AppSettings["sasUrl"] + "/whitebread.jpg" },
            { "Dark bread", ConfigurationManager.AppSettings["sasUrl"] + "/darkbread.jpg" },
            { "Spelt bread", ConfigurationManager.AppSettings["sasUrl"] + "/speltbread.jpg" },
            { "Whole Wheat Bread", ConfigurationManager.AppSettings["sasUrl"] + "/wholewheatbread.jpg" },
        };
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GetTypesDialog"/> class.
        /// </summary>
        public GetTypesDialog(IStatePropertyAccessor<TypeState> userProfileStateAccessor, ILoggerFactory loggerFactory, Storage storage)
            : base(nameof(GetTypesDialog))
        {
            _storage = storage;
            GetTypeAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    CheckTypeInput,
            };
            AddDialog(new WaterfallDialog(TypesDialog, waterfallSteps));
        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var typeState = await GetTypeAccessor.GetAsync(stepContext.Context, () => null);
            if (typeState == null)
            {
                var typeStateOptions = stepContext.Options as TypeState;
                if (typeStateOptions != null)
                {
                    await GetTypeAccessor.SetAsync(stepContext.Context, typeStateOptions);
                }
                else
                {
                    await GetTypeAccessor.SetAsync(stepContext.Context, new TypeState());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> CheckTypeInput(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var context = stepContext.Context;
            var typeState = await GetTypeAccessor.GetAsync(context);
            switch (typeState.CurrentType)
            {
                case "Bread":
                    await context.SendActivityAsync($"You're asking what kind of breads are available for a good tosti?");
                    await ShowList(context, breadTypes, cancellationToken);
                    break;
                case "Meat":
                    await context.SendActivityAsync($"I love meat just like you. Here's a list of meat you can put on your tosti:");
                    await ShowList(context, meatTypes, cancellationToken);
                    break;
                default:
                case "Cheese":
                    await context.SendActivityAsync($"Cheese lover? Here are some cheeses you can use on your tosti:");
                    await ShowList(context, cheeseTypes, cancellationToken);
                    break;
            }

            return await stepContext.EndDialogAsync();
        }

        private async Task<ResourceResponse> ShowList(ITurnContext context, Dictionary<string, string> list, CancellationToken cancellationToken)
        {
            List<Attachment> attachments = new List<Attachment>();
            foreach (KeyValuePair<string, string> item in list)
            {
                var hero = new HeroCard(
                   title: item.Key,
                   images: new CardImage[] { new CardImage(url: _storage.storageUrl + item.Value + _storage.sasUrl) },
                   buttons: null
                   )
               .ToAttachment();
                attachments.Add(hero);
            }
            var activity = MessageFactory.Carousel(attachments);

            return await context.SendActivityAsync(activity, cancellationToken: cancellationToken);
        }
    }
}
