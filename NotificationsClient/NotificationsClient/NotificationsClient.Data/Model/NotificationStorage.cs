using GalaSoft.MvvmLight.Ioc;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationsClient.Model
{
    // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/data-cloud/data/databases
    public class NotificationStorage
    {
        private const string DatabaseName = "notifications.db3";

        public const SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLiteOpenFlags.SharedCache;

        private static string DatabasePath
        {
            get
            {
                var appFolder = SimpleIoc.Default.GetInstance<Settings>().GetAppFolder();
                return Path.Combine(appFolder.FullName, DatabaseName);
            }
        }

        private Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(DatabasePath, Flags);
        });

        private SQLiteAsyncConnection _database => lazyInitializer.Value;
        private bool _initialized = false;

        public async Task InitializeAsync()
        {
            if (!_initialized)
            {
                if (!_database.TableMappings.Any(m => m.MappedType.Name == typeof(Notification).Name))
                {
                    await _database.CreateTablesAsync(CreateFlags.None, typeof(Notification)).ConfigureAwait(false);
                    _initialized = true;
                }
            }
        }

        private IList<Notification> GetChannelNotifications(string channel)
        {
            return null;
        }

        public void SaveNotification(Notification notif)
        {
            // Save in today's storage


            // Save in channel storage


        }

        public IList<Notification> GetNotifications(DateTime startDate, int days)
        {
            // return all notifications available in the timespan

            return null;
        }

        public void Delete(Notification notif)
        {
            // Move to Deleted storage
        }

        public async Task Synchronize()
        {
            // Synchronize with the remote database
        }
    }
}
