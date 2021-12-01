using System.Collections.Generic;
using System.Collections;

using System.Drawing;
using System.Reflection;
using System.Data.Common;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.IO;
using System;

using Logger;
using ut;

namespace MBTile
{
    public class Tile
    {
        public int service { get; set; }
        public int zoom { get; set; }
        public int column { get; set; }
        public int row { get; set; }
        public byte[] tile { get; set; }

        public Tile()
        {
            this.service = -1;
            this.zoom = -1;
            this.column = -1;
            this.row = -1;
            this.tile = null;
        }
        public Tile(int service, int zoom, int column, int row, byte[] tile)
        {
            this.service = service;
            this.zoom = zoom;
            this.column = column;
            this.row = row;
            this.tile = tile;
        }
        public Tile(int service, int zoom, int column, int row, Image tile)
        {
           ImageConverter _imageConverter = new ImageConverter();
           byte[] xBs = (byte[])_imageConverter.ConvertTo(tile, typeof(byte[]));

            this.service = service;
            this.zoom = zoom;
            this.column = column;
            this.row = row;
            this.tile = xBs;
        }
    }

    public class TileController :  usingLogger, IDisposable
    {
        DbConnection connection;

        string dbConnection;


        public TileController(string dbConnection, Loger log) : base (log)
        {
            this.dbConnection = dbConnection;

            using (SQLiteFactory factory = new SQLiteFactory())
            {
                try
                {
                    log.WriteLine(IMPORTANCELEVEL.Stats
                        , "Trying to connect to database. Connection string: '{0}'"
                        , dbConnection);

                    connection = factory.CreateConnection();
                    connection.ConnectionString = dbConnection;

                    connection.Open();
                }
                catch (DbException ex)
                {
                    WriteLine(IMPORTANCELEVEL.Error
                        , "Error while opening connection to database. Str connection: {0}, {1}"
                        , dbConnection, ex.Message);
                    throw new Exception("Error while opening connection: " + ex.Message);
                }
            }

            log.WriteLine(IMPORTANCELEVEL.Stats, "DB connection is open. Str connection: '{0}'", dbConnection);
        }

