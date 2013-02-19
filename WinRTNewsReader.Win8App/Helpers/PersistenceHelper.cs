using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinRTNewsReader.Win8App.Models;
using Windows.Foundation;
using SQLite;


namespace WinRTNewsReader.Common.Helpers
{
    public sealed class PersistenceHelper
    {
        private const string NEWS_DB_SETTINGS = "Settings\\feed_data.sqlite";
        private const string NEWS_DB_USER = "feed_data.sqlite";

        public static IAsyncOperation<IEnumerable<String>> GetUserFeedsAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var v = new List<string>();

                var db = new Database(NEWS_DB_SETTINGS);
                var stmt = db.PrepareStatement("SELECT uri FROM user_feeds");
                while (stmt.HasMore())
                {
                    var value = stmt.ColumnAsTextAt(0);
                    v.Add(value);
                }

                return (IEnumerable<string>)v;
            }).AsAsyncOperation();
        }

        public static IAsyncAction SaveFeedsToDBAsync(IEnumerable<object> feeds)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var db = GetUserDB())
                {
                    var stmt = db.PrepareStatement("DELETE FROM feeds");
                    stmt.Execute();

                    stmt = db.PrepareStatement("DELETE FROM feed_items");
                    stmt.Execute();

                    foreach (var feed in feeds)
                    {
                        var fi = (FeedInfo)feed;
                        if (fi.IsLoaded)
                        {
                            SaveFeedToDB(db, fi);
                        }
                    }
                }
            }).AsAsyncAction();
        }

        private static void SaveFeedToDB(Database db, FeedInfo feed)
        {
            var stmt = db.PrepareStatement("INSERT INTO feeds (uri, title, image_uri) values (?,?,?)");
            stmt.BindText(1, feed.Uri.AbsoluteUri);
            stmt.BindText(2, feed.Title);
            if (feed.ImageUri != null)
            {
                stmt.BindText(3, feed.ImageUri.AbsoluteUri);
            }
            stmt.Execute();

            stmt = db.PrepareStatement("SELECT last_insert_rowid()");
            int feedId = 0;
            if (stmt.HasMore())
            {
                feedId = stmt.ColumnAsIntAt(0);
            }
            else
            {
                throw new Exception("Last inserted row id was not found in sqlite");
            }

            foreach (var feedItem in feed.FeedItems)
            {
                var fi = (FeedItem)feedItem;
                if (fi.FullTextLoaded)
                {
                    SaveFeedItemToDB(db, feedId, fi);
                }
            }
        }

        private static void SaveFeedItemToDB(Database db, int feedId, FeedItem feedItem)
        {
            var stmt = db.PrepareStatement("INSERT INTO feed_items (feed_id, title, uri, image_uri, summary, text) VALUES (?, ?, ?, ?, ?, ?)");
            stmt.BindInt(1, feedId);
            stmt.BindText(2, feedItem.Title);
            stmt.BindText(3, feedItem.Link.AbsoluteUri);
            if (feedItem.ImageUri != null)
            {
                stmt.BindText(4, feedItem.ImageUri.AbsoluteUri);
            }
            stmt.BindText(5, feedItem.Summary);
            stmt.BindText(6, feedItem.FullText);
            stmt.Execute();
        }

        public static IAsyncOperation<IEnumerable<object>> GetFeedsFromDBAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var v = new List<object>();
                using (var db = GetUserDB())
                {
                    var stmt = db.PrepareStatement("SELECT id, uri,title, image_uri FROM feeds");
                    while (stmt.HasMore())
                    {
                        int id = stmt.ColumnAsIntAt(0);
                        var fi = new FeedInfo(
                            stmt.ColumnAsTextAt(1),
                            stmt.ColumnAsTextAt(2),
                            stmt.ColumnAsTextAt(3));
                        fi.IsLoaded = true;
                        LoadFeedItemsFor(db, id, fi);
                        v.Add(fi);

                    }
                    return (IEnumerable<object>)v;
                }
            }).AsAsyncOperation();

        }

        private static void LoadFeedItemsFor(Database db, int feedId, FeedInfo feedInfo)
        {
            var stmt = db.PrepareStatement("SELECT title, summary, uri, image_uri, text FROM feed_items where feed_id = ?");
            stmt.BindInt(1, feedId);
            while (stmt.HasMore())
            {
                FeedItem fi = new FeedItem(
                    stmt.ColumnAsTextAt(0),
                    stmt.ColumnAsTextAt(1),
                    stmt.ColumnAsTextAt(2),
                    stmt.ColumnAsTextAt(3),
                    stmt.ColumnAsTextAt(4));

                feedInfo.FeedItems.Add(fi);
            }
        }

        public static Database GetSettingsDB()
        {
            return new Database(NEWS_DB_SETTINGS, false);
        }

        private static Database GetUserDB()
        {
            var db = new Database(NEWS_DB_USER, true);
            var stmt1 = db.PrepareStatement("SELECT * FROM sqlite_master WHERE type = 'table' and name ='feed_items'");
            var stmt2 = db.PrepareStatement("SELECT * FROM sqlite_master WHERE type = 'table' and name ='feeds'");
            if (stmt1.HasMore() && stmt2.HasMore())
            {
                return db;
            }
            else
            {
                stmt1 = db.PrepareStatement("DROP table feed_items");
                stmt2 = db.PrepareStatement("DROP table feeds");
                if (stmt1.Valid)
                {
                    stmt1.Execute();
                }
                if (stmt2.Valid)
                {
                    stmt2.Execute();
                }

                stmt1 = db.PrepareStatement("CREATE TABLE \"feeds\" (\"id\" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , \"uri\" VARCHAR NOT NULL , \"title\" VARCHAR NOT NULL , \"image_uri\" VARCHAR)");
                stmt2 = db.PrepareStatement("CREATE TABLE feed_items  (\"id\" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,\"feed_id\" INTEGER NOT NULL,\"title\" VARCHAR NOT NULL,\"uri\" VARCHAR NOT NULL,  \"image_uri\" VARCHAR,\"summary\" TEXT,\"text\" TEXT)");

                stmt1.Execute();
                stmt2.Execute();
            }
            return db;
        }
    }
}