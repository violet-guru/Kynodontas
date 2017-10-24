using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kynodontas.Basic.Bll
{
    public class AccountBll
    {
        private readonly IDatabaseHelper<Record> _helper;

        public AccountBll(IDatabaseHelper<Record> databaseHelper)
        {
            _helper = databaseHelper;
        }

        /// <summary>
        /// Get record from db based on the tokenId
        /// </summary>
        public async Task<List<Record>> GetUserLogins(string tokenClientId)
        {
            return await _helper
                .SelectDocumentsWhere(d => !d.IsDeleted && d.RecordType == 5 && d.id == tokenClientId, false,
                    "//false, because only equality indexing");
        }

        public async Task<List<Record>> GetAllRecords()
        {
            return await _helper
                .SelectDocumentsWhere(d => d.AddedDate > ClockDate.MinValue, true, "//true, because only range indexing");
        }
    }
}
