using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.IRepository;
using BulkyBook.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class HeaderRepository : Repository<OrderHeader>, IHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        public HeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public void Update(OrderHeader Objs)
        {
            _context.orderHeaders.Update(Objs);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {

            var orderFromDb = _context.orderHeaders.FirstOrDefault(u => u.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _context.orderHeaders.FirstOrDefault(u => u.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
