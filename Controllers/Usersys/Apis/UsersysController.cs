
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using LibraryManageSystemApi.ApiRequsetCheck;
using LibraryManageSystemApi.JwtExtension;
using LibraryManageSystemApi.MiddlerWare;
using LibraryManageSystemApi.Controllers.Usersys.Dto;
using LibraryManageSystemApi.Model;
using LibraryManageSystemApi.MongoDbHelper;
using IdGen;
using Microsoft.Extensions.Options;
using LibraryManageSystemApi.Log;
using MongoDB.Bson;

namespace LibraryManageSystemApi.Controllers.Usersys.Apis
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersysController : Controller
    {

        public IOptions<JWTTokenOptions> Jwtoptions { get; }
        public IMongoDatabase Mongo { get; }
        public LoggerHelper Logger { get; }
        public IdGenerator Idgen { get; }
        public JwtInvorker JwtInvorker { get; }
        public MongoClient mongoclient { get; }

        public UsersysController(
           IOptions<JWTTokenOptions> jwtoptions, IMongoDatabase mongo, LoggerHelper logger,
           IdGenerator idgen, JwtInvorker jwtInvorker, MongoClient mongoclient)
        {
            Jwtoptions = jwtoptions;
            Mongo = mongo;
            Logger = logger;
            Idgen = idgen;
            JwtInvorker = jwtInvorker;
            this.mongoclient = mongoclient;
        }
        ///api/usersys/enroll
        [HttpPost]
        public async Task<Result> enroll([FromBody] UsersysEnrollDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }
            var user = await Mongo.DbGetCollection<User>().Find(s => s.account == dto.account).FirstOrDefaultAsync();//注册是否分发账户，不等于登录
            if (user != null)
            {
                return Result.SuccessError("账户已存在").SetData(new { result = false });
            }
            User uu = new User();
            uu.id = Idgen.CreateId();
            uu.name = dto.name;
            uu.account = dto.account;
            uu.password = dto.password;
            if (dto.level == 0)
            { uu.level = Level.ordinary_student; }
            else if (dto.level == 2)
            { uu.level = Level.ordinary_teacher; }
            else if (dto.level == 4)
            { uu.level = Level.unit_personnel; }
            uu.identification_number = dto.identification_number;
            uu.sex = uu.GetGenderFromIdentification_number(dto.identification_number);
            uu.birth_date = uu.GetBirthFromIdentification_number(dto.identification_number);
            uu.is_delete = false;
            uu.BuildPassword();
            try
            {
                await Mongo.DbGetCollection<User>().InsertOneAsync(uu);
                return Result.Success("注册成功").SetData(new { result = true });
            }
            catch (Exception ex)
            {
                Logger.Error($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                await Console.Out.WriteLineAsync($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                return Result.SuccessError("添加失败").SetData(new { result = false });
            }


        }



    }
}
