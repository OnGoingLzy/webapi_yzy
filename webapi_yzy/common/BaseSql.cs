
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace webapi_yzy
{
    public class BaseSql
    {
        //加载appsetting.json
        static IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

        /// <summary>
        /// 数据库连接字符串，配置文件在appsettings.json文件中
        /// </summary>
        public static readonly string connectString = configuration["DBSetting:ConnectString"];

        public static BaseSql us = null;


        public BaseSql()
        {
        }

        /// <summary>
        /// 单例模式的实现方法，保证只能被实例化一次
        /// </summary>
        /// <returns></returns>
        public static BaseSql getInstance()
        {
            if (us == null) us = new BaseSql();
            return us;
        }

        /// <summary>
        /// 执行sql 语句 select，返回DataSet
        /// </summary>
        /// <param name="ssql">sql语句字符串</param>
        /// <returns>DataSet</returns>
        public DataSet getDataSet(string ssql)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectString))
                {
                    conn.Open();
                    using (SqlDataAdapter dr = new SqlDataAdapter(ssql, conn))
                    {
                        DataSet ds = new DataSet();
                        dr.SelectCommand.CommandType = CommandType.Text;
                        dr.Fill(ds);
                        return ds;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 执行sql 语句 select，返回DataTable
        /// </summary>
        /// <param name="ssql">sql语句字符串</param>
        /// <returns>getDataTable</returns>
        public DataTable getDataTable(string ssql)
        {
            if (ssql == "" || ssql == null)
            {
                return null;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connectString))
                {
                    conn.Open();
                    using (SqlDataAdapter dr = new SqlDataAdapter(ssql, conn))
                    {
                        dr.SelectCommand.CommandType = CommandType.Text;
                        dr.SelectCommand.CommandTimeout = 600;
                        DataTable dt = new DataTable();
                        dr.Fill(dt);
                        return dt;

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 执行sql 语句insert,delete,update，不返回结果集
        /// </summary>
        /// <param name="ssql">sql语句字符串</param>
        /// <returns>0：执行失败，1：执行成功</returns>
        public int runSQLNonResult(string ssql)//涂利添加2014-06-05
        {
            if (ssql == "" || ssql == null)
            {
                return 0;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connectString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(ssql, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                        return 1;//执行成功
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 创建存储过程的参数
        /// </summary>
        /// <param name="ParamName">参数名称</param>
        /// <param name="DbType">SqlDbType</param>
        /// <param name="Size">参数大小</param>
        /// <param name="Direction">参数方向：input、output</param>
        /// <param name="Value">参数值</param>
        /// <returns>SqlParameter</returns>
        public SqlParameter createparam(string ParamName, SqlDbType DbType, Int32 Size, ParameterDirection Direction, object Value)
        {
            SqlParameter param;
            if (Size > 0)
            {
                param = new SqlParameter(ParamName, DbType, Size);
            }
            else
            {
                param = new SqlParameter(ParamName, DbType);
            }

            param.Direction = Direction;
            if (!(Direction == ParameterDirection.Output && Value == null))
            {
                param.Value = Value;
            }
            return param;
        }

        /// <summary>
        /// 执行不返回结果集的存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="prams">参数数组</param>
        /// <returns>1：成功</returns>
        public int RunProcstr(string procName, params SqlParameter[] prams)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(connectString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(procName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 600;
                        if (prams != null)
                        {
                            foreach (SqlParameter parameter in prams)
                            {
                                cmd.Parameters.Add(parameter);
                            }
                        }
                        cmd.ExecuteNonQuery();
                        return 1;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }

        /// <summary>
        /// 执行返回结果集的存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="ds">返回的结果集</param>
        /// <param name="prams">参数数组</param>
        /// <returns>1：成功</returns>
        public int RunProcdsstr(string procName, ref DataSet ds, params SqlParameter[] prams)
        {
            ds = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectString))
                {
                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(procName, conn))
                    {
                        da.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da.SelectCommand.CommandTimeout = 600;
                        if (prams != null)
                        {
                            foreach (SqlParameter parameter in prams)
                            {
                                da.SelectCommand.Parameters.Add(parameter);
                            }
                        }
                        da.Fill(ds);
                        return 1;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


        /// <summary>
        /// 执行procedure,返回执行结果
        /// 此类存储过程，返回select的结果
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public DataTable procedure(string proc, ArrayList paras)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectString))
            {
                SqlDataAdapter ada = new SqlDataAdapter(proc, conn);
                conn.Open();
                ada.SelectCommand.CommandTimeout = 600;
                ada.SelectCommand.CommandType = CommandType.StoredProcedure;
                //添加存储过程参数
                foreach (UProcPara p in paras)
                {
                    ada.SelectCommand.Parameters.Add(p.name, p.type, p.size).Value = p.value;
                }
                ada.Fill(dt);
            }
            return dt;
        }


        #region 周晶增加
        public SqlDataReader GetDataReader(string sql)
        {
            if (sql == null || sql.Length == 0) throw new ArgumentNullException("commandText");
            if (connectString == null || connectString.Length == 0) throw new ArgumentNullException("s_strconn");
            SqlConnection conn = new SqlConnection(connectString);

            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                return cmd.ExecuteReader();
            }
            catch (Exception)
            {
                conn.Close();
                throw;
            }
        }


        /// <summary>
        /// 返回第一行第一列值
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string GetFistString(string sql)
        {
            if (sql == null || sql.Length == 0) throw new ArgumentNullException("commandText");
            if (connectString == null || connectString.Length == 0) throw new ArgumentNullException("connectString");
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    conn.Open();
                    return Convert.ToString(cmd.ExecuteScalar());

                }
                catch (Exception)
                {
                    conn.Close();
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
        }



        #endregion

        #region 丁绍适增加
        #endregion



        #region 刘鹏宇
        #endregion

        #region 刘娇
        #endregion

        #region 涂利
        /// <summary>
        /// 批量数据插入一个表
        /// </summary>
        /// <param name="sql">选择需要插入的字段，即，目标表</param>
        /// <param name="dt">待插入的数据集</param>
        /// <returns>成功：1，否则抛出异常</returns>
        public static int pl_insert_builder(string sql, DataTable dt)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectString))
                {
                    SqlDataAdapter ada = new SqlDataAdapter(sql, conn);
                    SqlCommandBuilder cbuider = new SqlCommandBuilder(ada);
                    ada.Fill(dt);
                    ada.Update(dt);
                }
                return 1;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public int pl_insert(DataTable dt, string tabelName)
        {
            try
            {
                using (SqlBulkCopy sqlbulkcopy = new SqlBulkCopy(connectString, SqlBulkCopyOptions.UseInternalTransaction))
                {
                    sqlbulkcopy.DestinationTableName = tabelName;//数据库中的表名
                    sqlbulkcopy.WriteToServer(dt);
                }
                return 1;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion


        #region 尹立国
        #endregion
    }
}
