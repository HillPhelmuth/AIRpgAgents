﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SkPluginComponents.Models
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddRollDisplayService(this IServiceCollection services)
        {
            return services.AddScoped<RollDiceService>();
        }
    }
}
