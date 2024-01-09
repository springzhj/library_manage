namespace LibraryManageSystemApi.Extension
{
    public class MaskHelper
    {
        public static string MaskMiddle(string input, char maskChar = '*', int visibleChars = 4)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length <= visibleChars * 1.5)
            {
                return input; // 输入太短或者无需脱敏，直接返回
            }

            int prefixLength = visibleChars / 2;
            int suffixLength = visibleChars - prefixLength;

            // 获取前缀和后缀
            string prefix = input.Substring(0, prefixLength);
            string suffix = input.Substring(input.Length - suffixLength);

            // 构建脱敏部分
            string maskedPart = new string(maskChar, input.Length - (prefixLength + suffixLength));

            // 拼接前缀、脱敏部分和后缀
            string anonymizedData = prefix + maskedPart + suffix;

            return anonymizedData;
        }

    }
}
