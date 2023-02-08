using API.Models;
using Quartz;
using System.Data;
using System;
using System.Collections.Generic;
using Microsoft.ApplicationBlocks.Data;
using Helper;

namespace ScheduledTasks
{
    public class CalendarReminder : IJob
    {
        async System.Threading.Tasks.Task IJob.Execute(IJobExecutionContext context)
        {
            using (DBEntities db = new DBEntities())
            {
                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_reminder").Tables);
                var tables = await task;
                int c = tables[0].Rows.Count;
                List<sys_sendhub> sendhubs = new List<sys_sendhub>();
                List<Dictionary<string, dynamic>> sendsockets = new List<Dictionary<string, dynamic>>();
                foreach (DataRow cr in tables[0].Rows)
                {
                    //sendhub
                    var users = cr.ItemArray[3].ToString().Split(',');
                    foreach (String user_id in users)
                    {
                        sys_sendhub sh = new sys_sendhub();
                        sh.senhub_id = helper.GenKey();
                        sh.module_key = "M2";
                        sh.user_send = "chatbot";
                        sh.receiver = user_id;
                        sh.title = cr.ItemArray[1].ToString();
                        sh.contents = cr.ItemArray[2].ToString();
                        sh.type = 5; //Lịch họp
                        sh.is_type = int.Parse(cr.ItemArray[4].ToString());
                        sh.seen = false;
                        sh.date_send = DateTime.Now;
                        sh.id_key = cr.ItemArray[0].ToString();
                        sh.created_by = "chatbot";
                        sh.created_date = DateTime.Now;
                        sendhubs.Add(sh);
                    }

                    //send socket
                    var message = new Dictionary<string, dynamic>
                    {
                        { "event", "sendNotify" },
                        { "user_id", "chatbot" },
                        { "title", cr.ItemArray[1].ToString() },
                        { "contents", cr.ItemArray[2].ToString() },
                        { "date", DateTime.Now },
                        { "uids", users },
                    };
                    sendsockets.Add(message);
                }
                if (sendhubs.Count > 0)
                {
                    db.sys_sendhub.AddRange(sendhubs);
                    await db.SaveChangesAsync();
                }
                if (sendsockets.Count > 0)
                {
                    foreach (Dictionary<string, dynamic> par in sendsockets)
                    {
                        if (helper.socketClient != null && helper.socketClient.Connected == true)
                        {
                            try
                            {
                                await helper.socketClient.EmitAsync("sendData", par);
                            }
                            catch (Exception) { };
                        }
                    }
                }
            }
            await Console.Out.WriteLineAsync("BHBQP");
        }
    }
}