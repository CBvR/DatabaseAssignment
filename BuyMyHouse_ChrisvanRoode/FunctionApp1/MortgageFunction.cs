using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Services;

namespace BuyMyHouse
{
    public class MortgageFunction
    {
        private IUsersService _users { get; set; }
        ILogger logger { get; set;  }

        public MortgageFunction(IUsersService UsersService, ILogger<MortgageFunction> _log)
        {
            this._users = UsersService;
            this.logger = _log;
        }

        [Function("MortgageFunction")]
        public void Run([TimerTrigger("1 1 * 1 * *")] MyInfo myTimer, FunctionContext context)
        {
            _users.CalculateMortgage();
            logger.LogInformation($"User mortgage calculated at: {DateTime.Now}");
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
