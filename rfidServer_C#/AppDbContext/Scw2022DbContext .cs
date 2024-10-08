using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace rfidServer_C_.AppDbContext;

public class Scw2022DbContext: DbContext
{
    public Scw2022DbContext(DbContextOptions<Scw2022DbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }

    public class PackInfoDto
    {
        public string PalletNo { get; set; }
        public string PalletQty { get; set; }
        public string BoxQty { get; set; }
        public string BoxNoStart { get; set; }
        public string BoxNoEnd { get; set; }
        public string BatNo { get; set; }
        public string BoxGw { get; set; }
        public string BoxNw { get; set; }
        public string MaterialNo { get; set; }
        public string CustomerMaterialNo { get; set; }
        public string WorkOrderNo { get; set; }


        //PalletNo = reader["棧板號"].ToString(),
        //PalletQty = Convert.ToInt32(reader["棧板數量"]),
        //BoxQty = Convert.ToInt32(reader["箱數"]),
        //BoxNoStart = reader["箱數起"].ToString(),
        //BoxNoEnd = reader["箱數迄"].ToString(),
        //BatNo = reader["箱號"].ToString(),
        //BoxGw = Convert.ToDecimal(reader["毛重"]),
        //BoxNw = Convert.ToDecimal(reader["淨重"]),
        //MaterialNo = reader["料號"].ToString(),
        //CustomerMaterialNo = reader["客戶物料"].ToString(),
        //WorkOrderNo = reader["工單"].ToString()
    }

    public async Task<List<object>> GetPackInfoByPalletNoAsync(string tagId)
    {
        var result = new List<object>();

        var sql = @"
            select DISTINCT
              sap_mopackinfo.pi_palletno 棧板號 , sap_mopackinfo.pi_palletqty 棧板數量 , sap_mopackinfo.pi_boxqty 箱數 
            , sap_mopackinfo.pi_boxnostart 箱數起 , sap_mopackinfo.pi_boxnoend 箱數迄 , sap_mopackinfo.pi_batno 箱號 
            , sap_mopackinfo.pi_boxgw 毛重 , sap_mopackinfo.pi_boxnw 淨重 , sap_zsdr0049.MATNR 料號 , sap_zsdr0049.KDMAT 客戶物料
            , sap_mopackinfo.pi_mo 工單
             from sap_mopackinfo
            INNER JOIN sap_mopackinfoprehead on sap_mopackinfo.pi_mo =  sap_mopackinfoprehead.ph_mo
            INNER JOIN sap_zsdr0049 ON sap_mopackinfoprehead.ph_od = sap_zsdr0049.VBELN
            where sap_mopackinfo.pi_palletno not in ('1','2')
            and sap_mopackinfo.pi_palletqty > 0
            and sap_mopackinfo.pi_palletno =  @TagId -- 棧板號
            order by sap_mopackinfo.pi_palletno desc , sap_mopackinfo.pi_boxnostart asc
        ";

        using (var connection = this.Database.GetDbConnection())
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(sql, (SqlConnection)connection))
            {
                //command.Parameters.Add(new SqlParameter("@TagId", tagId)); 
                command.Parameters.Add(new SqlParameter("@TagId", "4647018014005")); 

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var packInfo = new PackInfoDto
                        {
                            PalletNo = reader["棧板號"].ToString(),
                            PalletQty = reader["棧板數量"].ToString(),
                            BoxQty = reader["箱數"].ToString(),
                            BoxNoStart = reader["箱數起"].ToString(),
                            BoxNoEnd = reader["箱數迄"].ToString(),
                            BatNo = reader["箱號"].ToString(),
                            BoxGw = reader["毛重"].ToString(),
                            BoxNw = reader["淨重"].ToString(),
                            MaterialNo = reader["料號"].ToString(),
                            CustomerMaterialNo = reader["客戶物料"].ToString(),
                            WorkOrderNo = reader["工單"].ToString()
                        };
                        result.Add(packInfo);
                    }
                }
            }
        }

        return result;
    }

}
