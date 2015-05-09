using System;
using System.Collections.Generic;

namespace Zelda.Game
{
    class Dialog
    {
        public string Id { get; set; }
        public string Text { get; set; }

        readonly Dictionary<string, string> _properties = new Dictionary<string, string>();
        public IDictionary<string, string> Properties
        {
            get { return _properties; }
        }

        public bool HasProperty(string key)
        {
            Debug.CheckAssertion(!String.IsNullOrEmpty(key) && key != "Text" && key != "DialogId",
                "Invalid property key for dialog");
            return _properties.ContainsKey(key);
        }

        public string GetProperty(string key)
        {
            Debug.CheckAssertion(!String.IsNullOrEmpty(key) && key != "Text" && key != "DialogId",
                "Invalid property key for dialog");
            Debug.CheckAssertion(HasProperty(key), "No such dialog property : '{0}'".F(key));
            return _properties[key];
        }

        public void SetProperty(string key, string value)
        {
            Debug.CheckAssertion(!String.IsNullOrEmpty(key) && key != "Text" && key != "DialogId",
                "Invalid property key for dialog");
            _properties[key] = value;
        }
    }
}
