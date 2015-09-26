using System.Collections.Generic;

namespace Zelda.Game
{
    public class Dialog
    {
        readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

        public string Id { get; set; }
        public string Text { get; set; }
        public IReadOnlyDictionary<string, string> Properties { get { return _properties; } }

        public bool HasProperty(string key)
        {
            Debug.CheckAssertion(!key.IsNullOrEmpty() && key != "Text" && key != "DialogId",
                "Invalid property key for dialog");
            return _properties.ContainsKey(key);
        }

        public string GetProperty(string key)
        {
            Debug.CheckAssertion(!key.IsNullOrEmpty() && key != "Text" && key != "DialogId",
                "Invalid property key for dialog");
            Debug.CheckAssertion(HasProperty(key), "No such dialog property : '{0}'".F(key));
            return _properties[key];
        }

        public void SetProperty(string key, string value)
        {
            Debug.CheckAssertion(!key.IsNullOrEmpty() && key != "Text" && key != "DialogId",
                "Invalid property key for dialog");
            _properties[key] = value;
        }
    }
}
