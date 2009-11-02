using System;
using System.Collections.Generic;
using System.Xml;

namespace GrowlExtras.FeedMonitor
{
    public class GenericFeedParser
    {
        public FeedInfo Parse(XmlReader reader)
        {
            try
            {
                FeedInfo info = new FeedInfo();

                //create a List of type Dictionary<string,string> for the element names and values
                var items = new List<Dictionary<string, string>>();

                // declare a Dictionary to capture each current Item in the while loop
                Dictionary<string, string> currentItem = null;

                bool parsingItems = false;

                /// Read each element with the reader
                while (reader.Read())
                {
                    // if it's an element, we want to process it
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        string name = reader.Name;

                        // rss1.0 - channel/title
                        // rss2.0 - channel/title
                        // atom0.3 - feed/title
                        // atom1.0 - feed/title

                        if (!parsingItems && name.ToLowerInvariant() == "title")
                        {
                            reader.Read();
                            info.ActualTitle = reader.Value;
                        }

                        if (name.ToLowerInvariant() == "item" || name.ToLowerInvariant() == "entry")
                        {
                            parsingItems = true;

                            // Save previous item
                            if (currentItem != null)
                                items.Add(currentItem);

                            // Create new item
                            currentItem = new Dictionary<string, string>();
                        }
                        else if (currentItem != null)
                        {
                            reader.Read();
                            // some feeds can have duplicate keys, so we don't want to blow up here:
                            if (!currentItem.ContainsKey(name))
                                currentItem.Add(name, reader.Value);
                        }
                    }
                }
                // Save previous item
                if (currentItem != null)
                    items.Add(currentItem);

                // now create a List of type GenericFeedItem
                var itemList = new List<FeedItem>();
                // iterate all our items from the reader
                foreach (var d in items)
                {
                    var itm = new FeedItem();
                    //do a switch on the Key of the Dictionary <string, string> of each item
                    foreach (string k in d.Keys)
                    {
                        switch (k)
                        {
                            case "title":
                                itm.Title = d[k];
                                break;
                            case "link":
                                itm.Link = d[k];
                                break;
                            case "published":
                            case "pubDate":
                            case "issued":
                                DateTime dt;
                                bool ok = Rfc822DateTime.TryParse(d[k], out dt);
                                itm.PubDate = ok ? dt : DateTime.Now;
                                break;
                            case "content":
                            case "description":
                                itm.Description = d[k];
                                break;
                            default:
                                break;
                        }
                    }
                    // add the created item to our List
                    itemList.Add(itm);
                }

                info.Items = itemList;

                return info;
            }
            catch
            {
                return null;
            }
        }
    }
}
