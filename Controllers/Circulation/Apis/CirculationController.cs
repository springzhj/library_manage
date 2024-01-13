using LibraryManageSystemApi.ApiRequsetCheck;
using LibraryManageSystemApi.GlobalSetting;
using LibraryManageSystemApi.JwtExtension;
using LibraryManageSystemApi.Log;
using LibraryManageSystemApi.MiddlerWare;
using LibraryManageSystemApi.Model;
using LibraryManageSystemApi.MongoDbHelper;
using LibraryManageSystemApi.Extention;
using IdGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using LibraryManageSystemApi.Controllers.Circulation.Dto;
using LibraryManageSystemApi.Controllers.Interview.Dto;
using DocumentFormat.OpenXml.Drawing.Charts;
using LibraryManageSystemApi.Controllers.Catalog.Dto;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Amazon.SecurityToken.Model;

namespace LibraryManageSystemApi.Controllers.Circulation.Apis
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CirculationController : ControllerBase
    {

        public IOptions<JWTTokenOptions> Jwtoptions { get; }
        public IMongoDatabase Mongo { get; }
        public LoggerHelper Logger { get; }
        public IdGenerator Idgen { get; }
        public JwtInvorker JwtInvorker { get; }
        public MongoClient mongoclient { get; }
        public CirculationController(
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
        //api/circulation/putappointmentlist 录入预约记录
        [HttpPost]
        public async Task<Result> putappointmentlist([FromBody] PutappointmentlistDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }
            var userfilter = Builders<User>.Filter.Eq(s => s.id, dto.userid); // 根据特定的 ID 查找记录
            var user = await Mongo.DbGetCollection<User>()
              .Find(userfilter).FirstOrDefaultAsync();
            if (Appiontmenthelper.can_appoint_number(user) <= 0)
            {
                return Result.SuccessError("借阅数量已达上限").SetData(new { result = false });
            }
            else
            {
                var userupdate = Builders<User>.Update.Set(s => s.appointed_number, user.appointed_number + 1);
                await Mongo.DbGetCollection<User>().UpdateOneAsync(userfilter, userupdate);
            }


            var bookfilter = Builders<Booklist>.Filter.Eq(s => s.id, dto.bookid); // 根据特定的 ID 查找记录
            var book = await Mongo.DbGetCollection<Booklist>()
              .Find(bookfilter).FirstOrDefaultAsync();
            var bookupdate = Builders<Booklist>.Update.Set(s => s.order_number, book.order_number + 1);
            await Mongo.DbGetCollection<Booklist>().UpdateOneAsync(bookfilter, bookupdate);

            Appointmentlist uu = new Appointmentlist();
            uu.id = Idgen.CreateId();
            uu.userid = dto.userid;
            uu.bookid = dto.bookid;
            uu.username = user.name;
            uu.bookname = book.title;
            uu.appointtime = AppTimeManager.GetAppTimeStamp();
            uu.status = Appointmentlist.Status.appointed;
            uu.catalog_number = book.catalog_number;
            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await Mongo.DbGetCollection<Appointmentlist>().InsertOneAsync(uu);
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
                    await Mongo.DbGetCollection<Appointmentlist>().InsertOneAsync(uu);
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

        //api/circulation/getappointmentlist 获取预约记录
        [HttpGet]
        // [Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> getappointmentlist(int pagenumber, int pagecounts, long userid, bool isnofinshed, string keywords = "")
        {
            var filter1 = Builders<Appointmentlist>.Filter.Or(
                Builders<Appointmentlist>.Filter.Regex(s => s.bookname, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Appointmentlist>.Filter.Regex(s => s.catalog_number, new BsonRegularExpression($".*{keywords}.*", "i"))
                );
            var filter = Builders<Appointmentlist>.Filter.And(
                 Builders<Appointmentlist>.Filter.Eq(s => s.userid, userid),
                 filter1
                 );
            if (isnofinshed)
            {
                filter = Builders<Appointmentlist>.Filter.And(
                Builders<Appointmentlist>.Filter.Eq(s => s.userid, userid),
                Builders<Appointmentlist>.Filter.Ne(s => s.status, Appointmentlist.Status.finished),
                filter1
                );
            }
            var counts = await Mongo.DbGetCollection<Appointmentlist>()
            .Find(filter).CountDocumentsAsync();
            var list = await Mongo.DbGetCollection<Appointmentlist>()
               .Find(filter).Skip((pagenumber - 1) * pagecounts).Limit(pagecounts).ToListAsync();
            return Result.Success("获取成功").SetData(new { result = true, appointmentlists = list, counts = counts });
        }

        //api/circulation/deleteappointment 删除预约信息
        [HttpDelete]
        //[Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> deleteappointment(long appointionid)
        {
            if (appointionid == 0)
            {
                throw new InvalidParamError("参数非法");
            }
            var ok = false;
            var filter = Builders<Appointmentlist>.Filter.Eq(s => s.id, appointionid); // 根据特定的 ID 查找记录
            var appointmentlist = await Mongo.DbGetCollection<Appointmentlist>().Find(filter).FirstOrDefaultAsync();
            if (appointmentlist == null)
            {
                return Result.SuccessError("无此预约记录").SetData(new { result = false });
            }
            if (appointmentlist.status != Appointmentlist.Status.appointed)
            {
                return Result.SuccessError("预约记录状态不可删除").SetData(new { result = false });
            }


            var userfilter = Builders<User>.Filter.Eq(s => s.id, appointmentlist.userid);
            var user = await Mongo.DbGetCollection<User>().Find(userfilter).FirstOrDefaultAsync();
            var userupdate = Builders<User>.Update.Set(s => s.appointed_number, user.appointed_number - 1);
            await Mongo.DbGetCollection<User>().UpdateOneAsync(userfilter, userupdate);

            var bookfilter = Builders<Booklist>.Filter.Eq(s => s.id, appointmentlist.bookid); // 根据特定的 ID 查找记录
            var book = await Mongo.DbGetCollection<Booklist>()
              .Find(bookfilter).FirstOrDefaultAsync();
            var bookupdate = Builders<Booklist>.Update.Set(s => s.order_number, book.order_number - 1);
            await Mongo.DbGetCollection<Booklist>().UpdateOneAsync(bookfilter, bookupdate);


            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    ok = (await Mongo.DbGetCollection<Appointmentlist>().DeleteOneAsync(s => s.id == appointionid)).IsAcknowledged;
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
                    ok = (await Mongo.DbGetCollection<Appointmentlist>().DeleteManyAsync(s => s.id == appointionid)).IsAcknowledged;
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

        //api/circulation/putlendingbooklist 借阅
        [HttpPost]
        public async Task<Result> putlendingbooklist([FromBody] PutlendingbooklistDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }
            var appointment = await Mongo.DbGetCollection<Appointmentlist>()
              .Find(Builders<Appointmentlist>.Filter.Eq(s => s.id, dto.appointmentid)).FirstOrDefaultAsync();// 根据特定的 ID 查找记录
            var book = await Mongo.DbGetCollection<Booklist>()
              .Find(Builders<Booklist>.Filter.Eq(s => s.id, appointment.bookid)).FirstOrDefaultAsync();


            if (book == null)
            {
                return Result.SuccessError("书籍不存在").SetData(new { result = false });
            }
            else
            {
                var userupdate = Builders<Booklist>.Update.Set(s => s.status, Booklist.Status.borrowed)
                    .Set(s => s.order_number, book.order_number - 1);
                await Mongo.DbGetCollection<Booklist>().UpdateOneAsync(Builders<Booklist>.Filter.Eq(s => s.id, appointment.bookid), userupdate);
            }


            var appointmentfilter = Builders<Appointmentlist>.Filter.Eq(s => s.id, dto.appointmentid); // 根据特定的 ID 查找记录
            var appointmentupdate = Builders<Appointmentlist>.Update.Set(s => s.status, Appointmentlist.Status.Borrowed);
            await Mongo.DbGetCollection<Appointmentlist>().UpdateOneAsync(appointmentfilter, appointmentupdate);
            Lendingbookslist uu = new Lendingbookslist();
            uu.id = Idgen.CreateId();
            uu.appointmentid = dto.appointmentid;
            uu.userid = appointment.userid;
            uu.bookid = appointment.bookid;
            uu.username = appointment.username;
            uu.bookname = appointment.bookname;
            uu.lendstarttime = AppTimeManager.GetAppTimeStamp();
            uu.lendendtime = AppTimeManager.GetAppTimeStamp() + (long)TimeSpan.FromDays(5).TotalMilliseconds;
            uu.catalog_number = appointment.catalog_number;
            uu.isreturn = false;
            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await Mongo.DbGetCollection<Lendingbookslist>().InsertOneAsync(uu);
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
                    await Mongo.DbGetCollection<Lendingbookslist>().InsertOneAsync(uu);
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

        //api/circulation/getlendingbooklist 获取图书借阅表
        [HttpGet]
        // [Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> getlendingbooklist(int pagenumber, int pagecounts, long userid, bool isreturn, string keywords = "")
        {
            var filter1 = Builders<Lendingbookslist>.Filter.Or(
                Builders<Lendingbookslist>.Filter.Regex(s => s.bookname, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Lendingbookslist>.Filter.Regex(s => s.catalog_number, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Lendingbookslist>.Filter.Regex(s => s.username, new BsonRegularExpression($".*{keywords}.*", "i"))
                );
            var filter = Builders<Lendingbookslist>.Filter.And(
                Builders<Lendingbookslist>.Filter.Eq(s => s.userid, userid),
                filter1
                );
            if (userid == 0)
            {
                filter = Builders<Lendingbookslist>.Filter.And(
                filter1
                );
                if (isreturn)
                {
                    filter = Builders<Lendingbookslist>.Filter.And(
                    Builders<Lendingbookslist>.Filter.Ne(s => s.isreturn, true),
                    filter1
                    );
                }
            }
            else
            {
                if (isreturn)
                {
                    filter = Builders<Lendingbookslist>.Filter.And(
                    Builders<Lendingbookslist>.Filter.Eq(s => s.userid, userid),
                    Builders<Lendingbookslist>.Filter.Ne(s => s.isreturn, true),
                    filter1
                    );
                }
            }
            var counts = await Mongo.DbGetCollection<Lendingbookslist>()
            .Find(filter).CountDocumentsAsync();
            var list = await Mongo.DbGetCollection<Lendingbookslist>()
               .Find(filter).Skip((pagenumber - 1) * pagecounts).Limit(pagecounts).ToListAsync();
            return Result.Success("获取成功").SetData(new { result = true, appointmentlists = list, counts = counts });
        }

        //api/circulation/putbackbook 还书
        [HttpPost]
        public async Task<Result> putbackbook([FromBody] PutbackbookDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }
            var lendbook = await Mongo.DbGetCollection<Lendingbookslist>()
              .Find(Builders<Lendingbookslist>.Filter.Eq(s => s.id, dto.lendingbooklistid)).FirstOrDefaultAsync();// 根据特定的 ID 查找记录
            var appointment = await Mongo.DbGetCollection<Appointmentlist>()
              .Find(Builders<Appointmentlist>.Filter.Eq(s => s.id, lendbook.appointmentid)).FirstOrDefaultAsync();


            if (appointment == null)
            {
                return Result.SuccessError("预约记录不存在").SetData(new { result = false });
            }
            else
            {
                var appointupdate = Builders<Appointmentlist>.Update.Set(s => s.status, Appointmentlist.Status.finished);
                await Mongo.DbGetCollection<Appointmentlist>().UpdateOneAsync(Builders<Appointmentlist>.Filter.Eq(s => s.id, lendbook.appointmentid), appointupdate);
            }

            if (AppTimeManager.GetAppTimeStamp() > lendbook.lendendtime)
            {
                //超期处理
                Overduelist uu = new Overduelist();
                uu.id = Idgen.CreateId();
                uu.lendingbookslistid = lendbook.id;
                uu.lendstarttime = lendbook.lendstarttime;
                uu.lendendtime = lendbook.lendendtime;
                uu.returntime = AppTimeManager.GetAppTimeStamp();
                uu.userid = lendbook.userid;
                uu.username = lendbook.username;
                uu.bookid = lendbook.bookid;
                uu.bookname = lendbook.bookname;
                uu.catalog_number = lendbook.catalog_number;
                Penaltylist aa = new Penaltylist();
                aa.id = Idgen.CreateId();
                aa.userid = lendbook.userid;
                aa.username = lendbook.username;
                aa.lendingbooklistid = lendbook.id;
                aa.penalty_number = 10;
                aa.isput = false;
                try
                {
                    await Mongo.DbGetCollection<Overduelist>().InsertOneAsync(uu);
                    await Mongo.DbGetCollection<Penaltylist>().InsertOneAsync(aa);
                    return Result.Success("借阅已超期，已生成超期记录").SetData(new { result = true });
                }
                catch (Exception ex)
                {
                    Logger.Error($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                    await Console.Out.WriteLineAsync($"发生错误：{ex.Message}，发生位置：{ex.StackTrace}");
                    return Result.SuccessError("修改失败").SetData(new { result = false });
                }

            }
            else
            {
                return Result.Success("还书成功").SetData(new { result = false });
                //未超期的处理
            }

        }

        //api/circulation/getpenaltylists 获取罚款明细表
        [HttpGet]
        // [Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> getpenaltylists(int pagenumber, int pagecounts, long userid, bool isput, string keywords = "")
        {
            var filter1 = Builders<Penaltylist>.Filter.Or(
                Builders<Penaltylist>.Filter.Regex(s => s.username, new BsonRegularExpression($".*{keywords}.*", "i"))
                );
            var filter = Builders<Penaltylist>.Filter.And(
                Builders<Penaltylist>.Filter.Eq(s => s.userid, userid),
                filter1
                );
            if (userid == 0)
            {
                filter = Builders<Penaltylist>.Filter.And(
                filter1
                );
                if (isput)
                {
                    filter = Builders<Penaltylist>.Filter.And(
                    Builders<Penaltylist>.Filter.Ne(s => s.isput, true),
                    filter1
                    );
                }
            }
            else
            {
                if (isput)
                {
                    filter = Builders<Penaltylist>.Filter.And(
                    Builders<Penaltylist>.Filter.Eq(s => s.userid, userid),
                    Builders<Penaltylist>.Filter.Ne(s => s.isput, true),
                    filter1
                    );
                }
            }
            var counts = await Mongo.DbGetCollection<Penaltylist>()
            .Find(filter).CountDocumentsAsync();
            var list = await Mongo.DbGetCollection<Penaltylist>()
               .Find(filter).Skip((pagenumber - 1) * pagecounts).Limit(pagecounts).ToListAsync();
            return Result.Success("获取成功").SetData(new { result = true, penaltylists = list, counts = counts });
        }

        //api/circulation/getoverduelists 获取超期记录表
        [HttpGet]
        // [Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> getoverduelists(int pagenumber, int pagecounts, long userid, string keywords = "")
        {
            var filter1 = Builders<Overduelist>.Filter.Or(
                Builders<Overduelist>.Filter.Regex(s => s.username, new BsonRegularExpression($".*{keywords}.*", "i"))
                );
            var filter = Builders<Overduelist>.Filter.And(
                Builders<Overduelist>.Filter.Eq(s => s.userid, userid),
                filter1
                );
            if (userid == 0)
            {
                filter = Builders<Overduelist>.Filter.And(
                filter1
                );
            }
            var counts = await Mongo.DbGetCollection<Overduelist>()
            .Find(filter).CountDocumentsAsync();
            var list = await Mongo.DbGetCollection<Overduelist>()
               .Find(filter).Skip((pagenumber - 1) * pagecounts).Limit(pagecounts).ToListAsync();
            return Result.Success("获取成功").SetData(new { result = true, overduelists = list, counts = counts });
        }

    }
    public class Appiontmenthelper
    {
        public static int can_appoint_number(User user)
        {
            if (user.level== User.Level.unit_personnel) {
                return 0;
            }
            if (user.level == User.Level.ordinary_student)
            {
                return 5-user.appointed_number;
            }
            if (user.level == User.Level.advanced_student)
            {
                return 10 - user.appointed_number;
            }
            if (user.level == User.Level.ordinary_teacher)
            {
                return 15 - user.appointed_number;
            }
            if (user.level == User.Level.advanced_teacher)
            {
                return 20 - user.appointed_number;
            }
            return 0;
        }
    }
}
