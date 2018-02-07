﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using OfficePointBot.Common;
using Microsoft.Bot.Builder.Dialogs;
using System.Text.RegularExpressions;

namespace OfficePointBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
            ICredentialStore _credentialStore;
            public MessagesController()
            {
            _credentialStore = ConversationTokens.Store;
            }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            Common.CommonConversation.Connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            Common.CommonConversation.CurrentActivity = activity;

            Activity reply = null;

            if (activity.Type == ActivityTypes.Message)
            {
                var userId = Common.CommonConversation.CurrentActivity?.From?.Id;
                var token = _credentialStore.GetToken(userId);

                if (string.IsNullOrEmpty(token))
                {
                    var loginUri = new Uri(Common.CoreConstants.AuthenticationHostUrl + $"/api/auth/home?UserId={userId}");
                    string prompt = $"Please authenticate your account at {loginUri.ToString()} to associate your user identity to your Microsoft Id.";
                    reply = Common.CommonConversation.CurrentActivity.CreateReply(prompt);

                    await Common.CommonConversation.Connector.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                    var userInput = await Helpers.InputHelper.ParseUserInput(Common.CommonConversation.CurrentActivity.Text);

                    if (userInput.intents.Where(w => w.score > .5).Count() <= 0)
                    {
                        await Conversation.SendAsync(activity, () => new Dialogs.SimpleAlarmDialog());
                    }
                    else
                    {
                        UserIntentType userIntent = UserIntentType.None;

                        Enum.TryParse(userInput.intents.First().intent, true, out userIntent);

                        string message = "You selected " + userIntent.ToString();

                        switch (userIntent)
                        {
                            case UserIntentType.RandomFact:

                                reply = await Helpers.ReplyHelper.CreateFactActivityAsync();

                                break;
                            case UserIntentType.TrendingNews:

                                reply = await Helpers.ReplyHelper.CreateNewsResultsActivity(Common.CommonConversation.Connector, Common.CommonConversation.CurrentActivity);

                                break;
                            case UserIntentType.RandomPassword:

                                reply = Common.CommonConversation.CurrentActivity.CreateReply(message);

                                break;
                            case UserIntentType.SalesBreakdown:

                                reply = Helpers.ReplyHelper.CreateSalesBreakdownActivity(userInput.entities.FirstOrDefault());

                                break;
                            case UserIntentType.PhoneCall:

                                var selectedEntity = userInput.entities.FirstOrDefault();

                                string selectedEntityValue = (selectedEntity == null) ? "None" : selectedEntity.entity;

                                PhoneCallType callType = PhoneCallType.None;

                                Enum.TryParse(selectedEntityValue.Replace(" ", ""), true, out callType);

                                message = (selectedEntity == null) ? "No one will be calling you." : selectedEntity.entity + " will call you shortly.";

                                reply = Common.CommonConversation.CurrentActivity.CreateReply(message);

                                break;
                            case UserIntentType.JustBacon:

                                reply = Common.CommonConversation.CurrentActivity.CreateReply(null);

                                reply.Recipient = Common.CommonConversation.CurrentActivity.From;
                                reply.Type = "message";
                                reply.Attachments = new List<Attachment>();

                                reply.Attachments.Add(new Attachment()
                                {
                                    ContentUrl = "http://www.meh.ro/wp-content/uploads/2010/08/meh.ro4999.jpg",
                                    ContentType = "image/jpeg",
                                    Name = "Delicious Bacon",

                                });

                                break;
                            default:

                                await Conversation.SendAsync(activity, () => new Dialogs.SimpleAlarmDialog());

                                break;
                        }
                    }
                    await Common.CommonConversation.Connector.Conversations.ReplyToActivityAsync(reply);
                }


            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        
        private static IDialog<OfficePointBot.Forms.DeviceOrder> MakeDeviceOrderDialog()
        {
            return Chain.From(() => Microsoft.Bot.Builder.FormFlow.FormDialog.FromForm(OfficePointBot.Forms.DeviceOrder.BuildForm));
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}