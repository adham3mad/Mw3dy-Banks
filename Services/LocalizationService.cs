using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Mw3dy.Services
{
    public class LocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _translations = new();
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalizationService(IHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            LoadTranslations(env.ContentRootPath);
        }

        private void LoadTranslations(string rootPath)
        {
            try
            {
                var filePath = Path.Combine(rootPath, "translations.json");
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json);
                    if (data != null)
                    {
                        foreach (var (lang, sections) in data)
                        {
                            _translations[lang.ToLower()] = sections;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading translations: {ex.Message}");
            }
        }

        public string GetCurrentCulture()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                // Check cookie
                if (context.Request.Cookies.TryGetValue("mw3dy-lang", out var lang))
                {
                    if (lang.ToLower() == "ar") return "ar";
                }
            }
            return "en"; // default
        }

        public string T(string key)
        {
            var culture = GetCurrentCulture();
            return T(key, culture);
        }

        public string T(string key, string culture)
        {
            culture = culture.ToLower();
            if (!_translations.ContainsKey(culture))
            {
                culture = "en"; // fallback
            }

            if (!_translations.TryGetValue(culture, out var sections))
            {
                return key;
            }

            var parts = key.Split('.');
            if (parts.Length != 2)
            {
                return key; // Invalid key format, expect "section.key"
            }

            var section = parts[0];
            var subKey = parts[1];

            if (sections.TryGetValue(section, out var keys) && keys.TryGetValue(subKey, out var value))
            {
                return value;
            }

            // Fallback to English if not found in current culture
            if (culture != "en" && _translations.TryGetValue("en", out var enSections))
            {
                if (enSections.TryGetValue(section, out var enKeys) && enKeys.TryGetValue(subKey, out var enValue))
                {
                    return enValue;
                }
            }

            return key;
        }
    }
}
