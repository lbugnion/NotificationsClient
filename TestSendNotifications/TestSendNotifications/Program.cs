using Microsoft.Azure.NotificationHubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestSendNotifications
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Console.WriteLine("Enter the title of the notification");
            //var title = Console.ReadLine();

            //Console.WriteLine("Enter the message of the notification");
            //var message = Console.ReadLine();

            //Console.WriteLine("Enter the channel of the notification");
            //var channel = Console.ReadLine();

            //var registrationId = await Notifications.Instance.Hub.CreateRegistrationIdAsync();

            var properties = new Dictionary<string, string>
            {
                {
                    "body",
                    "Hello world"
                },
                {
                    "title",
                    "This is a title"
                },
                {
                    "channel",
                    "The Channel"
                }
            };

            var outcome = await Notifications.Instance.Hub.SendTemplateNotificationAsync(  properties);
        }
    }
}
