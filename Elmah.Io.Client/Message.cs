using System;
using System.Collections.Generic;

namespace Elmah.Io.Client
{
    public class Message
    {
        public Message(string title)
        {
            Title = title;
        }

        public string Id { get; set; }

        public string Application { get; set; }

        public string Detail { get; set; }

        public string Hostname { get; set; }

        public string Title { get; set; }

        public string Source { get; set; }

        public int? StatusCode { get; set; }

        public DateTime DateTime { get; set; }

        public string Type { get; set; }

        public string User { get; set; }

        public Severity? Severity { get; set; }

        public string Url { get; set; }

        public string Version { get; set; }

        public List<Item> Cookies { get; set; }

        public List<Item> Form { get; set; }

        public List<Item> QueryString { get; set; }

        public List<Item> ServerVariables { get; set; }

        public List<Item> Data { get; set; }
    }

    public enum Severity
    {
        Verbose,
        Debug,
        Information,
        Warning,
        Error,
        Fatal,
    }

    public class Item
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}