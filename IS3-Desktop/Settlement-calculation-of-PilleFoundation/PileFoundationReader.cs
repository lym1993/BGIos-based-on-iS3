using System;
using System.Collections.Generic;
using System.Data.OleDb;
using Dapper;

namespace Settlement_calculation_of_PilleFoundation
{
    public class PileFoundationReader : IDisposable
    {
        #region Fields
        //私有连接字段
        private OleDbConnection _connection;
        #endregion

        #region Ctor
        //构造函数
        public PileFoundationReader(string mdbPath)
        {
            _connection = new OleDbConnection($"Provider=Microsoft.Jet.OleDb.4.0;Data Source={mdbPath}");
        }
        #endregion

        public IEnumerable<PileFoundationDto> GetPileFoundations()
        {
            var list = new List<PileFoundationDto>();
            var lookup = new Dictionary<long, PileFoundationDto>();
            _connection.Query<PileFoundationDto, PileFoundationStrataInfoDto, PileFoundationDto>(@"select pf.*,pfs.* from dbo_PileFoundation pf
            inner join dbo_PileFoundationStrataInfo pfs ON pf.[ID] = pfs.[PileFoundationID];", (pf, pfs) =>
            {
                PileFoundationDto pileFoundation;
                if (!lookup.TryGetValue(pfs.PileFoundationID, out pileFoundation))
                {
                    lookup.Add(pfs.PileFoundationID, pileFoundation = pf);
                }
                //如果strataInfo是空的，新建列表并加入地质数据，以PileFoundationID为筛选分界
                if (pileFoundation.PileFoundationStrataInfos == null)
                    pileFoundation.PileFoundationStrataInfos = new List<PileFoundationStrataInfoDto>();
                pileFoundation.PileFoundationStrataInfos.Add(pfs);
                return pileFoundation;
            }, splitOn: "PileFoundationID");

            return lookup.Values;
        }


        //内存释放方法
        public void Dispose()
        {
            try
            {
                _connection.Close();
                _connection.Dispose();
            }
            catch
            {

            }
        }
    }
}