        public void New()
        {
            try
            {
                WriteLine(IMPORTANCELEVEL.Debug
                    , "no set DB, creating a new database. Connection string: '{0}'"
                    , dbConnection);

                //connection.Open();
                ///Основные поля в таблице 'metadata'
                ///--name: Название тайлсета на английском языке(только латиница)
                ///--type: overlay или baselayer
                ///--version: Версия тайлсета (целочисленное значение)
                ///--description: Описание слоя хранящегося в тайлсете
                ///--format: Формат, в котором хранятся тайлы png или jpg

                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE metadata (" + Environment.NewLine +
                                   "  id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," + Environment.NewLine +
                                   "  service     TEXT NOT NULL," + Environment.NewLine +
                                   "  name        TEXT NOT NULL," + Environment.NewLine +
                                   "  value       TEXT NOT NULL," + Environment.NewLine +
                                   "  FOREIGN KEY(service) REFERENCES service(id)" + Environment.NewLine +
                                   ");";
                    cmd.ExecuteNonQuery();
                    //osm
                    cmd.CommandText = "INSERT INTO metadata (service, name, value) VALUES(0, 'type', 'baselayer');"; cmd.ExecuteNonQuery();
                    cmd.CommandText = "INSERT INTO metadata (service, name, value) VALUES(0, 'format', 'png');"; cmd.ExecuteNonQuery();
                    //visicom
                    cmd.CommandText = "INSERT INTO metadata (service, name, value) VALUES(1, 'type', 'baselayer');"; cmd.ExecuteNonQuery();
                    cmd.CommandText = "INSERT INTO metadata (service, name, value) VALUES(1, 'format', 'png');"; cmd.ExecuteNonQuery();
                    ///yandex
                    cmd.CommandText = "INSERT INTO metadata (service, name, value) VALUES(2, 'type', 'baselayer');"; cmd.ExecuteNonQuery();
                    cmd.CommandText = "INSERT INTO metadata (service, name, value) VALUES(2, 'format', 'png');"; cmd.ExecuteNonQuery();
                }
                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE tiles (" + Environment.NewLine +
                                   "  service         INT  NOT NULL," + Environment.NewLine +
                                   "  zoom_level      INT  NOT NULL," + Environment.NewLine +
                                   "  tile_column     INT  NOT NULL," + Environment.NewLine +
                                   "  tile_row        INT  NOT NULL," + Environment.NewLine +
                                   "  tile_data       BLOB NOT NULL," + Environment.NewLine +
                                   "  PRIMARY KEY(service, zoom_level, tile_column, tile_row)," + Environment.NewLine +
                                   "  FOREIGN KEY(service) REFERENCES service(id)" + Environment.NewLine +
                                   ");";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "create index tiles_i on tiles(zoom_level)";
                    cmd.ExecuteNonQuery();
                }
                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE service (" + Environment.NewLine +
                                   "  id          INT  NOT NULL PRIMARY KEY," + Environment.NewLine +
                                   "  name        TEXT NOT NULL," + Environment.NewLine +
                                   "  cmt         TEXT " + Environment.NewLine +
                                   ");";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "INSERT INTO service (id, name, cmt) VALUES(0, 'osm', 'http://a.tile.openstreetmap.org/{0}/{1}/{2}.png');"; cmd.ExecuteNonQuery();
                    cmd.CommandText = "INSERT INTO service (id, name, cmt) VALUES(1, 'visicom', 'http://tms0.visicom.ua/2.0.0/planet3/base/{0}/{1}/{2}.png');"; cmd.ExecuteNonQuery();
                    cmd.CommandText = "INSERT INTO service (id, name, cmt) VALUES(2, 'yandex', 'http://vec01.maps.yandex.net/tiles?l=map&v=2.28.0&y={2}&x={1}&z={0}&lang=ru-RU');"; cmd.ExecuteNonQuery();
                }
                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE usages (" + Environment.NewLine +
                                   "  service         TEXT  NOT NULL," + Environment.NewLine +
                                   "  zoom_level      INT  NOT NULL," + Environment.NewLine +
                                   "  tile_column     INT  NOT NULL," + Environment.NewLine +
                                   "  tile_row        INT  NOT NULL," + Environment.NewLine +
                                   "  load            DateTime NOT NULL," + Environment.NewLine +
                                   "  usage           INT  NOT NULL," + Environment.NewLine +
                                   "  FOREIGN KEY(service, zoom_level, tile_column, tile_row) " +
                                   "  REFERENCES tiles(service, zoom_level, tile_column, tile_row)" + Environment.NewLine +
                                  ");";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "create index usages_i on usages(zoom_level, usage)";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = 
@"create view usages_v as select 
        u.[zoom_level] zm
     ,  u.[tile_column] x
     ,  u.[tile_row]    y
     ,  s.[name] nm     
     ,  date(datetime(u.[load]))    
     ,  date(datetime ('1970-01-01',u.[usage] ||' seconds'))  usage    
     ,  u.[usage] uSecs     
     ,  s.name nm     
from usages u, service s 
where 
  s.id = u.service
order by  u.usage
";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (DbException ex)
            {
                WriteLine(IMPORTANCELEVEL.Error
                    , "Error while creating new DB! {0}"
                    , ex.Message);
                throw new Exception("Error while creating new DB: " + ex.Message);
            }
        }

        public void dbIni(){
            ArrayList nms = new ArrayList();
          //  nms.Add("0.0.0"); nms.Add("1.0.0"); nms.Add("1.0.1"); nms.Add("1.1.0");
          //  nms.Add("1.1.1"); 
            nms.Add("2.0.0"); nms.Add("2.0.1"); nms.Add("2.0.2");
            nms.Add("2.0.3"); nms.Add("2.1.0"); nms.Add("2.1.1"); nms.Add("2.1.2");
            nms.Add("2.1.3"); nms.Add("2.2.0"); nms.Add("2.2.1"); nms.Add("2.2.2");
            nms.Add("2.2.3"); nms.Add("2.3.0"); nms.Add("2.3.1"); nms.Add("2.3.2");
            nms.Add("2.3.3"); nms.Add("3.0.0"); nms.Add("3.0.1"); nms.Add("3.0.2");
            nms.Add("3.0.3"); nms.Add("3.0.4"); nms.Add("3.0.5"); nms.Add("3.0.6");
            nms.Add("3.0.7"); nms.Add("3.1.0"); nms.Add("3.1.1"); nms.Add("3.1.2");
            nms.Add("3.1.3"); nms.Add("3.1.4"); nms.Add("3.1.5"); nms.Add("3.1.6");
            nms.Add("3.1.7"); nms.Add("3.2.0"); nms.Add("3.2.1"); nms.Add("3.2.2");
            nms.Add("3.2.3"); nms.Add("3.2.4"); nms.Add("3.2.5"); nms.Add("3.2.6");
            nms.Add("3.2.7"); nms.Add("3.3.0"); nms.Add("3.3.1"); nms.Add("3.3.2");
            nms.Add("3.3.3"); nms.Add("3.5.6"); nms.Add("3.4.3"); nms.Add("3.6.6");
            nms.Add("3.3.4"); nms.Add("3.5.7"); nms.Add("3.4.4"); nms.Add("3.6.7");
            nms.Add("3.3.5"); nms.Add("3.6.0"); nms.Add("3.4.5"); nms.Add("3.7.0");
            nms.Add("3.3.6"); nms.Add("3.6.1"); nms.Add("3.4.6"); nms.Add("3.7.1");
            nms.Add("3.3.7"); nms.Add("3.6.2"); nms.Add("3.4.7"); nms.Add("3.7.2");
            nms.Add("3.4.0"); nms.Add("3.6.3"); nms.Add("3.5.0"); nms.Add("3.7.3");
            nms.Add("3.4.1"); nms.Add("3.6.4"); nms.Add("3.5.1"); nms.Add("3.7.4");
            nms.Add("3.4.2"); nms.Add("3.6.5"); nms.Add("3.5.2"); nms.Add("3.7.5");
            nms.Add("3.5.3"); nms.Add("3.7.6"); nms.Add("3.5.4"); nms.Add("3.7.7");
            nms.Add("3.5.5");
           
            WriteLine(IMPORTANCELEVEL.Error
                    , " {0} is here!"
                    , "dbIni");
                 
     		   global::System.Resources.ResourceManager rm 
                         = new global::System.Resources.ResourceManager(
                         "tiles"                                   ///фром tiles.resources 
                         , typeof(TileController).Assembly);       ///из сборки текущего класса
        
           iniOne ("OpenStreetMap", 0,  rm, nms);
           iniOne ("Visicom", 1,  rm, nms);
           iniOne ("Yandex", 2,  rm, nms);

        
           WriteLine(IMPORTANCELEVEL.Error
                    , " {0} is finished!"
                    , "dbIni");
        }

        void iniOne (string service, int oNo, global::System.Resources.ResourceManager rm, ArrayList nms){
           char delimiter = '.';
           int i = 0, z = 0, x = 0, y = 0;
           string imNm = "";
           string z_x_y = "";
           String[] nums ;



           Image im;
           string frmt = "{1}.{0}"; 
           Tile t ;

           for  (i=0; i<nms.Count;i++) {
             z_x_y = (string)nms[i];
             imNm = String.Format(frmt, nms[i], service);
             WriteLine(IMPORTANCELEVEL.Spam
                      , "{0}th attemp, next tile is '{1}'"
                      , i, imNm );
          	 nums = z_x_y.Split(delimiter); 
          	 if (nums.Length > 2){
          	    if(Int32.TryParse(nums[0], out z) 
          	         && Int32.TryParse(nums[1], out x) 
          	           && Int32.TryParse(nums[2], out y)) {
                    	 im = (Image)rm.GetObject(imNm);      //// взятие каринки из ресурсов
                    	 t = new Tile(oNo, z, x, y, im);
                    	 Insert(t);
          	    }
          	    else {
                       WriteLine(IMPORTANCELEVEL.Error
                           , " {0}: Cannot save current image: name/z/x/y:  '{1}'/{2}/{3}/{4}"
                           , "dbIni", imNm, nums[0], nums[1], nums[2] );
          	    }  
          	 }
          	 else {
                     WriteLine(IMPORTANCELEVEL.Error
                        , " {0}: Cannot save current image: name/length: '{1}'/{2}"
                        , "dbIni", imNm, nums.Length);
          	 }
          	
           }
        
        
        }


        public void Clear()
        {
            try
            {
                WriteLine(IMPORTANCELEVEL.Debug
                    , "Clear in database this tables : {1}, {2}. Str connection: {0}"
                    , dbConnection, "tiles", "usages");

                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM usages where zoom_level > 3;";
                    cmd.ExecuteNonQuery();
                }
                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM tiles  where zoom_level > 3;";
                    cmd.ExecuteNonQuery();
                }
            }
            catch(Exception ex)
            {
                WriteLine(IMPORTANCELEVEL.Error
                    , "Error while clear in database this tables : {1}, {2}. Str connection: {0}. {3}"
                    , dbConnection, "tiles", "usages", ex.Message);

                throw new Exception("Error while clear in database tables: tiles, usages" + ex.Message);
            }
        }
        public void Insert(Tile tile)
        {
            try
            {
                WriteLine(IMPORTANCELEVEL.Debug
                    , "Insert tile ( {0}: zoom/column/row: {1}/{2}/{3} ) in database ( {4} )"
                    , tile.service, tile.zoom, tile.column, tile.row, dbConnection);

                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO tiles ("
                        + "service, zoom_level, tile_column, tile_row, tile_data"
                        + ") VALUES ( @serv, @zoom, @column, @row, @data );";
                    //SELECT last_insert_rowid();";

                    DbParameter serv = cmd.CreateParameter();
                    serv.SourceColumn = "service";
                    serv.ParameterName = "@serv";
                    serv.Value = tile.service;
                    cmd.Parameters.Add(serv);

                    DbParameter zoom = cmd.CreateParameter();
                    zoom.SourceColumn = "zoom_level";
                    zoom.ParameterName = "@zoom";
                    zoom.Value = tile.zoom;
                    cmd.Parameters.Add(zoom);

                    DbParameter column = cmd.CreateParameter();
                    column.SourceColumn = "tile_column";
                    column.ParameterName = "@column";
                    column.Value = tile.column;
                    cmd.Parameters.Add(column);

                    DbParameter row = cmd.CreateParameter();
                    row.SourceColumn = "tile_row";
                    row.ParameterName = "@row";
                    row.Value = tile.row;
                    cmd.Parameters.Add(row);

                    DbParameter data = cmd.CreateParameter();
                    data.SourceColumn = "tile_data";
                    data.ParameterName = "@data";
                    data.DbType = DbType.Binary;
                    data.Size = tile.tile.Length;
                    data.Value = tile.tile;
                    cmd.Parameters.Add(data);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (DbException e)
            {
                WriteLine(IMPORTANCELEVEL.Error
                    , "Error while insert tile ( {0}: zoom/column/row: {1}/{2}/{3} ) in database ( {4} )! {5}"
                    , tile.service, tile.zoom, tile.column, tile.row, dbConnection, e.Message);

                throw new Exception("service, zoom, column, row : { " + tile.service + ", " + tile.zoom + ", " + tile.column + ", " + tile.row + " }. " + e.Message);
            }
            InsertUsages(tile);
        }
        public void UpdateData(double countCache)
        {
            try
            {
                WriteLine(IMPORTANCELEVEL.Stats
                    , "Update database ( {0} ): deleting tiles that have been used for a long time. Size database must be less than {1}"
                    , dbConnection, countCache);

                var list = new List<List<string>> { };
                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM usages ORDER BY usage DESC";
                    using (DbDataReader dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            list.Add(new List<string> { });
                            for (int i = 0; i < dataReader.FieldCount; i++)
                                list[list.Count - 1].Add(dataReader[i] + " ");
                        }
                    }
                }
                double countData = 0;
                for (int i = 0; i < list.Count; i++, countData += 25.965)
                {
                    if (countData > countCache)
                    {   if (Convert.ToInt32(list[i][1]) > 3) {
                          DeleteUsages(Convert.ToInt32(list[i][0]),
                                       Convert.ToInt32(list[i][1]),
                                       Convert.ToInt32(list[i][2]),
                                       Convert.ToInt32(list[i][3]));
                          Delete(Convert.ToInt32(list[i][0]),
                                 Convert.ToInt32(list[i][1]),
                                 Convert.ToInt32(list[i][2]),
                                 Convert.ToInt32(list[i][3]));
                       }
                       else 
                         WriteLine(IMPORTANCELEVEL.Spam
                             , "Zoom level less than 4 is not for deleting: zoom:{0}"
                             , list[i][1]);
                    }
                }
            }
            catch (Exception e)
            {
                WriteLine(IMPORTANCELEVEL.Error
                    , "Error while update database ( {0} ). {1}"
                    , dbConnection, e.Message);

                throw new Exception("Error while update database: " + e.Message);
            }
        }

