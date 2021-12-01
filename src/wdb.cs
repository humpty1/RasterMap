using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Linq;
using System.IO;
using System;

using Logger;
using ut;
using System.Data.SQLite;

namespace MBTile
{
    public class WorkDb : usingLogger, IDisposable
    {
        TileController tc;

        public WorkDb(string connectionDB, Loger log) : base(log)
        {
            tc = new TileController(connectionDB, log);
        }

        /// <summary>
        /// Создание новых таблиц в базе даных
        /// </summary>
        public void NewTable()
        {
            tc.New();
            tc.dbIni();
        }
        /// <summary>
        /// Очистка базы данных
        /// </summary>
        public void RemoveDB()
        {
            tc.Clear();
        }
        /// <summary>
        /// Обновление базы данных до нужного размера (удаляються не использованые тайлы)
        /// </summary>
        /// <param name="countCache">Допустимый размер базы данных (КБ)</param>
        public void RemoveCache(int countCache)
        {
            tc.UpdateData(countCache);
        }

        /// <summary>
        /// Возвращает таблицу с тайлами (без блобов) которые лежат в базе даных
        /// </summary>
        /// <returns></returns>
        public List<List<string>> SelectAll()
        {
            return tc.SelectUsages();
        }

        public void Dispose()
        {
            tc.Dispose();
        }
    }
}
