using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Xiyu.DeepSeekApi;
using Xiyu.DeepSeekApi.Request;

namespace Xiyu.Expand
{
    public static class MessageCollectorExpand
    {
        /// <summary>
        /// 将消息收集器保存到文件，返回文件路径
        /// <para>-<see cref="fileName"/>为空时 则保存到 Application.persistentDataPath 目录下</para>
        /// <para>-<see cref="append"/>为true时 如果文件存在则追加内容并更新日期，负责生成一个新名称写入文件然后返回新路径</para>
        /// </summary>
        /// <param name="messageCollector"></param>
        /// <param name="fileName">文件名称（包含后缀）</param>
        /// <param name="path">文件路径（文件夹）</param>
        /// <param name="append">文件名重复是时否增加内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>返回完整的文件路径</returns>
        public static async UniTask<string> SaveToHistoryAsync(this MessageCollector messageCollector,
            [NotNull] string fileName,
            string path = null,
            bool append = false,
            CancellationToken? cancellationToken = null)
        {
            if (messageCollector.Messages.Count == 0)
            {
                throw new ArgumentException("消息收集器中没有消息");
            }

            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(Application.persistentDataPath, "historys");
            }

            Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, fileName);

            var jArray = new JArray();
            foreach (var messageUnit in messageCollector)
            {
                if (messageUnit.Role == RoleType.Tool) continue;

                var objectInstance = JObject.Parse(messageUnit.ToJson(Formatting.None));

                if (messageUnit.Role == RoleType.Assistant)
                {
                    _ = objectInstance.Remove("prefix");
                }

                jArray.Add(objectInstance);
            }

            if (!File.Exists(filePath))
            {
                await WriteAllTextAsync(filePath, jArray, cancellationToken ?? CancellationToken.None);

                return filePath;
            }

            if (append)
            {
                var rawJsonContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken ?? CancellationToken.None);

                if (string.IsNullOrEmpty(rawJsonContent) || (rawJsonContent.StartsWith('{') && rawJsonContent.EndsWith('}')))
                {
                    await WriteAllTextAsync(filePath, jArray, cancellationToken ?? CancellationToken.None);

                    return filePath;
                }

                var jObject = JObject.Parse(rawJsonContent);
                jObject["created_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                // 将新的消息添加到原有的消息中
                jObject["messages"] = new JArray(jObject["messages"]!.Concat(jArray));

                await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(jObject, Formatting.Indented), Encoding.UTF8, cancellationToken ?? CancellationToken.None);

                return filePath;
            }

            filePath = Path.Combine(path, GenerateFileName(path, fileName));
            await WriteAllTextAsync(filePath, jArray, cancellationToken ?? CancellationToken.None);

            return filePath;
        }

        private static UniTask WriteAllTextAsync(string filePath, JArray jArray, CancellationToken cancellationToken)
        {
            return UniTask.FromResult(File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(new
            {
                created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                messages = jArray
            }, Formatting.Indented), Encoding.UTF8, cancellationToken));
        }


        // 生成不重复的文件名
        private static string GenerateFileName(string path, string fileName)
        {
            var filePath = Path.Combine(path, fileName);
            if (!File.Exists(filePath))
            {
                return fileName;
            }

            var index = 1;
            var extension = Path.GetExtension(fileName);
            var name = Path.GetFileNameWithoutExtension(fileName);
            while (File.Exists(filePath))
            {
                fileName = $"{name}({index++}){extension}";
                filePath = Path.Combine(path, fileName);
            }

            return fileName;
        }
    }
}