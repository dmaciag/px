using MySql.Data.MySqlClient;
using PxServices.Interfaces;
using PxServices.Models;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace PxServices.Repositories
{
    public class PhaseSeriesRepository : IPhaseSeriesRepository
    {
        private readonly string _connectionString = "Data Source=localhost;Initial Catalog=px;User ID=root;Password=";

        public IList<PhaseSeriesConfig> GetConfigs()
        {
            var phaseSeriesConfigs = new List<PhaseSeriesConfig>();
            MySqlConnection? conn = null;
            try
            {
                conn = new MySqlConnection(_connectionString);
                conn.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT * FROM phase_series_config";

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var psConfig = new PhaseSeriesConfig();

                    psConfig.TickerShift = reader["ticker_shift"]?.ToString();
                    psConfig.TickerHistorical = reader["ticker_historical"]?.ToString();
                    psConfig.TickerInflation = reader["ticker_inflation"]?.ToString();

                    if (Int32.TryParse(reader["id"].ToString(), out Int32 id))
                        psConfig.Id = id;
                    if (DateTime.TryParse(reader["start_date_shift"].ToString(), out DateTime startDateShift))
                        psConfig.StartDateShift = startDateShift;
                    if (DateTime.TryParse(reader["start_date_historical"].ToString(), out DateTime startDateHistorical))
                        psConfig.StartDateHistorical = startDateHistorical;
                    if (Decimal.TryParse(reader["offset"].ToString(), out Decimal offset))
                        psConfig.Offset = offset;

                    phaseSeriesConfigs.Add(psConfig);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                conn?.Close();
            }

            return phaseSeriesConfigs;
        }

        public void SaveConfig(PhaseSeriesConfig config)
        {
            MySqlConnection? conn = null;
            try
            {
                conn = new MySqlConnection(_connectionString);
                conn.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = "INSERT INTO `px`.`phase_series_config` (`ticker_shift`, `ticker_historical`, `start_date_shift`, `start_date_historical`, `ticker_inflation`, `offset`)VALUES(@ticker_shift, @ticker_historical, @start_date_shift, @start_date_historical, @ticker_inflation, @offset);";
                
                cmd.Parameters.AddWithValue("@ticker_shift", config.TickerShift);
                cmd.Parameters.AddWithValue("@ticker_historical", config.TickerHistorical);
                cmd.Parameters.AddWithValue("@ticker_inflation", config.TickerInflation);
                cmd.Parameters.AddWithValue("@start_date_shift", config.StartDateShift);
                cmd.Parameters.AddWithValue("@start_date_historical", config.StartDateHistorical);
                cmd.Parameters.AddWithValue("@offset", config.Offset);

                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
            }
            finally
            {
                conn?.Close();
            }
        }

        public void DeleteConfig(int configId)
        {
            MySqlConnection? conn = null;
            try
            {
                conn = new MySqlConnection(_connectionString);
                conn.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "DELETE FROM phase_series_config where Id=@configId";
                cmd.Parameters.AddWithValue("@configId", configId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
            }
            finally
            {
                conn?.Close();
            }
        }
    }
}
