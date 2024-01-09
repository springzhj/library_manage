using Microsoft.AspNetCore.Routing.Template;
using MiniSoftware;
using System.Text;

namespace LibraryManageSystemApi.Extention
{
    public class WordHelper
    {
        public static void Test()
        {
            var path = Path.Combine(System.Environment.CurrentDirectory, "wwwroot");
            path = Path.Combine(path, "wordtemplate");
            var outputPath = Path.Combine(path, "out.docx");
            var templatePath = Path.Combine(path, "test.docx");
            var value = new Dictionary<string, object>()
            {
                ["code"] = "202312025",
                ["title"] = "HelloWord",
                ["name"] = "海狸鼠",
                ["code1"] = "12353",
                ["name1"] = "位置",
                ["type"] = "888888888888888888888888",
            };
            MiniSoftware.MiniWord.SaveAsByTemplate(outputPath, templatePath, value);
        }

        public static string Word_To_Base64(string filepath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(filepath.Trim()))
                {
                    if(!File.Exists(filepath))
                    {
                        return "your message";
                    }
                    else
                    {
                            FileStream filestream = new FileStream(filepath, FileMode.Open);
                            byte[] bt = new byte[filestream.Length];
                            filestream.Read(bt, 0, bt.Length);
                            filestream.Close();
                            return Convert.ToBase64String(bt);
                    }
                }
                else
                {
                    return "your message2";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string Base64_To_Word (string base64String,string filepath,string filename)
        {
            try
            {
                byte[] bt = Convert.FromBase64String(base64String);
                string path = Path.Combine(System.Environment.CurrentDirectory, "wwwroot");
                path = Path.Combine(path, filepath);
                path = Path.Combine(path, filename);
                if(!File.Exists(path)) {
                    FileStream fs = new FileStream(path, FileMode.CreateNew);
                    fs.Write(bt, 0, bt.Length);
                    fs.Close();
                }
                return path;
            }
            catch(Exception ex) {
                throw ex;
            }
        
        }
        // 生成随机字符串
        public static string GetRandomCharacters(int n = 10, bool Number = true, bool Lowercase = false, bool Capital = false)  
        {
            StringBuilder tmp = new StringBuilder();
            Random rand = new Random();
            string characters = (Capital ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : null) + (Number ? "0123456789" : null) + (Lowercase ? "abcdefghijklmnopqrstuvwxyz" : null);
            if (characters.Length < 1)
            {
                return (null);
            }
            for (int i = 0; i < n; i++)
            {
                tmp.Append(characters[rand.Next(0, characters.Length)].ToString());
            }
            return (tmp.ToString());
        }
    }
}