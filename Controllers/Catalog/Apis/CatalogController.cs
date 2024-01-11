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
using LibraryManageSystemApi.Controllers.Catalog.Dto;
using LibraryManageSystemApi.Controllers.Interview.Dto;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace LibraryManageSystemApi.Controllers.Catalog.Apis
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CatalogController : Controller
    {

        public IOptions<JWTTokenOptions> Jwtoptions { get; }
        public IMongoDatabase Mongo { get; }
        public LoggerHelper Logger { get; }
        public IdGenerator Idgen { get; }
        public JwtInvorker JwtInvorker { get; }
        public MongoClient mongoclient { get; }
        public CatalogController(
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
        //api/catalog/getchecklist 获取验收清单
        [HttpGet]
        // [Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> getchecklist(int pagenumber, int pagecounts, bool iscataloged ,string keywords = "" )
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
                Builders<Orderlist>.Filter.Eq(s => s.status, Orderlist.Status.check)
            );

            var counts = await Mongo.DbGetCollection<Orderlist>()
            .Find(filter2).CountDocumentsAsync();
            var list = await Mongo.DbGetCollection<Orderlist>()
               .Find(filter2).Skip((pagenumber - 1) * pagecounts).Limit(pagecounts).ToListAsync();

            List<checkHelper> checkHelperlist = new List<checkHelper>();
            foreach (var item in list)
            {
                var filter3 = Builders<Checklist>.Filter.And(
                    Builders<Checklist>.Filter.Eq(s => s.id, item.checklistid),
                    Builders<Checklist>.Filter.Eq(s => s.iscataloged, iscataloged)
                );
                var checklist = await Mongo.DbGetCollection<Checklist>().Find(filter3).FirstOrDefaultAsync();
                if (checklist != null)
                {
                    checkHelper checkhelper = new checkHelper();
                    checkhelper.id = checklist.id;
                    checkhelper.checkperson = checklist.checkperson;
                    checkhelper.bookseller = checklist.bookseller;
                    checkhelper.printshop = checklist.printshop;
                    checkhelper.iscataloged = checklist.iscataloged;
                    checkhelper.order_serial_number = item.order_serial_number;
                    checkhelper.order_date = item.order_date;
                    checkhelper.ISBN = item.ISBN;
                    checkhelper.documen_type = item.documen_type;
                    checkhelper.title = item.title;
                    checkhelper.author = item.author;
                    checkhelper.publish_house = item.publish_house;
                    checkhelper.order_price = item.order_price;
                    checkhelper.edition = item.edition;
                    checkhelper.currency = item.currency;
                    checkhelper.status = item.status;
                    checkhelper.effective = item.effective;
                    checkhelper.description = item.description;
                    checkHelperlist.Add(checkhelper);

                }

            }

            return Result.Success("获取成功").SetData(new { result = true, returnlists = checkHelperlist, counts = checkHelperlist.Count });
        }

        //api/catalog/catalogbook 直接编目
        [HttpPost]
        public async Task<Result> catalogbook([FromBody] CatalogbookDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }
            var filter = Builders<Checklist>.Filter.Eq(s => s.id, dto.checklistid); // 根据特定的 ID 查找记录
            var checklist = await Mongo.DbGetCollection<Checklist>()
              .Find(filter).FirstOrDefaultAsync();
            Booklist uu = new Booklist();
            if (checklist != null)
            {
                var filter2 = Builders<Orderlist>.Filter.Eq(s => s.id, checklist.orderlistid); // 根据特定的 ID 查找记录
                var orderlist = await Mongo.DbGetCollection<Orderlist>()
                   .Find(filter2).FirstOrDefaultAsync();
                if (orderlist.status!=Orderlist.Status.check)
                {
                    return Result.SuccessError("订单状态应为验收").SetData(new { result = false });
                }
                else if (orderlist == null)
                {
                    return Result.SuccessError("订单不存在").SetData(new { result = false });
                }
                else
                {
                    var update = Builders<Orderlist>.Update.Set(s => s.status, Orderlist.Status.catalog);
                    await Mongo.DbGetCollection<Orderlist>().UpdateOneAsync(filter2, update);
                    //创建验收清单
                    
                    uu.id = Idgen.CreateId();
                    uu.ISBN = orderlist.ISBN;
                    uu.documen_type = orderlist.documen_type;
                    uu.title = orderlist.title;
                    uu.author = orderlist.author;
                    uu.publish_house = orderlist.publish_house;
                    uu.edition  = orderlist.edition;
                    uu.currency = orderlist.currency;
                    uu.order_price = orderlist.order_price;
                    uu.catalog_number = dto.catalog_number;
                    uu.status = Booklist.Status.no_borrow;
                    uu.order_number = 0;
                    uu.catalog_time = AppTimeManager.GetAppTimeStamp();
                }
            }
            

            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await Mongo.DbGetCollection<Booklist>().InsertOneAsync(uu);
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
                    await Mongo.DbGetCollection<Booklist>().InsertOneAsync(uu);
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

        //api/catalog/catalogbooks 直接编目多条数据
        [HttpPost]
        public async Task<Result> catalogbooks([FromBody] CatalogbooksDto dto)
        {
            if (ParamValidator.CheckParamIsValid(dto) == false)
            {
                throw new InvalidParamError("参数非法");
            }
            if (dto.checklistids.Count==0||dto.catalog_numbers.Count!=dto.checklistids.Count)
            {
                throw new InvalidParamError("参数非法");
            }
            int i = 0;
            List<Booklist> list = new List<Booklist>();
            foreach (var item in dto.checklistids) {
                var catalog_number = dto.catalog_numbers[i];
                i++;
                var filter = Builders<Checklist>.Filter.Eq(s => s.id, item); // 根据特定的 ID 查找记录
                var checklist = await Mongo.DbGetCollection<Checklist>()
                  .Find(filter).FirstOrDefaultAsync();
                Booklist uu = new Booklist();
                if (checklist != null)
                {
                    var filter2 = Builders<Orderlist>.Filter.Eq(s => s.id, checklist.orderlistid); // 根据特定的 ID 查找记录
                    var orderlist = await Mongo.DbGetCollection<Orderlist>()
                       .Find(filter2).FirstOrDefaultAsync();
                    if (orderlist.status != Orderlist.Status.check)
                    {
                        return Result.SuccessError("订单状态应为验收").SetData(new { result = false });
                    }
                    else if (orderlist == null)
                    {
                        return Result.SuccessError("订单不存在").SetData(new { result = false });
                    }
                    else
                    {
                        var update = Builders<Orderlist>.Update.Set(s => s.status, Orderlist.Status.catalog);
                        await Mongo.DbGetCollection<Orderlist>().UpdateOneAsync(filter2, update);
                        //创建图书流动表
                        uu.id = Idgen.CreateId();
                        uu.ISBN = orderlist.ISBN;
                        uu.documen_type = orderlist.documen_type;
                        uu.title = orderlist.title;
                        uu.author = orderlist.author;
                        uu.publish_house = orderlist.publish_house;
                        uu.edition = orderlist.edition;
                        uu.currency = orderlist.currency;
                        uu.order_price = orderlist.order_price;
                        uu.catalog_number = catalog_number;
                        uu.status = Booklist.Status.no_borrow;
                        uu.order_number = 0;
                        uu.catalog_time = AppTimeManager.GetAppTimeStamp();
                        list.Add(uu);
                    }
                }
            }
           


            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await Mongo.DbGetCollection<Booklist>().InsertManyAsync(list);
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
                    await Mongo.DbGetCollection<Booklist>().InsertManyAsync(list);
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

        //api/catalog/getbooklists 获取图书流通表
        [HttpGet]
        // [Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> getbooklists(int pagenumber, int pagecounts, string keywords = "")
        {

            var filter = Builders<Booklist>.Filter.Or(
                Builders<Booklist>.Filter.Regex(s => s.catalog_number, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Booklist>.Filter.Regex(s => s.title, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Booklist>.Filter.Regex(s => s.author, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Booklist>.Filter.Regex(s => s.ISBN, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Booklist>.Filter.Regex(s => s.publish_house, new BsonRegularExpression($".*{keywords}.*", "i"))
            );


                var counts = await Mongo.DbGetCollection<Booklist>()
            .Find(filter).CountDocumentsAsync();
                var list = await Mongo.DbGetCollection<Booklist>()
                   .Find(filter).Skip((pagenumber - 1) * pagecounts).Limit(pagecounts).ToListAsync();
                return Result.Success("获取成功").SetData(new { result = true, booklists = list, counts = counts });            
        }

        //api/catalog/deletebooklist 注销报损

        [HttpDelete]
        //[Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> deletebooklist(long booklistid)
        {
            if (booklistid == 0)
            {
                throw new InvalidParamError("参数非法");
            }
            var ok = false;
            var filter = Builders<Booklist>.Filter.Eq(s => s.id, booklistid); // 根据特定的 ID 查找记录
            var book = await Mongo.DbGetCollection<Booklist>().Find(filter).FirstOrDefaultAsync();
            if (book == null)
            {
                return Result.SuccessError("无此书目").SetData(new { result = false });
            }
            if (book.status == Booklist.Status.borrowed)
            {
                return Result.SuccessError("借阅中不可注销").SetData(new { result = false });
            }
            if (AppEnvironment.UseTransaction)
            {
                IClientSessionHandle session = await mongoclient.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    ok = (await Mongo.DbGetCollection<Booklist>().DeleteOneAsync(s => s.id == booklistid)).IsAcknowledged;
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
                    ok = (await Mongo.DbGetCollection<Booklist>().DeleteManyAsync(s => s.id == booklistid)).IsAcknowledged;
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

        //api/catalog/getnewbooks 获取新书
        [HttpGet]
        // [Authorize(Policy = JWTService.PolicyName, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Result> getnewbooks(int pagenumber, int pagecounts, string keywords = "")
        {

            var filter = Builders<Booklist>.Filter.Or(
                Builders<Booklist>.Filter.Regex(s => s.catalog_number, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Booklist>.Filter.Regex(s => s.title, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Booklist>.Filter.Regex(s => s.author, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Booklist>.Filter.Regex(s => s.ISBN, new BsonRegularExpression($".*{keywords}.*", "i")),
                Builders<Booklist>.Filter.Regex(s => s.publish_house, new BsonRegularExpression($".*{keywords}.*", "i"))
            );
            var yesterdayTimestamp = AppTimeManager.GetAppTimeStamp() - TimeSpan.FromDays(1).TotalMilliseconds;
            var filter1 = Builders<Booklist>.Filter.And(
                filter,
                Builders<Booklist>.Filter.Gt(s => s.catalog_time, yesterdayTimestamp)
            );
           
            var counts = await Mongo.DbGetCollection<Booklist>()
            .Find(filter).CountDocumentsAsync();
            var list = await Mongo.DbGetCollection<Booklist>()
               .Find(filter).Skip((pagenumber - 1) * pagecounts).Limit(pagecounts).ToListAsync();
            return Result.Success("获取成功").SetData(new { result = true, booklists = list, counts = counts });
        }

    }
    public class checkHelper
    {
        public long id { get; set; }
        public long orderlistid { get; set; }
        public string? checkperson { get; set; }
        public string? bookseller { get; set; }
        public string? printshop { get; set; }
        public bool iscataloged { get; set; }
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

