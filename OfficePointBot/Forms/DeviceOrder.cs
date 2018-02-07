using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;

namespace OfficePointBot.Forms
    {
        public enum AppOptions
        {
            None,
            [Terms(new string[] { "crap apps", "crappy" })]
            LotsOfCrummyApps,
            ReallyTrendyApps,
            FreeGames,
            ProfessionalApps,
            BankingApps
        };

        public enum DeviceType { Phone, Tablet, Desktop };
        public enum IntellgenceLevel { Low, Average, High };
        public enum AgeRange
        {
            [Describe("I'm just a teenager")]
            Teenager,

            [Describe("I'm young and trendy")]
            Millenial,

            [Describe("I've lived life and know things")]
            SeasonedAdult
        };

    [Serializable]
    public class DeviceOrder
    {
        [Prompt("What kind of device are you looking for? {||}")]
        public DeviceType? DeviceType;

        [Optional]
        [Prompt("What's your IQ?? {||}")]
        public IntellgenceLevel? IntellgenceLevel;

        [Prompt("How old are you? {||}")]
        public AgeRange? Age;

        [Prompt("What kind of apps do you want support for? {||}")]
        [Template(TemplateUsage.NotUnderstood, "What does \"{0}\" mean???", ChoiceStyle = ChoiceStyleOptions.Auto)]
        [Describe("Types of apps")]
        public List<AppOptions> AppOptions;

        public static IForm<DeviceOrder> BuildForm()
        {
            return new FormBuilder<DeviceOrder>()
                    .Message("Welcome to the personality-based device recommendation bot!")
                    .Confirm("Do you really want to buy this tablet?")                     
                    .OnCompletion(async (context, state) =>
                    {
                        var reply = Common.CommonConversation.CurrentActivity.CreateReply(GetDeviceRecommendationMessage(context, state));

                        await context.PostAsync(reply);

                    })
                    .Build();
        }

        private static string GetDeviceRecommendationMessage(IDialogContext context, DeviceOrder state)
        {
            string recommendationLabel = "For your personality and app preferences, we recommend a";

            if (state.Age < AgeRange.SeasonedAdult)
            {
                if (state.IntellgenceLevel == Forms.IntellgenceLevel.Low)
                {
                    recommendationLabel += "n iOS";
                }
                else if (state.IntellgenceLevel == Forms.IntellgenceLevel.High)
                {
                    recommendationLabel += " Windows";
                }
                else
                {
                    recommendationLabel += "n Android";
                }
            }
            else
            {
                if (state.IntellgenceLevel == Forms.IntellgenceLevel.Low)
                {
                    recommendationLabel += " Windows";
                }
                else if (state.IntellgenceLevel == Forms.IntellgenceLevel.High)
                {
                    recommendationLabel += " Windows";
                }
                else
                {
                    recommendationLabel += "n Android";
                }
            }

            recommendationLabel += " " + state.DeviceType + ".";

            return recommendationLabel;
        }

    }
}