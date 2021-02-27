using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using ValheimLib.Util;

namespace ValheimLib
{
    public static class Language
    {
        public const char TokenFirstChar = '$';

        public static List<TokenValue> AdditionalTokens = new List<TokenValue>();

        public struct TokenValue
        {
            public string Token;
            public string Value;
        }

        public static void Init()
        {
            var _ = new Hook(
                typeof(Localization).GetMethod(nameof(Localization.SetupLanguage), HookHelper.AllBindingFlags),
                typeof(Language).GetMethod(nameof(AddTokens), HookHelper.AllBindingFlags));
        }

        // Todo : for now the custom tokens are getting to any language
        public static void AddToken(string token, string value, bool forceReplace = false)
        {
            if (token[0] != TokenFirstChar)
            {
                throw new Exception($"Token first char should be $ ! (token : {token})");
            }

            if (!forceReplace)
            {
                foreach (var pair in AdditionalTokens)
                {
                    if (pair.Token == token)
                    {
                        throw new Exception($"Token named {token} already exist !");
                    }
                }
            }

            AdditionalTokens.Add(new TokenValue { Token = token.Substring(1), Value = value });
        }

        private static bool AddTokens(Func<Localization, string, bool> orig, Localization self, string language)
        {
            var res = orig(self, language);

            if (res)
            {
                foreach (var pair in AdditionalTokens)
                {
                    self.AddWord(pair.Token, pair.Value);
                }
            }

            return res;
        }
    }
}
