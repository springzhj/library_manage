using DocumentFormat.OpenXml.InkML;
using MongoDB.Bson.Serialization.Attributes;
using LibraryManageSystemApi.Extension;

namespace LibraryManageSystemApi.Model
{
    public class User
    {
        [BsonElement("_id")]
        public long id { get; set; }
        public string? account { get; set; }
        public string? password { get; set; }

        public string? salt { get; set; }
        public string? name { get; set; }
        public Sex sex { get; set; }
        public long birth_date { get; set; }
        public string? unitid { get; set; }
        public string? address { get; set; }
        public string? family_phone { get; set; }
        public string? mobile_phone { get; set; }
        public string? fax_number { get; set; }
        public string? e_mail { get; set; }
        public string? identification_number { get; set; }
        public Appointed_number appointed_number { get; set; }
        public Level level { get; set; }
        public bool is_delete { get; set; }

        /// <summary>
        /// 构建密码，MD5盐值加密
        /// </summary>
        public User BuildPassword(string? pwd = null)
        {
            //如果不传值，那就把自己的password当作传进来的password
            if (pwd == null)
            {
                if (password == null)
                {
                    throw new ArgumentNullException("Password不能为空");
                }
                pwd = password;
            }
            salt = MD5Helper.GenerateSalt();
            password = MD5Helper.SHA2Encode(pwd, salt);
            return this;
        }

        /// <summary>
        /// 判断密码和加密后的密码是否相同
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool JudgePassword(string pwd)
        {
            if (this.salt is null)
            {
                throw new ArgumentNullException(this.salt);
            }
            //var p = TicketAppApi.Extension.MD5Helper.SHA2Encode(password, Salt);
            if (password == MD5Helper.SHA2Encode(pwd, salt))
            {
                return true;
            }
            return false;
        }
        public Sex GetGenderFromIdentification_number(string identification_number)
        {

            int genderCode = int.Parse(identification_number.Substring(16, 1));
            if (genderCode % 2 == 0)
            {
                return Sex.female;
            }
            else 
            {
                return Sex.male;
            }
           

        }
        public long GetBirthFromIdentification_number(string? identification_number)
        {
            long BirthCode = long.Parse(identification_number.Substring(6, 8));
            string strBirthCode = BirthCode.ToString();

            // 将字符串形式的日期转换为 DateTime 对象
            DateTime birthdate = DateTime.ParseExact(strBirthCode, "yyyyMMdd", null);

            // 获取当前时间
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // 计算出生日期与 epoch 之间的时间间隔
            TimeSpan timeSinceEpoch = birthdate - epoch;

            // 获取时间戳（秒数）
            long timestamp = (long)timeSinceEpoch.TotalSeconds;

            //Console.WriteLine($"出生日期：{birthdate}");
            //Console.WriteLine($"时间戳：{timestamp}");

            return timestamp;

        }
    }



    public enum Sex
    {
        male = 0,
        female = 1,
    }
    //public enum Birth_date
    //{
    //    timestamp,
    //}
    public enum Appointed_number
    {
        number,
    }
    public enum Level
    {
        ordinary_student = 0,
        advanced_student = 1,
        ordinary_teacher = 2,
        advanced_teacher = 3,
        unit_personnel = 4,
        manager = 5,
    }

}


