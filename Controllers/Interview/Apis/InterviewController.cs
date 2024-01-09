using LibraryManageSystemApi.ApiRequsetCheck;
using LibraryManageSystemApi.Extension;
using LibraryManageSystemApi.GlobalSetting;
using LibraryManageSystemApi.JwtExtension;
using LibraryManageSystemApi.Log;
using LibraryManageSystemApi.MiddlerWare;
using LibraryManageSystemApi.Model;
using LibraryManageSystemApi.MongoDbHelper;
using LibraryManageSystemApi.ServiceInject;
using LibraryManageSystemApi.Extention;
using IdGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog.Filters;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using DocumentFormat.OpenXml.Wordprocessing;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using SharpCompress.Common;
using LibraryManageSystemApi.Controllers.Interview.Dto;
namespace LibraryManageSystemApi.Controllers.Interview.Apis
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class InterviewController : Controller
    {

        public IOptions<JWTTokenOptions> Jwtoptions { get; }
        public IMongoDatabase Mongo { get; }
        public LoggerHelper Logger { get; }
        public IdGenerator Idgen { get; }
        public JwtInvorker JwtInvorker { get; }
        public MongoClient mongoclient { get; }
        public WordHelper WordHelper { get; }
        public InterviewController(
           IOptions<JWTTokenOptions> jwtoptions, IMongoDatabase mongo, LoggerHelper logger,
           IdGenerator idgen, JwtInvorker jwtInvorker, MongoClient mongoclient, WordHelper wordHelper)
        {
            Jwtoptions = jwtoptions;
            Mongo = mongo;
            Logger = logger;
            Idgen = idgen;
            JwtInvorker = jwtInvorker;
            this.mongoclient = mongoclient;
            WordHelper = wordHelper;
        }
        //api/interview/getinterviewlist 获取采访清单
        [HttpGet]
       // [Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> getinterviewlist(int pagenumber, int pagecounts, string keywords = "")
        {

            var filter = Builders<Interviewlist>.Filter.Or(
                Builders<Interviewlist>.Filter.Eq(s => s.inter_personname, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Interviewlist>.Filter.Regex(s => s.title, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Interviewlist>.Filter.Regex(s => s.author, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Interviewlist>.Filter.Regex(s => s.ISBN, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Interviewlist>.Filter.Regex(s => s.publish_house, new BsonRegularExpression($".*{keywords}.*", "i"))
            );
            var counts = await Mongo.DbGetCollection<Interviewlist>()
            .Find(filter).CountDocumentsAsync();
            var list = await Mongo.DbGetCollection<Interviewlist>()
               .Find(filter).Skip((pagenumber - 1) * pagecounts).Limit(pagecounts).ToListAsync();
            return Result.Success("获取成功").SetData(new { result = true, data = list, counts = counts });
        }
        //api/interview/putinterviewlist 提交荐书信息
        [HttpPost]
        //[Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> putinterviewlist([FromBody] PutinterviewDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }

            //创建采访清单
            Interviewlist uu = new Interviewlist();
            uu.id = Idgen.CreateId();
            uu.inter_personid = dto.inter_personid;
            uu.inter_personname = dto.inter_personname;
            uu.title = dto.title;
            uu.author = dto.author;
            uu.ISBN = dto.ISBN;
            uu.publish_house = dto.publish_house;
            uu.edition = dto.edition;
            uu.currency = dto.currency;
            uu.order_price = dto.order_price;
            uu.order_number = dto.order_number;
            uu.allprice = dto.allprice;
            uu.documen_type = dto.documen_type;
            uu.status = Interviewlist.Status.interview;

            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await Mongo.DbGetCollection<Interviewlist>().InsertOneAsync(uu);
                    await session.CommitTransactionAsync();
                    return Result.Success("修改成功").SetData(new { result = true });
                }
                catch (Exception ex)
                {
                    Logger.Error($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                    await Console.Out.WriteLineAsync($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                    await session.AbortTransactionAsync();
                    return Result.SuccessError("修改失败").SetData(new { result = false });
                }
            }
            else
            {
                try
                {
                    await Mongo.DbGetCollection<Interviewlist>().InsertOneAsync(uu);
                    return Result.Success("修改成功").SetData(new { result = true });
                }
                catch (Exception ex)
                {
                    Logger.Error($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                    await Console.Out.WriteLineAsync($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                    return Result.SuccessError("修改失败").SetData(new { result = false });
                }
            }

        }
        //api/interview/putorders 新增订单((通过采访清单生成)
        [HttpPost]
        public async Task<Result> putorders([FromBody] PutorderDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }

            //创建订单
            List<Orderlist> orders = new List<Orderlist>();
            Orderlist uu =new Orderlist();
            for (int i = 0; i < dto.order_number;i++)
            {
                uu.id = Idgen.CreateId();
                uu.order_serial_number = WordHelper.GetRandomCharacters();
                uu.title = dto.title;
                uu.author = dto.author;
                uu.ISBN = dto.ISBN;
                uu.publish_house = dto.publish_house;
                uu.edition = dto.edition;
                uu.currency = dto.currency;
                uu.order_price = dto.order_price;
                uu.documen_type = dto.documen_type;
                uu.status = Orderlist.Status.order;
                uu.effective = true;
                uu.description = dto.description;
                orders.Add(uu);
            }
            

            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await Mongo.DbGetCollection<Orderlist>().InsertManyAsync(orders);
                    await session.CommitTransactionAsync();
                    return Result.Success("修改成功").SetData(new { result = true });
                }
                catch (Exception ex)
                {
                    Logger.Error($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                    await Console.Out.WriteLineAsync($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                    await session.AbortTransactionAsync();
                    return Result.SuccessError("修改失败").SetData(new { result = false });
                }
            }
            else
            {
                try
                {
                    await Mongo.DbGetCollection<Orderlist>().InsertManyAsync(orders);
                    return Result.Success("修改成功").SetData(new { result = true });
                }
                catch (Exception ex)
                {
                    Logger.Error($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                    await Console.Out.WriteLineAsync($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                    return Result.SuccessError("修改失败").SetData(new { result = false });
                }
            }

        }
    }
}
