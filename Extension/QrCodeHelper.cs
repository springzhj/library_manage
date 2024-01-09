using LibraryManageSystemApi.GlobalSetting;
using Dumpify;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using Spectre.Console;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace LibraryManageSystemApi.Extension
{
    public class QrCodeHelper
    {
        public static string InsertTitle(string originalSvg, string title, string title_color = "red", int title_fonrsize = 1)
        {
            // 使用正则表达式找到</svg>标签
            string pattern = @"</svg\s*>";

            // 在</svg>标签之前插入新文本
            string result = Regex.Replace(originalSvg, pattern,
                $"<text x=\"50%\" y=\"97%\" text-anchor=\"middle\" font-family=\"Arial\"  font-size=\"{title_fonrsize}\" fill=\"{title_color}\">{title}</text>$0");

            return result;
        }
        public static string ConvertSvgToBase64(string svgString, int width, int height)
        {
            // 创建一个Bitmap对象，并设置其尺寸
            using (var bitmap = new Bitmap(width, height))
            {
                // 创建一个Graphics对象，用于绘制
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    // 创建一个MemoryStream来保存图像数据
                    using (var memoryStream = new MemoryStream())
                    {
                        // 创建一个Image对象，从SVG字符串加载
                        using (var svgImage = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgString)))
                        {
                            using (var image = Image.FromStream(svgImage))
                            {
                                // 将SVG图像绘制到Bitmap上
                                graphics.DrawImage(image, new Rectangle(0, 0, width, height));
                            }
                        }

                        // 保存Bitmap为PNG格式到MemoryStream
                        bitmap.Save(memoryStream, ImageFormat.Png);

                        // 将MemoryStream中的图像数据转换为Base64编码的字符串
                        byte[] imageBytes = memoryStream.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);

                        return base64String;
                    }
                }
            }
        }

        public MemoryStream ConvertSvgStringToStream(string svgString)
        {
            //ConvertSvgToBase64(svgString, 200, 200).Dump();
            byte[] svgBytes = Encoding.UTF8.GetBytes(svgString);
            // 将字节数组转换为内存流
            MemoryStream stream = new MemoryStream(svgBytes);
            return stream;
        }
    }
}

