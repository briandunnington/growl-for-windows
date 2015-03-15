using System;
using System.Collections.Generic;
using System.Xml;

namespace GrowlExtras.Subscriptions.FeedMonitor
{
    public class GenericFeedParser
    {
        public FeedInfo Parse(XmlReader reader)
        {
            try
            {
                FeedInfo info = new FeedInfo();

                //create a List of type Dictionary<string,string> for the element names and values
                List<Dictionary<string, string>> items = new List<Dictionary<string, string>>();

                // declare a Dictionary to capture each current Item in the while loop
                Dictionary<string, string> currentItem = null;

                bool parsingItems = false;

                /// Read each element with the reader
                while (reader.Read())
                {
                    // if it's an element, we want to process it
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        string name = reader.Name.ToLowerInvariant();

                        // handle title
                        if (!parsingItems && name == "title")
                        {
                            reader.Read();
                            info.Title = reader.Value;
                        }

                        // handle language
                        if (!parsingItems && name == "language")
                        {
                            reader.Read();
                            info.Language = reader.Value;
                        }

                        /* most feed logos/images/icons are not square, so we are not using this for now
                        // handle icon
                        if (!parsingItems && name == "logo")    // atom
                        {
                            reader.Read();
                            info.Icon = reader.Value;
                        }
                        else if (!parsingItems && name == "image")  // rss
                        {
                            bool search = true;
                            while (search)
                            {
                                reader.Read();
                                if (reader.Name == "url")
                                {
                                    reader.Read();
                                    info.Icon = reader.Value;
                                }

                                if (reader.Name == "image" && reader.NodeType == XmlNodeType.EndElement)
                                    search = false;
                            }
                        }
                         * */


                        if (name == "item" || name == "entry")
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
                            // some feeds can have duplicate keys, so we don't want to blow up here:
                            if (!currentItem.ContainsKey(name))
                            {
                                // handle <link> href attribute
                                string href = String.Empty;
                                if (name == "link" && reader.HasAttributes)
                                {
                                    href = reader.GetAttribute("href");
                                }

                                reader.Read();
                                string val = reader.Value.Trim();

                                if (!String.IsNullOrEmpty(href) && String.IsNullOrEmpty(val))
                                    val = href;

                                currentItem.Add(name, val);
                            }
                        }
                    }
                }
                // Save previous item
                if (currentItem != null)
                    items.Add(currentItem);

                // now create a List of type GenericFeedItem
                List<FeedItem> itemList = new List<FeedItem>();
                // iterate all our items from the reader
                foreach (Dictionary<string, string> d in items)
                {
                    FeedItem itm = new FeedItem();
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
                            case "pubdate":
                            case "issued":
                            case "updated":
                                DateTime dt;
                                bool ok = Rfc822DateTime.TryParse(d[k], info.Language, out dt);
                                itm.PubDate = ok ? dt : DateTime.Now;
                                // log failed date parsing so we can update the format.xml file if needed
                                if (!ok)
                                {
                                    string msg = String.Format("Feed Plugin: DateTime parsing failed for value '{0}'; Feed '{1}'", d[k], info.Url);
                                    Growl.CoreLibrary.DebugInfo.WriteLine(msg);
                                }
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
