using IMSWEB.Data;
using IMSWEB.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMSWEB.Model.TOs;

namespace IMSWEB.Service
{
    public class StockDetailService : IStockDetailService
    {
        private readonly IBaseRepository<StockDetail> _stockDetailRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepository<SRVisitDetail> _SRVisitDetailRepository;

        public StockDetailService(IBaseRepository<StockDetail> stockDetailRepository, IUnitOfWork unitOfWork, IBaseRepository<SRVisitDetail> sRVisitDetailRepository)
        {
            _stockDetailRepository = stockDetailRepository;
            _SRVisitDetailRepository = sRVisitDetailRepository;
            _unitOfWork = unitOfWork;
        }

        public void AddStockDetail(StockDetail StockDetail)
        {
            _stockDetailRepository.Add(StockDetail);
        }

        public void SaveStockDetail()
        {
            _unitOfWork.Commit();
        }

        public IEnumerable<StockDetail> GetStockDetailByProductId(int id)
        {
            return _stockDetailRepository.FindBy(x=>x.ProductID == id);
        }
       
        public void DeleteStockDetail(int id)
        {
            _stockDetailRepository.Delete(x => x.SDetailID == id);
        }
        public StockDetail GetById(int id)
        {
            return _stockDetailRepository.FindBy(x => x.SDetailID == id).FirstOrDefault();
        }


        public IQueryable<StockDetail> GetAll()
        {
            return _stockDetailRepository.All;
        }

        public StockDetail GetStockDetail(int ProductID, int ColorID, string IMEI)
        {
            return _stockDetailRepository.FindBy(i=>i.ProductID==ProductID && i.ColorID==ColorID && i.IMENO.Equals(IMEI.Trim())).FirstOrDefault();
        }
        public IEnumerable<StockDetail> GetStockDetailByProductIdColorID(int ProductID,int ColorID)
        {
            return _stockDetailRepository.FindBy(x => x.ProductID == ProductID && x.ColorID == ColorID && x.Status==(int)EnumStockStatus.Stock);
        }
        public void Update(StockDetail StockDetail)
        {
            _stockDetailRepository.Update(StockDetail);
        }

        public decimal GetRemainingQuantityForSRVisit(int productId, int colorId, int concernId, int GodownId, int? employeeId = null, int stockDetailsId = 0)
        {
            decimal remainingQty = 0m;
            string empQuery = string.Empty;
            if (employeeId.HasValue)
                empQuery = string.Format(@" AND sr.EmployeeID != {0}", employeeId);

            string query = string.Format(@"SELECT SUM((sv.Quantity)) SRQty, 
                                            (SELECT s1.Quantity FROM Stocks s1 WHERE s.ProductID = s1.ProductID AND s.ColorID = s1.ColorID AND s.GodownID = s1.GodownID) StockQty
                                            FROM SRVisitDetails sv 
                                            JOIN SRVisits sr ON sv.SRVisitID = sr.SRVisitID
                                            JOIN Stocks s ON sv.ProductID = s.ProductID AND sv.ColorID = s.ColorID
                                            JOIN SRVProductDetails sd ON sv.SRVisitDID = sd.SRVisitDID
                                            WHERE  sd.Status = 1 AND sv.ProductID = {0} AND sv.ColorID = {1} AND s.ConcernID = {2} AND s.GodownID = {3} {4}
                                            GROUP BY s.ProductID, s.ColorID, s.GodownID", productId, colorId, concernId, GodownId, empQuery);



            SRQuantity data = _SRVisitDetailRepository.SQLQuery<SRQuantity>(query);
            if (data != null)
                remainingQty = data.StockQty - data.SRQty;
            else
            {
                string stockQuery = string.Empty;
                if (stockDetailsId > 0)
                {
                    stockQuery = string.Format(@"SELECT SUM(sd.Quantity) StockQty, CAST(0 as DECIMAL(18, 2)) SRQty FROM Stocks s
                                                    JOIN StockDetails sd ON s.StockID = sd.StockID
                                                    WHERE sd.IsDamage = 0 AND s.ProductID = {0} AND s.ColorID = {1} AND s.ConcernID = {2} AND s.GodownID = {3}", productId, colorId, concernId, GodownId, stockDetailsId);
                }
                else
                {
                    stockQuery = string.Format(@"SELECT Quantity StockQty, CAST(0 as DECIMAL(18, 2)) SRQty FROM Stocks WHERE ProductID = {0} AND ColorID = {1} AND ConcernID = {2} AND s.GodownID = {3}", productId, colorId, concernId, GodownId);
                }
                SRQuantity stockData = _SRVisitDetailRepository.SQLQuery<SRQuantity>(stockQuery);
                remainingQty = stockData.StockQty;
            }

            return remainingQty;
        }

    }
}
