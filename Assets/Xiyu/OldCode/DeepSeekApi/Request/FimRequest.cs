#if DEEPSEEK_PAST_CODE
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiyu.Old.DeepSeekApi.Request
{
    /*呜呜呜，我不想写注释*/
    [JetBrains.Annotations.PublicAPI]
    public class FimRequest
    {
        /// <summary>
        /// 初始化 FIM 请求体
        /// </summary>
        /// <param name="prompt">前缀内容</param>
        /// <param name="suffix">后缀内容</param>
        public FimRequest(string prompt, string suffix = null)
        {
            Prompt = prompt;
            Suffix = suffix;
        }

        /// <summary>
        /// deepseek-reasoner 模型不支持
        /// </summary>
        public ModelType Model => ModelType.DeepseekChat;

        /// <summary>
        /// 用于生成完成内容的提示
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// 制定被补全内容的后缀。
        /// </summary>
        public string Suffix { get; set; } = null;

        /// <summary>
        /// 在输出中，把 prompt 的内容也输出出来
        /// </summary>
        public bool Echo { get; set; } = false;

        /// <summary>
        /// 介于 -2.0 和 2.0 之间的数字。
        /// 如果该值为正，那么新 token 会根据其在已有文本中的出现频率受到相应的惩罚，降低模型重复相同内容的可能性。
        /// </summary>
        public float FrequencyPenalty { get; set; } = 0F;

        /// <summary>
        /// 制定输出中包含 logprobs 最可能输出 token 的对数概率，包含采样的 token。
        /// 例如，如果 logprobs 是 20，API 将返回一个包含 20 个最可能的 token 的列表。
        /// API 将始终返回采样 token 的对数概率，因此响应中可能会有最多 logprobs+1 个元素。
        /// logprobs 的最大值是 20。
        /// </summary>
        public int Logprobs { get; set; } = 0;

        /// <summary>
        /// 最大生成 token 数量。
        /// </summary>
        public int MaxTokens { get; set; } = 256;

        /// <summary>
        /// 介于 -2.0 和 2.0 之间的数字。
        /// 如果该值为正，那么新 token 会根据其是否已在已有文本中出现受到相应的惩罚，从而增加模型谈论新主题的可能性。
        /// </summary>
        public float PresencePenalty { get; set; } = 0F;

        /// <summary>
        /// 官网文档没写 （— v —）
        /// <para>
        /// ai 生成时遇到这些词会停止生成更多的 token。
        /// </para>
        /// </summary>
        public StopOptions StopOptions { get; set; }

        /// <summary>
        /// 不为空时，将会以 SSE（server-sent events）的形式以流式发送消息增量。
        /// </summary>
        public StreamOptions? StreamOptions { get; set; } = null;


        /// <summary>
        /// 采样温度，介于 0 和 2 之间。
        /// 更高的值，如 0.8，会使输出更随机，而更低的值，如 0.2，会使其更加集中和确定。
        /// 我们通常建议可以更改这个值或者更改 top_p，但不建议同时对两者进行修改。
        /// </summary>
        public float Temperature { get; set; } = 1F;

        /// <summary>
        /// 作为调节采样温度的替代方案，模型会考虑前 top_p 概率的 token 的结果。
        /// 所以 0.1 就意味着只有包括在最高 10% 概率中的 token 会被考虑。
        /// 我们通常建议修改这个值或者更改 temperature，但不建议同时对两者进行修改。
        /// </summary>
        public float TopP { get; set; } = 1;

        /// <summary>
        /// 将对象转换为 JSON 字符串，用于发送请求 Body
        /// </summary>
        /// <param name="instance">允许传入一个 <see cref="JObject"/> 实例</param>
        /// <param name="formatting">JSON 格式 （默认为：工整的）</param>
        /// <returns></returns>
        public string ToJson(JObject instance = null, Formatting formatting = Formatting.Indented)
        {
            instance ??= new JObject();

            instance.Add("model", "deepseek-chat");
            instance.Add("prompt", Prompt);

            if (!string.IsNullOrEmpty(Suffix))
            {
                instance.Add("suffix", Suffix);
            }
            else instance.Add("echo", Echo);

            instance.Add("frequency_penalty", Math.Clamp(FrequencyPenalty, -2F, 2F));

            if (!Echo)
            {
                instance.Add("logprobs", Math.Clamp(Logprobs, 0, 20));
            }

            instance.Add("max_tokens", Math.Clamp(MaxTokens, 1, 8192));
            instance.Add("presence_penalty", Math.Clamp(PresencePenalty, -2F, 2F));

            if (StopOptions is { Count: > 0 })
            {
                instance.Add("stop", new JArray(StopOptions));
            }

            if (StreamOptions is not null)
            {
                instance.Add("stream", true);
                instance.Add("stream_options", JObject.FromObject(StreamOptions.Value));
            }

            instance.Add("temperature", Math.Clamp(Temperature, 0F, 2F));
            instance.Add("top_p", Math.Clamp(TopP, 0F, 1F));

            return instance.ToString(formatting);
        }
    }
}
#endif