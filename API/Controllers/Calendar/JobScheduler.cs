using Quartz;
using Quartz.Impl;

namespace ScheduledTasks
{
    public class JobScheduler
    {
        public static async System.Threading.Tasks.Task Start()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            IJobDetail job = JobBuilder.Create<CalendarReminder>().Build();
            //ITrigger trigger = TriggerBuilder.Create()
            //    .WithDailyTimeIntervalSchedule
            //      (s =>
            //         s.WithIntervalInHours(24)
            //        .OnEveryDay()
            //        .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))
            //      )
            //    .Build();
            ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("cmsn", "BHBQP")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(30)
                .RepeatForever())
            .Build();
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}