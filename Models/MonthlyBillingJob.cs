using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Quartz;
using Quartz.Impl;

namespace EBS.Models
{
    public class MonthlyBillingJob
    {
        // ConnectionString Instance
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;

        public static async Task Main()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();

            IJobDetail job = JobBuilder.Create<BillingJob>()
              .WithIdentity("monthlyBillingJob")
              .Build();

            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity("monthlyBillingTrigger")
              .WithCronSchedule("0 0 1 1/1 * ? *") // run at 00:00 on day-of-month 1
              .Build();

            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
        }
    }

    public class BillingJob : IJob
    {
        // ConnectionString Instance
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;
        public Task Execute(IJobExecutionContext context)
        {
            using (SqlConnection conn = new SqlConnection(SecConn))
            {
                conn.Open();
                string query = "UPDATE CustomerTbl SET isBilledThisMonth = 0";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }
    }
}