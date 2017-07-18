﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using DBComparator.Models;

namespace DBComparator.Server
{
    public class StoredProcedureComparator
    {
        /// <summary>
        /// Compare the stroed procedures between two databases
        /// </summary>
        /// <param name="conn1"></param>
        /// <param name="conn2"></param>
        /// <returns></returns>
        public List<StoredProcedureDiff> compareStoredProcedure(SqlConnection conn1, SqlConnection conn2)
        {
            List<StoredProcedure> procedures1 = getStoredProcedures(conn1);
            List<StoredProcedure> procedures2 = getStoredProcedures(conn2);

            return getDiffProcedures(procedures1, procedures2);
        }

        /// <summary>
        /// Compare and return the different stroed procedures 
        /// </summary>
        /// <param name="procedures1"></param>
        /// <param name="procedures2"></param>
        /// <returns></returns>
        private List<StoredProcedureDiff> getDiffProcedures(List<StoredProcedure> procedures1, List<StoredProcedure> procedures2)
        {
            List<StoredProcedureDiff> rtn = new List<StoredProcedureDiff>();

            procedures1.Sort((a, b) => a.name.CompareTo(b.name));
            procedures2.Sort((a, b) => a.name.CompareTo(b.name));

            string dbname1 = procedures1.Count() > 0 ? procedures1[0].dbname : "";
            string dbname2 = procedures2.Count() > 0 ? procedures2[0].dbname : "";

            int pos1 = 0, pos2 = 0;
            while (pos1 < procedures1.Count() && pos2 < procedures2.Count())
            {
                StoredProcedure procedure1 = procedures1[pos1];
                StoredProcedure procedure2 = procedures2[pos2];

                // two procedure have the same name
                if (procedure1.name == procedure2.name)
                {
                    // two procedure have different statements
                    if (!procedure1.compare(procedure2))
                    {
                        StoredProcedureDiff proDiff = new StoredProcedureDiff(procedure1.name, true);
                        proDiff.different.Add(procedure1);
                        proDiff.different.Add(procedure2);
                        rtn.Add(proDiff);
                    }
                    pos1++;
                    pos2++;
                }
                else if (procedure1.name.CompareTo(procedure2.name) < 0)
                {
                    StoredProcedureDiff proDiff = new StoredProcedureDiff(procedure1.name, false);
                    proDiff.different.Add(procedure1);
                    proDiff.different.Add(new StoredProcedure(dbname2, procedure1.name, "", false));
                    rtn.Add(proDiff);
                    pos1++;
                }
                else if (procedure1.name.CompareTo(procedure2.name) > 0)
                {
                    StoredProcedureDiff proDiff = new StoredProcedureDiff(procedure2.name, false);
                    proDiff.different.Add(new StoredProcedure(dbname1, procedure2.name, "", false));
                    proDiff.different.Add(procedure2);
                    rtn.Add(proDiff);
                    pos2++;
                }
            }
            while (pos1 < procedures1.Count())
            {
                StoredProcedure procedure1 = procedures1[pos1];
                StoredProcedureDiff proDiff = new StoredProcedureDiff(procedure1.name, false);
                proDiff.different.Add(procedure1);
                proDiff.different.Add(new StoredProcedure(dbname2, procedure1.name, "", false));
                rtn.Add(proDiff);
                pos1++;
            }
            while (pos2 < procedures2.Count())
            {
                StoredProcedure procedure2 = procedures2[pos2];
                StoredProcedureDiff proDiff = new StoredProcedureDiff(procedure2.name, false);
                proDiff.different.Add(new StoredProcedure(dbname1, procedure2.name, "", false));
                proDiff.different.Add(procedure2);
                rtn.Add(proDiff);
                pos2++;
            }

            return rtn;
        }

        /// <summary>
        /// Get all the stored producers from the database
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private List<StoredProcedure> getStoredProcedures(SqlConnection connection)
        {
            List<StoredProcedure> pros = new List<StoredProcedure>();
            List<string> proceduerNames = new List<string>();

            // get stored procedures names
            string command = "select name from dbo.sysobjects where OBJECTPROPERTY(id, N'IsProcedure') = 1 order by name";
            SqlDataReader dr = SqlServer.ExcuteSqlCommandReader(command, connection);
            while (dr.Read())
            {
                proceduerNames.Add(dr[0].ToString());
            }

            // get stored procedures statements
            foreach (string item in proceduerNames)
            {
                string commandStat = "select text from syscomments where id=object_id('" + item + "')";
                SqlDataReader drStat = SqlServer.ExcuteSqlCommandReader(commandStat, connection);

                drStat.Read();
                pros.Add(new StoredProcedure(connection.Database, item, drStat[0].ToString(), true));
            }

            return pros;
        }

    }
}