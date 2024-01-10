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
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Bibliography;

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
      // public WordHelper WordHelper { get; }
        public InterviewController(
           IOptions<JWTTokenOptions> jwtoptions, IMongoDatabase mongo, LoggerHelper logger,
           IdGenerator idgen, JwtInvorker jwtInvorker, MongoClient mongoclient)
        {
            Jwtoptions = jwtoptions;
            Mongo = mongo;
            Logger = logger;
            Idgen = idgen;
            JwtInvorker = jwtInvorker;
            this.mongoclient = mongoclient;
            //WordHelper = wordHelper;
        }
        //api/interview/getinterviewlist 获取采访清单
        [HttpGet]
       // [Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> getinterviewlist(int pagenumber, int pagecounts, string keywords = "")
        {
           
            var filter = Builders<Interviewlist>.Filter.Or(
                Builders<Interviewlist>.Filter.Regex(s => s.inter_personname, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Interviewlist>.Filter.Regex(s => s.title, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Interviewlist>.Filter.Regex(s => s.author, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Interviewlist>.Filter.Regex(s => s.ISBN, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Interviewlist>.Filter.Regex(s => s.publish_house, new BsonRegularExpression($".*{keywords}.*", "i"))
            );
            var counts = await Mongo.DbGetCollection<Interviewlist>()
            .Find(filter).CountDocumentsAsync();
            var list = await Mongo.DbGetCollection<Interviewlist>()
               .Find(filter).Skip((pagenumber - 1) * pagecounts).Limit(pagecounts).ToListAsync();
            return Result.Success("获取成功").SetData(new { result = true, interviewlists = list, counts = counts });
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
            for (int i = 0; i < dto.order_number;i++)
            {
                Orderlist uu = new Orderlist();
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
        //api/interview/getorders 获取订单
        [HttpGet]
        // [Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> getorders(int pagenumber, int pagecounts, string keywords = "")
        {

            var filter = Builders<Orderlist>.Filter.Or(
                Builders<Orderlist>.Filter.Regex(s => s.order_serial_number, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Orderlist>.Filter.Regex(s => s.title, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Orderlist>.Filter.Regex(s => s.author, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Orderlist>.Filter.Regex(s => s.ISBN, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Orderlist>.Filter.Regex(s => s.publish_house, new BsonRegularExpression($".*{keywords}.*", "i"))
            );
            var counts = await Mongo.DbGetCollection<Orderlist>()
            .Find(filter).CountDocumentsAsync();
            var list = await Mongo.DbGetCollection<Orderlist>()
               .Find(filter).Skip((pagenumber - 1) * pagecounts).Limit(pagecounts).ToListAsync();
            return Result.Success("获取成功").SetData(new { result = true, orderlists = list, counts = counts });
        }
        //api/interview/changeorder  修改订单
        [HttpPost]
        public async Task<Result> changeorder([FromBody] ChangeorderDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }

            var filter = Builders<Orderlist>.Filter.Eq(s => s.id, dto.orderid); // 根据特定的 ID 查找记录
            var order = await Mongo.DbGetCollection<Orderlist>().Find(filter).FirstOrDefaultAsync();
            if (order == null)
            {
                return Result.SuccessError("无此订单").SetData(new { result = false });
            }
            var update = Builders<Orderlist>.Update.Set(s => s.ISBN, dto.ISBN)
            .Set(s => s.title, dto.title)
            .Set(s => s.documen_type, dto.documen_type)
            .Set(s => s.author, dto.author)
            .Set(s => s.publish_house, dto.publish_house)
            .Set(s => s.order_price, dto.order_price)
            .Set(s => s.edition, dto.edition)
            .Set(s => s.currency, dto.currency)
            .Set(s => s.description, dto.description);


            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await Mongo.DbGetCollection<Orderlist>().UpdateOneAsync(filter, update);
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
                    await Mongo.DbGetCollection<Orderlist>().UpdateOneAsync(filter, update);
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

        //api/interview/deleteorder删除订单
        [HttpDelete]
        //[Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> deleteorder(long orderid)
        {
            if (orderid == 0)
            {
                throw new InvalidParamError("参数非法");
            }
            var ok = false;
            var filter = Builders<Orderlist>.Filter.Eq(s => s.id, orderid); // 根据特定的 ID 查找记录
            var order = await Mongo.DbGetCollection<Orderlist>().Find(filter).FirstOrDefaultAsync();
            if (order == null)
            {
                return Result.SuccessError("无此订单").SetData(new { result = false });
            }
            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    ok = (await Mongo.DbGetCollection<Orderlist>().DeleteOneAsync(s => s.id == orderid)).IsAcknowledged;
                    await session.CommitTransactionAsync();
                    return Result.Success("删除成功").SetData(new { result = ok });
                }
                catch (Exception ex)
                {
                    Logger.Error($"发生错误：{ex.Message}，发位置：{ex.StackTrace}");
                    await Console.Out.WriteLineAsync($"发生错误：{ex.Message}，发位置：{ex.StackTrace}");
                    await session.AbortTransactionAsync();
                    return Result.SuccessError("删除失败").SetData(new { result = false });
                }
            }
            else
            {
                try
                {
                    ok = (await Mongo.DbGetCollection<Orderlist>().DeleteManyAsync(s => s.id == orderid)).IsAcknowledged;
                    return Result.Success("删除成功").SetData(new { result = ok });
                }
                catch (Exception ex)
                {
                    Logger.Error($"发生错误：{ex.Message}，发位置：{ex.StackTrace}");
                    await Console.Out.WriteLineAsync($"发生错误：{ex.Message}，发位置：{ex.StackTrace}");
                    return Result.SuccessError("删除失败").SetData(new { result = false });
                }
            }
        }

        //api/interview/deleteorders 批量删除
        [HttpDelete]
        //[Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> deleteorders([FromBody] List<long> orderids)
        {
            if (orderids.Count == 0)
            {
                throw new InvalidParamError("参数非法");
            }
            List<FilterDefinition<Orderlist>> filters = new List<FilterDefinition<Orderlist>>();
            foreach (var item in orderids)
            {
                filters.Add(Builders<Orderlist>.Filter.Eq(s => s.id, item));
            }
            var f = Builders<Orderlist>.Filter.Or(filters);
            var ok = false;
            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    ok = (await Mongo.DbGetCollection<Orderlist>().DeleteManyAsync(f)).IsAcknowledged;
                    await session.CommitTransactionAsync();
                    return Result.Success("修改成功").SetData(new { result = ok });
                }
                catch (Exception ex)
                {
                    Logger.Error($"发生错误：{ex.Message}，发位置：{ex.StackTrace}");
                    await Console.Out.WriteLineAsync($"发生错误：{ex.Message}，发位置：{ex.StackTrace}");
                    await session.AbortTransactionAsync();
                    return Result.SuccessError("修改失败").SetData(new { result = false });
                }
            }
            else
            {
                try
                {
                    ok = (await Mongo.DbGetCollection<Orderlist>().DeleteManyAsync(f)).IsAcknowledged;
                    return Result.Success("修改成功").SetData(new { result = ok });
                }
                catch (Exception ex)
                {
                    Logger.Error($"发生错误：{ex.Message}，发位置：{ex.StackTrace}");
                    await Console.Out.WriteLineAsync($"发生错误：{ex.Message}，发位置：{ex.StackTrace}");
                    return Result.SuccessError("修改失败").SetData(new { result = false });
                }
            }
        }

        //api/interview/checkorder 验收订单
        [HttpPost]
        public async Task<Result> checkorder([FromBody] CheckorderDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }

            //创建验收清单
            Checklist uu = new Checklist();
            uu.id= Idgen.CreateId();
            uu.checkperson = dto.checkperson;
            uu.orderlistid = dto.orderid;
            uu.bookseller = dto.bookseller;
            uu.printshop = dto.printshop;
            uu.iscataloged = false;

            var filter = Builders<Orderlist>.Filter.Eq(s => s.id, dto.orderid); // 根据特定的 ID 查找记录
            var orderlist = await Mongo.DbGetCollection<Orderlist>()
               .Find(filter).FirstOrDefaultAsync();
            if (orderlist == null)
            {
                return Result.SuccessError("订单不存在").SetData(new { result = false });
            }
            else if (orderlist.status != Orderlist.Status.order)
            {
                return Result.SuccessError("订单状态不可验收请重试").SetData(new { result = false });
            }
            var update = Builders<Orderlist>.Update.Set(s => s.status, Orderlist.Status.check)
                .Set(s => s.checklistid, uu.id);
            await Mongo.DbGetCollection<Orderlist>().UpdateOneAsync(filter, update);

            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await Mongo.DbGetCollection<Checklist>().InsertOneAsync(uu);
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
                    await Mongo.DbGetCollection<Checklist>().InsertOneAsync(uu);
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

        //api/interview/checkorders 批量验收订单
        [HttpPost]
        public async Task<Result> checkorders([FromBody] CheckordersDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }
            if (dto.orderids == null || dto.orderids.Count == 0)
            {
                throw new InvalidParamError("参数非法");
            }


            //创建验收清单
            List<Checklist> checklists = new List<Checklist>();
            foreach (long item in dto.orderids)
            {
                Checklist uu = new Checklist();
                uu.id = Idgen.CreateId();
                uu.checkperson = dto.checkperson;
                uu.orderlistid = item;
                uu.bookseller = dto.bookseller;
                uu.printshop = dto.printshop;
                uu.iscataloged = false;
                var filter = Builders<Orderlist>.Filter.Eq(s => s.id, item); // 根据特定的 ID 查找记录
                var orderlist = await Mongo.DbGetCollection<Orderlist>()
               .Find(filter).FirstOrDefaultAsync();
                if (orderlist == null)
                {
                    return Result.SuccessError("订单不存在").SetData(new { result = false });
                }
                else if (orderlist.status != Orderlist.Status.order)
                {
                    return Result.SuccessError("订单状态不可验收请重试").SetData(new { result = false });
                }
                var update = Builders<Orderlist>.Update.Set(s => s.status, Orderlist.Status.check)
                    .Set(s => s.checklistid, uu.id);
                await Mongo.DbGetCollection<Orderlist>().UpdateOneAsync(filter, update);
                checklists.Add(uu);
            }
            
                
            


            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await Mongo.DbGetCollection<Checklist>().InsertManyAsync(checklists);
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
                    await Mongo.DbGetCollection<Checklist>().InsertManyAsync(checklists);
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
        //api/interview/returnorder 退货订单
        [HttpPost]
        public async Task<Result> returnorder([FromBody] ReturnorderDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }

            //创建验收清单
            Returnlist uu = new Returnlist();
            uu.id = Idgen.CreateId();
            uu.checkperson = dto.checkperson;
            uu.orderlistid = dto.orderid;
            uu.bookseller = dto.bookseller;
            uu.printshop = dto.printshop;
            uu.return_reason = dto.return_reason;

            var filter = Builders<Orderlist>.Filter.Eq(s => s.id, dto.orderid); // 根据特定的 ID 查找记录
            var orderlist = await Mongo.DbGetCollection<Orderlist>()
               .Find(filter).FirstOrDefaultAsync();
            if (orderlist == null)
            {
                return Result.SuccessError("订单不存在").SetData(new { result = false });
            }
            else if( orderlist.status != Orderlist.Status.order){
                return Result.SuccessError("订单状态不可退货请重试").SetData(new { result = false });
            }
            var update = Builders<Orderlist>.Update.Set(s => s.status, Orderlist.Status.returned)
                    .Set(s => s.return_reason, dto.return_reason)
                    .Set(s => s.returnlistid, uu.id); 
            await Mongo.DbGetCollection<Orderlist>().UpdateOneAsync(filter, update);

            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await Mongo.DbGetCollection<Returnlist>().InsertOneAsync(uu);
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
                    await Mongo.DbGetCollection<Returnlist>().InsertOneAsync(uu);
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

        //api/interview/returnorders 批量退货订单
        [HttpPost]
        public async Task<Result> returnorders([FromBody] ReturnordersDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }
            if (dto.orderids==null||dto.orderids.Count == 0)
            {
                throw new InvalidParamError("参数非法");
            }


            //创建退货订单
            List<Returnlist> returnlists = new List<Returnlist>();
            foreach (long item in dto.orderids)
            {
                
                Returnlist uu = new Returnlist();
                uu.id = Idgen.CreateId();
                uu.orderlistid = item;
                uu.checkperson = dto.checkperson;
                uu.bookseller = dto.bookseller;
                uu.printshop = dto.printshop;
                uu.return_reason = dto.return_reason;
                var filter = Builders<Orderlist>.Filter.Eq(s => s.id,item); // 根据特定的 ID 查找记录
                var orderlist = await Mongo.DbGetCollection<Orderlist>()
               .Find(filter).FirstOrDefaultAsync();
                if (orderlist == null)
                {
                    return Result.SuccessError("订单不存在").SetData(new { result = false });
                }
                else if (orderlist.status != Orderlist.Status.order)
                {
                    return Result.SuccessError("订单状态不可退货请重试").SetData(new { result = false });
                }
                var update = Builders<Orderlist>.Update.Set(s => s.status, Orderlist.Status.returned)
                    .Set(s => s.return_reason, dto.return_reason)
                    .Set(s => s.returnlistid, uu.id);
                await Mongo.DbGetCollection<Orderlist>().UpdateOneAsync(filter, update);
                returnlists.Add(uu);
            }





            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await Mongo.DbGetCollection<Returnlist>().InsertManyAsync(returnlists);
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
                    await Mongo.DbGetCollection<Returnlist>().InsertManyAsync(returnlists);
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

        //api/interview/getreturns 获取退货清单
        [HttpGet]
        // [Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> getreturns(int pagenumber, int pagecounts, string keywords = "")
        {

            var filter = Builders<Orderlist>.Filter.Or(
                Builders<Orderlist>.Filter.Regex(s => s.order_serial_number, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Orderlist>.Filter.Regex(s => s.title, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Orderlist>.Filter.Regex(s => s.author, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Orderlist>.Filter.Regex(s => s.ISBN, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Orderlist>.Filter.Regex(s => s.publish_house, new BsonRegularExpression($".*{keywords}.*", "i"))
            );
            var filter2 = Builders<Orderlist>.Filter.And(
                filter,
                Builders<Orderlist>.Filter.Eq(s => s.status, Orderlist.Status.returned)
            );
            
            var counts = await Mongo.DbGetCollection<Orderlist>()
            .Find(filter2).CountDocumentsAsync();
            var list = await Mongo.DbGetCollection<Orderlist>()
               .Find(filter2).Skip((pagenumber - 1) * pagecounts).Limit(pagecounts).ToListAsync();

            List<ReturnHelper> returnHelperlist = new List<ReturnHelper>();
            foreach (var item in list)
            {
                var filter3 = Builders<Returnlist>.Filter.Eq(
                    s=>s.id,item.returnlistid
                );
                var returnlist = await Mongo.DbGetCollection<Returnlist>().Find(filter3).FirstOrDefaultAsync();
                if ( returnlist != null )
                {
                    ReturnHelper returnHelper = new ReturnHelper();
                    returnHelper.id= returnlist.id;
                    returnHelper.checkperson = returnlist.checkperson;
                    returnHelper.bookseller=returnlist.bookseller;
                    returnHelper.printshop = returnlist.printshop;
                    returnHelper.return_reason = returnlist.return_reason;
                    returnHelper.order_serial_number = item.order_serial_number;
                    returnHelper.order_date = item.order_date;
                    returnHelper.ISBN=item.ISBN;
                    returnHelper.documen_type = item.documen_type;
                    returnHelper.title  = item.title;
                    returnHelper.author = item.author;
                    returnHelper.publish_house=item.publish_house;
                    returnHelper.order_price=item.order_price;
                    returnHelper.edition = item.edition;
                    returnHelper.currency = item.currency;
                    returnHelper.status = item.status;
                    returnHelper.effective= item.effective;
                    returnHelper.description = item.description;
                    returnHelperlist.Add( returnHelper );

                }
               
            }
            
            return Result.Success("获取成功").SetData(new { result = true,returnlists = returnHelperlist, counts = counts });
        }
    }
    public class ReturnHelper
    {
        public long id { get; set; }
        public long orderlistid { get; set; }
        public string? checkperson { get; set; }
        public string? bookseller { get; set; }
        public string? printshop { get; set; }
        public string? return_reason { get; set; }
        public long checklistid { get; set; }
        public long returnlistid { get; set; }

        public string? order_serial_number { get; set; }
        public long order_date { get; set; }
        public string? ISBN { get; set; }
        public Interviewlist.DocumenType documen_type { get; set; }
        public string? title { get; set; }
        public string? author { get; set; }
        public string? publish_house { get; set; }
        public string? order_price { get; set; }
        public string? edition { get; set; }
        public Interviewlist.Currency currency { get; set; }
        public Orderlist.Status status { get; set; }
        public bool effective { get; set; }
        public string? description { get; set; }
    }
}