        public void Delete(int serive, int zoom, int column, int row)
        {
            try
            {
                WriteLine(IMPORTANCELEVEL.Debug
                    , "Delete tile ({0}: zoom/column/row: {1}/{2}/{3}) of databse ( {4} )"
                    , serive, zoom, column, row, dbConnection);

                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = String.Format(
                    "DELETE FROM tiles WHERE service={0} AND zoom_level={1} AND tile_column={2} AND tile_row={3}"
                           , serive, zoom, column, row);
                    cmd.ExecuteNonQuery();
                }
            }
            catch(Exception ex)
            {
                WriteLine(IMPORTANCELEVEL.Error
                    , "Error while delete tile ({0}: zoom/column/row: {1}/{2}/{3}) of databse ( {4} )! {5}"
                    , serive, zoom, column, row, dbConnection, ex.Message);

                throw new Exception("Error while delete tile of databse ( " + dbConnection + " )! " + ex.Message);
            }
        }
        public void DeleteAll()
        {
            try
            {
                WriteLine(IMPORTANCELEVEL.Debug
                    , "Delete all tiles of database ( {0} )"
                    , dbConnection);

                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM usages where zoom_level > 3;";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM tiles where  zoom_level > 3";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                WriteLine(IMPORTANCELEVEL.Error
                    , "Error while delete all tiles of databse ( {4} )! {5}"
                    , dbConnection, ex.Message);

                throw new Exception("Error while delete all tiles of databse ( " + dbConnection + " )! " + ex.Message);
            }
        }

