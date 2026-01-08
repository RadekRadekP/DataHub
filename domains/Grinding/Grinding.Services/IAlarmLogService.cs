using RPK_BlazorApp.Models; // Assuming AlarmLogEntry is in this namespace
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPK_BlazorApp.Services
{
    public interface IAlarmLogService
    {
        Task<List<AlarmLogEntry>> GetAlarmsAsync();
    }
}