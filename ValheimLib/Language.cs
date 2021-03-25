using BepInEx;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.IO;
using ValheimLib.Util.Reflection;

namespace ValheimLib
{
    /// <summary>
    /// Class for adding / replacing localization tokens
    /// </summary>
    public static class Language
    {
        /// <summary>
        /// Your token must start with this character.
        /// </summary>
        public const char TokenFirstChar = '$';

        /// <summary>
        /// Default language of the game
        /// </summary>
        public const string DefaultLanguage = "English";

        internal static Dictionary<string, Dictionary<string, string>> AdditionalTokens =
            new Dictionary<string, Dictionary<string, string>>();

        internal static Func<StringReader, List<List<string>>> DoQuoteLineSplit;

        internal static void Init()
        {
            _ = new Hook(
                typeof(Localization).GetMethod(nameof(Localization.SetupLanguage), ReflectionHelper.AllBindingFlags),
                typeof(Language).GetMethod(nameof(AddTokens), ReflectionHelper.AllBindingFlags));

            var doQuoteLineSplitMethodInfo = typeof(Localization).GetMethod(nameof(Localization.DoQuoteLineSplit), ReflectionHelper.AllBindingFlags);
            DoQuoteLineSplit = (Func<StringReader, List<List<string>>>)
                Delegate.CreateDelegate(typeof(Func<StringReader, List<List<string>>>), null, doQuoteLineSplitMethodInfo);

            AddLanguageFilesFromPluginFolder();
        }

        private static Dictionary<string, string> GetLanguageDict(string language)
        {
            if (!AdditionalTokens.TryGetValue(language, out var languageDict))
            {
                languageDict = new Dictionary<string, string>();
                AdditionalTokens.Add(language, languageDict);
            }

            return languageDict;
        }

        private static void AddLanguageFilesFromPluginFolder()
        {
            var languagePaths = Directory.GetFiles(Paths.PluginPath, "*.language", SearchOption.AllDirectories);
            foreach (var path in languagePaths)
            {
                AddPath(path);
            }
        }

        /// <summary>
        ///  Add a token and its value to the specified language (default to English)
        /// </summary>
        /// <param name="token">token / key</param>
        /// <param name="value">value that will be printed in the game</param>
        /// <param name="language"></param>
        /// <param name="forceReplace">replace the token if it already exists</param>
        public static void AddToken(string token, string value, string language = DefaultLanguage, bool forceReplace = false)
        {
            if (token[0] != TokenFirstChar)
            {
                throw new Exception($"Token first char should be {TokenFirstChar} ! (token : {token})");
            }

            Dictionary<string, string> languageDict;

            if (!forceReplace)
            {
                if (AdditionalTokens.TryGetValue(language, out languageDict))
                {
                    foreach (var pair in languageDict)
                    {
                        if (pair.Key == token)
                        {
                            throw new Exception($"Token named {token} already exist !");
                        }
                    }
                }
            }

            languageDict = GetLanguageDict(language);
            languageDict.Add(token.Substring(1), value);
        }

        /// <summary>
        /// Add a token and its value to the English language
        /// </summary>
        /// <param name="token">token / key</param>
        /// <param name="value">value that will be printed in the game</param>
        /// <param name="forceReplace">replace the token if it already exists</param>
        public static void AddToken(string token, string value, bool forceReplace = false) =>
            AddToken(token, value, DefaultLanguage, forceReplace);

        /// <summary>
        /// Add a file via path
        /// </summary>
        /// <param name="path">absolute path to file</param>
        public static void AddPath(string path)
        {
            if (path == null)
            {
                throw new NullReferenceException($"param {nameof(path)} is null");
            }

            var fileContent = File.ReadAllText(path);
            Add(fileContent);
            Log.LogInfo($"Added language file {Path.GetFileName(path)}");
        }

        /// <summary>
        /// Add a language file
        /// </summary>
        /// <param name="fileContent">Entire file as string</param>
        public static void Add(string fileContent)
        {
            if (fileContent == null)
            {
                throw new NullReferenceException($"param {nameof(fileContent)} is null");
            }

            LoadLanguageFile(fileContent);
        }

        private static void LoadLanguageFile(string fileContent)
        {
            var stringReader = new StringReader(fileContent);
            var languages = stringReader.ReadLine().Split(new []{ ',' });

            foreach (List<string> keyAndValues in DoQuoteLineSplit(stringReader))
            {
                if (keyAndValues.Count != 0)
                {
                    var token = keyAndValues[0];
                    if (!token.StartsWith("//") && token.Length != 0)
                    {
                        for (var i = 0; i < languages.Length; i++)
                        {
                            var language = languages[i];

                            string tokenValue = keyAndValues[i];
                            if (string.IsNullOrEmpty(tokenValue) || tokenValue[0] == '\r')
                            {
                                tokenValue = keyAndValues[1];
                            }

                            var languageDict = GetLanguageDict(language);
                            languageDict.Add(token, tokenValue);
                        }
                    }
                }
            }
        }

        private static bool AddTokens(Func<Localization, string, bool> orig, Localization self, string language)
        {
            var res = orig(self, language);

            if (res)
            {
                if (AdditionalTokens.TryGetValue(language, out var tokens))
                {
                    foreach (var pair in tokens)
                    {
                        self.AddWord(pair.Key, pair.Value);
                    }
                }
            }

            return res;
        }
    }
}
