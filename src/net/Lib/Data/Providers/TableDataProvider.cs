using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Security.KeyVault.Secrets;
using Lib.Model;

namespace Lib.Data.Providers
{
    public class TableDataProvider : IDataProvider, IDisposable
    {
        public TableServiceClient TableServiceClient;
        public TableClient TableClient;

        public async Task InitializeAsync(TokenCredential credential, SecretClient secretClient)
        {
            var storageKey = await secretClient.GetSecretAsync(Config.StorageKeySecretName);

            TableClient = new TableClient(Config.StorageTableEndpoint,
                Config.StorageTableName,
                new TableSharedKeyCredential(Config.StorageAccountName, storageKey.Value.Value));

            await TableClient.CreateIfNotExistsAsync();
        }

        public void Dispose()
        {
        }

        public async Task<Image> GetImageAsync(string id)
        {
            var image = new ImageTableEntity() { Id = id };

            await foreach (ImageTableEntity item in TableClient.QueryAsync<ImageTableEntity>(i => i.PartitionKey == image.PartitionKey && i.RowKey == image.RowKey))
            {
                return item;
            }
            return null;
        }

        public async Task<Image> DeleteImageAsync(string id)
        {
            var image = new ImageTableEntity { Id = id };
            await TableClient.DeleteEntityAsync(image.PartitionKey, image.RowKey);
            return image;

            //TODO : Bubble up errors through the stack
        }

        public async IAsyncEnumerable<Image> GetImagesAsync()
        {
            await foreach (var item in TableClient.QueryAsync<ImageTableEntity>())
            {
                yield return item;
            }
        }

        public async Task<Image> UpsertImageAsync(IImage image)
        {
            var response = await TableClient.UpsertEntityAsync<ImageTableEntity>(image as ImageTableEntity);
            return image as ImageTableEntity;
        }

        public IImage DeserializeImage(string json)
        {
            return JsonSerializer.Deserialize<ImageTableEntity>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}