
using Dumpify;
using PloliticalScienceSystemApi.Extension;
using PloliticalScienceSystemApi.GlobalSetting;
using PloliticalScienceSystemApi.Log;
using PloliticalScienceSystemApi.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;



namespace PloliticalScienceSystemApi.MongoDbHelper
{
    public static class DBHelper
    {
        public static IMongoDatabase Db { get; set; }
        public static MongoClient Mongoclient { get; set; }
        public static async Task<IServiceCollection> DBInit(this IServiceCollection services)
        {
            Mongoclient = new MongoClient(Appsettings.app("MongoDb:Url"));
            Db = Mongoclient.GetDatabase(Appsettings.app("MongoDb:Database"));
            Db.InitTables(new List<Type>()
            {
                //typeof(AnswerModel),
                //typeof(StuGroup),
                //typeof(GroupRelationship),
                //typeof(ModuleModel),
                //typeof(ModelWorkRelationship),
                //typeof(Sysuser),
                //typeof(TaskModel),
                //typeof(Workmodel),
                typeof(Unit),
                typeof(User),

            });

            services.AddSingleton(Db);
            services.AddSingleton(Mongoclient);
            AddSands();
            return services;
        }
        public static async void AddSands()
        {
            //var res = await Db.DbGetCollection<Sysuser>().Find(s => s.username == "xblg").AnyAsync();
            //if (res == false)
            //{
            //    Sysuser uu = new Sysuser();
            //    uu.username = "xblg";
            //    uu.password = "xblg123.456";
            //    uu.BuildPassword();
            //    uu.id = IdGenFunc.CreateOneId();
            //    uu.name = "维护管理员账户";
            //    uu.loginip = "127.0.0.1";
            //    uu.role = UserRole.Manager;
            //    uu.createtime = AppTimeManager.GetAppTimeStamp();
            //    uu.logintime = AppTimeManager.GetAppTimeStamp();
            //    uu.describle = "无";
            //    await Db.DbGetCollection<Sysuser>().InsertOneAsync(uu);
            //}
        }
        public static IMongoCollection<T> DbGetCollection<T>(this IMongoDatabase mongoDatabase, string collectionname = "null")
        {
            return mongoDatabase.GetCollection<T>((collectionname == "null" || string.IsNullOrEmpty(collectionname)) ? typeof(T).Name : collectionname);
        }
        public static void InitTables(this IMongoDatabase mongoDatabase, List<Type> types)
        {
            //Type workerType = typeof(IMongoDatabase);
            //MethodInfo doWorkMethod = workerType.GetMethod("InitTables");
            bool isinit = true;
            foreach (var item in types)
            {
                var filter = new BsonDocument("name", item.Name);

                var collections = mongoDatabase.ListCollections(new ListCollectionsOptions { Filter = filter });

                if (collections.Any() == false)
                {
                    try
                    {
                        mongoDatabase.CreateCollection(item.Name);
                        ($"------创建【 {item.Name} 】表成功------").Dump();
                        LogHelper.logger.Info($"------创建【 {item.Name} 】表成功------");
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    isinit = false;
                }
            }
            if (isinit)
            {
                Console.WriteLine("初始化tables");
                //mongoDatabase.DbGetCollection<Flow>().InsertOne(new Flow());
                //mongoDatabase.DbGetCollection<FlowType>().InsertOne(new FlowType());
                //mongoDatabase.DbGetCollection<BusinessDepartment>().InsertOne(new BusinessDepartment());
                //mongoDatabase.DbGetCollection<Company>().InsertOne(new Company());
                //mongoDatabase.DbGetCollection<FinancialStaff>().InsertOne(new FinancialStaff());
                //mongoDatabase.DbGetCollection<IncomeExpenseStatement>().InsertOne(new IncomeExpenseStatement());
                //mongoDatabase.DbGetCollection<Project>().InsertOne(new Project());
                //mongoDatabase.DbGetCollection<SupportingMaterial>().InsertOne(new SupportingMaterial());
                //LogHelper.Log(IdGenFunc.CreateOneId(),"Api服务开始",OprateType.Other);
            }

            //mongoDatabase.DbGetCollection<Flow>().DeleteOne(s => s.Id == 0);
            //mongoDatabase.DbGetCollection<FlowType>().DeleteOne(s => s.Id == 0);
            //mongoDatabase.DbGetCollection<BusinessDepartment>().DeleteOne(s => s.Id == 0);
            //mongoDatabase.DbGetCollection<Company>().DeleteOne(s => s.Id == 0);
            //mongoDatabase.DbGetCollection<FinancialStaff>().DeleteOne(s => s.Id == 0);
            //mongoDatabase.DbGetCollection<IncomeExpenseStatement>().DeleteOne(s => s.Id == 0);
            //mongoDatabase.DbGetCollection<Project>().DeleteOne(s => s.Id == 0);
            //mongoDatabase.DbGetCollection<SupportingMaterial>().DeleteOne(s => s.Id == 0);
        }
        public static async void InitTables<T>(this IMongoDatabase mongoDatabase, T t) where T : new()
        {
            var filter = new BsonDocument("name", typeof(T).Name);

            var collections = await mongoDatabase.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });

            if (await collections.AnyAsync() == false)
            {
                await mongoDatabase.DbGetCollection<T>().InsertOneAsync(new T());
            }
        }
    }
}