        public int Count()
        {
            int count = -1;

            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Count(*) FROM tiles";
                count = int.Parse(cmd.ExecuteScalar() + "");
            }

            return count;
        }

        public List<List<string>> SelectWithoutBlob()
        {
            var list = new List<List<string>> { };

            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT service, zoom_level, tile_column, tile_row FROM tiles";
                using (DbDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        list.Add(new List<string> { });
                        for (int i = 0; i < dataReader.FieldCount; i++)
                            list[list.Count - 1].Add(dataReader[i] + " ");
                    }
                }
            }
            return list;
        }
        public List<Tile> SelectAll()
        {
            List<Tile> tileList = new List<Tile> { };

            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM tiles";
                using (DbDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Tile tile = new Tile();
                        tile.service = dataReader.GetInt32(0);
                        tile.zoom = dataReader.GetInt32(1);
                        tile.column = dataReader.GetInt32(2);
                        tile.row = dataReader.GetInt32(3);

                        // Size of the BLOB buffer.
                        int bufferSize = 4096;
                        // The BLOB byte[] buffer to be filled by GetBytes.
                        byte[] outByte = new byte[bufferSize];
                        // The bytes returned from GetBytes.
                        long retval;
                        // The starting position in the BLOB output.
                        long startIndex = 0;

                        retval = dataReader.GetBytes(4, startIndex, outByte, 0, bufferSize);
                        MemoryStream ms = new MemoryStream();
                        // Continue while there are bytes beyond the size of the buffer.
                        while (retval == bufferSize)
                        {
                            ms.Write(outByte, 0, bufferSize);
                            // Reposition start index to end of last buffer and fill buffer.
                            startIndex += bufferSize;
                            retval = dataReader.GetBytes(4, startIndex, outByte, 0, bufferSize);
                        }
                        ms.Write(outByte, 0, (int)retval - 1);
                        tile.tile = ms.ToArray();
                        tileList.Add(tile);
                    }
                }
            }

            return tileList;
        }
        public Tile Select(int service, int zoom, int column, int row)
        {
            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = String.Format("SELECT * FROM tiles WHERE service={0} AND zoom_level={1} AND tile_column={2} AND tile_row={3}", service, zoom, column, row);
                using (DbDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Tile tile = new Tile();
                        tile.service = dataReader.GetInt32(0);
                        tile.zoom = dataReader.GetInt32(1);
                        tile.column = dataReader.GetInt32(2);
                        tile.row = dataReader.GetInt32(3);

                        // Size of the BLOB buffer.
                        int bufferSize = 4096;
                        // The BLOB byte[] buffer to be filled by GetBytes.
                        byte[] outByte = new byte[bufferSize];
                        // The bytes returned from GetBytes.
                        long retval;
                        // The starting position in the BLOB output.
                        long startIndex = 0;

                        retval = dataReader.GetBytes(4, startIndex, outByte, 0, bufferSize);
                        MemoryStream ms = new MemoryStream();
                        // Continue while there are bytes beyond the size of the buffer.
                        while (retval == bufferSize)
                        {
                            ms.Write(outByte, 0, bufferSize);
                            // Reposition start index to end of last buffer and fill buffer.
                            startIndex += bufferSize;
                            retval = dataReader.GetBytes(4, startIndex, outByte, 0, bufferSize);
                        }
                        ms.Write(outByte, 0, (int)retval - 1);
                        tile.tile = ms.ToArray();
                        return tile;
                    }
                }
            }
            return null;
        }

        public void Dispose()
        {
            connection.Close();
        }

        public void DeleteUsages(int serive, int zoom, int column, int row)
        {
            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = String.Format(
                "DELETE FROM usages WHERE service={0} AND zoom_level={1} AND tile_column={2} AND tile_row={3}"
                                , serive, zoom, column, row);

                cmd.ExecuteNonQuery();
            }
        }
        public void UpdateUsages(Tile tile)
        {
            try
            {
                WriteLine(IMPORTANCELEVEL.Debug
                    , "Update tile ( {0}:  zoom/column/row: {1}/{2}/{3} ) in 'usages' in database ( {4} )"
                    , tile.service, tile.zoom, tile.column, tile.row, dbConnection);

                using (DbCommand cmd = connection.CreateCommand())
                {
                    string newDateUsage = DateTime.Now.Date.ToShortDateString() + "/" + DateTime.Now.ToLongTimeString();
                    cmd.CommandText = "UPDATE usages SET usage=strftime('%s', 'now', 'localtime') "
                        + " WHERE service=" + tile.service + " AND zoom_level=" + tile.zoom + " AND tile_column=" + tile.column + " AND tile_row=" + tile.row + ";";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                WriteLine(IMPORTANCELEVEL.Debug
                    , "Error while updating the tile ( {0}:  zoom/column/row: {1}/{2}/{3} ) in 'usages' in database ( {4} )! {5}"
                    , tile.service, tile.zoom, tile.column, tile.row, dbConnection, e.Message);

                throw new Exception(
                    "Error while updating the tile ( service: " + tile.service + " zoom/column/row : " + tile.zoom + "/ " + tile.column + "/ " + tile.row + " ) to 'usages'. "
                    + e.Message);
            }
        }
        public void InsertUsages(Tile tile)
        {
            try
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
   @"INSERT INTO usages (
     service, zoom_level, tile_column, tile_row, load, usage) 
     VALUES ( @serv, @zoom, @column, @row, datetime('now', 'localtime')
     , strftime('%s', 'now', 'localtime') )";

                    DbParameter serv = cmd.CreateParameter();
                    serv.SourceColumn = "service";
                    serv.ParameterName = "@serv";
                    serv.Value = tile.service;
                    cmd.Parameters.Add(serv);

                    DbParameter zoom = cmd.CreateParameter();
                    zoom.SourceColumn = "zoom_level";
                    zoom.ParameterName = "@zoom";
                    zoom.Value = tile.zoom;
                    cmd.Parameters.Add(zoom);

                    DbParameter column = cmd.CreateParameter();
                    column.SourceColumn = "tile_column";
                    column.ParameterName = "@column";
                    column.Value = tile.column;
                    cmd.Parameters.Add(column);

                    DbParameter row = cmd.CreateParameter();
                    row.SourceColumn = "tile_row";
                    row.ParameterName = "@row";
                    row.Value = tile.row;
                    cmd.Parameters.Add(row);
/*                             
                    DbParameter load = cmd.CreateParameter();
                    load.SourceColumn = "load";
                    load.ParameterName = "@load";
                    load.Value = DateTime.Now.Date.ToShortDateString() + "/" + DateTime.Now.ToLongTimeString();
                    cmd.Parameters.Add(load);

                    DbParameter usage = cmd.CreateParameter();
                    usage.SourceColumn = "usage";
                    usage.ParameterName = "@usage";
                    usage.Value = load.Value;
                    cmd.Parameters.Add(usage);
*/

                    cmd.ExecuteNonQuery();

                    WriteLine(IMPORTANCELEVEL.Debug
                        , "Insert tile ( {0}: zoom/column/row: {1}/{2}/{3}) to 'usages'; connection: '{4})"
                        , tile.service, tile.zoom, tile.column, tile.row,  dbConnection);
                }
            }
            catch (DbException e)
            {
                WriteLine(IMPORTANCELEVEL.Error
                        , "Error while insert tile ( {0}: zoom/column/row: {1}/{2}/{3} ) to 'usages'; connection: '{4}'\n {5}"
                        , tile.service, tile.zoom, tile.column, tile.row, dbConnection, e.Message);

                throw new Exception(
                    "Error while insert tile ( service: " + tile.service + " zoom/column/row : " + tile.zoom + "/ " + tile.column + "/ " + tile.row + " ) to 'usages'. " 
                    + e.Message);
            }
        }
        public List<List<string>> SelectUsages()
        {
            var list = new List<List<string>> { };

            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM usages";
                using (DbDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        list.Add(new List<string> { });
                        for (int i = 0; i < dataReader.FieldCount; i++)
                            list[list.Count - 1].Add(dataReader[i] + " ");
                    }
                }
            }
            return list;
        }

        public int SelectService(string nameService)
        {
            try
            {
                WriteLine(IMPORTANCELEVEL.Debug
                    , "Select service ( {0} ) in database ( {1} )"
                    , nameService, dbConnection);

                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT id FROM service WHERE name='" + nameService + "'";

                    return (int)cmd.ExecuteScalar();
                }
            }
            catch (DbException e)
            {
                WriteLine(IMPORTANCELEVEL.Error
                    , "This service ( {0} ) is absent in database ( {1} )! {2}"
                    , nameService, dbConnection, e.Message);

                throw new Exception("This service is absent in database! Insert it. " + e.Message);
            }
        }
        public void InsertService(List<string> service)
        {
            using (DbCommand cmd = connection.CreateCommand())
            {
                for (int i = 0; i < service.Count; i++)
                {
                    cmd.CommandText = "INSERT INTO service ("
                        + "id, name, cmt"
                        + ") VALUES ( @id, @name, @cmt);";

                    DbParameter id = cmd.CreateParameter();
                    id.SourceColumn = "id";
                    id.ParameterName = "@id";
                    id.Value = i;
                    cmd.Parameters.Add(id);

                    DbParameter name = cmd.CreateParameter();
                    name.SourceColumn = "name";
                    name.ParameterName = "@name";
                    name.Value = service[i];
                    cmd.Parameters.Add(name);

                    DbParameter cmt = cmd.CreateParameter();
                    cmt.SourceColumn = "cmt";
                    cmt.ParameterName = "@cmt";
                    cmt.Value = " ";
                    cmd.Parameters.Add(cmt);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}

