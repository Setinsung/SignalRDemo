using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SignalRJWT.Hubs;
using System.Data;
using System.Globalization;

namespace SignalRJWT
{
    public class ImportExecutor
    {
        private readonly IHubContext<ChatRoomHub> hubContext;
        private readonly string optionsConnStr;

        public ImportExecutor(IHubContext<ChatRoomHub> hubContext, IOptionsSnapshot<ConnStrOptions> connConfig)
        {
            this.hubContext = hubContext;
            this.optionsConnStr = connConfig.Value.Default ?? "";
        }

        // 用于抓取异常并通知
        public async Task ExecuteAsync(string connectionId)
        {
            ISingleClientProxy client = hubContext.Clients.Client(connectionId);
            try
            {
                await DoExecuteAsync(client);
            }
            catch (Exception ex)
            {
                await client.SendAsync("DictImportState", "导入失败");
                Console.WriteLine("ImportExecutor出现异常," + ex.Message);
            }
        }

        public async Task DoExecuteAsync(ISingleClientProxy client)
        {
            await client.SendAsync("DictImportState", "准备数据...");
            using var reader = new StreamReader(@"./stardict.csv");
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
            });
            var records = csv.GetRecordsAsync<DictRecord>();
            List<DictRecord> dictRecords = await records.ToListAsync();

            using SqlBulkCopy bulkCopy = new(optionsConnStr);
            bulkCopy.DestinationTableName = "T_WordItems";
            bulkCopy.ColumnMappings.Add("Word", "Word");
            bulkCopy.ColumnMappings.Add("Phonetic", "Phonetic");
            bulkCopy.ColumnMappings.Add("Definition", "Definition");
            bulkCopy.ColumnMappings.Add("Translation", "Translation");

            using DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Word");
            dataTable.Columns.Add("Phonetic");
            dataTable.Columns.Add("Definition");
            dataTable.Columns.Add("Translation");

            await client.SendAsync("DictImportState", "导入中...");
            int totalCount = dictRecords.Count();
            for (int i = 0; i < totalCount; i++)
            {
                DataRow row = dataTable.NewRow();
                row["Word"] = dictRecords[i].Word;
                row["Phonetic"] = dictRecords[i].Phonetic;
                row["Definition"] = dictRecords[i].Definition;
                row["Translation"] = dictRecords[i].Translation;
                dataTable.Rows.Add(row);
                // 每1000条提交一次
                if (dataTable.Rows.Count == 1000)
                {
                    await bulkCopy.WriteToServerAsync(dataTable);
                    dataTable.Clear();
                    await client.SendAsync("DictImportProgress", i + 1, totalCount);
                }
            }
            await bulkCopy.WriteToServerAsync(dataTable); // 剩余的一组
            await client.SendAsync("DictImportProgress", totalCount, totalCount);
            await client.SendAsync("DictImportState", "导入完成");
        }
    }
}
