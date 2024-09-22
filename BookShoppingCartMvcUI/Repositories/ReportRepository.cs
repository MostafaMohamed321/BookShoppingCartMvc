﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMvcUI.Repositories
{
    public class ReportRepository: IReportRepository
    {
        private readonly ApplicationDbContext _context;
        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<TopNSoldBookModel>> GetTopNSellingBooksByDate(DateTime startDate,DateTime endDate)
        {
            var startDateParam = new SqlParameter("@startDate",startDate);
            var endDateParam = new SqlParameter("@endDate",endDate);
            var topFiveSoldBooks=await _context.Database.SqlQueryRaw<TopNSoldBookModel>("exec " +
                "Usp_GetTopNSellingBooksByDate @startDate,@endDate",startDate,endDate).ToListAsync();
            return topFiveSoldBooks;
        }
    }

    public interface IReportRepository
    {
        Task<IEnumerable<TopNSoldBookModel>> GetTopNSellingBooksByDate(DateTime startDate, DateTime endDate);
        
    }
}
