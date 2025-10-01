using DnsClient.Protocol;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Mono.Cecil.Cil;
using System.Reflection.Metadata.Ecma335;
using Tangerine.ModuleRunDataTransferApi.Models;

namespace Tangerine.ModuleRunDataTransferApi.Common
{
    public class MongoDbHelper
    {
        public string DatabaseName { get; set; }
        private readonly  MongoClient _mongoClient ;

        public MongoConfig MongoConfig { get; set; }

        public MongoDbHelper(MongoClient mongoClient, MongoConfig config)
        {
            _mongoClient = mongoClient;
            MongoConfig = config;
            DatabaseName = MongoConfig.DataBaseName;
        }
        public MongoDbHelper(MongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
        
            DatabaseName = databaseName;
        }



        public IMongoDatabase GetDatabase(string databaseName)
        {
            return _mongoClient.GetDatabase(databaseName);
        }


        public IMongoCollection<T> GetCollection<T>(string databaseName,string tableName) where T:class
        {
            var db = GetDatabase(databaseName);
            var collection = db.GetCollection<T>(tableName);
            return collection;
        }

        public bool HasCollection(string databaseName, string tableName)
        {
            var db = GetDatabase(databaseName);
            var ret = db.ListCollectionNames()
                    .ToList() // 获取所有集合名称
                    .Contains(tableName); // 判断是否包含目标集合
            return ret;
        }

        public bool HasIndex (string databaseName,string tableName,string indexName)
        {
            var collection = GetCollection<BsonDocument>(databaseName,tableName);
            var indexes = collection.Indexes.List().ToList();

            // 检查是否存在某个索引（例如：检查 "username_1" 索引）
            bool indexExists = indexes.Any(index =>
                index["name"].AsString == indexName);// 索引名称

            return indexExists;
        }

        public bool HasIndex<T>(IMongoCollection<T> collection, string indexName) where T:class
        {
            var indexes = collection.Indexes.List().ToList();

            // 检查是否存在某个索引（例如：检查 "username_1" 索引）
            bool indexExists = indexes.Any(index =>
                index["name"].AsString == indexName);// 索引名称

            return indexExists;
        }


        public bool CreateIndex<T>(IMongoCollection<T> collection, IndexKeysDefinition<T> indexKeys,CreateIndexOptions option) where T : class
        {
            try
            {
                var indexName = option.Name;
                var indexExists = HasIndex<T>(collection, indexName);

                if (!indexExists)
                {
                    collection.Indexes.CreateOne(new CreateIndexModel<T>(indexKeys, option));
                }
                return true;

            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public bool CreateIndexs<T>(IMongoCollection<T> collection, IndexModel<T> timeIndex, IndexModel<T> unionIndex = null, IndexModel<T> uniqueIndex = null) where T : class
        {
            try
            {

                if (timeIndex != null)
                {
                   
                    var indexKey = timeIndex.indexKeys;
                    var option = timeIndex.option;
                    var indexExists = HasIndex<T>(collection, option.Name);

                    if (!indexExists)
                    {
                        collection.Indexes.CreateOne(new CreateIndexModel<T>(indexKey, option));
                    }
                }
                if (unionIndex != null)
                {

                    var indexKey = unionIndex.indexKeys;
                    var option = unionIndex.option;
                    var indexExists = HasIndex<T>(collection, option.Name);

                    if (!indexExists)
                    {
                        collection.Indexes.CreateOne(new CreateIndexModel<T>(indexKey, option));
                    }
                }
                if (uniqueIndex != null)
                {
                    var indexKey = uniqueIndex.indexKeys;
                    var option = uniqueIndex.option;
                    var indexExists = HasIndex<T>(collection, option.Name);

                    if (!indexExists)
                    {
                        collection.Indexes.CreateOne(new CreateIndexModel<T>(indexKey, option));
                    }
                }

                return true;

            }
            catch (Exception ex)
            {

                return false;
            }
        }



    }

    public class IndexModel<T>
    {
        public IndexKeysDefinition<T> indexKeys  { get; set; }

        public CreateIndexOptions option { get; set; }
    }
}
