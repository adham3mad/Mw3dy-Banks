using System;

namespace Mw3dy.Models
{
    public class Service
    {
        public string Id { get; set; } = string.Empty; // "account", "loan", "mortgage", etc.
        public string Icon { get; set; } = string.Empty; // "wallet", "home", etc.
        public string Title { get; set; } = string.Empty; // Default English Title
        public string Desc { get; set; } = string.Empty; // Default English Description
    }
}
