using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xiyu.DeepSeek.Cryptography;
using Xiyu.DeepSeek.Messages;

namespace Xiyu.DeepSeek.Persistence
{
    public static class Serialize
    {
        private static readonly JsonSerializerSettings DefaultSettings = new()
        {
            Converters = new List<JsonConverter> { new StringEnumConverter() },
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };


        public static Func<string> DoSerialize(this IEnumerable<IMessage> collector, JsonSerializerSettings settings = null)
        {
            return () => JsonConvert.SerializeObject(collector, settings ?? DefaultSettings);
        }


  
        public static Func<string> DoEncryption(this Func<string> collector, string password)
        {
            return () => CryptoExtensions.Encrypt(collector(), password);
        }

      
        public static Func<string> DoDecryption(this Func<string> collector, string password)
        {
            return () => CryptoExtensions.Decrypt(collector(), password);
        }


        public static Func<string> DoEncryption(this Func<string> collector, byte[] salt)
        {
            return () =>
            {
                var rfc = new Rfc2898DeriveBytes(collector(), salt, 10000, HashAlgorithmName.SHA256);
                return Convert.ToBase64String(rfc.GetBytes(32));
            };
        }

        public static UniTask AsFileAsync(this Func<string> collector, string path, Encoding encoding = null, CancellationToken cancellationToken = default)
        {
            return File.WriteAllTextAsync(path, collector(), encoding ?? Encoding.UTF8, cancellationToken).AsUniTask();
        }

        public static IEnumerable<IMessage> DoRemove(this IEnumerable<IMessage> collector, Reserved reserved = Reserved.Simple)
        {
            if (reserved == 0)
                return collector;

            if (reserved == Reserved.All)
                return Enumerable.Empty<IMessage>();

            return collector.Where(msg => msg.Role switch
            {
                Role.System => (reserved & Reserved.System) != 0,
                Role.User => (reserved & Reserved.User) != 0,
                Role.Assistant => (msg is AssistantReasonerMessage && (reserved & Reserved.AssistantReasoner) != 0) ||
                                  (msg is AssistantPrefixMessage && (reserved & Reserved.AssistantPrefix) != 0) ||
                                  (msg is not AssistantReasonerMessage && msg is not AssistantPrefixMessage && (reserved & Reserved.Assistant) != 0),
                Role.Tool => (reserved & Reserved.Tool) != 0,
                _ => throw new ArgumentOutOfRangeException(nameof(msg.Role), "Invalid role")
            });
        }
    }
}