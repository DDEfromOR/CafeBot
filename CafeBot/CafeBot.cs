using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace CafeBot
{
    public class CafeBot : IBot
    {
        private readonly DialogSet dialogs;
        static DateTime reservationDate;
        static int partySize;
        static string reservationName;
        const string DATETIMEPROMPT = "dateTimePrompt";
        const string PARYSIZEPROMPT = "partySizePrompt";
        const string RESERVATIONNAMEPROMPT = "reservationNamePrompt";


        public CafeBot()
        {
            dialogs = new DialogSet();
            dialogs.Add("reserveTable", new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await dc.Context.SendActivity("Welcome to the reservation service.");
                    await dc.Prompt(DATETIMEPROMPT, "Please provide a reservation date and time.");
                },
                async (dc, args, next) =>
                {
                    var dateTimeResult = ((DateTimeResult)args).Resolution.First();
                    reservationDate = Convert.ToDateTime(dateTimeResult.Value);

                    await dc.Prompt(PARYSIZEPROMPT, "How many people are in your party?");
                },
                async (dc, args, next) =>
                {
                    partySize = (int)args["Value"];

                    await dc.Prompt(RESERVATIONNAMEPROMPT, "Whose name will this be under?");
                },
                async (dc, args, next) =>
                {
                    reservationName = (string)args["Text"];
                    string msg = "Reservation confirmed. Reservation details - " +
                        $"\nDate/Time: {reservationDate.ToString()} " +
                        $"\nParty size: {partySize.ToString()} " +
                        $"\nReservation name: {reservationName}";

                    await dc.Context.SendActivity(msg);
                    await dc.End();
                }
            });

            dialogs.Add(DATETIMEPROMPT, new Microsoft.Bot.Builder.Dialogs.DateTimePrompt(Culture.English));
            dialogs.Add(PARYSIZEPROMPT, new Microsoft.Bot.Builder.Dialogs.NumberPrompt<int>(Culture.English));
            dialogs.Add(RESERVATIONNAMEPROMPT, new Microsoft.Bot.Builder.Dialogs.TextPrompt());
        }

        public async Task OnTurn(ITurnContext context)
        {
            if(context.Activity.Type == ActivityTypes.Message)
            {
                var state = ConversationState<Dictionary<string, object>>.Get(context);
                var dc = dialogs.CreateContext(context, state);
                await dc.Continue();

                if(!context.Responded)
                {
                    if (context.Activity.Text.ToLowerInvariant().Contains("reserve table"))
                    {
                        await dc.Begin("reserveTable");
                    }
                    else
                    {
                        await context.SendActivity($"You said '{context.Activity.Text}'");
                    }
                }
            }
        }
    }
}
