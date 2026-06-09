using Microsoft.EntityFrameworkCore;
using MMoneyWeb.Web.Data;

namespace MMoneyWeb.Web.Services;

public sealed class LancamentosViewService(IDbContextFactory<MMoneyDbContext> dbContextFactory)
    : LancamentosServiceBase(dbContextFactory);
